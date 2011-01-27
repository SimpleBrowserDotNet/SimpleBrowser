using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using SimpleBrowser.Parser;

namespace SimpleBrowser
{
	public class Browser
	{
		private WebProxy _proxy;
		private int _timeoutMilliseconds = 30000;
		private Uri _referrerUrl;
		private NameValueCollection _includeFormValues;
		private CookieContainer _cookies = new CookieContainer();
		private XDocument _doc;
		private HttpRequestLog _lastRequestLog;

		static Browser()
		{
			if(ServicePointManager.Expect100Continue)
				ServicePointManager.Expect100Continue = false;
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
		}

		public Browser()
		{
			CurrentHtml = null;
			UserAgent = "SimpleBrowser (http://github.com/axefrog/SimpleBrowser)";
		}

		public string UserAgent { get; set; }
		public Uri Url { get; private set; }
		public string CurrentHtml { get; private set; }
		public string ResponseText { get; private set; }
		public string ContentType { get; private set; }

		public event Action<Browser, string> MessageLogged;
		public event Action<Browser, HttpRequestLog> RequestLogged;

		public void Log(string message)
		{
			if(MessageLogged != null)
				MessageLogged(this, message);
		}

		public void LogRequestData()
		{
			HttpRequestLog log = AcquireRequestData();
			if(log != null && RequestLogged != null)
				RequestLogged(this, log);
		}

		private HashSet<string> _extraHeaders = new HashSet<string>();
		public void SetHeader(string header)
		{
			_extraHeaders.Add(header);
		}

		public void RemoveHeader(string header)
		{
			_extraHeaders.Remove(header);
		}

		public void SetProxy(string host, int port)
		{
			_proxy = new WebProxy(host, port);
		}

		public void SetProxy(string host, int port, string username, string password)
		{
			SetProxy(host, port);
			_proxy.Credentials = new NetworkCredential(username, password);
		}

		public bool Navigate(string url)
		{
			return Navigate(new Uri(url));
		}

		public bool Navigate(string url, int timeoutMilliseconds)
		{
			return Navigate(new Uri(url), timeoutMilliseconds);
		}

		public bool Navigate(Uri url)
		{
			return DoRequest(url, "GET", null, null, null, _timeoutMilliseconds);
		}

		public bool Navigate(Uri url, string postData, string contentType)
		{
			return DoRequest(url, "POST", null, postData, contentType, _timeoutMilliseconds);
		}

		public bool Navigate(Uri url, int timeoutMilliseconds)
		{
			_timeoutMilliseconds = timeoutMilliseconds;
			return Navigate(url);
		}

		public HtmlResult Find(ElementType elementType, FindBy findBy, string value)
		{
			return GetHtmlResult(FindElement(elementType, findBy, value));
		}

		public HtmlResult Find(string tagName, FindBy findBy, string value)
		{
			return GetHtmlResult(FindElement(FindElements(tagName), findBy, value));
		}

		public HtmlResult Find(string id)
		{
			return GetHtmlResult(FilterElementsByAttribute(XDocument.Descendants().ToList(), "id", id, false));
		}

		public HtmlResult Find(ElementType elementType, string attributeName, string attributeValue)
		{
			var elementsOfCorrectType = FindElements(elementType);
			return GetHtmlResult(FilterElementsByAttribute(elementsOfCorrectType, attributeName, attributeValue, false));
		}

		public HtmlResult Find(ElementType elementType, object elementAttributes)
		{
			var list = FindElements(elementType);
			foreach(var p in elementAttributes.GetType().GetProperties())
			{
				object o = p.GetValue(elementAttributes, null);
				if(o == null)
					continue;
				list = FilterElementsByAttribute(list, p.Name, o.ToString(), false);
			}
			return GetHtmlResult(list);
		}

		public HtmlResult Find(string tagName, object elementAttributes)
		{
			var list = FindElements(tagName);
			foreach(var p in elementAttributes.GetType().GetProperties())
			{
				object o = p.GetValue(elementAttributes, null);
				if(o == null)
					continue;
				list = FilterElementsByAttribute(list, p.Name, o.ToString(), false);
			}
			return GetHtmlResult(list);
		}

		public HtmlResult FindAll(string tagName)
		{
			return GetHtmlResult(FindElements(tagName));
		}

