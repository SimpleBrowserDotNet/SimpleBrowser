using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace SimpleBrowser.UnitTests
{
	public class Helper
	{
		internal static string GetFromResources(string resourceName)
		{
			Assembly assem = Assembly.GetExecutingAssembly();
			using (Stream stream = assem.GetManifestResourceStream(resourceName))
			{
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

	}
}
