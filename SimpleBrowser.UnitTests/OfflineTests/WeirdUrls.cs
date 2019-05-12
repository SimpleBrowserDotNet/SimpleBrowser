// -----------------------------------------------------------------------
// <copyright file="WeirdUrls.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
    using NUnit.Framework;

    [TestFixture]
    internal class WeirdUrls
    {
        [Test]
        public void JavascriptUrl()
        {
            Browser b = new Browser(); // does not need network to fail
            bool res = b.Navigate("javascript:'';");
            Assert.False(res);
        }
    }
}