// -----------------------------------------------------------------------
// <copyright file="DecodedValue.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class DecodedValue
    {
        [Test]
        public void HtmlElement_DecodedValue()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.DecodedValue.htm"));

            HtmlResult div = b.Select("div");
            Assert.That(div.ToList()[0].DecodedValue, Is.EqualTo("£ sign"));
            Assert.That(div.ToList()[1].DecodedValue, Is.EqualTo("üü"));
        }

        [Test]
        public void HtmlElement_DecodedValue_MalformedDocument()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.DecodedValue-malformed.htm"));

            HtmlResult div = b.Select("div");
            Assert.That(div.ToList()[0].DecodedValue, Is.EqualTo("Blah £ sign üü"));
            Assert.That(div.ToList()[1].DecodedValue, Is.EqualTo("£ sign"));
            Assert.That(div.ToList()[2].DecodedValue, Is.EqualTo("üü"));
        }
    }
}