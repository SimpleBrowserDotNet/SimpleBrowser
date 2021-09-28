// -----------------------------------------------------------------------
// <copyright file="WindowsAndFrames.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class WindowsAndFrames
    {
        [Test]
        public async Task Clicking_Target_Blank()
        {
            Browser b = new Browser(Helper.GetMoviesRequestMocker());
            HttpRequestLog lastRequest = null;
            b.RequestLogged += (br, l) =>
            {
                lastRequest = l;
            };
            await b.NavigateAsync("http://localhost/movies/");
            Assert.That(b.Url == new Uri("http://localhost/movies/"));
            HtmlResult link = b.Find(ElementType.Anchor, FindBy.Text, "About us");
            await link.ClickAsync();
            Assert.That(b.Url == new Uri("http://localhost/movies/"));
            Assert.That(b.Windows.Count() == 2);
            await link.ClickAsync();
            Assert.That(b.Windows.Count() == 3);
            Browser newBrowserWindow = b.Windows.First(br => br.WindowHandle != b.WindowHandle);
            Assert.That(newBrowserWindow.Url == new Uri("http://localhost/movies/About"));
        }

        [Test]
        public async Task Holding_Ctrl_Shft_Opens_New_Window()
        {
            Browser b = new Browser(Helper.GetMoviesRequestMocker());
            HttpRequestLog lastRequest = null;
            b.RequestLogged += (br, l) =>
            {
                lastRequest = l;
            };
            await b.NavigateAsync("http://localhost/movies/");
            Assert.That(b.Url == new Uri("http://localhost/movies/"));
            HtmlResult link = b.Find(ElementType.Anchor, FindBy.Text, "Home");
            await link.ClickAsync();
            Assert.That(b.Windows.Count() == 1);
            link = b.Find(ElementType.Anchor, FindBy.Text, "Home");
            b.KeyState = KeyStateOption.Ctrl;
            await link.ClickAsync();
            Assert.That(b.Windows.Count() == 2);
            link = b.Find(ElementType.Anchor, FindBy.Text, "Home");
            b.KeyState = KeyStateOption.Shift;
            await link.ClickAsync();
            Assert.That(b.Windows.Count() == 3);
            link = b.Find(ElementType.Anchor, FindBy.Text, "Home");
            b.KeyState = KeyStateOption.Alt;
            await link.ClickAsync();
            Assert.That(b.Windows.Count() == 3); // alt does not open new browser
        }

        [Test]
        public async Task Accessing_New_Windows_Using_Event()
        {
            Browser b = new Browser(Helper.GetMoviesRequestMocker());
            Browser newlyOpened = null;
            b.NewWindowOpened += (b1, b2) =>
            {
                newlyOpened = b2;
            };
            await b.NavigateAsync("http://localhost/movies/");
            Assert.That(b.Url == new Uri("http://localhost/movies/"));
            HtmlResult link = b.Find(ElementType.Anchor, FindBy.Text, "Details");
            b.KeyState = KeyStateOption.Ctrl;
            await link.ClickAsync();
            Assert.That(b.Windows.Count() == 2);
            Assert.NotNull(newlyOpened);
            Assert.That(b.Url.ToString() == "http://localhost/movies/");
            Assert.That(newlyOpened.Url.ToString() == "http://localhost/movies/Movies/Details/1");
        }

        [Test]
        public async Task ClosingBrowsers()
        {
            Browser b = new Browser(Helper.GetMoviesRequestMocker());
            HttpRequestLog lastRequest = null;
            b.RequestLogged += (br, l) =>
            {
                lastRequest = l;
            };
            await b.NavigateAsync("http://localhost/movies/");
            Assert.That(b.Url == new Uri("http://localhost/movies/"));
            HtmlResult link = b.Find(ElementType.Anchor, FindBy.Text, "About us");
            await link.ClickAsync();
            Assert.That(b.Url == new Uri("http://localhost/movies/"));
            Assert.That(b.Windows.Count() == 2);
            b.Close();
            Assert.That(b.Windows.Count() == 1);
            b.Windows.First().Close();
            Assert.That(b.Windows.Count() == 0);
            Assert.Throws(typeof(ObjectDisposedException), () => { Uri s = b.Url; });
        }

        [Test]
        public async Task Page_With_IFrames()
        {
            Browser b = new Browser(Helper.GetIFramesMock());
            HttpRequestLog lastRequest = null;
            b.RequestLogged += (br, l) =>
            {
                lastRequest = l;
            };
            await b.NavigateAsync("http://localhost/");
            Assert.That(b.Frames.Count() == 2);

            // now navigate away to a page without frames
            await b.NavigateAsync("http://localhost/bla");
            Assert.That(b.Frames.Count() == 0);
            Assert.That(b.Windows.Count() == 1);
        }

        [Test]
        public async Task Navigate_LinkWithParentTarget_OpensPageInParentWindow()
        {
            // Arrange
            Browser b = new Browser(Helper.GetFramesetMock());

            // Act
            await b.NavigateAsync("http://localhost/");
            await b.Frames.First().Find("parentFrameLink").ClickAsync();

            // Assert
            Assert.True(b.Find("parentFrameLink").Exists);
        }

        [Test]
        public async Task GetAttribute_Backdoor_FrameHandle()
        {
            Browser b = new Browser(Helper.GetIFramesMock());
            HttpRequestLog lastRequest = null;
            b.RequestLogged += (br, l) =>
            {
                lastRequest = l;
            };
            await b.NavigateAsync("http://localhost/");
            HtmlResult elm = b.Select("iframe");
            string handle = elm.GetAttribute("SimpleBrowser.WebDriver:frameWindowHandle");
            Assert.AreEqual(handle, "frame1");
        }

        [Test]
        public async Task Navigating_IFrames_Using_Target()
        {
            Browser b = new Browser(Helper.GetIFramesMock());
            HttpRequestLog lastRequest = null;
            b.RequestLogged += (br, l) =>
            {
                lastRequest = l;
            };
            await b.NavigateAsync("http://localhost/");
            Assert.That(b.Frames.Count() == 2);
            Assert.That(b.Frames.First().Url == new Uri("http://localhost/subdirectory/frame.htm"));

            await b.Find("framelink").ClickAsync();
            Assert.That(b.Frames.Count() == 2);
            Assert.That(b.Url == new Uri("http://localhost/"));
            Assert.That(b.Frames.First().Url == new Uri("http://localhost/bla.htm"));
        }

        [Test]
        public void Static_scoped_clear_works()
        {
            Browser b1 = new Browser(Helper.GetIFramesMock());
            Browser b2 = new Browser(Helper.GetIFramesMock());
            Browser.ClearWindows();
            Assert.ThrowsAsync(typeof(ObjectDisposedException), async () => await b1.NavigateAsync("http://localhost/"));
        }

        [Test]
        public async Task Instance_scoped_clear_works()
        {
            Browser b1 = new Browser(Helper.GetIFramesMock());
            Browser b2 = new Browser(Helper.GetIFramesMock());
            b2.ClearWindowsInContext();
            await b1.NavigateAsync("http://localhost/");
            Assert.That(b1.Url.ToString() == "http://localhost/");
            Assert.ThrowsAsync(typeof(ObjectDisposedException), async () => await b2.NavigateAsync("http://localhost/"));

        }

        [Test]
        public async Task IFrame_Url_ParsesCorrectly()
        {
            Browser b = new Browser(Helper.GetIFramesMock());

            await b.NavigateAsync("http://localhost/");
            Assert.That(b.Frames.Count() == 2);
            Assert.AreEqual(b.Frames.Last().Url.AbsoluteUri, @"https://www.example.com/BurstingPipe?cn=ot&onetagid=7128&ns=1&activityValues=$$Session=-Session-$$&retargetingValues=$$$$&dynamicRetargetingValues=$$$$&acp=$$$$&");
        }
    }
}