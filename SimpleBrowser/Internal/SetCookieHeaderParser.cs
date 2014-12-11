// Shamelessly stolen from:
// http://stackoverflow.com/questions/4792638/mapping-header-cookie-string-to-cookiecollection-and-vice-versa
// http://snipplr.com/view/4427/

namespace SimpleBrowser.Internal
{
	using System;
	using System.Collections;
	using System.Net;
	using System.Text;

	/// <summary>
	/// A class to parse the set-cookies HTTP header and return a CookieCollection.
	/// </summary>
	class SetCookieHeaderParser
	{
		/// <summary>
		/// Parses the set-cookie HTTP header.
		/// </summary>
		/// <param name="strHost">The host sending the header.</param>
		/// <param name="strHeader">The-set cookie header received from the host.</param>
		/// <returns></returns>
		public static CookieCollection GetAllCookiesFromHeader(string strHost, string strHeader)
		{
			ArrayList al = new ArrayList();
			CookieCollection cc = new CookieCollection();
			if (strHeader != string.Empty)
			{
				al = ConvertCookieHeaderToArrayList(strHeader);
				cc = ConvertCookieArraysToCookieCollection(al, strHost);
			}
			return cc;
		}

		/// <summary>
		/// Converts a set-cookie header into a collection of its component parts as strings.
		/// </summary>
		/// <param name="strCookHeader">The set-cookie header to convert.</param>
		/// <returns>A collection of string components of the header</returns>
		private static ArrayList ConvertCookieHeaderToArrayList(string strCookHeader)
		{
			strCookHeader = strCookHeader.Replace("\r", "");
			strCookHeader = strCookHeader.Replace("\n", "");
			string[] strCookTemp = strCookHeader.Split(',');
			ArrayList al = new ArrayList();
			int i = 0;
			int n = strCookTemp.Length;
			while (i < n)
			{
				if (strCookTemp[i].IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0)
				{
					al.Add(strCookTemp[i] + "," + strCookTemp[i + 1]);
					i = i + 1;
				}
				else
				{
					al.Add(strCookTemp[i]);
				}
				i = i + 1;
			}
			return al;
		}

		/// <summary>
		/// Converts a collection of component string parts into a CookeCollection of cookies.
		/// </summary>
		/// <param name="al">The collection of set cookie header string components</param>
		/// <param name="strHost">The host sending the header</param>
		/// <returns>A CookieCollection</returns>
		private static CookieCollection ConvertCookieArraysToCookieCollection(ArrayList al, string strHost)
		{
			CookieCollection cc = new CookieCollection();

			int alcount = al.Count;
			string strEachCook;
			string[] strEachCookParts;
			for (int i = 0; i < alcount; i++)
			{
				strEachCook = al[i].ToString();
				strEachCookParts = strEachCook.Split(';');
				int intEachCookPartsCount = strEachCookParts.Length;
				string strCNameAndCValue = string.Empty;
				string strPNameAndPValue = string.Empty;
				string[] NameValuePairTemp;
				Cookie cookTemp = new Cookie();

				for (int j = 0; j < intEachCookPartsCount; j++)
				{
					if (j == 0)
					{
						strCNameAndCValue = strEachCookParts[j];
						if (strCNameAndCValue != string.Empty)
						{
							int firstEqual = strCNameAndCValue.IndexOf("=");
							string firstName = strCNameAndCValue.Substring(0, firstEqual);
							string allValue = strCNameAndCValue.Substring(firstEqual + 1, strCNameAndCValue.Length - (firstEqual + 1));
							cookTemp.Name = EncodeCookieName(firstName);
							cookTemp.Value = allValue;
						}

						continue;
					}

					if (strEachCookParts[j].IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						strPNameAndPValue = strEachCookParts[j];
						if (strPNameAndPValue != string.Empty)
						{
							NameValuePairTemp = strPNameAndPValue.Split('=');
							if (NameValuePairTemp[1] != string.Empty)
							{
								cookTemp.Path = NameValuePairTemp[1];
							}
							else
							{
								cookTemp.Path = "/";
							}
						}

						continue;
					}

					if (strEachCookParts[j].IndexOf("domain", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						strPNameAndPValue = strEachCookParts[j];
						if (strPNameAndPValue != string.Empty)
						{
							NameValuePairTemp = strPNameAndPValue.Split('=');

							if (NameValuePairTemp[1] != string.Empty)
							{
								cookTemp.Domain = NameValuePairTemp[1];
							}
							else
							{
								cookTemp.Domain = strHost;
							}
						}

						continue;
					}
				}

				if (cookTemp.Path == string.Empty)
				{
					cookTemp.Path = "/";
				}

				if (cookTemp.Domain == string.Empty)
				{
					cookTemp.Domain = strHost;
				}

				cc.Add(cookTemp);
			}

			return cc;
		}

		/// <summary>
		/// Encodes the cookie name (key) so that it contains only valid characters.
		/// </summary>
		/// <param name="name">The name to encode</param>
		/// <remarks>
		/// This method is essentially URL encoding, but only encodes the characters that are invalid
		/// for a cookie name, as defined by the .NET Framework 4 documentation of the System.Net.Cookie.Name
		/// property, specifically:
		/// 
		/// "The following characters must not be used inside the Name property: equal sign, semicolon,
		/// comma, newline (\n), return (\r), tab (\t), and space character. The dollar sign character
		/// ("$") cannot be the first character."
		///
		/// Note: In true Microsoft fashion, this this differs from what is defined in RFC 2616, section 2.2.
		/// RFC 2616 specifies 51 characters that are technically not allowed to be in a cookie name.
		/// </remarks>
		/// <returns>The encoded cookie name.</returns>
		private static string EncodeCookieName(string name)
		{
			StringBuilder validName = new StringBuilder(name);
			validName.Replace("=", "%3D");
			validName.Replace(";", "%3B");
			validName.Replace(",", "%2C");
			validName.Replace("\n", "%10");
			validName.Replace("\r", "%13");
			validName.Replace("\t", "%09");
			validName.Replace(" ", "%20");

			if (name.StartsWith("$"))
			{
				validName.Replace("$", "%24", 0, 1);
			}

			return validName.ToString();
		}
	}
}
