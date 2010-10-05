using System;

namespace SimpleBrowser
{
	public class HtmlParserException : Exception
	{
		public string Html { get; private set; }

		internal HtmlParserException(string message, string html) : base(message)
		{
			Html = html;
		}

		internal HtmlParserException(string message, string html, Exception ex) : base(message, ex)
		{
			Html = html;
		}
	}
}