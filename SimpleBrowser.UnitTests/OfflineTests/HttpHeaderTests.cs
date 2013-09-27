using System;
using NUnit.Framework;

namespace SimpleBrowser.UnitTests.OfflineTests 
{
	[TestFixture]
	public class HttpHeaderTests 
	{
		[Test]
		public void AddHostHeaderDoesNotThrow() 
		{
			var browser = new Browser();
			Assert.DoesNotThrow(() => browser.SetHeader("host:www.google.com"));
		}
	}
}