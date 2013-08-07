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
            Browser b = new Browser();
            b.UseGZip = true;
            HttpRequestLog lastRequest = null;
            b.RequestLogged += (br, l) =>
            {
                lastRequest = l;
            };
            b.Navigate("http://www.facebook.com/");
            Assert.That(b.Url.Host == "www.facebook.com");
            Assert.That(b.Select("Title") != null);
            Assert.That(b.Select("Title").Value.IndexOf("Facebook", StringComparison.OrdinalIgnoreCase) > -1);
        }

    }
}
