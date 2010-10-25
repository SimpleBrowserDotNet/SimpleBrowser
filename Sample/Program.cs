using System;
using System.Collections.Generic;
using System.IO;
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
			browser.RequestLogged += OnBrowserRequestLogged;
			browser.Navigate("http://delicious.com");
			browser.Find("a", FindBy.Text, "Hotlist").Click();
			for(var element = browser.FindAll("h4"); element.Exists; element.Next())
				Console.WriteLine(element.Value);
		}

		static void OnBrowserRequestLogged(HttpBrowserBase arg1, HttpRequestLog arg2)
		{
			var dir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"));
			if(!dir.Exists) dir.Create();
			var path = Path.Combine(dir.FullName, "output-" + DateTime.UtcNow.Ticks + ".xml");
			arg2.ToXml().Save(path);
		}
	}
}
