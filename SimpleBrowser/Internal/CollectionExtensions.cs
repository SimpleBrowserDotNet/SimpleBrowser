using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SimpleBrowser
{
	internal static class CollectionExtensions
	{
		public static string Concat<T>(this IEnumerable<T> values, string delimiter)
		{
			StringBuilder sb = new StringBuilder();
			int c = 0;
			if(values == null) values = new T[0];
			foreach(T k in values)
			{
				if(c++ > 0)
					sb.Append(delimiter);
				sb.Append(k);
			}
			return sb.ToString();
		}

		public delegate string StringEncodeHandler<T>(T input);
		public static string Concat<T>(this IEnumerable<T> values, StringEncodeHandler<T> encodeValue)
		{
			return values.Concat("", encodeValue);
		}

		public static string Concat<T>(this IEnumerable<T> values, string delimiter, StringEncodeHandler<T> encodeValue)
		{
			StringBuilder sb = new StringBuilder();
			int c = 0;
			if(values == null) values = new T[0];
			foreach(T k in values)
			{
				if(c++ > 0)
					sb.Append(delimiter);
				sb.Append(encodeValue(k));
			}
			return sb.ToString();
		}

		public static string FriendlyConcat<T>(this IEnumerable<T> values)
		{
			return values.FriendlyConcat<T>(h => "" + h); // can't call ToString() on a null value
		}

		public static string FriendlyConcat<T>(this IEnumerable<T> values, StringEncodeHandler<T> encodeValue)
		{
			var sb = new StringBuilder();
			var len = values.Count();
			int i = 0;
			foreach(var v in values)
			{
				if(i > 0 && i < len - 1)
					sb.Append(", ");
				else if(i == len - 1 && len > 1)
					sb.Append(" and ");
				sb.Append(encodeValue(v));
				i++;
			}
			return sb.ToString();
		}

		public delegate string StringEncodeHandler(string input);
		public static string Concat(this IList<string> values, string delimiter, StringEncodeHandler encodeValue)
		{
			if(values.Count == 0)
				return string.Empty;

			StringBuilder sb = new StringBuilder(encodeValue(values[0]));
			for(int i = 1; i < values.Count; i++)
				sb.Append(delimiter).Append(encodeValue(values[i]));
			return sb.ToString();
		}

		public static string ToHexString(this byte[] bytes)
		{
			StringBuilder sb = new StringBuilder();
			foreach(byte b in bytes)
			{
				string s = b.ToString("X");
				if(s.Length == 1)
					sb.Append("0");
				sb.Append(s);
			}
			return sb.ToString();
		}

		public static string ToQueryString(this NameValueCollection nvc)
		{
			return StringUtil.MakeQueryString(nvc);
		}

		public static XElement ToXElement(this NameValueCollection nvc, string name)
		{
			var e = new XElement(name);
			foreach(var key in nvc.AllKeys)
			{
				var vals = nvc.GetValues(key);
				switch(vals.Length)
				{
					case 0:
						e.Add(new XElement("Value", new XAttribute("Name", key)));
						break;
					case 1:
						e.Add(new XElement("Value", new XAttribute("Name", key), vals[0]));
						break;
					default:
					{
						foreach(var val in vals)
							e.Add(new XElement("Value", new XAttribute("Name", key), val));
						break;
					}
				}
			}
			return e;
		}
	}
}