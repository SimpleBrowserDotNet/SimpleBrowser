using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace SimpleBrowser
{
	public static class HtmlParser
	{
		static HtmlParser()
		{
			HtmlNode.ElementsFlags.Remove("form");
		}

		public static XDocument CreateBlankHtmlDocument()
		{
			return XDocument.Parse("<?xml version=\"1.0\"?>\r\n<html><body /></html>");
		}

		public static XDocument SanitizeHtml(string html)
		{
			var xmlStr = SanitizeInternal(html);
			XDocument doc;
			try { doc = XDocument.Parse(xmlStr); }
			catch(XmlException ex)
			{
				if(ex.Message.Contains("multiple root elements") || ex.Message.Contains("Root element is missing"))
				{
					xmlStr = "<?xml version=\"1.0\"?>\r\n<html>" + Regex.Replace(xmlStr, @"\<\?xml[^\>]*\?\>", "") + "\r\n</html>";
					try
					{
						doc = XDocument.Parse(xmlStr);
					}
					catch(Exception ex1)
					{
						throw new HtmlParserException("Unable to parse HTML code", html, ex1);
					}
				}
				else
					throw new HtmlParserException("Unable to parse HTML code", html, ex);
			}
			return doc;
		}

		private static string SanitizeInternal(string html)
		{
			var hdoc = new HtmlDocument();
			hdoc.LoadHtml(html ?? "");
			using(StringWriter writer = new StringWriter())
			{
				using(XmlTextWriter xtw = new XmlTextWriter(writer))
				{
					xtw.Formatting = Formatting.Indented;
					xtw.IndentChar = '\t';
					xtw.Indentation = 1;
					hdoc.DocumentNode.WriteTo(xtw);
					xtw.Close();
					return SanitizeXmlString(writer.ToString());
				}
			}
		}
		private static string SanitizeXmlString(string xml)
		{
			if(xml == null)
				throw new ArgumentNullException("xml");

			StringBuilder buffer = new StringBuilder(xml.Length);
			foreach(char c in xml.Where(c => IsLegalXmlChar(c)))
				buffer.Append(c);

			return buffer.ToString();
		}
		private static bool IsLegalXmlChar(int character)
		{
			return
				(
					character == 0x9 /* == '\t' == 9   */          ||
					character == 0xA /* == '\n' == 10  */          ||
					character == 0xD /* == '\r' == 13  */          ||
					(character >= 0x20 && character <= 0xD7FF) ||
					(character >= 0xE000 && character <= 0xFFFD) ||
					(character >= 0x10000 && character <= 0x10FFFF)
				);
		}
	}
}
