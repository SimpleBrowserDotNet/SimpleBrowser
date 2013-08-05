using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SimpleBrowser.UnitTests.OfflineTests
{
    [TestFixture]
    class Http
    {
        [Test]
        public void Host_Header_Should_Be_Sent()
        {
            Browser b = new Browser(Helper.GetAllways200RequestMocker());
            HttpRequestLog lastLog = null;
            b.RequestLogged += (br, l) =>
            {
                lastLog = l;
            };
            b.Navigate("http://www.blah.com/yadayada.html");
            Assert.That(lastLog.RequestHeaders.AllKeys.Contains("Host"));
            Assert.That(lastLog.RequestHeaders["Host"] == "www.blah.com");
        }
    }
}
