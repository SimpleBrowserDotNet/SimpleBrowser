using System.IO;
using System.Text.RegularExpressions;

namespace SimpleBrowser.Query
{
	internal static class ExpressionUtil
	{
		static readonly Regex RxNoQuoting = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");
		public static void WriteString(this TextWriter writer, string str)
		{
			if(!RxNoQuoting.IsMatch(str))
			{
				var c = str.Contains("'") ? '"' : '\'';
				str = str.Replace(@"\", "\0x27"); // preserve existing backslashes by replacing with the ESC control character
				// escape anything that needs to be escaped
				if(c == '"')
					str = str.Replace(@"""", @"\""");
				else
					str = str.Replace("'", @"\'");
				// restore and escape original backslashes
				str = string.Concat(c, str.Replace("\0x27", @"\\"), c);
			}
			writer.Write(str);
		}
	}
}