		public HtmlResult FindClosestAncestor(HtmlResult element, string ancestorTagName, object elementAttributes = null)
		{
			XElement anc = element.CurrentElement.Element;
			for(; ; )
			{
				anc = ObtainAncestor(anc, ancestorTagName);
				if(elementAttributes == null)
					break;
				bool succeeded = true;
				foreach(var p in elementAttributes.GetType().GetProperties())
				{
					object o = p.GetValue(elementAttributes, null);
					if(o == null)
						continue;
					var attr = GetAttribute(anc, p.Name);
					if(attr == null || attr.Value.ToLower() != o.ToString().ToLower())
					{
						succeeded = false;
						break;
					}
				}
				if(succeeded)
					break;
				anc = anc.Parent;
			}
			return GetHtmlResult(anc);
		}

		//\/////////////////////////////////////////////////
		// RAW HTML BROWSER
		//\/////////////////////////////////////////////////


		private HtmlResult GetHtmlResult(List<XElement> list)
		{
			List<HtmlElement> xlist = new List<HtmlElement>();
			foreach(var e in list)
			{
				var htmlElement = new HtmlElement(e);
				htmlElement.Clicked += htmlElement_Clicked;
				htmlElement.FormSubmitted += OnHtmlElementSubmittedAsForm;
				xlist.Add(htmlElement);
			}
			return new HtmlResult(xlist);
		}

		private HtmlResult GetHtmlResult(XElement e)
		{
			var htmlElement = new HtmlElement(e);
			htmlElement.Clicked += htmlElement_Clicked;
			htmlElement.FormSubmitted += OnHtmlElementSubmittedAsForm;
			return new HtmlResult(htmlElement);
		}

		private void htmlElement_Clicked(HtmlElement element)
		{
			Log("Clicked element: " + element.Value);

			switch(element.TagName.ToLower())
			{
				case "a": ClickLink(element.Element); return;
				case "input":
					switch(element.InputType)
					{
						case "radio": CheckRadioButton(element.Element); return;
						case "checkbox": CheckCheckbox(element.Element); return;
						case "image":
						case "submit": element.SubmitForm(); return;
						default: return;
					}
			}
		}

