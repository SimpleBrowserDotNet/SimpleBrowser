using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SimpleBrowser.Network
{
	class WebRequestWrapper : IHttpWebRequest
	{
		public WebRequestWrapper(Uri url)
		{
			_wr = (HttpWebRequest)HttpWebRequest.Create(url);
		}
		HttpWebRequest _wr = null;

		#region IHttpWebRequest Members

		public System.IO.Stream GetRequestStream()
		{
			return _wr.GetRequestStream();
		}

		public IHttpWebResponse GetResponse()
		{
			return new WebResponseWrapper((HttpWebResponse)_wr.GetResponse());
		}

		public DecompressionMethods AutomaticDecompression
		{
			get
			{
				return _wr.AutomaticDecompression;
			}
			set
			{
				_wr.AutomaticDecompression = value;
			}
		}

		public long ContentLength
		{
			get
			{
				return _wr.ContentLength;
			}
			set
			{
				_wr.ContentLength = value;
			}
		}

		public WebHeaderCollection Headers
		{
			get
			{
				return _wr.Headers;
			}
			set
			{
				_wr.Headers = value;
			}
		}

		public string ContentType
		{
			get
			{
				return _wr.ContentType;
			}
			set
			{
				_wr.ContentType = value;
			}
		}

		public string Method
		{
			get
			{
				return _wr.Method;
			}
			set
			{
				_wr.Method = value;
			}
		}

		public string UserAgent
		{
			get
			{
				return _wr.UserAgent;
			}
			set
			{
				_wr.UserAgent = value;
			}
		}

		public string Accept
		{
			get
			{
				return _wr.Accept;
			}
			set
			{
				_wr.Accept = value; ;
			}
		}

		public int Timeout
		{
			get
			{
				return _wr.Timeout;
			}
			set
			{
				_wr.Timeout = value;
			}
		}

		public bool AllowAutoRedirect
		{
			get
			{
				return _wr.AllowAutoRedirect;
			}
			set
			{
				_wr.AllowAutoRedirect = value;
			}
		}

		public CookieContainer CookieContainer
		{
			get
			{
				return _wr.CookieContainer;
			}
			set
			{
				_wr.CookieContainer = value;
			}
		}

		public IWebProxy Proxy
		{
			get
			{
				return _wr.Proxy;
			}
			set
			{
				_wr.Proxy = value;
			}
		}

		public string Referer
		{
			get
			{
				return _wr.Referer;
			}
			set
			{
				_wr.Referer = Uri.EscapeUriString(value);
			}
		}

		public string Host
		{
			get
			{
				return _wr.Host;
			}
			set
			{
				_wr.Host = value;
			}
		}
	  #endregion
	}
}
