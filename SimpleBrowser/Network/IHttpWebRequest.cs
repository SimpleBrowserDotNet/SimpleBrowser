using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace SimpleBrowser.Network
{
	public interface IHttpWebRequest
	{
		Stream GetRequestStream();

		IHttpWebResponse GetResponse();

		long ContentLength { get; set; }

		WebHeaderCollection Headers { get; set; }
		
		DecompressionMethods AutomaticDecompression { get; set; }

		string ContentType { get; set; }

		string Method { get; set; }

		string UserAgent { get; set; }

		string Accept { get; set; }

		int Timeout { get; set; }

		bool AllowAutoRedirect { get; set; }

		CookieContainer CookieContainer { get; set; }

		IWebProxy Proxy { get; set; }

		string Referer { get; set; }

		string Host { get; set; }
	}
}
