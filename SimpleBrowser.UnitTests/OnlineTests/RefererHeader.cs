// -----------------------------------------------------------------------
// <copyright file="RefererHeader.cs" company="SimpleBrowser">
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OnlineTests
{
    using NUnit.Framework;

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
        public void When_Testing_Referer_NoneWhenDowngrade_Typical()
        {
            string startingUrl = "http://afn.org/~afn07998/simplebrowser/test1.htm";

            Browser b = new Browser();
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.NoneWhenDowngrade);

            b.Navigate(startingUrl);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            var link = b.Find("test1");
            Assert.IsNotNull(link);

            link.Click();
            Assert.IsNotNull(b.CurrentState);
            Assert.AreEqual(b.Referer.ToString(), startingUrl);
        }

        /// <summary>
        /// Tests the None Referrer Policy State
        /// </summary>
        [Test]
        public void When_Testing_Referer_None_Typical()
        {
            string startingUrl = "http://afn.org/~afn07998/simplebrowser/test1.htm";

            Browser b = new Browser();
            b.RefererMode = Browser.RefererModes.None;
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.None);

            b.Navigate(startingUrl);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            var link = b.Find("test1");
            Assert.IsNotNull(link);

            link.Click();
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);
        }

        /// <summary>
        /// Tests the None When Downgrade Referrer Policy State when transitioning from secure to unsecure.
        /// </summary>
        [Test]
        public void When_Testing_Referer_NoneWhenDowngrade_Secure_Transition()
        {
            string startingUrl = "https://www.example.com/";

            Browser b = new Browser();
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.NoneWhenDowngrade);

            b.Navigate(startingUrl);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            var link = b.Find(ElementType.Anchor, FindBy.Text, "More information...");
            Assert.IsNotNull(link);

            link.Click();
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);
        }

        /// <summary>
        /// Tests the Origin Referrer Policy State
        /// </summary>
        [Test]
        public void When_Testing_Referer_Origin_Typical()
        {
            string startingUrl = "http://www.iana.org/domains/reserved";

            Browser b = new Browser();
            b.RefererMode = Browser.RefererModes.Origin;
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.Origin);

            b.Navigate(startingUrl);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            var link = b.Find(ElementType.Anchor, "href", "/");
            Assert.IsNotNull(link);

            link.Click();
            Assert.IsNotNull(b.CurrentState);
            Assert.AreEqual(b.Referer.ToString(), "http://www.iana.org/");
        }

        /// <summary>
        /// Tests the Unsafe URL Referrer Policy State with a secure transition.
        /// </summary>
        [Test]
        public void When_Testing_Referer_Unsafe_Url_Secure_Transition()
        {
            string startingUrl = "https://www.example.com/";

            Browser b = new Browser();
            b.RefererMode = Browser.RefererModes.UnsafeUrl;
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.UnsafeUrl);

            b.Navigate(startingUrl);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            var link = b.Find(ElementType.Anchor, FindBy.Text, "More information...");
            Assert.IsNotNull(link);

            string targetHref = link.GetAttribute("href");
            Assert.AreEqual(targetHref, "http://www.iana.org/domains/example");

            link.Click();
            Assert.IsNotNull(b.CurrentState);
            Assert.AreEqual(b.Referer.ToString(), startingUrl);

            // This explicitly tests that a 300 redirect preserves the original referrer.
            Assert.AreEqual(b.CurrentState.Url.ToString(), "http://www.iana.org/domains/reserved");
            Assert.AreNotEqual(b.Referer.ToString(), targetHref);
        }

        /// <summary>
        /// Test the Referrer Policy State when using the referrer meta tag.
        /// </summary>
        [Test]
        public void When_Testing_Referer_MetaReferrer()
        {
            string startingUrl = "http://afn.org/~afn07998/simplebrowser/testmeta.htm";

            Browser b = new Browser();
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.NoneWhenDowngrade);

            b.Navigate(startingUrl);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            var link = b.Find("test1");
            Assert.IsNotNull(link);

            link.Click();
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);
        }

        /// <summary>
        /// Test the Referrer Policy State when using the anchor $rel$ attribute.
        /// </summary>
        [Test]
        public void When_Testing_Referer_RelNoReferrer()
        {
            string startingUrl = "http://afn.org/~afn07998/simplebrowser/testrel.htm";

            Browser b = new Browser();
            Assert.AreEqual(b.RefererMode, Browser.RefererModes.NoneWhenDowngrade);

            b.Navigate(startingUrl);
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);

            var link = b.Find("test1");
            Assert.IsNotNull(link);

            link.Click();
            Assert.IsNotNull(b.CurrentState);
            Assert.IsNull(b.Referer);
        }
    }
}