// -----------------------------------------------------------------------
// <copyright file="History.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class History
    {
        [Test]
        public async Task When_Navigate_Back_Current_Url_Should_Change()
        {
            Browser b = new Browser(Helper.GetMoviesRequestMocker());
            HttpRequestLog lastRequest = null;
            b.RequestLogged += (br, l) =>
            {
                lastRequest = l;
            };
            await b.NavigateAsync("http://localhost/movies/");
            Assert.That(b.Url == new Uri("http://localhost/movies/"));
            await b.NavigateAsync("http://localhost/movies2/");
            Assert.That(b.Url == new Uri("http://localhost/movies2/"));
            b.NavigateBack();
            Assert.AreEqual(new Uri("http://localhost/movies/"), b.Url);
            HtmlResult link = b.Find(ElementType.Anchor, FindBy.Text, "Create New");
            Assert.NotNull(link, "After navigating back, the 'Create New' link should be found");
            b.NavigateForward();
            Assert.AreEqual(new Uri("http://localhost/movies2/"), b.Url);
            link = b.Find(ElementType.Anchor, FindBy.Text, "Create New");
            Assert.AreEqual(false, link.Exists, "After navigating forward, the 'Create New' link should NOT be found");
        }

        [Test]
        public async Task History_Should_Be_Limited_To_20_States()
        {
            Browser b = new Browser(Helper.GetAllways200RequestMocker());
            HttpRequestLog lastRequest = null;
            b.RequestLogged += (br, l) =>
            {
                lastRequest = l;
            };
            for (int i = 0; i < 25; i++)
            {
                await b.NavigateAsync(string.Format("http://localhost/movies{0}/", i));
            }
            IDictionary<int, Uri> history = b.NavigationHistory;
            Assert.LessOrEqual(history.Keys.Count, 20, "The history shouldn't grow beyond 20");
            Assert.That(!history.ContainsKey(1));
            Assert.That(history.ContainsKey(0));
            Assert.That(history.ContainsKey(-19));
            Assert.That(!history.ContainsKey(-20));
        }

        [Test]
        public async Task Navigating_Beyond_History_Boundaries_Should_Return_False()
        {
            Browser b = new Browser(Helper.GetAllways200RequestMocker());
            HttpRequestLog lastRequest = null;
            b.RequestLogged += (br, l) =>
            {
                lastRequest = l;
            };
            await b.NavigateAsync("http://localhost/movies1/");
            await b.NavigateAsync("http://localhost/movies2/");
            Assert.False(b.NavigateForward());
            Assert.True(b.NavigateBack());
            Assert.False(b.NavigateBack());
        }

        [Test]
        public async Task Navigating_To_A_Url_With_Querystring_Parameters_Retains_Parameters()
        {
            Browser b = new Browser(Helper.GetMoviesRequestMocker());
            await b.NavigateAsync("http://localhost/movies/");
            HtmlResult link = b.Find(ElementType.Anchor, FindBy.Text, "Rio Bravo");
            await link.ClickAsync();
            Assert.AreEqual(new Uri("http://www.example.com/movie.html?id=4"), b.Url);
        }

        [Test]
        public async Task After_Navigating_Away_HtmlResult_Should_Throw_Exception()
        {
            Browser b = new Browser(Helper.GetMoviesRequestMocker());
            HttpRequestLog lastRequest = null;
            b.RequestLogged += (br, l) =>
            {
                lastRequest = l;
            };
            await b.NavigateAsync("http://localhost/movies/");
            Assert.That(b.Url == new Uri("http://localhost/movies/"));
            HtmlResult link = b.Find(ElementType.Anchor, FindBy.Text, "Create New");
            await link.ClickAsync();
            Assert.AreEqual(new Uri("http://localhost/movies/Movies/Create"), b.Url);
            Assert.Throws(typeof(InvalidOperationException), () => link.ClickAsync().GetAwaiter().GetResult(), "Clicking the link should now throw an exception");
        }

        [Test]
        public void Browser_GetMaximumNavigationHistory_ReturnsPositiveValue()
        {
            // Arrange
            Browser b = new Browser();

            // Act
            int value = b.MaximumNavigationHistoryCount;

            // Assert
            Assert.Greater(value, 0);
        }

        [Test]
        public void Browser_SetMaximumNavigationHistoryToNegativeValue_ThrowsOutOfRangeException()
        {
            // Arrange
            Browser b = new Browser();

            // Act

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                b.MaximumNavigationHistoryCount = -2;
            });
        }

        [Test]
        public void Browser_SetMaximumNavigationHistoryToLargerSize_SetsValue()
        {
            // Arrange
            Browser b = new Browser();
            int before = b.MaximumNavigationHistoryCount;

            // Act
            b.MaximumNavigationHistoryCount = 50;

            // Assert
            Assert.Greater(b.MaximumNavigationHistoryCount, before);
        }

        [Test]
        public void Browser_SetMaximumNavigationHistoryToSameSize_SetsValue()
        {
            // Arrange
            Browser b = new Browser();
            int before = b.MaximumNavigationHistoryCount;

            // Act
            b.MaximumNavigationHistoryCount = before;

            // Assert
            Assert.AreEqual(b.MaximumNavigationHistoryCount, before);
        }

        [Test]
        public async Task Browser_SetMaximumNavigationHistoryToSmallerSize_SetsValue()
        {
            // Arrange
            Browser b = new Browser();
            int before = b.MaximumNavigationHistoryCount;
            await b.NavigateAsync("http://www.ms.com");
            await b.NavigateAsync("http://www.microsoft.com");
            await b.NavigateAsync("http://www.github.com");
            await b.NavigateAsync("http://www.bytewerx.com");
            await b.NavigateAsync("http://www.yenc.org");

            await b.NavigateAsync("http://www.ms.com");
            await b.NavigateAsync("http://www.microsoft.com");
            await b.NavigateAsync("http://www.github.com");
            await b.NavigateAsync("http://www.bytewerx.com");
            await b.NavigateAsync("http://www.yenc.org");

            await b.NavigateAsync("http://www.ms.com");
            await b.NavigateAsync("http://www.microsoft.com");
            await b.NavigateAsync("http://www.github.com");
            await b.NavigateAsync("http://www.bytewerx.com");
            await b.NavigateAsync("http://www.yenc.org");

            await b.NavigateAsync("http://www.ms.com");
            await b.NavigateAsync("http://www.microsoft.com");
            await b.NavigateAsync("http://www.github.com");
            await b.NavigateAsync("http://www.bytewerx.com");
            await b.NavigateAsync("http://www.yenc.org");

            // Act
            b.MaximumNavigationHistoryCount = 10;

            // Assert
            Assert.Less(b.MaximumNavigationHistoryCount, before);
        }
    }
}