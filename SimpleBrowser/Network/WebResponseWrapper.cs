using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SimpleBrowser.Network
{
    internal class WebResponseWrapper : IHttpWebResponse
    {
        HttpWebResponse _wr;

        public WebResponseWrapper(HttpWebResponse resp)
        {
            _wr = resp;
        }

        #region IHttpWebResponse Members

        public Stream GetResponseStream()
            => _wr.GetResponseStream();

        public string CharacterSet
        {
            get => _wr.CharacterSet;
            set => throw new NotImplementedException();
        }

        public string ContentType
        {
            get => _wr.ContentType;
            set => _wr.ContentType = value;
        }

        public WebHeaderCollection Headers
        {
            get => _wr.Headers;
            set => throw new NotImplementedException();
        }

        public HttpStatusCode StatusCode
        {
            get => _wr.StatusCode;
            set => throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            (_wr as IDisposable)?.Dispose();
        }

        #endregion
    }
}