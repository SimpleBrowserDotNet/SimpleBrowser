// -----------------------------------------------------------------------
// <copyright file="HttpHeaderTests.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
    using NUnit.Framework;

    [TestFixture]
    public class HttpHeaderTests
    {
        [Test]
        public void AddHostHeaderDoesNotThrow()
        {
            Browser browser = new Browser();
            Assert.DoesNotThrow(() => browser.SetHeader("host:www.google.com"));
        }
    }
}