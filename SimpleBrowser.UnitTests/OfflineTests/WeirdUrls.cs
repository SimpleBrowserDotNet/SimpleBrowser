// -----------------------------------------------------------------------
// <copyright file="WeirdUrls.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
    using NUnit.Framework;
    using System.Threading.Tasks;

    [TestFixture]
    internal class WeirdUrls
    {
        [Test]
        public async Task JavascriptUrl()
        {
            Browser b = new Browser(); // does not need network to fail
            bool res = await b.NavigateAsync("javascript:'';");
            Assert.False(res);
        }
    }
}