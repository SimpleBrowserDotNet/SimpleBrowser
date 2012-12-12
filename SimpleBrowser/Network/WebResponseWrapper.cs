using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace SimpleBrowser.Network
{
	class WebResponseWrapper: IHttpWebResponse
	{
		public WebResponseWrapper(HttpWebResponse resp)
		{
			_wr = resp;
		}
		HttpWebResponse _wr;

		#region IHttpWebResponse Members

		public Stream GetResponseStream()
		{
			return _wr.GetResponseStream();
		}

		public string CharacterSet
		{
			get
			{
				return _wr.CharacterSet;
			}
			set
			{
				throw new NotImplementedException();
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

		public System.Net.WebHeaderCollection Headers
		{
			get
			{
				return _wr.Headers;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public HttpStatusCode StatusCode
		{
			get
			{
				return _wr.StatusCode;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		#endregion


		#region IDisposable Members

		public void Dispose()
		{
			((IDisposable)_wr).Dispose();
		}

		#endregion
	}
}