		private void OnHtmlElementSubmittedAsForm(HtmlElement element)
		{
			SubmitForm(ObtainAncestor(element.Element, "form"), element.Element);
		}
		private void ClickLink(XElement element)
		{
			var attr = GetAttribute(element, "href");
			if(attr == null || attr.Value.StartsWith("#"))
				return;
			Uri href;
			string attrValue = HttpUtility.HtmlDecode(attr.Value);
			if(Uri.IsWellFormedUriString(attrValue, UriKind.Absolute))
				href = new Uri(attrValue);
			else
				href = new Uri(Url, attrValue);
			DoRequest(href, "GET", null, null, null, _timeoutMilliseconds);
		}
		private void CheckRadioButton(XElement target)
		{
			var nameAttr = GetAttribute(target, "name");
			if(nameAttr == null)
				return;
			var group = FindElements(ElementType.RadioButton)
				//.Where(h => h.Attributes().Where(k => k.Name.LocalName.ToLower() == "checked").Count() > 0)
				.Where(h => h.Attributes().Where(k => k.Name.LocalName.ToLower() == "name" && k.Value.ToLower() == nameAttr.Value.ToLower()).Count() > 0)
				.ToList();
			foreach(var element in group)
			{
				var attr = GetAttribute(element, "checked");
				var value = ReferenceEquals(element, target) ? "checked" : null;
				element.SetAttributeValue(attr == null ? "checked" : attr.Name.LocalName, value); // we do it this way to account for case variations
			}
		}
		private void CheckCheckbox(XElement target)
		{
			var attr = GetAttribute(target, "checked");
			if(attr == null) // if we didnt find it, set it
				target.SetAttributeValue("checked", "checked");
			else // if we found it, it needs to be removed
				target.SetAttributeValue(attr.Name.LocalName, null);
		}
		private XElement ObtainAncestor(XElement descendent, string ancestorTagName)
		{
			if(descendent != null && descendent.Name.LocalName.ToLower() == ancestorTagName)
				return descendent;
			if(descendent == null || descendent.Parent == null)
				throw new InvalidOperationException("The target element does not reside inside an element of type \"" + ancestorTagName + "\"");
			return ObtainAncestor(descendent.Parent, ancestorTagName);
		}
		private void SubmitForm(XElement form, XElement clickedElement)
		{
			Dictionary<string, bool> radioValuesCompleted = new Dictionary<string, bool>();
			NameValueCollection data = new NameValueCollection();
			string[] names = new[] { "input", "textarea", "select" };
			var elements = form.Descendants().Where(h => names.Contains(h.Name.LocalName.ToLower())).ToList();
			foreach(var element in elements)
			{
				if(GetAttribute(element, "disabled") != null)
					continue;
				var nameAttr = GetAttribute(element, "name");
				if(nameAttr == null)
					continue;
				var name = nameAttr.Value;
				switch(element.Name.LocalName.ToLower())
				{
					case "input":
						var typeAttr = GetAttribute(element, "type");
						string typeName;
						if(typeAttr == null)
							typeName = "text";
						else
							typeName = typeAttr.Value;
						var valueAttr = GetAttribute(element, "value") ?? new XAttribute("value", "");
						switch(typeName)
						{
							case "radio":
								if(GetAttribute(element, "checked") == null || radioValuesCompleted.ContainsKey(name))
									continue;
								radioValuesCompleted.Add(name, true);
								data.Add(name, valueAttr.Value);
								break;

							case "checkbox":
								if(GetAttribute(element, "checked") == null)
									continue;
								data.Add(name, valueAttr.Value);
								break;

							case "submit":
							case "image":
								if(!ReferenceEquals(element, clickedElement))
									continue;
								data.Add(name, valueAttr.Value);
								break;

							case "hidden":
							case "text":
							case "password":
								data.Add(name, valueAttr.Value);
								break;
						}
						break;

					case "textarea":
						data.Add(name, element.Value);
						break;

					case "select":
						var multipleAttr = GetAttribute(element, "multiple");
						if(multipleAttr != null)
							foreach(var option in element.Descendants()
								.Where(h => h.Name.LocalName.ToLower() == "option"
											&& h.Attributes().Where(k => k.Name.LocalName.ToLower() == "selected").Count() > 0)
								.ToList())
								data.Add(name, GetOptionValue(option));
						else
						{
							var option = element.Descendants()
								.Where(h => h.Name.LocalName.ToLower() == "option"
											&& h.Attributes().Where(k => k.Name.LocalName.ToLower() == "selected").Count() > 0)
								.FirstOrDefault();
							if(option != null)
								data.Add(name, GetOptionValue(option));
						}
						break;
				}
			}
			var methodAttr = GetAttribute(form, "method");
			var method = methodAttr == null ? "GET" : methodAttr.Value.ToUpper() == "POST" ? "POST" : "GET";
			var actionAttr = GetAttribute(form, "action");
			var action = actionAttr == null ? Url : Uri.IsWellFormedUriString(actionAttr.Value, UriKind.Absolute) ? new Uri(actionAttr.Value) : new Uri(Url, actionAttr.Value);

			DoRequest(action, method, data, null, null, _timeoutMilliseconds);
		}
		private string GetOptionValue(XElement option)
		{
			var attr = GetAttribute(option, "value");
			if(attr == null)
				return option.Value;
			return attr.Value;
		}
		private List<XElement> FindElements(string tagName)
		{
			return XDocument.Descendants()
				.Where(h => h.Name.LocalName.ToLower() == tagName.ToLower())
				.ToList();
		}
		private List<XElement> FindElements(ElementType elementType)
		{
			List<XElement> list;
			switch(elementType)
			{
				case ElementType.Anchor:
					list = XDocument.Descendants()
						.Where(h => h.Name.LocalName.ToLower() == "a" && h.Attributes()
																			.Where(k => k.Name.LocalName.ToLower() == "href").Count() > 0)
						.ToList();
					break;

				case ElementType.Button:
					list = XDocument.Descendants()
						.Where(h => h.Name.LocalName.ToLower() == "button" ||
									(h.Name.LocalName.ToLower() == "input" && h.Attributes()
																				.Where(k => k.Name.LocalName.ToLower() == "type"
																							&& (k.Value.ToLower() == "submit" || k.Value.ToLower() == "image")).Count() > 0))
						.ToList();
					break;

				case ElementType.TextField:
					list = XDocument.Descendants()
						.Where(h => h.Name.LocalName.ToLower() == "textarea" ||
									(h.Name.LocalName.ToLower() == "input" && h.Attributes()
																				.Where(k => k.Name.LocalName.ToLower() == "type"
																							&& (k.Value.ToLower() == "text" || k.Value.ToLower() == "password" || k.Value.ToLower() == "hidden")).Count() > 0))
						.ToList();
					list.AddRange(XDocument.Descendants() // also add input elements with no "type" attribute (they default to type="text")
									.Where(h => h.Name.LocalName.ToLower() == "input" && h.Attributes()
																							.Where(k => k.Name.LocalName.ToLower() == "type").Count() == 0));
					break;

				case ElementType.Checkbox:
					list = XDocument.Descendants()
						.Where(h => h.Name.LocalName.ToLower() == "input" && h.Attributes()
																				.Where(k => k.Name.LocalName.ToLower() == "type" && k.Value.ToLower() == "checkbox").Count() > 0)
						.ToList();
					break;

				case ElementType.RadioButton:
					list = XDocument.Descendants()
						.Where(h => h.Name.LocalName.ToLower() == "input" && h.Attributes()
																				.Where(k => k.Name.LocalName.ToLower() == "type" && k.Value.ToLower() == "radio").Count() > 0)
						.ToList();
					break;

				case ElementType.SelectBox:
					list = XDocument.Descendants()
						.Where(h => h.Name.LocalName.ToLower() == "select")
						.ToList();
					break;

				case ElementType.Script:
					list = XDocument.Descendants()
						.Where(h => h.Name.LocalName.ToLower() == "script")
						.ToList();
					break;

				default:
					list = new List<XElement>();
					break;
			}
			return list;
		}
		private List<XElement> FilterElementsByAttribute(List<XElement> elements, string attributeName, string value, bool allowPartialMatch)
		{
			if(allowPartialMatch)
				return elements.Where(h => h.Attributes()
											.Where(k => k.Name.LocalName.ToLower() == attributeName.ToLower()
														&& k.Value.ToLower().Contains(value.ToLower())).Count() > 0)
					.ToList();
			return elements.Where(h => h.Attributes()
										.Where(k => k.Name.LocalName.ToLower() == attributeName.ToLower()
													&& k.Value.ToLower() == value.ToLower()).Count() > 0)
				.ToList();
		}
		private List<XElement> FilterElementsByInnerText(List<XElement> elements, string tagName, string value, bool allowPartialMatch)
		{
			if(allowPartialMatch)
				return elements.Where(h => (tagName == null || h.Name.LocalName.ToLower() == tagName.ToLower())
										   && h.Value.ToLower().Trim().Contains(value.ToLower().Trim()))
					.ToList();
			return elements.Where(h => (tagName == null || h.Name.LocalName.ToLower() == tagName.ToLower())
									   && h.Value.ToLower().Trim() == value.ToLower().Trim())
				.ToList();
		}
		private List<XElement> FindElement(ElementType elementType, FindBy findBy, string value)
		{
			return FindElement(FindElements(elementType), findBy, value);
		}
		private List<XElement> FindElement(List<XElement> elements, FindBy findBy, string value)
		{
			switch(findBy)
			{
				case FindBy.Text: return FilterElementsByInnerText(elements, null, value, false);
				case FindBy.Class: return FilterElementsByAttribute(elements, "class", value, false);
				case FindBy.Id: return FilterElementsByAttribute(elements, "id", value, false);
				case FindBy.Name: return FilterElementsByAttribute(elements, "name", value, false);
				case FindBy.Value:
					{
						var newlist = FilterElementsByAttribute(elements, "value", value, false);
						newlist.AddRange(FilterElementsByInnerText(elements, "textarea", value, false));
						newlist.AddRange(FilterElementsByInnerText(elements, "button", value, false));
						return newlist;
					}
				case FindBy.PartialText: return FilterElementsByInnerText(elements, null, value, true);
				case FindBy.PartialClass: return FilterElementsByAttribute(elements, "class", value, true);
				case FindBy.PartialId: return FilterElementsByAttribute(elements, "id", value, true);
				case FindBy.PartialName: return FilterElementsByAttribute(elements, "name", value, true);
				case FindBy.PartialValue:
					{
						var newlist = FilterElementsByAttribute(elements, "value", value, true);
						newlist.AddRange(FilterElementsByInnerText(elements, "textarea", value, true));
						newlist.AddRange(FilterElementsByInnerText(elements, "button", value, true));
						return newlist;
					}
				default:
					return null;
			}
		}
		private XAttribute GetAttribute(XElement element, string name)
		{
			return element.Attributes().Where(h => h.Name.LocalName.ToLower() == name.ToLower()).FirstOrDefault();
		}

