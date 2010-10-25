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
using SimpleBrowser;
using SimpleBrowser.Parser;

namespace SimpleBrowser
{
	public class Browser : HttpBrowserBase
	{
		public Browser()
		{
			_currentHtml = null;
			if(ServicePointManager.Expect100Continue)
				ServicePointManager.Expect100Continue = false;
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			if(_userAgent == null)
				_userAgent = WebCrawler.GetRandomUserAgent();
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

		public override void SetProxy(string host, int port)
		{
			_proxy = new WebProxy(host, port);
		}

		public override void SetProxy(string host, int port, string username, string password)
		{
			SetProxy(host, port);
			_proxy.Credentials = new NetworkCredential(username, password);
		}

		private WebProxy _proxy;
		private int _timeoutMilliseconds = 30000;

		public override bool Navigate(Uri url)
		{
			return DoRequest(url, "GET", null, null, null, _timeoutMilliseconds);
		}

		public bool Navigate(Uri url, string postData, string contentType)
		{
			return DoRequest(url, "POST", null, postData, contentType, _timeoutMilliseconds);
		}

		public override bool Navigate(Uri url, int timeoutMilliseconds)
		{
			_timeoutMilliseconds = timeoutMilliseconds;
			return Navigate(url);
		}

		public override HtmlResult Find(ElementType elementType, FindBy findBy, string value)
		{
			return GetHtmlResult(FindElement(elementType, findBy, value));
		}

		public override HtmlResult Find(string tagName, FindBy findBy, string value)
		{
			return GetHtmlResult(FindElement(FindElements(tagName), findBy, value));
		}

		public override HtmlResult Find(string id)
		{
			return GetHtmlResult(FilterElementsByAttribute(HtmlXml.Descendants().ToList(), "id", id, false));
		}

		public override HtmlResult Find(ElementType elementType, string attributeName, string attributeValue)
		{
			return GetHtmlResult(FindElement(FilterElementsByAttribute(FindElements(elementType), "name", attributeName, false), FindBy.Value, attributeValue));
		}

		public override HtmlResult Find(ElementType elementType, object elementAttributes)
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

		public override HtmlResult Find(string tagName, object elementAttributes)
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

		public override HtmlResult FindAll(string tagName)
		{
			return GetHtmlResult(FindElements(tagName));
		}

		public override HtmlResult FindClosestAncestor(HtmlResult element, string ancestorTagName)
		{
			return FindClosestAncestor(element, ancestorTagName, null);
		}

		public override HtmlResult FindClosestAncestor(HtmlResult element, string ancestorTagName, object elementAttributes)
		{
			XElement anc = ((XHtmlElement)element.Element).Element;
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

		public override void Dispose()
		{
		}

		//\/////////////////////////////////////////////////
		// RAW HTML BROWSER
		//\/////////////////////////////////////////////////

		private string _currentHtml;
		private Uri _referrerUrl;
		private string _contentType;
		private NameValueCollection _includeFormValues;
		private CookieContainer _cookies = new CookieContainer();
		private NameValueCollection _cookieReference = new NameValueCollection();
		private XDocument _doc;
		private HttpRequestLog _lastRequestLog;

		private HtmlResult GetHtmlResult(List<XElement> list)
		{
			List<IElement> xlist = new List<IElement>();
			foreach(var e in list)
			{
				var htmlElement = new XHtmlElement(e);
				htmlElement.Clicked += htmlElement_Clicked;
				htmlElement.FormSubmitted += htmlElement_FormSubmitted;
				xlist.Add(htmlElement);
			}
			return new HtmlResult(xlist);
		}

		private HtmlResult GetHtmlResult(XElement e)
		{
			var htmlElement = new XHtmlElement(e);
			htmlElement.Clicked += htmlElement_Clicked;
			htmlElement.FormSubmitted += htmlElement_FormSubmitted;
			return new HtmlResult(htmlElement);
		}

		void htmlElement_Clicked(XHtmlElement element)
		{
			if(AutoLogStatusMessages)
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

		void htmlElement_FormSubmitted(XHtmlElement element)
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
			string[] names = new [] { "input", "textarea", "select" };
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
			return HtmlXml.Descendants()
				.Where(h => h.Name.LocalName.ToLower() == tagName.ToLower())
				.ToList();
		}
		private List<XElement> FindElements(ElementType elementType)
		{
			List<XElement> list;
			switch(elementType)
			{
				case ElementType.Anchor:
					list = HtmlXml.Descendants()
						.Where(h => h.Name.LocalName.ToLower() == "a" && h.Attributes()
						                                                 	.Where(k => k.Name.LocalName.ToLower() == "href").Count() > 0)
						.ToList();
					break;

				case ElementType.Button:
					list = HtmlXml.Descendants()
						.Where(h => h.Name.LocalName.ToLower() == "button" ||
						            (h.Name.LocalName.ToLower() == "input" && h.Attributes()
						                                                      	.Where(k => k.Name.LocalName.ToLower() == "type"
						                                                      	            && (k.Value.ToLower() == "submit" || k.Value.ToLower() == "image")).Count() > 0))
						.ToList();
					break;

				case ElementType.TextField:
					list = HtmlXml.Descendants()
						.Where(h => h.Name.LocalName.ToLower() == "textarea" ||
						            (h.Name.LocalName.ToLower() == "input" && h.Attributes()
						                                                      	.Where(k => k.Name.LocalName.ToLower() == "type"
						                                                      	            && (k.Value.ToLower() == "text" || k.Value.ToLower() == "password" || k.Value.ToLower() == "hidden")).Count() > 0))
						.ToList();
					list.AddRange(HtmlXml.Descendants() // also add input elements with no "type" attribute (they default to type="text")
					              	.Where(h => h.Name.LocalName.ToLower() == "input" && h.Attributes()
					              	                                                     	.Where(k => k.Name.LocalName.ToLower() == "type").Count() == 0));
					break;

				case ElementType.Checkbox:
					list = HtmlXml.Descendants()
						.Where(h => h.Name.LocalName.ToLower() == "input" && h.Attributes()
						                                                     	.Where(k => k.Name.LocalName.ToLower() == "type" && k.Value.ToLower() == "checkbox").Count() > 0)
						.ToList();
					break;

				case ElementType.RadioButton:
					list = HtmlXml.Descendants()
						.Where(h => h.Name.LocalName.ToLower() == "input" && h.Attributes()
						                                                     	.Where(k => k.Name.LocalName.ToLower() == "type" && k.Value.ToLower() == "radio").Count() > 0)
						.ToList();
					break;

				case ElementType.SelectBox:
					list = HtmlXml.Descendants()
						.Where(h => h.Name.LocalName.ToLower() == "select")
						.ToList();
					break;

				case ElementType.Script:
					list = HtmlXml.Descendants()
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

		protected XDocument HtmlXml
		{
			get
			{
				if(_doc == null)
				{
					try
					{
						_doc = _currentHtml.ParseHtml();
					}
					catch(HtmlParserException ex)
					{
						Log("Error converting HTML to XML for URL " + Url);
						Log(ex.Message);
						_doc = HtmlParser.CreateBlankHtmlDocument();
					}
				}
				return _doc;
			}
		}

		private string _userAgent;
		private HttpWebRequest PrepareRequestObject(Uri url, string method, int timeoutMilliseconds)
		{
			HttpWebRequest req = new WebCrawler().CreateRequestObject(url, method, _userAgent);
			req.Timeout = timeoutMilliseconds;
			req.AllowAutoRedirect = false;
			req.CookieContainer = _cookies;
			if(_proxy != null)
				req.Proxy = _proxy;
			if(_referrerUrl != null)
				req.Referer = _referrerUrl.ToString();
			return req;
		}
		//private static string AdjustUrl(string originalURL, string newURL)
		//{
		//    if(Regex.IsMatch(newURL, "^https?:"))
		//        return newURL;
		//    Uri uri = new Uri(originalURL);
		//    string absPath;
		//    if(newURL.StartsWith("/"))
		//        absPath = "";
		//    else if(uri.AbsolutePath.EndsWith("/"))
		//        absPath = uri.AbsolutePath;
		//    else
		//        absPath = uri.AbsolutePath.Substring(0, uri.AbsolutePath.LastIndexOf("/") + 1);
		//    return new Uri(uri.Scheme + "://" + uri.Host + absPath + newURL).ToString();
		//}

		private bool DoRequest(Uri uri, string method, NameValueCollection postVars, string postData, string contentType,
		                       int timeoutMilliseconds)
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
			 * CookieContainer also has a horrible bug relating to the specified cookie domain. Basically, if it contains
			 * a cookie where the "domain" token is specified as ".domain.xxx" and you attempt to request http://domain.ext,
			 * the cookies are not retrievable via that Uri, as you would expect them to be. CookieContainer is incorrectly
			 * assuming that the leading dot is a prerequisite specifying that a subdomain is required as opposed to the
			 * correct behaviour which would be to take it to mean that the domain and all subdomains are valid for the cookie.
			 * http://channel9.msdn.com/forums/TechOff/260235-Bug-in-CookieContainer-where-do-I-report/?CommentID=397315
			 * 
			 * The workaround is as follows:
			 * When retrieving the response, iterate through the Set-Cookie header and any cookie that explicitly sets
			 * the domain token with a leading dot should also set the cookie without the leading dot.
			 */

			bool handle301or302Redirect;
			int maxRedirects = 10;
			string html;
			do
			{
				Debug.WriteLine(uri.ToString());
				if(maxRedirects-- == 0)
				{
					if(AutoLogStatusMessages)
						Log("Too many 302 redirects");
					return false;
				}
				handle301or302Redirect = false;
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
					using(HttpWebResponse response = new WebCrawler().GetResponse(req))
					{
						StreamReader reader = new StreamReader(response.GetResponseStream());
						html = reader.ReadToEnd();
						ResponseText = html;
						reader.Close();
						string oldHTML = html;
						//html = StripAndRebuildHtml(html);
						_currentHtml = html;
						_contentType = response.ContentType;
						_doc = null;
						_includeFormValues = null;
						_lastRequestLog = new HttpRequestLog
						                  {
						                  	Text = oldHTML,
											ParsedHtml = HtmlXml.ToString(),
						                  	Method = method,
						                  	PostData = postVars,
						                  	RequestHeaders = req.Headers,
						                  	ResponseHeaders = response.Headers,
						                  	StatusCode = (int)response.StatusCode,
						                  	Url = uri
						                  };
						if(AutoLogRequestData)
							LogRequestData();
						//string host = uri.Host;
						//string[] parts = host.Split('.');
						//if(parts.Length > 2)
						//    host = parts[parts.Length - 2] + "." + parts[parts.Length - 1];
						//var cookieUri = new Uri("http://" + host);
						//foreach(Cookie cookie in response.Cookies)
						//{
						//    if(cookie.Domain != null)
						//    {
						//        string domain = cookie.Domain;
						//        if(domain.StartsWith("."))
						//            domain = domain.Substring(1);
						//        if(domain != cookieUri.Host)
						//            _cookies.Add(new Uri("http://" + domain), new Cookie(cookie.Name, cookie.Value));
						//        _cookies.Add(cookieUri, new Cookie(cookie.Name, cookie.Value));
						//    }
						//}
						//Regex rx = new Regex(@"(?:^|,)(\S.*?)=(.*?);");
						//foreach(Match match in rx.Matches(response.Headers[HttpResponseHeader.SetCookie] ?? string.Empty))
						//{
						//    try
						//    {
						//        var cookieName = HttpUtility.UrlDecode(match.Groups[1].Value);
						//        var cookieValue = HttpUtility.UrlDecode(match.Groups[2].Value);
						//        if(cookieValue.Contains(","))
						//            cookieValue = cookieValue.Replace(",", "%2C");
						//        Cookie cookie = new Cookie(cookieName, cookieValue);
						//        _cookieReference[cookieName] = cookieValue;
						//        if((int)response.StatusCode == 302)
						//            _cookies.Add(cookieUri, cookie);
						//    }
						//    catch(CookieException ex)
						//    {
						//        if(AutoLogStatusMessages)
						//            Log("An error was encountered while trying to write out the cookie. The error was: " + ex.Message);
						//    }
						//}
						if((int)response.StatusCode == 302 || (int)response.StatusCode == 301)
						{
							//url = AdjustUrl(url, response.Headers["Location"]);
							uri = new Uri(uri, response.Headers["Location"]);
							handle301or302Redirect = true;
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
					if(AutoLogStatusMessages)
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
			} while(handle301or302Redirect);
			Url = uri;
			_referrerUrl = uri;
			_currentHtml = html;
			return true;
		}

		public WebException LastWebException { get; private set; }

		protected override HttpRequestLog AcquireRequestData()
		{
			return _lastRequestLog;
		}

		public override bool ContainsText(string text)
		{
			text = HttpUtility.HtmlDecode(text);
			string source = HttpUtility.HtmlDecode(HtmlXml.Root.Value).Replace("&nbsp;", " ");
			return new Regex(Regex.Replace(Regex.Escape(text).Replace(@"\ ", " "), @"\s+", @"\s+"), RegexOptions.IgnoreCase).IsMatch(source);
		}

		public void SetContent(string content)
		{
			_currentHtml = content;
			_contentType = "image/html";
			_doc = null;
		}

		public NameValueCollection AdditionalPostVars
		{
			get
			{
				if(_includeFormValues == null)
					_includeFormValues = new NameValueCollection();
				return _includeFormValues;
			}
		}
	}
}


