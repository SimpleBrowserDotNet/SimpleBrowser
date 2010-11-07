using System.Xml.Linq;

namespace SimpleBrowser.Parser
{
	public static class HtmlParser
	{
		public static XDocument ParseHtml(this string html, bool removeExtraWhiteSpace = true)
		{
			var tokens = HtmlTokenizer.Parse(html, removeExtraWhiteSpace);
			var doc = DocumentBuilder.Parse(tokens);
			DocumentCleaner.Rebuild(doc);
			return doc;
		}

	    public static XDocument CreateBlankHtmlDocument()
	    {
	        return XDocument.Parse("<?xml version=\"1.0\"?>\r\n<html><body /></html>");
	    }
	}
}
