// -----------------------------------------------------------------------
// <copyright file="WebResponseWrapper.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Network
{
    using System;
    using System.IO;
    using System.Net;

    internal class WebResponseWrapper : IHttpWebResponse
    {
        private HttpWebResponse _wr;

        public WebResponseWrapper(HttpWebResponse resp)
        {
            this._wr = resp;
        }

        #region IHttpWebResponse Members

        public Stream GetResponseStream()
            => this._wr.GetResponseStream();

        public string CharacterSet
        {
            get
            {
                return this._wr.CharacterSet;
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
                return this._wr.ContentType;
            }

            set
            {
                this._wr.ContentType = value;
            }
        }

        public WebHeaderCollection Headers
        {
            get
            {
                return this._wr.Headers;
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
                return this._wr.StatusCode;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion IHttpWebResponse Members

        #region IDisposable Members

        public void Dispose()
        {
            (this._wr as IDisposable)?.Dispose();
        }

        #endregion IDisposable Members
    }
}