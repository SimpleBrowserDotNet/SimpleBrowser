using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace SimpleBrowser
{
	internal static class StringUtil
	{
		private static readonly Random Randomizer = new Random(Convert.ToInt32(DateTime.UtcNow.Ticks % int.MaxValue));
		public static string GenerateRandomString(int chars)
		{
			string s = "";
			for (int i = 0; i < chars; i++)
			{
				int x = Randomizer.Next(2);
				switch (x)
				{
					case 0: s += (char)(Randomizer.Next(10) + 48); break; // 0-9
					case 1: s += (char)(Randomizer.Next(26) + 65); break; // A-Z
					case 2: s += (char)(Randomizer.Next(26) + 97); break; // a-z
				}
			}
			return s;
		}

		public class LowerCaseComparer : IEqualityComparer<string>
		{
			public bool Equals(string x, string y)
			{
				return string.Compare(x, y, true) == 0;
			}

			public int GetHashCode(string obj)
			{
				return obj.ToLower().GetHashCode();
			}
		}

		public static string MakeQueryString(IDictionary values)
		{
			if (values == null)
				return string.Empty;
			List<string> list = new List<string>();
			foreach (object key in values.Keys)
				list.Add(HttpUtility.UrlEncode(key.ToString()) + "=" + HttpUtility.UrlEncode(values[key].ToString()));
			return list.Concat("&");
		}

		public static string MakeQueryString(NameValueCollection values)
		{
			if (values == null)
				return string.Empty;
			List<string> list = new List<string>();
			foreach (string key in values.Keys)
			{
				foreach (string value in values.GetValues(key))
					list.Add(HttpUtility.UrlEncode(key) + "=" + HttpUtility.UrlEncode(value));
			}
			return list.Concat("&");
		}

		public static string MakeQueryString(params KeyValuePair<string, string>[] values)
		{
			Dictionary<string, string> v = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> kvp in values)
				v[kvp.Key] = kvp.Value;
			return MakeQueryString(v);
		}

		public static NameValueCollection MakeCollectionFromQueryString(string queryString)
		{
			if (queryString == null)
				return new NameValueCollection();

			NameValueCollection values = new NameValueCollection();
			foreach (string kvp in queryString.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries))
			{
				string[] val = kvp.Split('=');
				if (val.Length > 1)
					values.Add(val[0], val[1]);
				else if (val.Length == 1)
					values.Add(val[0], string.Empty);
			}
			return values;
		}
	}
}
