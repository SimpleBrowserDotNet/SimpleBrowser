// -----------------------------------------------------------------------
// <copyright file="HtmlParser.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Parser
{
    using System.Xml.Linq;

    public static class HtmlParser
    {
        public static XDocument ParseHtml(this string html, bool removeExtraWhiteSpace = true)
        {
            System.Collections.Generic.List<HtmlParserToken> tokens = HtmlTokenizer.Parse(html, removeExtraWhiteSpace);
            XDocument doc = DocumentBuilder.Parse(tokens);
            DocumentCleaner.Rebuild(doc);
            return doc;
        }

        public static XDocument CreateBlankHtmlDocument()
        {
            return XDocument.Parse("<?xml version=\"1.0\"?>\r\n<html><body /></html>");
        }
    }
}