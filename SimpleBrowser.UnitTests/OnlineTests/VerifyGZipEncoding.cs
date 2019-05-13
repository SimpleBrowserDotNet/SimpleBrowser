// -----------------------------------------------------------------------
// <copyright file="VerifyGZipEncoding.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OnlineTests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class VerifyGZipEncoding
    {
        [Test]
        public void When_Setting_GZip_Encoding_Content_Should_Still_Be_Returned_As_Text()
        {
            Browser browser = new Browser { UseGZip = true };
            browser.Navigate("http://www.facebook.com/");
            Assert.That(browser.Url.Host == "www.facebook.com");
            Assert.That(browser.Select("Title") != null);
            Assert.That(browser.Select("Title").Value.IndexOf("Facebook", StringComparison.OrdinalIgnoreCase) > -1);
        }
    }
}