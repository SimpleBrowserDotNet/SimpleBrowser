using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleBrowser
{
	internal static class StringExtensions
	{
		public static bool MatchesAny(this string source, params string[] comparisons)
		{
			foreach(string s in comparisons)
				if(s == source)
					return true;
			return false;
		}

		public static bool CaseInsensitiveCompare(this string str1, string str2)
		{
			return string.Compare(str1, str2, true) == 0;
		}

		public static bool ToBool(this string value)
		{
			if(value == null) return false;
			return !MatchesAny(value.ToLower(), "", "no", "false", "off", "0", null);
		}

		public static int ToInt(this string s)
		{
			int n;
			if(!int.TryParse(s, out n))
				return 0;
			return n;
		}

		public static long ToLong(this string s)
		{
			long n;
			if(!long.TryParse(s, out n))
				return 0;
			return n;
		}

		public static double ToDouble(this string s)
		{
			double n;
			if(!double.TryParse(s, out n))
				return 0;
			return n;
		}

		public static double ToDecimal(this string s)
		{
			double n;
			if(!double.TryParse(s, out n))
				return 0;
			return n;
		}

		public static Color ToColor(this string s)
		{
			s = s.Trim();
			if(s.Length == 0)
				return Color.Black;
			// Can't include detection of hex string because it would be indistinguishable from integer values that have 6 digits. Instead, use ToColorFromHex.
			//if (Regex.IsMatch(s, @"^\#?[0-9A-Fa-f]{6}$"))
			//    return ColorFromHexString(s);
			if(s.IndexOf(',') > -1)
			{
				string[] arr = s.Split(',');
				if(arr.Length == 3)
				{
					int[] vals = new int[3];
					for(int i = 0; i < 3; i++)
					{
						int n;
						if(!int.TryParse(arr[i], out n))
							goto failed;
						if(n < 0 || n > 255)
							goto failed;
						vals[i] = n;
					}
					return Color.FromArgb(vals[0], vals[1], vals[2]);
				}
			}
		failed:
			if(char.IsDigit(s[0]) || s[0] == '-')
				return Color.FromArgb(s.ToInt());
			return Color.FromKnownColor(s.ToEnum<KnownColor>());
		}

		public static Color ToColorFromHex(this string s)
		{
			byte[] b = (s.StartsWith("#") ? s.Substring(1) : s).HexToBytes();
			if(b.Length != 3) throw new ArgumentException("Invalid hex string");
			return Color.FromArgb(b[0], b[1], b[2]);
		}

		public static byte[] HexToBytes(this string str)
		{
			byte[] bytes = new byte[str.Length / 2];
			for(int i = 0; i < str.Length; i += 2)
				bytes[i / 2] = byte.Parse(str.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
			return bytes;
		}

		public static bool IsHexColorString(this string s)
		{
			return Regex.IsMatch(s, @"^\#?[0-9A-Fa-f]{6}$");
		}

		public static bool IsHexString(this string s)
		{
			return Regex.IsMatch(s, @"^(?:[0-9A-Fa-f]{2})+$");
		}

		public static T ToEnum<T>(this string s)
		{
			try { return (T)Enum.Parse(typeof(T), s, true); }
			catch { return default(T); }
		}

		public static T ToEnum<T>(this string s, T defaultValue)
		{
			try { return (T)Enum.Parse(typeof(T), s, true); }
			catch { return defaultValue; }
		}

		public static bool IsEmailAddress(this string str)
		{
			try { new MailAddress(str); }
			catch { return false; }
			return true;
		}

		public static string ShortenTo(this string str, int length)
		{
			return ShortenTo(str, length, false);
		}

		public static string ShortenTo(this string str, int length, bool ellipsis)
		{
			if(str.Length > length)
			{
				str = str.Substring(0, length);
				if(ellipsis)
					str += "&hellip;";
			}
			return str;
		}

		public static List<string> Split(this string delimitedList, char delimiter, bool trimValues, bool stripDuplicates, bool caseSensitiveDuplicateMatch)
		{
			if(delimitedList == null)
				return new List<string>();
			StringUtil.LowerCaseComparer lcc = new StringUtil.LowerCaseComparer();
			List<string> list = new List<string>();
			string[] arr = delimitedList.Split(delimiter);
			for(int i = 0; i < arr.Length; i++)
			{
				string val = trimValues ? arr[i].Trim() : arr[i];
				if(val.Length > 0)
				{
					if(stripDuplicates)
					{
						if(caseSensitiveDuplicateMatch)
						{
							if(!list.Contains(val))
								list.Add(val);
						}
						else if(!list.Contains(val, lcc))
							list.Add(val);
					}
					else
						list.Add(val);
				}
			}
			return list;
		}

		public static List<string> SplitLines(this string listWithOnePerLine, bool trimValues, bool stripDuplicates, bool caseSensitiveDuplicateMatch)
		{
			if(listWithOnePerLine == null)
				return new List<string>();
			var lcc = new StringUtil.LowerCaseComparer();
			var list = new List<string>();
			using(var reader = new System.IO.StringReader(listWithOnePerLine))
			{
				var val = reader.ReadLine();
				while(val != null)
				{
					if(trimValues)
						val = val.Trim();
					if(val.Length > 0 || !trimValues)
					{
						if(stripDuplicates)
						{
							if(caseSensitiveDuplicateMatch)
							{
								if(!list.Contains(val))
									list.Add(val);
							}
							else if(!list.Contains(val, lcc))
								list.Add(val);
						}
						else
							list.Add(val);
					}
					val = reader.ReadLine();
				}
			}
			return list;
		}

		public static string ReplaceTokens(this string str, string tokenPrefix, string tokenSuffix, object values)
		{
			if(values == null)
				return str;
			if(values is string || !values.GetType().IsClass)
				throw new Exception("Cannot replace tokens; 'values' should have been an anonymous type");
			foreach(var p in values.GetType().GetProperties())
				str = str.Replace(
					(tokenPrefix ?? "") + p.Name + (tokenSuffix ?? ""),
					(p.GetValue(values, null) ?? "").ToString()
				);
			return str;
		}

		public static string ToBase52String(this int n)
		{
			return Convert.ToInt64(n).ToBase52String();
		}

		public static string ToBase52String(this long n)
		{
			var sb = new StringBuilder();
			var k = n;
			while (k > 0)
			{
				var m = Convert.ToInt32(k % 52);
				if(m < 26)
					sb.Insert(0, m < 26 ? Convert.ToChar('A' + m) : Convert.ToChar('a' + (m - 26)));
				k -= m;
				k /= 52;
			}
			return sb.ToString();
		}

		/// <summary>
		/// Levenshtein Distance Calculator
		/// </summary>
		public static int DistanceFrom(this string s, string t)
		{
			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// Step 1
			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++)
			{
			}

			for (int j = 0; j <= m; d[0, j] = j++)
			{
			}

			// Step 3
			for (int i = 1; i <= n; i++)
			{
				//Step 4
				for (int j = 1; j <= m; j++)
				{
					// Step 5
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

					// Step 6
					d[i, j] = Math.Min(
						Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
						d[i - 1, j - 1] + cost);
				}
			}
			// Step 7
			return d[n, m];
		}
	}
}