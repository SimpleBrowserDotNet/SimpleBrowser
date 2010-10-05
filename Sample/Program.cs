using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleBrowser;

namespace Sample
{
	class Program
	{
		static void Main(string[] args)
		{
			var browser = new Browser();
			browser.Navigate("http://delicious.com");
			browser.Find("a", FindBy.Text, "Hotlist").Click();
			for(var element = browser.FindAll("h4"); element.Exists; element.Next())
				Console.WriteLine(element.Value);
		}
	}
}
