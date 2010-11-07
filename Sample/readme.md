SimpleBrowser
=============
SimpleBrowser is a lightweight, yet highly capable browser automation system that is the predecessor to the as-yet unreleased
[XBrowser](http://github.com/axefrog/XBrowser). Unlike XBrowser, SimpleBrowser does not support JavaScript or other advanced
features, but it does provide an intuitive automation API that makes quickly loading website pages, navigating through websites
and extracting data from those pages quite easy.

Features
--------
* Multiple ways of locating and interacting with page elements
* Automatic cookie/session management
* Extensive logging support to make it easy to identify problems loading and automating browsing sessions

Example
-------

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

		static void OnBrowserRequestLogged(HttpBrowserBase req, HttpRequestLog log)
		{
			Console.WriteLine(log.Url.ToString());
			var dir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"));
			if(!dir.Exists) dir.Create();
			var path = Path.Combine(dir.FullName, "output-" + DateTime.UtcNow.Ticks + ".xml");
			log.ToXml().Save(path);
			path = Path.Combine(dir.FullName, "output-" + DateTime.UtcNow.Ticks + ".html");
			File.WriteAllText(path, log.Text);
		}
	}
