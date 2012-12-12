using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SimpleBrowser.UnitTests.OfflineTests
{
	[TestFixture]
	public class WindowsAndFrames
	{
		[SetUp]
		public void ClearAllWindows()
		{
			Browser.ClearWindows();
		}
		[Test]
		public void Clicking_Target_Blank()
		{
			Browser b = new Browser(Helper.GetMoviesRequestMocker());
			HttpRequestLog lastRequest = null;
			b.RequestLogged += (br, l) =>
			{
				lastRequest = l;
			};
			b.Navigate("http://localhost/movies/");
			Assert.That(b.Url == new Uri("http://localhost/movies/"));
			var link = b.Find(ElementType.Anchor, FindBy.Text, "About us");
			link.Click();
			Assert.That(b.Url == new Uri("http://localhost/movies/"));
			Assert.That(Browser.Windows.Count() == 2);
			link.Click();
			Assert.That(Browser.Windows.Count() == 3);
			var newBrowserWindow = Browser.Windows.First(br => br.WindowHandle != b.WindowHandle);
			Assert.That(newBrowserWindow.Url == new Uri("http://localhost/movies/About"));


		}
		[Test]
		public void ClosingBrowsers()
		{
			Browser b = new Browser(Helper.GetMoviesRequestMocker());
			HttpRequestLog lastRequest = null;
			b.RequestLogged += (br, l) =>
			{
				lastRequest = l;
			};
			b.Navigate("http://localhost/movies/");
			Assert.That(b.Url == new Uri("http://localhost/movies/"));
			var link = b.Find(ElementType.Anchor, FindBy.Text, "About us");
			link.Click();
			Assert.That(b.Url == new Uri("http://localhost/movies/"));
			Assert.That(Browser.Windows.Count() == 2);
			b.Close();
			Assert.That(Browser.Windows.Count() == 1);
			Browser.Windows.First().Close();
			Assert.That(Browser.Windows.Count() == 0);
			Assert.Throws(typeof(ObjectDisposedException), () => { Uri s = b.Url; });
		}
		[Test]
		public void Page_With_IFrames()
		{
			Browser b = new Browser(Helper.GetFramesMock());
			HttpRequestLog lastRequest = null;
			b.RequestLogged += (br, l) =>
			{
				lastRequest = l;
			};
			b.Navigate("http://localhost/");
			Assert.That(b.Frames.Count() == 2);

			// now navigate away to a page without frames
			b.Navigate("http://localhost/bla");
			Assert.That(b.Frames.Count() == 0);
			Assert.That(Browser.Windows.Count() == 1);
		}
		[Test]
		public void GetAttribute_Backdoor_FrameHandle()
		{
			Browser b = new Browser(Helper.GetFramesMock());
			HttpRequestLog lastRequest = null;
			b.RequestLogged += (br, l) =>
			{
				lastRequest = l;
			};
			b.Navigate("http://localhost/");
			var elm = b.Select("iframe");
			string handle = elm.GetAttribute("SimpleBrowser.WebDriver:frameWindowHandle");
			Assert.AreEqual(handle, "frame1");
		}	
		[Test]
		public void Navigating_IFrames_Using_Target()
		{
			Browser b = new Browser(Helper.GetFramesMock());
			HttpRequestLog lastRequest = null;
			b.RequestLogged += (br, l) =>
			{
				lastRequest = l;
			};
			b.Navigate("http://localhost/");
			Assert.That(b.Frames.Count() == 2);
			Assert.That(b.Frames.First().Url == new Uri("http://localhost/subdirectory/frame.htm"));

			b.Find("framelink").Click();
			Assert.That(b.Frames.Count() == 2);
			Assert.That(b.Url == new Uri("http://localhost/"));
			Assert.That(b.Frames.First().Url == new Uri("http://localhost/bla.htm"));

		}
	}
}
