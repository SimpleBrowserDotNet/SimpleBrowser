using NUnit.Framework;

namespace SimpleBrowser.UnitTests.OfflineTests
{
	[TestFixture]
	public class CurrentHtml
	{
		[Test]
		public void CurrentHtml_Setter()
		{
			Browser b = new Browser();

			b.CurrentHtml = "<div>test</div>";
			HtmlResult div = b.Select("div");

			Assert.That(div.TotalElementsFound, Is.EqualTo(1));

			Assert.That(b.NavigateBack(), Is.False);
		}
	}
}
