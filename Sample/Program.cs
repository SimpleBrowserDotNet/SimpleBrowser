// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace Sample
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using RazorEngine.Templating;
    using SimpleBrowser;

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using Browser browser = new Browser();
            try
            {
                // log the browser request/response data to files so we can interrogate them in case of an issue with our scraping
                browser.RequestLogged += OnBrowserRequestLogged;
                browser.MessageLogged += new Action<Browser, string>(OnBrowserMessageLogged);

                // we'll fake the user agent for websites that alter their content for unrecognised browsers
                browser.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.10 (KHTML, like Gecko) Chrome/8.0.552.224 Safari/534.10";

                // browse to GitHub
                await browser.NavigateAsync("http://github.com/");
                if (LastRequestFailed(browser))
                {
                    // always check the last request in case the page failed to load
                    return;
                }

                // click the login link and click it
                browser.Log("First we need to log in, so browse to the login page, fill in the login details and submit the form.");
                HtmlResult loginLink = browser.Find("a", FindBy.Text, "Sign&nbsp;in");
                if (!loginLink.Exists)
                {
                    browser.Log("Can't find the login link! Perhaps the site is down for maintenance?");
                }
                else
                {
                    await loginLink.ClickAsync();
                    if (LastRequestFailed(browser))
                    {
                        return;
                    }

                    // fill in the form and click the login button - the fields are easy to locate because they have ID attributes
                    browser.Find("login_field").Value = "youremail@domain.com";
                    browser.Find("password").Value = "yourpassword";
                    await browser.Find(ElementType.Button, "name", "commit").ClickAsync();
                    if (LastRequestFailed(browser))
                    {
                        return;
                    }

                    // see if the login succeeded - ContainsText() is very forgiving, so don't worry about whitespace, casing, html tags separating the text, etc.
                    if (browser.ContainsText("Incorrect username or password"))
                    {
                        browser.Log("Login failed!", LogMessageType.Error);
                    }
                    else
                    {
                        // After logging in, we should check that the page contains elements that we recognise
                        if (!browser.ContainsText("Your Repositories"))
                        {
                            browser.Log("There wasn't the usual login failure message, but the text we normally expect isn't present on the page");
                        }
                        else
                        {
                            browser.Log("Your News Feed:");
                            // we can use simple jquery selectors, though advanced selectors are yet to be implemented
                            foreach (HtmlResult item in browser.Select("div.news .title"))
                            {
                                browser.Log("* " + item.Value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                browser.Log(ex.Message, LogMessageType.Error);
                browser.Log(ex.StackTrace, LogMessageType.StackTrace);
            }
            finally
            {
                RenderService rsvc = new RenderService();

                string path = WriteFile("log-" + DateTime.UtcNow.Ticks + ".html", browser.RenderHtmlLogFile( rsvc, "SimpleBrowser Sample - Request Log"));

                Console.WriteLine("Log file published to:");
                Console.WriteLine(path);

                var process = new Process();
                process.StartInfo.FileName = path;
                process.StartInfo.UseShellExecute = true;
                process.Start();
            }
        }

        private static bool LastRequestFailed(Browser browser)
        {
            if (browser.LastWebException != null)
            {
                browser.Log("There was an error loading the page: " + browser.LastWebException.Message);
                return true;
            }
            return false;
        }

        private static void OnBrowserMessageLogged(Browser browser, string log)
        {
            Console.WriteLine(log);
        }

        private static void OnBrowserRequestLogged(Browser req, HttpRequestLog log)
        {
            Console.WriteLine(" -> " + log.Method + " request to " + log.Url);
            Console.WriteLine(" <- Response status code: " + log.ResponseCode);
        }

        private static string WriteFile(string filename, string text)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"));
            if (!dir.Exists)
            {
                dir.Create();
            }

            string path = Path.Combine(dir.FullName, filename);
            File.WriteAllText(path, text);
            return path;
        }
    }

    public class RenderService : HtmlLogFormatter.IViewRenderService
    {
        public string RenderToString<TModel>(string template, string title, TModel model)
        {
            
            return RazorEngine.Engine.Razor.RunCompile(template, title, model.GetType(), model);
        }
    }
}