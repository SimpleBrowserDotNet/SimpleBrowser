// -----------------------------------------------------------------------
// <copyright file="Uploading.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
    using System;
    using System.IO;
    using NUnit.Framework;

    [TestFixture]
    public class Uploading
    {
        [Test]
        public void Uploading_A_File_With_Enctype_MultipartMime()
        {
            Browser b = new Browser(Helper.GetAllways200RequestMocker());
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.FileUpload.htm"));

            HttpRequestLog lastLog = null;
            b.RequestLogged += (br, l) =>
                {
                    lastLog = l;
                };

            HtmlResult form = b.Select("form");
            HtmlResult file = b.Select("input[name=theFile]");
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            file.Value = dir.GetFiles()[3].FullName;
            form.SubmitForm();

            Assert.NotNull(lastLog);
            Assert.That(lastLog.Method == "POST");
        }
    }
}