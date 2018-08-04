// -----------------------------------------------------------------------
// <copyright file="DocumentCleaner.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Parser
{
    using System.Xml.Linq;

    /// <summary>
    /// Implements a class to clean up HTML
    /// </summary>
    public static class DocumentCleaner
    {
        /// <summary>
        /// Rebuilds the document
        /// </summary>
        /// <param name="doc">The document to clean</param>
        public static void Rebuild(XDocument doc)
        {
            if (string.Compare(doc.Root.Name.LocalName, "html", true) != 0)
            {
                XElement root = new XElement("html");
                doc.Root.ReplaceWith(root);
            }
        }
    }
}