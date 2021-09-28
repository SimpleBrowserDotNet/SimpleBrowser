﻿// -----------------------------------------------------------------------
// <copyright file="HttpHeaderTests.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OnlineTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class HttpHeaderTests
    {
        [Test]
        public async Task CustomHostHeaderIsSent()
        {
            Browser browser = new Browser();

            await browser.NavigateAsync("http://204.144.122.42");
            Assert.That(browser.RequestData().Host, Is.EqualTo("204.144.122.42"), "Expected host header to be default from url.");

            // I happen to know that this domain name is not in dns (my company owns it)
            // but that ip (also ours) is serving content for said domain.
            // Is there another way to confirm the overriden header is sent that does
            // not depend on some random internet server?
            browser.SetHeader("host:uscloud.asldkfhjawoeif.com");
            await browser.NavigateAsync("http://204.144.122.42");

            Assert.That(browser.RequestData().Address, Is.EqualTo(new Uri("http://204.144.122.42")), "Expected the address to be the website url.");
            Assert.That(browser.RequestData().Host, Is.EqualTo("uscloud.asldkfhjawoeif.com"), "Expected the manually set host.");
        }

        [Test]
        public async Task CustomHeaderIsSent()
        {
            const string headername = "X-MyCustomHeader";
            const string headervalue = "hello.world";

            Browser browser = new Browser();
            browser.SetHeader($"{headername}:{headervalue}");
            await browser.NavigateAsync("http://localhost.me");

            Assert.That(
                browser.RequestData()?.RequestHeaders?[headername],
                Is.EqualTo(headervalue),
                $"Expected header {headername} to be inserted with value {headervalue}");
        }
    }
}