using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SimpleBrowser.Network
{
    internal class WebRequestWrapper : IHttpWebRequest
    {
        private static int[] _allowedRedirectStatusCodes = { 300, 301, 302, 303, 307, 308 };
        HttpWebRequest _wr = null;

        public WebRequestWrapper(Uri url)
        {
            _wr = (HttpWebRequest)HttpWebRequest.Create(url);
        }

        #region IHttpWebRequest Members

        public System.IO.Stream GetRequestStream()
        {
            return _wr.GetRequestStream();
        }

        public IHttpWebResponse GetResponse()
        {
            HttpWebResponse response;

#if NETSTANDARD2_0
            try
            {
#endif
                response = (HttpWebResponse)_wr.GetResponse();
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
            get => _wr.AutomaticDecompression;
            set => _wr.AutomaticDecompression = value;
        }

        public long ContentLength
        {
            get => _wr.ContentLength;
            set => _wr.ContentLength = value;
        }

        public WebHeaderCollection Headers
        {
            get => _wr.Headers;
            set => _wr.Headers = value;
        }

        public string ContentType
        {
            get => _wr.ContentType;
            set => _wr.ContentType = value;
        }

        public string Method
        {
            get => _wr.Method;
            set => _wr.Method = value;
        }

        public string UserAgent
        {
            get => _wr.UserAgent;
            set => _wr.UserAgent = value;
        }

        public string Accept
        {
            get => _wr.Accept;
            set => _wr.Accept = value;
        }

        public int Timeout
        {
            get => _wr.Timeout;
            set => _wr.Timeout = value;
        }

        public bool AllowAutoRedirect
        {
            get => _wr.AllowAutoRedirect;
            set => _wr.AllowAutoRedirect = value;
        }

        public CookieContainer CookieContainer
        {
            get => _wr.CookieContainer;
            set => _wr.CookieContainer = value;
        }

        public IWebProxy Proxy
        {
            get => _wr.Proxy;
            set => _wr.Proxy = value;
        }

        public string Referer
        {
            get => _wr.Referer;
            set => _wr.Referer = Uri.EscapeUriString(value);
        }

        public Uri Address
        {
            get => _wr.Address;
        }

        public string Host
        {
            get => _wr.Host;
            set => _wr.Host = value;
        }
#endregion
    }
}