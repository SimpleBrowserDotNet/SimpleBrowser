using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SimpleBrowser.UnitTests.OfflineTests
{
	[TestFixture]
	public class History
	{
		[Test]
		public void When_Navigate_Back_Current_Url_Should_Change()
		{
			Browser b = new Browser(Helper.GetMoviesRequestMocker());
			HttpRequestLog lastRequest = null;
			b.RequestLogged += (br, l) =>
			{
				lastRequest = l;
			};
			b.Navigate("http://localhost/movies/");
			Assert.That(b.Url == new Uri("http://localhost/movies/"));
			b.Navigate("http://localhost/movies2/");
			Assert.That(b.Url == new Uri("http://localhost/movies2/"));
			b.NavigateBack();
			Assert.AreEqual(new Uri("http://localhost/movies/"), b.Url);
			var link = b.Find(ElementType.Anchor, FindBy.Text, "Create New");
			Assert.NotNull(link, "After navigating back, the 'Create New' link should be found");
			b.NavigateForward();
			Assert.AreEqual(new Uri("http://localhost/movies2/"), b.Url);
			link = b.Find(ElementType.Anchor, FindBy.Text, "Create New");
			Assert.AreEqual(false, link.Exists, "After navigating forward, the 'Create New' link should NOT be found");
		}

		[Test]
		public void History_Should_Be_Limited_To_20_States()
		{
			Browser b = new Browser(Helper.GetAllways200RequestMocker());
			HttpRequestLog lastRequest = null;
			b.RequestLogged += (br, l) =>
			{
				lastRequest = l;
			};
			for (int i = 0; i < 25; i++)
			{
				b.Navigate(String.Format("http://localhost/movies{0}/", i));
			}
			var history = b.NavigationHistory;
			Assert.LessOrEqual(history.Keys.Count, 20, "The history shouldn't grow beyond 20");
			Assert.That(!history.ContainsKey(1));
			Assert.That(history.ContainsKey(0));
			Assert.That(history.ContainsKey(-19));
			Assert.That(!history.ContainsKey(-20));
		}
		[Test]
		public void Navigating_Beyond_History_Boundaries_Should_Return_False()
		{
			Browser b = new Browser(Helper.GetAllways200RequestMocker());
			HttpRequestLog lastRequest = null;
			b.RequestLogged += (br, l) =>
			{
				lastRequest = l;
			};
			b.Navigate("http://localhost/movies1/");
			b.Navigate("http://localhost/movies2/");
			Assert.False(b.NavigateForward());
			Assert.True(b.NavigateBack());
			Assert.False(b.NavigateBack());
		}
		[Test]
		public void Navigating_To_A_Url_With_Querystring_Parameters_Retains_Parameters()
		{
			Browser b = new Browser(Helper.GetMoviesRequestMocker());
			b.Navigate("http://localhost/movies/");
			var link = b.Find(ElementType.Anchor, FindBy.Text, "Rio Bravo");
			link.Click();
			Assert.AreEqual(new Uri("http://www.example.com/movie.html?id=4"), b.Url);
		}
		[Test]
		public void After_Navigating_Away_HtmlResult_Should_Throw_Exception()
		{
			Browser b = new Browser(Helper.GetMoviesRequestMocker());
			HttpRequestLog lastRequest = null;
			b.RequestLogged += (br, l) =>
			{
				lastRequest = l;
			};
			b.Navigate("http://localhost/movies/");
			Assert.That(b.Url == new Uri("http://localhost/movies/"));
			var link = b.Find(ElementType.Anchor, FindBy.Text, "Create New");
			link.Click();
			Assert.AreEqual(new Uri("http://localhost/movies/Movies/Create"), b.Url);
			Assert.Throws(typeof(InvalidOperationException), () => link.Click(), "Clicking the link should now throw an exception");
		}


	}
}
