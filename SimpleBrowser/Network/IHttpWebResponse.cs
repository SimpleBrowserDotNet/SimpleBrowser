using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace SimpleBrowser.Network
{
	public interface IHttpWebResponse : IDisposable
	{
		Stream GetResponseStream();

		string CharacterSet { get; set; }

		string ContentType { get; set; }

		WebHeaderCollection Headers { get; set; }

		HttpStatusCode StatusCode { get; set; }
	}
}
