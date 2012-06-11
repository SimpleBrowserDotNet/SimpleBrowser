﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SimpleBrowser.UnitTests.OfflineTests
{
    [TestFixture]
    public class DecodedValue
    {
        [Test]
        public void HtmlElement_DecodedValue()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.DecodedValue.htm"));

            var div = b.Select("div");
            Assert.That(div.DecodedValue, Is.EqualTo("£ sign"));
        }
    }
}
