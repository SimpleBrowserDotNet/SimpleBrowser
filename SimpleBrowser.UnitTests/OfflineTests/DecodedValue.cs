using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SimpleBrowser.UnitTests.OfflineTests
{
    [TestFixture]
    public class DecodedValue
    {
        [Test]
        public void HtmlElement_DecodedValue()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.DecodedValue.htm"));

            var div = b.Select("div");
            Assert.That(div.ToList()[0].DecodedValue, Is.EqualTo("£ sign"));
			Assert.That(div.ToList()[1].DecodedValue, Is.EqualTo("üü"));
		}

        [Test]
        public void HtmlElement_DecodedValue_MalformedDocument()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.DecodedValue-malformed.htm"));

            var div = b.Select("div");
            Assert.That(div.ToList()[0].DecodedValue, Is.EqualTo("Blah £ sign üü"));
            Assert.That(div.ToList()[1].DecodedValue, Is.EqualTo("£ sign"));
            Assert.That(div.ToList()[2].DecodedValue, Is.EqualTo("üü"));
        }
    }
}
