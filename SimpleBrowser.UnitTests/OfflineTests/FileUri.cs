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
    using NUnit.Framework;

    [TestFixture]
    public class FileUri
    {
        [Test]
        public void CanLoadHtmlFromFile()
        {
            FileInfo f = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SampleDocs\movies1.htm"));
            string uri = string.Format("file:///{0}", f.FullName);
            uri = uri.Replace("\\", "/");

            Browser b = new Browser();
            b.Navigate(uri);
            Assert.AreEqual(b.Select("ul#menu>li").Count(), 3, "Not loaded");
        }

        [Test]
        public void CanLoadHtmlFromFilesWithAbsolutePath()
        {
            if (Directory.Exists("C:\\Windows\\Temp"))
            {
                File.Copy(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SampleDocs\movies1.htm"),
                    @"C:\Windows\Temp\movies1.htm", true);

                Browser b = new Browser();
                b.Navigate("file:///c:/Windows/Temp/movies1.htm");
                Assert.AreEqual(b.Select("ul#menu>li").Count(), 3);

                b.Navigate("file:///c|/Windows/Temp/movies1.htm");
                Assert.AreEqual(b.Select("ul#menu>li").Count(), 3);

                b.Navigate("file:///c|\\Windows\\Temp\\movies1.htm");
                Assert.AreEqual(b.Select("ul#menu>li").Count(), 3);

                b.Navigate("file://\\c|\\Windows\\Temp\\movies1.htm");
                Assert.AreEqual(b.Select("ul#menu>li").Count(), 3);
            }
        }
    }
}