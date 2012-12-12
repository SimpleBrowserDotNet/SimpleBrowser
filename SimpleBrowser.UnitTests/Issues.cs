using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SimpleBrowser.UnitTests
{
	[TestFixture]
	public class Issues
	{
		[Test]
		public void SampleApp()
		{
			Browser b = new Browser(Helper.GetMoviesRequestMocker());
			HttpRequestLog lastRequest = null;
			b.RequestLogged += (br, l) =>
			{
				lastRequest = l;
			};
			b.Navigate("http://localhost/movies/");
			var link = b.Find(ElementType.Anchor, FindBy.Text, "Create New");
			link.Click();
			var box = b.Select("input[name=Title]");
			box.Value = "1234";
			box = b.Select("input[name=ReleaseDate]");
			box.Value = "2011-01-01";
			box = b.Select("input[name=Genre]");
			box.Value = "dark";
			box = b.Select("input[name=Price]");
			box.Value = "51";
			box = b.Select("input[name=Rating]");
			box.Value = "***";
			link = b.Select("input[type=submit]");
			link.Click();
			Assert.That(b.LastWebException == null, "Webexception detected");
			Assert.That(lastRequest.PostBody.Contains("&Price=51&"));

		}
	}
}
