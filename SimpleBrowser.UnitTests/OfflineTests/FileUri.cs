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
	}
}
