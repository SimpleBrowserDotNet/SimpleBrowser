using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using SimpleBrowser;

namespace SimpleBrowser
{
	public class WebCrawler
	{
		private readonly CookieContainer _cookieContainer;
		private readonly IEnumerable<string> _headers;

		public WebCrawler()
		{
			_cookieContainer = new CookieContainer();
		}

		public WebCrawler(IEnumerable<string> headers)
		{
			_headers = headers;
		}

		public WebCrawler(CookieContainer cookieContainer)
		{
			_cookieContainer = cookieContainer;
		}

		public WebCrawler(CookieContainer cookieContainer, IEnumerable<string> headers)
		{
			_cookieContainer = cookieContainer;
			_headers = headers;
		}

		public WebCrawlResult Get(Uri uri)
		{
			return Get(uri, null, null);
		}

		public WebCrawlResult Get(Uri uri, int timeoutMilliseconds)
		{
			return Get(uri, null, timeoutMilliseconds);
		}

		public WebCrawlResult Get(Uri uri, WebProxy proxy)
		{
			return Get(uri, null, proxy);
		}

		public WebCrawlResult Get(Uri uri, object queryStringData)
		{
			return Get(uri, null, null, queryStringData);
		}

		public WebCrawlResult Get(Uri uri, object queryStringData, int timeoutMilliseconds)
		{
			return Get(uri, null, timeoutMilliseconds, queryStringData);
		}

		public WebCrawlResult Get(Uri uri, object queryStringData, WebProxy proxy)
		{
			return Get(uri, proxy, null, queryStringData);
		}

		public WebCrawlResult Get(Uri uri, WebProxy proxy, int? timeoutMilliseconds, object queryStringData)
		{
			if(queryStringData != null)
				new Uri(uri.AbsolutePath + "?" + queryStringData.ToQueryString());

			WebCrawlResult result = new WebCrawlResult();
			var req = CreateRequestObject(uri, "GET");
			req.Timeout = timeoutMilliseconds.HasValue ? timeoutMilliseconds.Value : 30000;
			if(proxy != null)
				req.Proxy = proxy;
			try
			{
				using(var response = GetResponse(req))
				{
					result.Status = response.StatusCode;
					var reader = new StreamReader(response.GetResponseStream());
					string output = reader.ReadToEnd();
					result.ResponseText = output;
				}
			}
			catch(WebException ex)
			{
				var response = ex.Response as HttpWebResponse;
				if(response != null)
					result.Status = response.StatusCode;
				result.ErrorMessage = ex.Message;
			}
			catch(Exception ex)
			{
				result.ErrorMessage = ex.Message;
			}
			return result;
		}

		public WebCrawlResult Post(Uri uri, string formKey, string value)
		{
			NameValueCollection nvc = new NameValueCollection();
			nvc[formKey] = value;
			return Post(uri, nvc);
		}

		public WebCrawlResult Post(Uri uri, NameValueCollection data)
		{
			return PostString(uri, StringUtil.MakeQueryString(data), null);
		}

		public WebCrawlResult Post(Uri uri, string data)
		{
			return PostString(uri, data, null);
		}

		public WebCrawlResult PostString(Uri uri, string postData, string contentType)
		{
			WebCrawlResult result = new WebCrawlResult();
			try
			{
				var req = CreateRequestObject(uri, "POST");
				if(!string.IsNullOrEmpty(contentType))
					req.Headers.Add("Content-Type", contentType);
				byte[] data = Encoding.ASCII.GetBytes(postData);
				req.ContentLength = data.Length;
				Stream stream = req.GetRequestStream();
				stream.Write(data, 0, data.Length);
				stream.Close();
				using(var response = GetResponse(req))
				{
					StreamReader reader = new StreamReader(response.GetResponseStream());
					result.ResponseText = reader.ReadToEnd();
					result.Status = HttpStatusCode.OK;
				}
			}
			catch(WebException ex)
			{
				var response = ex.Response as HttpWebResponse;
				if(response != null)
				{
					result.Status = response.StatusCode;
					var stream = response.GetResponseStream();
					if(stream != null)
					{
						StreamReader reader = new StreamReader(stream);
						result.ResponseText = reader.ReadToEnd();
					}
					else
						result.ResponseText = "";
				}
				result.ErrorMessage = ex.Message;
			}
			catch(Exception ex)
			{
				result.ErrorMessage = ex.Message;
				result.ResponseText = ex.ToString();
			}
			return result;
		}

