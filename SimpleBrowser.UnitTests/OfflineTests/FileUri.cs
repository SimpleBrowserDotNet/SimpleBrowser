using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace SimpleBrowser.UnitTests.OfflineTests
{
	[TestFixture]
	public class FileUri
	{
		[Test]
		public void CanLoadHtmlFromFile()
		{
			Regex start = new Regex("^([a-z]):\\\\");
			var b = new Browser();
			var f = new FileInfo(".\\SampleDocs\\movies1.htm");
			string uri = start.Replace(f.FullName, "file:///$1/");
			uri = uri.Replace("\\", "/");
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
