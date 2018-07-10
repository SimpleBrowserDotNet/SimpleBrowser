using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SimpleBrowser.UnitTests.OnlineTests
{
    [TestFixture]
    public class VerifyGZipEncoding
    {
        [Test]
        public void When_Setting_GZip_Encoding_Content_Should_Still_Be_Returned_As_Text()
        {
            var browser = new Browser { UseGZip = true };
            browser.Navigate("http://www.facebook.com/");
            Assert.That(browser.Url.Host == "www.facebook.com");
            Assert.That(browser.Select("Title") != null);
            Assert.That(browser.Select("Title").Value.IndexOf("Facebook", StringComparison.OrdinalIgnoreCase) > -1);
        }

    }
}