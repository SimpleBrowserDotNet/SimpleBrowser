// -----------------------------------------------------------------------
// <copyright file="WebRequestWrapper.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Network
{
    using System;
    using System.Linq;
    using System.Net;

    internal class WebRequestWrapper : IHttpWebRequest
    {
        private static int[] _allowedRedirectStatusCodes = { 300, 301, 302, 303, 307, 308 };
        private HttpWebRequest _wr = null;

        public WebRequestWrapper(Uri url)
        {
            this._wr = (HttpWebRequest)HttpWebRequest.Create(url);
        }

        #region IHttpWebRequest Members

        public System.IO.Stream GetRequestStream()
        {
            return this._wr.GetRequestStream();
        }

        public IHttpWebResponse GetResponse()
        {
            HttpWebResponse response;

#if NETSTANDARD2_0
            try
            {
#endif
            response = (HttpWebResponse)this._wr.GetResponse();
#if NETSTANDARD2_0
            }
            // .NET Core throws an exception on the redirect status codes
            // thus we need to handle the exception and inspect the actual
            // response to determine if we need to redirect.
            catch (WebException ex)
                when (_allowedRedirectStatusCodes.Contains(((int?)(ex.Response as HttpWebResponse)?.StatusCode) ?? -1))
            {
                response = (HttpWebResponse)ex.Response;
            }
#endif

            return new WebResponseWrapper(response);
        }

        public DecompressionMethods AutomaticDecompression
        {
            get => this._wr.AutomaticDecompression;
            set => this._wr.AutomaticDecompression = value;
        }

        public long ContentLength
        {
            get => this._wr.ContentLength;
            set => this._wr.ContentLength = value;
        }

        public WebHeaderCollection Headers
        {
            get => this._wr.Headers;
            set => this._wr.Headers = value;
        }

        public string ContentType
        {
            get => this._wr.ContentType;
            set => this._wr.ContentType = value;
        }

        public string Method
        {
            get => this._wr.Method;
            set => this._wr.Method = value;
        }

        public string UserAgent
        {
            get => this._wr.UserAgent;
            set => this._wr.UserAgent = value;
        }

        public string Accept
        {
            get => this._wr.Accept;
            set => this._wr.Accept = value;
        }

        public int Timeout
        {
            get => this._wr.Timeout;
            set => this._wr.Timeout = value;
        }

        public bool AllowAutoRedirect
        {
            get => this._wr.AllowAutoRedirect;
            set => this._wr.AllowAutoRedirect = value;
        }

        public CookieContainer CookieContainer
        {
            get => this._wr.CookieContainer;
            set => this._wr.CookieContainer = value;
        }

        public IWebProxy Proxy
        {
            get => this._wr.Proxy;
            set => this._wr.Proxy = value;
        }

        public string Referer
        {
            get => this._wr.Referer;
            set => this._wr.Referer = Uri.EscapeUriString(value);
        }

        public Uri Address
        {
            get => this._wr.Address;
        }

        public string Host
        {
            get => this._wr.Host;
            set => this._wr.Host = value;
        }

        #endregion IHttpWebRequest Members
    }
}