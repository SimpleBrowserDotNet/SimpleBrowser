// -----------------------------------------------------------------------
// <copyright file="Parsing.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
    using NUnit.Framework;

    /// <summary>
    /// A class for testing the HTML parser
    /// </summary>
    [TestFixture]
    public class Parsing
    {
        /// <summary>
        /// Tests that the pre element contains the exact pre-formatted content from the HTML document.
        /// </summary>
        [Test]
        public void PreElement()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.Elements.htm"));

            HtmlResult preContent = b.Find("preTestElement");
            Assert.IsTrue(
                preContent.Value.Contains("space         \r\n            and") ||
                preContent.Value.Contains("space         \n            and"));
        }
    }
}