		/// <summary>
		/// Returns the current HTML document parsed and converted to a valid XDocument object. Note that the
		/// originating HTML does not have to be valid XML; the parser will use a variety of methods to convert any
		/// invalid markup to valid XML.
		/// </summary>
		private XDocument XDocument
		{
			get
			{
				if(_doc == null)
				{
					try
					{
						_doc = CurrentHtml.ParseHtml();
					}
					catch(Exception ex)
					{
						Log("Error converting HTML to XML for URL " + Url);
						Log(ex.Message);
						_doc = HtmlParser.CreateBlankHtmlDocument();
					}
				}
				return _doc;
			}
		}

		private HttpWebRequest PrepareRequestObject(Uri url, string method, int timeoutMilliseconds)
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			req.Method = method;
			req.ContentType = "application/x-www-form-urlencoded";
			req.UserAgent = UserAgent;
			req.Accept = "*/*";
			req.Timeout = timeoutMilliseconds;
			req.AllowAutoRedirect = false;
			req.CookieContainer = _cookies;
			if(_proxy != null)
				req.Proxy = _proxy;
			if(_referrerUrl != null)
				req.Referer = _referrerUrl.ToString();
			return req;
		}

		private bool DoRequest(Uri uri, string method, NameValueCollection postVars, string postData, string contentType, int timeoutMilliseconds)
		{
			/* IMPORTANT INFORMATION:
			 * HttpWebRequest has a bug where if a 302 redirect is encountered (such as from a Response.Redirect), any cookies
			 * generated during the request are ignored and discarded during the internal redirect process. The headers are in
			 * fact returned, but the normal process where the cookie headers are turned into Cookie objects in the cookie
			 * container is skipped, thus breaking the login processes of half the sites on the internet.
			 * 
			 * The workaround is as follows:
			 * 1. Turn off AllowAutoRedirect so we can intercept the redirect and do things manually
			 * 2. Read the Set-Cookie headers from the response and manually insert them into the cookie container
			 * 3. Get the Location header and redirect to the location specified in the "Location" response header
			 * 
			 * Worth noting that even if this bug has been solved in .Net 4 (I haven't checked) we should still use manual
			 * redirection so that we can properly log responses.
			 * 
			 * OBSOLETE ISSUE: (Bug has been resolved in the .Net 4 framework, which this library is now targeted at)
			 * //CookieContainer also has a horrible bug relating to the specified cookie domain. Basically, if it contains
			 * //a cookie where the "domain" token is specified as ".domain.xxx" and you attempt to request http://domain.ext,
			 * //the cookies are not retrievable via that Uri, as you would expect them to be. CookieContainer is incorrectly
			 * //assuming that the leading dot is a prerequisite specifying that a subdomain is required as opposed to the
			 * //correct behaviour which would be to take it to mean that the domain and all subdomains are valid for the cookie.
			 * //http://channel9.msdn.com/forums/TechOff/260235-Bug-in-CookieContainer-where-do-I-report/?CommentID=397315
			 * //The workaround is as follows:
			 * //When retrieving the response, iterate through the Set-Cookie header and any cookie that explicitly sets
			 * //the domain token with a leading dot should also set the cookie without the leading dot.
			 */

			bool handle301Or302Redirect;
			int maxRedirects = 10;
			string html;
			do
			{
				Debug.WriteLine(uri.ToString());
				if(maxRedirects-- == 0)
				{
					Log("Too many 302 redirects");
					return false;
				}
				handle301Or302Redirect = false;
				HttpWebRequest req = PrepareRequestObject(uri, method, timeoutMilliseconds);
				foreach(var header in _extraHeaders)
					req.Headers.Add(header);
				if(_includeFormValues != null)
				{
					if(postVars == null)
						postVars = _includeFormValues;
					else
						postVars.Add(_includeFormValues);
				}

				if(postVars != null)
				{
					if(method == "POST")
					{
						byte[] data = Encoding.ASCII.GetBytes(StringUtil.MakeQueryString(postVars));
						req.ContentLength = data.Length;
						Stream stream = req.GetRequestStream();
						stream.Write(data, 0, data.Length);
						stream.Close();
					}
					else
					{
						uri = new Uri(uri.Scheme + "://" + uri.Host + uri.AbsolutePath + "?" + StringUtil.MakeQueryString(postVars));
						req = PrepareRequestObject(uri, method, timeoutMilliseconds);
					}
				}
				else if(postData != null)
				{
					if(method == "GET")
						throw new InvalidOperationException("Cannot call DoRequest with method GET and non-null postData");
					byte[] data = Encoding.ASCII.GetBytes(postData);
					req.ContentLength = data.Length;
					Stream stream = req.GetRequestStream();
					stream.Write(data, 0, data.Length);
					stream.Close();
				}

				if(contentType != null)
					req.ContentType = contentType;

				try
				{
					using(HttpWebResponse response = (HttpWebResponse)req.GetResponse())
					{
						StreamReader reader = new StreamReader(response.GetResponseStream());
						html = reader.ReadToEnd();
						ResponseText = html;
						reader.Close();
						string oldHTML = html;
						//html = StripAndRebuildHtml(html);
						CurrentHtml = html;
						ContentType = response.ContentType;
						_doc = null;
						_includeFormValues = null;
						_lastRequestLog = new HttpRequestLog
										  {
											  Text = oldHTML,
											  ParsedHtml = XDocument.ToString(),
											  Method = method,
											  PostData = postVars,
											  RequestHeaders = req.Headers,
											  ResponseHeaders = response.Headers,
											  StatusCode = (int)response.StatusCode,
											  Url = uri
										  };
						LogRequestData();
						if((int)response.StatusCode == 302 || (int)response.StatusCode == 301)
						{
							//url = AdjustUrl(url, response.Headers["Location"]);
							uri = new Uri(uri, response.Headers["Location"]);
							handle301Or302Redirect = true;
							Url = uri;
							Debug.WriteLine("Redirecting to: " + Url);
							method = "GET";
							postData = null;
						}
					}
				}
				catch(WebException ex)
				{
					LastWebException = ex;
					switch(ex.Status)
					{
						case WebExceptionStatus.Timeout:
							Log("A timeout occurred while trying to load the web page");
							break;

						case WebExceptionStatus.ReceiveFailure:
							Log("The response was cut short prematurely");
							break;

						default:
							Log("An exception was thrown while trying to load the page: " + ex.Message);
							break;
					}
					return false;
				}
			} while(handle301Or302Redirect);
			Url = uri;
			_referrerUrl = uri;
			CurrentHtml = html;
			return true;
		}

