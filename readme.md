SimpleBrowser
=============
SimpleBrowser is a lightweight, yet highly capable browser automation engine designed for automation and testing scenarios.
It provides an intuitive API that makes it simple to quickly extract specific elements of a page using a variety of matching
techniques, and then interact with those elements with methods such as `Click()`, `SubmitForm()` and many more. SimpleBrowser
does not support JavaScript, but allows for manual manipulation of the user agent, referrer, request headers, form values and
other values before submission or navigation.

Requirements
------------
* .Net Framework 4.0

Features
--------
* Multiple ways of locating and interacting with page elements
* A highly permissive HTML parser that converts any HTML, no matter how badly formed, to a valid XDocument object
* Automatic cookie/session management
* Extensive logging support to make it easy to identify problems loading and automating browsing sessions

Example
-------

	class Program
	{
		static void Main(string[] args)
		{
			var browser = new Browser();
			
			// log the browser request/response data to files so we can interrogate them in case of an issue with our scraping
			browser.RequestLogged += OnBrowserRequestLogged;

			// we'll fake the user agent for websites that alter their content for unrecognised browsers
			browser.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.10 (KHTML, like Gecko) Chrome/8.0.552.224 Safari/534.10";
			
			// browse to StackOverflow.com
			browser.Navigate("http://stackoverflow.com");
			if(LastRequestFailed(browser)) return; // always check the last request in case the page failed to load

			// click the "Tags" tab, which has an explicit ID of "nav-tags"
			browser.Find("nav-tags").Click();
			if(LastRequestFailed(browser)) return;

			// list the tags found on the page, then click the 5th tag
			var result = browser.Find(ElementType.Anchor, "rel", "tag");
			if(!result.Exists)
			{
				Console.WriteLine("Couldn't find any tag links on the page! Maybe they changed the HTML?");
				return;
			}

			// iterate through the results using the standard method provided by the HtmlResult class
			Console.Write("The following tags were found on the first tags page: " + result.Value);
			while(result.Next())
				Console.Write(", " + result.Value);
			Console.WriteLine(Environment.NewLine); // 2 lines

			// obtain the 5th tag by treating the result as an IEnumerable collection instead of using the standard method above
			var fifthTag = result.Skip(4).FirstOrDefault();
			if(fifthTag == null)
			{
				Console.WriteLine("There is no 5th tag.");
				return;
			}
			var tagName = fifthTag.Value;
			fifthTag.Click();
			if(LastRequestFailed(browser)) return;

			// display the top 5 questions for the selected tag
			result = browser.Find(ElementType.Anchor, FindBy.Class, "question-hyperlink");
			if(!result.Exists)
			{
				Console.WriteLine("No questions were found on the page. Has the HTML structure changed since this sample was written?");
				return;
			}
			Console.WriteLine("The top 5 questions for the tag [" + tagName + "] are:");
			var i = 0;
			foreach(var element in result.Take(5))
				Console.WriteLine("{0}. {1}", ++i, element.Value);
		}

		static bool LastRequestFailed(Browser browser)
		{
			if(browser.LastWebException != null)
			{
				Console.WriteLine("There was an error loading the page: " + browser.LastWebException.Message);
				return true;
			}
			return false;
		}

		static void OnBrowserRequestLogged(Browser req, HttpRequestLog log)
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