		public HttpWebRequest CreateRequestObject(Uri uri, string method)
		{
			return CreateRequestObject(uri, method, GetRandomUserAgent());
		}

		public HttpWebRequest CreateRequestObject(Uri uri, string method, string userAgent)
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
			req.Timeout = 30000;
			req.Method = method;
			req.ContentType = "application/x-www-form-urlencoded";
			req.UserAgent = userAgent;
			req.Accept = "*/*";
			if(_cookieContainer != null)
				req.CookieContainer = _cookieContainer;
			if(_headers != null)
				foreach(var header in _headers)
					req.Headers.Add(header);
			return req;
		}

		static string[] _userAgents = new[] {
			"Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/532.6 (KHTML, like Gecko) Chrome/4.0.266.0 Safari/532.6 - You! ",
			"Mozilla/5.0 (Windows; U; Windows NT 6.0; pl; rv:1.9.1.6) Gecko/20091201 Firefox/3.5.6 (.NET CLR 3.5.30729) ",
			"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET C ",
			"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR ",
			"Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_2; en-US) AppleWebKit/532.5 (KHTML, like Gecko) Chrome/4.0.249.43 Safari/ ",
			"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; GTB6; .NET CLR 2.0.50727; OfficeLiveConnector.1.3; OfficeLivePatch.0. ",
			"Mozilla/5.0 (en-us) AppleWebKit/525.13 (KHTML, like Gecko; Google Wireless Transcoder) Version/3.1 Safari/525.13 ",
			"Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.0.16) Gecko/2009120208 Firefox/3.0.16 (.NET CLR 3.5.30729) ",
			"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; GTB6; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; OfficeLiveConne ",
			"Mozilla/5.0 (Windows; U; Windows NT 5.1; de; rv:1.9.1.4) Gecko/20091016 Firefox/3.5.4 (.NET CLR 3.5.30729) ",
			"Mozilla/5.0 (Windows; U; Windows NT 6.0; de; rv:1.9.1.6) Gecko/20091201 Firefox/3.5.6 ",
			"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR ",
			"Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.1.6) Gecko/20091201 Firefox/3.5.6 (.NET CLR 3.5.30729) ",
			"Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.0.16) Gecko/2009120208 Firefox/3.0.16 (.NET CLR 3.5.30729) ",
			"Opera/9.80 (Windows 98; U; en) Presto/2.2.15 Version/10.01 ",
			"Mozilla/5.0 (Windows; U; Windows NT 5.1; de; rv:1.9.1.4) Gecko/20091016 Firefox/3.5.4 (.NET CLR 3.5.30729) ",
			"Mozilla/5.0 (Windows; U; Windows NT 5.1; de; rv:1.9.1.6) Gecko/20091201 Firefox/3.5.6 ",
			"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; Trident/4.0; GTB6.3; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; ",
			"Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0) ",
			"Mozilla/5.0 (Windows; U; Windows NT 5.1; pl; rv:1.9.0.16) Gecko/2009120208 Firefox/3.0.16 ",
		};
		static Random _rand = new Random();
		public static string GetRandomUserAgent()
		{
			return _userAgents[_rand.Next(_userAgents.Length)];
		}

		public HttpWebResponse GetResponse(HttpWebRequest req)
		{
			return (HttpWebResponse)req.GetResponse();
		}
	}

	public class WebCrawlResult
	{
		public string ResponseText { get; set; }
		public HttpStatusCode Status { get; set; }
		public string ErrorMessage { get; set; }
		public bool HasError
		{
			get { return !string.IsNullOrEmpty(ErrorMessage) || (Status != HttpStatusCode.OK); }
		}

		public T As<T>()
		{
			var jss = new JavaScriptSerializer();
			if(HasError)
				return jss.Deserialize<T>(jss.Serialize(this));
			return new JavaScriptSerializer().Deserialize<T>(ResponseText ?? "{}");
		}
	}
}