		public WebException LastWebException { get; private set; }

		private HttpRequestLog AcquireRequestData()
		{
			return _lastRequestLog;
		}

		/// <summary>
		/// Performs a culture-invariant text search on the current document, ignoring whitespace, html elements and case, which reduces the 
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public bool ContainsText(string text)
		{
			text = HttpUtility.HtmlDecode(text);
			string source = HttpUtility.HtmlDecode(XDocument.Root.Value).Replace("&nbsp;", " ");
			return new Regex(Regex.Replace(Regex.Escape(text).Replace(@"\ ", " "), @"\s+", @"\s+"), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).IsMatch(source);
		}

		/// <summary>
		/// Overwrites the CurrentHtml property with new content, allowing it to be queried and analyzed as though it
		/// was navigated to in the last request.
		/// </summary>
		/// <param name="content">A string containing a</param>
		public void SetContent(string content)
		{
			CurrentHtml = content;
			ContentType = "image/html";
			_doc = null;
		}

		/// <summary>
		/// This collection allows you to specify additional key/value pairs that will be sent in the next request. Some
		/// websites use JavaScript or other dynamic methods to dictate what is submitted to the next page and these
		/// cannot be determined automatically from the originating HTML. In those cases, investigate the process using
		/// an HTTP sniffer, such as the HttpFox plugin for Firefox, and determine what values are being sent in the
		/// form submission. The additional unexpected values can then be automated by populating this property. Any
		/// values specified here are cleared after each request.
		/// </summary>
		public NameValueCollection ExtraFormValues
		{
			get { return _includeFormValues ?? (_includeFormValues = new NameValueCollection()); }
		}
	}
}


