using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using System.IO;

namespace SimpleBrowser.UnitTests.OfflineTests
{
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
			var form = b.Select("form");
			var file = b.Select("input[name=theFile]");
			DirectoryInfo dir = new FileInfo(Assembly.GetCallingAssembly().Location).Directory;
			file.Value = dir.GetFiles()[3].FullName;
			form.SubmitForm();

			Assert.NotNull(lastLog);
			Assert.That(lastLog.Method == "POST");
		}
	}

}
 