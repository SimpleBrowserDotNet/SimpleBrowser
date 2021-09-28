// -----------------------------------------------------------------------
// <copyright file="FileUri.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class FileUri
    {
        [Test]
        public async Task CanLoadHtmlFromFile()
        {
            FileInfo f = null;
            string uri = string.Empty;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                f = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SampleDocs\movies1.htm"));
                uri = string.Format("file:///{0}", f.FullName);
                uri = uri.Replace("\\", "/");
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                f = new FileInfo(Path.Combine("home", AppDomain.CurrentDomain.BaseDirectory, @"SampleDocs/movies1.htm"));
                uri = string.Format("file://{0}", f.FullName);
            }
            else
            {
                throw new NotImplementedException("Please write unit tests for this unknown platform. (MacOS?)");
            }

            Browser b = new Browser();
            await b.NavigateAsync(uri);
            Assert.AreEqual(b.Select("ul#menu>li").Count(), 3, "Not loaded");
        }

        [Test]
        public async Task CanLoadHtmlFromFilesWithAbsolutePath()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Directory.Exists("C:\\Windows\\Temp"))
            {
                File.Copy(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SampleDocs\movies1.htm"),
                    @"C:\Windows\Temp\movies1.htm", true);

                Browser b = new Browser();
                await b.NavigateAsync("file:///c:/Windows/Temp/movies1.htm");
                Assert.AreEqual(b.Select("ul#menu>li").Count(), 3);

                await b.NavigateAsync("file:///c|/Windows/Temp/movies1.htm");
                Assert.AreEqual(b.Select("ul#menu>li").Count(), 3);

                await b.NavigateAsync("file:///c|\\Windows\\Temp\\movies1.htm");
                Assert.AreEqual(b.Select("ul#menu>li").Count(), 3);

                await b.NavigateAsync("file://\\c|\\Windows\\Temp\\movies1.htm");
                Assert.AreEqual(b.Select("ul#menu>li").Count(), 3);

                File.Delete(@"C:\Windows\Temp\movies1.htm");
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix &&
                Directory.Exists("/tmp"))
            {
                File.Copy(
                    Path.Combine("home", AppDomain.CurrentDomain.BaseDirectory, @"SampleDocs/movies1.htm"),
                    @"/tmp/movies1.htm", true);

                Browser b = new Browser();
                await b.NavigateAsync("file:///tmp/movies1.htm");
                Assert.AreEqual(b.Select("ul#menu>li").Count(), 3);

                File.Delete(@"/tmp/movies1.htm");
            }
        }
    }
}