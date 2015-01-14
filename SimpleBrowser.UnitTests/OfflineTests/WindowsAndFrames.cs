using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SimpleBrowser.UnitTests.OfflineTests
{
	[TestFixture]
	public class WindowsAndFrames
	{
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
			Assert.That(b.Windows.Count() == 2);
			link.Click();
			Assert.That(b.Windows.Count() == 3);
			var newBrowserWindow = b.Windows.First(br => br.WindowHandle != b.WindowHandle);
			Assert.That(newBrowserWindow.Url == new Uri("http://localhost/movies/About"));
		}

		[Test]
		public void Holding_Ctrl_Shft_Opens_New_Window()
		{
			Browser b = new Browser(Helper.GetMoviesRequestMocker());
			HttpRequestLog lastRequest = null;
			b.RequestLogged += (br, l) =>
			{
				lastRequest = l;
			};
			b.Navigate("http://localhost/movies/");
			Assert.That(b.Url == new Uri("http://localhost/movies/"));
			var link = b.Find(ElementType.Anchor, FindBy.Text, "Home");
			link.Click();
			Assert.That(b.Windows.Count() == 1);
			link = b.Find(ElementType.Anchor, FindBy.Text, "Home");
			b.KeyState = KeyStateOption.Ctrl;
			link.Click();
			Assert.That(b.Windows.Count() == 2);
			link = b.Find(ElementType.Anchor, FindBy.Text, "Home");
			b.KeyState = KeyStateOption.Shift;
			link.Click();
			Assert.That(b.Windows.Count() == 3);
			link = b.Find(ElementType.Anchor, FindBy.Text, "Home");
			b.KeyState = KeyStateOption.Alt;
			link.Click();
			Assert.That(b.Windows.Count() == 3); // alt does not open new browser
		}


		[Test]
		public void Accessing_New_Windows_Using_Event()
		{
            Browser b = new Browser(Helper.GetMoviesRequestMocker());
			Browser newlyOpened = null;
			b.NewWindowOpened += (b1, b2) =>
			{
				newlyOpened = b2;
			};
			b.Navigate("http://localhost/movies/");
			Assert.That(b.Url == new Uri("http://localhost/movies/"));
			var link = b.Find(ElementType.Anchor, FindBy.Text, "Details");
			b.KeyState = KeyStateOption.Ctrl;
			link.Click();
			Assert.That(b.Windows.Count() == 2);
			Assert.NotNull(newlyOpened);
			Assert.That(b.Url.ToString() == "http://localhost/movies/");
			Assert.That(newlyOpened.Url.ToString() == "http://localhost/movies/Movies/Details/1");
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
			Assert.That(b.Windows.Count() == 2);
			b.Close();
			Assert.That(b.Windows.Count() == 1);
			b.Windows.First().Close();
			Assert.That(b.Windows.Count() == 0);
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
			Assert.That(b.Windows.Count() == 1);
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
        [Test]
        public void Static_scoped_clear_works()
        {
            Browser b1 = new Browser(Helper.GetFramesMock());
            Browser b2 = new Browser(Helper.GetFramesMock());
            Browser.ClearWindows();
            Assert.Throws(typeof(ObjectDisposedException), () => b1.Navigate("http://localhost/"));
        }
        [Test]
        public void Instance_scoped_clear_works()   
        {
            Browser b1 = new Browser(Helper.GetFramesMock());
            Browser b2 = new Browser(Helper.GetFramesMock());
            b2.ClearWindowsInContext();
            b1.Navigate("http://localhost/");
            Assert.That(b1.Url.ToString() == "http://localhost/");
            Assert.Throws(typeof(ObjectDisposedException), () => b2.Navigate("http://localhost/"));
        }
    }
}
