namespace SimpleBrowser.Internal
{
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
		/// <param name="defaultHost">The host sending the header.</param>
		/// <param name="header">The-set cookie header received from the host.</param>
		/// <returns></returns>
		public static CookieCollection GetAllCookiesFromHeader(string defaultHost, string header)
		{
            header = header.Replace("\r", "");
            header = header.Replace("\n", "");
            int index = 0;
            CookieCollection cc = new CookieCollection();
            while (index < header.Length)
		    {
		        index = ParseCookie(header, index, cc, defaultHost);
		    }
			return cc;
		}

	    private static int ParseCookie(string header, int beginIndex, CookieCollection cc, string defaultHost)
	    {
            int index = beginIndex;

	        var cookie = new Cookie();
            index = ParseKeyValueFragment(header, index, cookie);
	        while (index < header.Length && header[index] == ';')
	        {
	            index = ParseCookieAttribute(header, index+1, cookie);
	        }
            index++;

            if (cookie.Domain == string.Empty)
            {
                cookie.Domain = defaultHost;
            }
            if (cookie.Path == string.Empty)
            {
                cookie.Path = "/";
            }

            cc.Add(cookie);
            return index;
        }

	    private static int ParseKeyValueFragment(string header, int beginIndex, Cookie cookie)
	    {
	        int index = beginIndex;
	        while (index < header.Length)
	        {
	            switch (header[index])
	            {
	                case '=':
	                    cookie.Name = EncodeCookieName(header.Substring(beginIndex, index - beginIndex).Trim());
	                    string value;
	                    index = ParseValue(header, index+1, out value);
	                    cookie.Value = value;
	                    return index;
                    case ',':
                    case ';':
                        var parsedValue = header.Substring(beginIndex, index - beginIndex).Trim();
                        cookie.Name = parsedValue;
	                    return index;
	            }

	            index++;
	        }
	        cookie.Name = header.Substring(beginIndex);
	        return index;
	    }

        private static int ParseCookieAttribute(string header, int beginIndex, Cookie cookie)
        {
	        int index = beginIndex;
            while (index < header.Length)
            {
                switch (header[index])
                {
                    case '=':
                        string attributeName = header.Substring(beginIndex, index - beginIndex).Trim();
                        if (attributeName.ToLower() == "expires")
                        {
                            return ParseExpiresValue(header, index+1, cookie);
                        }
                        else
                        {
                            string value;
                            index = ParseValue(header, index+1, out value);
                            if (attributeName.ToLower() == "domain")
                            {
                                cookie.Domain = value;
                            }
                            else if (attributeName.ToLower() == "path")
                            {
                                cookie.Path = value;
                            }
                            return index;
                        }
                    case ',':
                    case ';':
                        return index;
                }

                index++;
            }

            return index;
        }

	    private static int ParseValue(string header, int beginIndex, out string value)
	    {
	        int index = beginIndex;
	        bool isQuoted = false;
	        while (index < header.Length)
	        {
	            switch (header[index])
	            {
                    case ',':
	                case ';':
	                    if (isQuoted == false)
	                    {
	                        value = header.Substring(beginIndex, index - beginIndex);
	                        value = value.TrimStart('"').TrimEnd('"');
	                        return index;
	                    }
	                    break;
                    case '"':
	                    isQuoted = !isQuoted;
	                    break;
	            }
	            index++;
	        }
            value = header.Substring(beginIndex, index - beginIndex);
            value = value.TrimStart('"').TrimEnd('"');
            return index;
        }

	    private static int ParseExpiresValue(string header, int beginIndex, Cookie cookie)
	    {
	        int index = beginIndex;
	        int noCommas = 0;
	        while (index < header.Length)
	        {
	            switch (header[index])
	            {
	                case ',':
	                    if (noCommas == 0)
	                    {
	                        noCommas++;
	                    }
	                    else
	                    {
                            return index;
                        }
	                    break;
	                case ';':
                        return index;
                }
	            index++;
	        }
	        return index;
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
