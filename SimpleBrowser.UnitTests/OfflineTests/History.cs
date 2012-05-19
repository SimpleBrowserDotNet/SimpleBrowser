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
		public void After_navigating_away_htmlresult_should_throw_exception()
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
			Assert.AreEqual(new Uri("http://localhost/movies/Movies/Create?"), b.Url);
			Assert.Throws(typeof(InvalidOperationException), () => link.Click(), "Clicking the link should now throw an exception");
		}


	}
}
