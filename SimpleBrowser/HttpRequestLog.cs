using System;
using System.Collections.Specialized;
using System.Net;
using System.Xml.Linq;

namespace SimpleBrowser
{
	public abstract class LogItem
	{
		protected LogItem()
		{
			ServerTime = DateTime.UtcNow;
		}

		public DateTime ServerTime { get; set; }
	}

	public class HttpRequestLog : LogItem
	{
		public string Text { get; set; }
		public string Method { get; set; }
		public NameValueCollection PostData { get; set; }
		public string PostBody { get; set; }
		public NameValueCollection QueryStringData { get; set; }
		public WebHeaderCollection RequestHeaders { get; set; }
		public WebHeaderCollection ResponseHeaders { get; set; }
		public int StatusCode { get; set; }
		public Uri Url { get; set; }

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

		public override string ToString()
		{
			return string.Concat("{", Method, " to ", Url.ToString().ShortenTo(50, true), "}");
		}
	}

	public class LogMessage : LogItem
	{
		public LogMessage(string message, LogMessageType type = LogMessageType.User)
		{
			Message = message;
			Type = type;
		}

		public string Message { get; set; }
		public LogMessageType Type { get; set; }

		public override string ToString()
		{
			return string.Concat("{", Message.ShortenTo(80, true), "}");
		}
	}

	public enum LogMessageType
	{
		User,
		Internal,
		Error,
		StackTrace
	}
}