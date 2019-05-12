// -----------------------------------------------------------------------
// <copyright file="HttpRequestLog.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser
{
    using System;
    using System.Collections.Specialized;
    using System.Net;
    using System.Xml.Linq;

    public abstract class LogItem
    {
        protected LogItem()
        {
            this.ServerTime = DateTime.UtcNow;
        }

        public DateTime ServerTime { get; set; }
    }

    public class HttpRequestLog : LogItem
    {
        public string Text { get; set; }
        public string ParsedHtml { get; set; }
        public string Method { get; set; }
        public NameValueCollection PostData { get; set; }
        public string PostBody { get; set; }
        public NameValueCollection QueryStringData { get; set; }
        public WebHeaderCollection RequestHeaders { get; set; }
        public WebHeaderCollection ResponseHeaders { get; set; }
        public int ResponseCode { get; set; }
        public Uri Url { get; set; }
        public Uri Address { get; set; }
        public string Host { get; set; }

        public XDocument ToXml()
        {
            XDocument doc = new XDocument(
                new XElement("HttpRequestLog",
                    new XAttribute("Date", DateTime.UtcNow.ToString("u")),
                    new XElement("Url", this.Url),
                    new XElement("Method", this.Method),
                    new XElement("ResponseCode", this.ResponseCode),
                    new XElement("ResponseText", new XCData(this.Text))
                )
            );
            if (this.PostData != null)
            {
                doc.Root.Add(this.PostData.ToXElement("PostData"));
            }

            if (this.RequestHeaders != null)
            {
                doc.Root.Add(this.RequestHeaders.ToXElement("RequestHeaders"));
            }

            if (this.ResponseHeaders != null)
            {
                doc.Root.Add(this.ResponseHeaders.ToXElement("ResponseHeaders"));
            }

            return doc;
        }

        public override string ToString()
        {
            return string.Concat("{", this.Method, " to ", this.Url.ToString().ShortenTo(50, true), "}");
        }
    }

    public class LogMessage : LogItem
    {
        public LogMessage(string message, LogMessageType type = LogMessageType.User)
        {
            this.Message = message;
            this.Type = type;
        }

        public string Message { get; set; }
        public LogMessageType Type { get; set; }

        public override string ToString()
        {
            return string.Concat("{", this.Message.ShortenTo(80, true), "}");
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