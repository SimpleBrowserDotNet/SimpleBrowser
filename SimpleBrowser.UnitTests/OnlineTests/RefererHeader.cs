// -----------------------------------------------------------------------
// <copyright file="RefererHeader.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OnlineTests
{
    using NUnit.Framework;
    using System.Threading.Tasks;

    /// <summary>
    /// A test class for testing the $referer$ header.
    /// </summary>
    [TestFixture]
    public class RefererHeader
    {
        /// <summary>
        /// Tests the None When Downgrade Referrer Policy State when not transitioning from secure to unsecure
        /// </summary>
        [Test]
        public async Task When_Testing_Referer_NoneWhenDowngrade_Typical()
        {
            string startingUrl = "http://yenc-post.org/simplebrowser/test1.htm";

            Browser b = new Browser();
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.NoneWhenDowngrade);

            bool success = await b.NavigateAsync(startingUrl);
            Assert.IsTrue(success);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            HtmlResult link = b.Find("test1");
            Assert.IsNotNull(link);

            await link.ClickAsync();
            Assert.IsNotNull(b.CurrentState);
            Assert.AreEqual(b.Referer.ToString(), startingUrl);
        }

        /// <summary>
        /// Tests the None Referrer Policy State
        /// </summary>
        [Test]
        public async Task When_Testing_Referer_None_Typical()
        {
            string startingUrl = "http://yenc-post.org/simplebrowser/test1.htm";

            Browser b = new Browser();
            b.RefererMode = Browser.RefererModes.None;
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.None);

            bool success = await b.NavigateAsync(startingUrl);
            Assert.IsTrue(success);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            HtmlResult link = b.Find("test1");
            Assert.IsNotNull(link);

            await link.ClickAsync();
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);
        }

        /// <summary>
        /// Tests the None When Downgrade Referrer Policy State when transitioning from secure to unsecure.
        /// </summary>
        [Test]
#if NETCOREAPP2_0
        [Ignore("External website browsing has problems. To be investigated to use different provider.")]
#endif
        public async Task When_Testing_Referer_NoneWhenDowngrade_Secure_Transition()
        {
            string startingUrl = "https://www.greatrace.com/";

            Browser b = new Browser();
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.NoneWhenDowngrade);

            bool success = await b.NavigateAsync(startingUrl);
            Assert.IsTrue(success);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            HtmlResult link = b.Find(ElementType.Anchor, "href", "http://www.timewise.us/");
            link.XElement.RemoveAttributeCI("target");
            Assert.IsNotNull(link);

            await link.ClickAsync();
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);
        }

        /// <summary>
        /// Tests the Origin Referrer Policy State
        /// </summary>
        [Test]
        public async Task When_Testing_Referer_Origin_Typical()
        {
            string startingUrl = "http://www.iana.org/domains/reserved";

            Browser b = new Browser();
            b.RefererMode = Browser.RefererModes.Origin;
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.Origin);

            bool success = await b.NavigateAsync(startingUrl);
            Assert.IsTrue(success);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            HtmlResult link = b.Find(ElementType.Anchor, "href", "/");
            Assert.IsNotNull(link);

            await link.ClickAsync();
            Assert.IsNotNull(b.CurrentState);
            Assert.AreEqual(b.Referer.ToString(), "http://www.iana.org/");
        }

        /// <summary>
        /// Tests the Unsafe URL Referrer Policy State with a secure transition.
        /// </summary>
        [Test]
        public async Task When_Testing_Referer_Unsafe_Url_Secure_Transition()
        {
            string startingUrl = "https://www.codeproject.com/";

            Browser b = new Browser();
            b.RefererMode = Browser.RefererModes.UnsafeUrl;
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.UnsafeUrl);

            bool success = await b.NavigateAsync(startingUrl);
            Assert.IsTrue(success);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            HtmlResult link = b.Find("ctl00_AdvertiseLink");
            Assert.IsNotNull(link);

            link.XElement.SetAttributeValue("href", "http://yenc-post.org/simplebrowser/testmeta.htm");
            string targetHref = link.GetAttribute("href");
            Assert.AreEqual(targetHref, "http://yenc-post.org/simplebrowser/testmeta.htm");

            await link.ClickAsync();
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNotNull(b.Referer);
            Assert.AreEqual(b.Referer.ToString(), startingUrl);
        }

        /// <summary>
        /// Test the Referrer Policy State when using the referrer meta tag.
        /// </summary>
        [Test]
        public async Task When_Testing_Referer_MetaReferrer()
        {
            string startingUrl = "http://yenc-post.org/simplebrowser/testmeta.htm";

            Browser b = new Browser();
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.NoneWhenDowngrade);

            bool success = await b.NavigateAsync(startingUrl);
            Assert.IsTrue(success);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            HtmlResult link = b.Find("test1");
            Assert.IsNotNull(link);

            await link.ClickAsync();
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);
        }

        /// <summary>
        /// Test the Referrer Policy State when using the anchor $rel$ attribute.
        /// </summary>
        [Test]
        public async Task When_Testing_Referer_RelNoReferrer()
        {
            string startingUrl = "http://yenc-post.org/simplebrowser/testrel.htm";

            Browser b = new Browser();
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.NoneWhenDowngrade);

            bool success = await b.NavigateAsync(startingUrl);
            Assert.IsTrue(success);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            HtmlResult link = b.Find("test1");
            Assert.IsNotNull(link);

            await link.ClickAsync();
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);
        }
    }
}