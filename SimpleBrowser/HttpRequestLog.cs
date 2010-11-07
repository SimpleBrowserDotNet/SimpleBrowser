using System;
using System.Collections.Specialized;
using System.Net;
using System.Xml.Linq;

namespace SimpleBrowser
{
	public class HttpRequestLog
	{
		public string Text { get; set; }
		public string Method { get; set; }
		public NameValueCollection PostData { get; set; }
		public WebHeaderCollection RequestHeaders { get; set; }
		public WebHeaderCollection ResponseHeaders { get; set; }
		public int StatusCode { get; set; }
		public Uri Url { get; set; }
		public string ParsedHtml { get; set; }

		public XDocument ToXml()
		{
			var doc = new XDocument(
				new XElement("HttpRequestLog",
					new XAttribute("Date", DateTime.UtcNow.ToString("u")),
					new XElement("Url", Url),
					new XElement("Method", Method),
					new XElement("StatusCode", StatusCode),
					new XElement("ResponseText", new XCData(Text))
				)
			);
			if(PostData != null) doc.Root.Add(PostData.ToXElement("PostData"));
			if(RequestHeaders != null) doc.Root.Add(RequestHeaders.ToXElement("RequestHeaders"));
			if(ResponseHeaders != null) doc.Root.Add(ResponseHeaders.ToXElement("ResponseHeaders"));
			return doc;
		}
	}
}