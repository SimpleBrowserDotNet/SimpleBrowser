using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SimpleBrowser.UnitTests.OnlineTests
{
	[TestFixture]
	public class RefererHeader
	{
		[Test]
		public void When_Testing_Referer_DefaultMode_Typical()
		{
			string startingUrl = "http://afn.org/~afn07998/simplebrowser/test1.htm";

			Browser b = new Browser();
			Assert.AreEqual(b.RefererMode, Browser.RefererModes.Default);

			b.Navigate(startingUrl);
			Assert.IsNotNull(b.CurrentState);
			Assert.IsNull(b.Referer);

			var link = b.Find("test1");
			Assert.IsNotNull(link);

			link.Click();
			Assert.IsNotNull(b.CurrentState);
			Assert.AreEqual(b.Referer.ToString(), startingUrl);
		}

		[Test]
		public void When_Testing_Referer_NeverMode_Typical()
		{
			string startingUrl = "http://afn.org/~afn07998/simplebrowser/test1.htm";

			Browser b = new Browser();
			b.RefererMode = Browser.RefererModes.Never;
			Assert.AreEqual(b.RefererMode, Browser.RefererModes.Never);


			b.Navigate(startingUrl);
			Assert.IsNotNull(b.CurrentState);
			Assert.IsNull(b.Referer);

			var link = b.Find("test1");
			Assert.IsNotNull(link);

			link.Click();
			Assert.IsNotNull(b.CurrentState);
			Assert.IsNull(b.Referer);
		}

		[Test]
		public void When_Testing_Referer_DefaultMode_Secure_Transition()
		{
			string startingUrl = "https://www.example.com/";

			Browser b = new Browser();
			Assert.AreEqual(b.RefererMode, Browser.RefererModes.Default);

			b.Navigate(startingUrl);
			Assert.IsNotNull(b.CurrentState);
			Assert.IsNull(b.Referer);

			var link = b.Find(ElementType.Anchor, FindBy.Text, "More information...");
			Assert.IsNotNull(link);

			link.Click();
			Assert.IsNotNull(b.CurrentState);
			Assert.IsNull(b.Referer);
		}

		[Test]
		public void When_Testing_Referer_OriginMode_Typical()
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

		[Test]
		public void When_Testing_Referer_AlwaysMode_Secure_Transition()
		{
			string startingUrl = "https://www.example.com/";

			Browser b = new Browser();
			b.RefererMode = Browser.RefererModes.Always;
			Assert.AreEqual(b.RefererMode, Browser.RefererModes.Always);

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

		[Test]
		public void When_Testing_Referer_MetaReferrer()
		{
			string startingUrl = "http://afn.org/~afn07998/simplebrowser/testmeta.htm";

			Browser b = new Browser();
			Assert.AreEqual(b.RefererMode, Browser.RefererModes.Default);

			b.Navigate(startingUrl);
			Assert.IsNotNull(b.CurrentState);
			Assert.IsNull(b.Referer);

			var link = b.Find("test1");
			Assert.IsNotNull(link);

			link.Click();
			Assert.IsNotNull(b.CurrentState);
			Assert.IsNull(b.Referer);
		}

		[Test]
		public void When_Testing_Referer_RelNoReferrer()
		{
			string startingUrl = "http://afn.org/~afn07998/simplebrowser/testrel.htm";

			Browser b = new Browser();
			Assert.AreEqual(b.RefererMode, Browser.RefererModes.Default);

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
