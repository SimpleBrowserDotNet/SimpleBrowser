namespace SimpleBrowser.UnitTests.OfflineTests
{
	using System.IO;
	using System.Linq;
	using NUnit.Framework;

	[TestFixture]
	public class FileUri
	{
		[Test]
		public void CanLoadHtmlFromFile()
		{
			var f = new FileInfo(".\\SampleDocs\\movies1.htm");
			string uri = string.Format("file:///{0}", f.FullName);
			uri = uri.Replace("\\", "/");

			var b = new Browser();
			b.Navigate(uri);
			Assert.AreEqual(b.Select("ul#menu>li").Count(), 3, "Not loaded");
		}

		[Test]
		public void CanLoadHtmlFromFilesWithAbsolutePath()
		{
			if (Directory.Exists("C:\\Windows\\Temp"))
			{
				File.Copy("SampleDocs\\movies1.htm", "C:\\Windows\\Temp\\movies1.htm", true);

				var b = new Browser();
				b.Navigate("file:///c:/Windows/Temp/movies1.htm");
				Assert.AreEqual(b.Select("ul#menu>li").Count(), 3);

				b.Navigate("file:///c|/Windows/Temp/movies1.htm");
				Assert.AreEqual(b.Select("ul#menu>li").Count(), 3);

				b.Navigate("file:///c|\\Windows\\Temp\\movies1.htm");
				Assert.AreEqual(b.Select("ul#menu>li").Count(), 3);

				b.Navigate("file://\\c|\\Windows\\Temp\\movies1.htm");
				Assert.AreEqual(b.Select("ul#menu>li").Count(), 3);
			}
		}
	}
}
