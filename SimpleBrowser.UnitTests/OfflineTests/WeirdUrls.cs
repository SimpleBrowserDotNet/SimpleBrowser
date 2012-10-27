using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SimpleBrowser.UnitTests.OfflineTests
{
	[TestFixture]
	class WeirdUrls
	{
		[Test]
		public void JavascriptUrl()
		{
			Browser b = new Browser(); // does not need network to fail
			var res = b.Navigate("javascript:'';");
			Assert.False(res);
		}
	}
}
