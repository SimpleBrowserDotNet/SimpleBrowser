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
			Browser b = new Browser();
			b.Navigate("http://localhost:17267/");
			var link = b.Find(ElementType.Anchor, FindBy.Text, "Create New");
			link.Click();
			link = b.Select("input[type=submit]");
			link.Click();
		}
	}
}
