using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SimpleBrowser.UnitTests.OfflineTests
{
    [TestFixture]
    public class Uploading
    {
        [Test]
        public void Uploading_A_File_With_Enctype_MultipartMime()
        {
            var b = new Browser(Helper.GetAllways200RequestMocker());
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.FileUpload.htm"));

            HttpRequestLog lastLog = null;
            b.RequestLogged += (br, l) =>
                {
                    lastLog = l;
                };

            var form = b.Select("form");
            var file = b.Select("input[name=theFile]");
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            file.Value = dir.GetFiles()[3].FullName;
            form.SubmitForm();

            Assert.NotNull(lastLog);
            Assert.That(lastLog.Method == "POST");
        }
    }
}