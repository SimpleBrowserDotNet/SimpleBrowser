// -----------------------------------------------------------------------
// <copyright file="WebRequestWrapper.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Network
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    internal class WebRequestWrapper : IHttpWebRequest
    {
        private static int[] allowedRedirectStatusCodes = { 300, 301, 302, 303, 307, 308 };

        private readonly HttpWebRequest webRequest = null;

        public WebRequestWrapper(Uri url)
        {
            this.webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
        }

        #region IHttpWebRequest Members

        public async Task<Stream> GetRequestStreamAsync()
        {
            return await this.webRequest.GetRequestStreamAsync();
        }

        public async Task<IHttpWebResponse> GetResponseAsync()
        {
            HttpWebResponse response;

            try
            {
                response = (HttpWebResponse)(await this.webRequest.GetResponseAsync());
            }
            // .NET Core throws an exception on the redirect status codes
            // thus we need to handle the exception and inspect the actual
            // response to determine if we need to redirect.
            catch (WebException ex)
                when (allowedRedirectStatusCodes.Contains(((int?)(ex.Response as HttpWebResponse)?.StatusCode) ?? -1))
            {
                response = (HttpWebResponse)ex.Response;
            }

            return new WebResponseWrapper(response);
        }

        public DecompressionMethods AutomaticDecompression
        {
            get
            {
                return this.webRequest.AutomaticDecompression;
            }

            set
            {
                this.webRequest.AutomaticDecompression = value;
            }
        }

        public long ContentLength
        {
            get
            {
                return this.webRequest.ContentLength;
            }

            set
            {
                this.webRequest.ContentLength = value;
            }
        }

        public WebHeaderCollection Headers
        {
            get
            {
                return this.webRequest.Headers;
            }

            set
            {
                this.webRequest.Headers = value;
            }
        }

        public string ContentType
        {
            get
            {
                return this.webRequest.ContentType;
            }

            set
            {
                this.webRequest.ContentType = value;
            }
        }

        public string Method
        {
            get
            {
                return this.webRequest.Method;
            }

            set
            {
                this.webRequest.Method = value;
            }
        }

        public string UserAgent
        {
            get
            {
                return this.webRequest.UserAgent;
            }

            set
            {
                this.webRequest.UserAgent = value;
            }
        }

        public string Accept
        {
            get
            {
                return this.webRequest.Accept;
            }

            set
            {
                this.webRequest.Accept = value;
            }
        }

        public int Timeout
        {
            get
            {
                return this.webRequest.Timeout;
            }

            set
            {
                this.webRequest.Timeout = value;
            }
        }

        public bool AllowAutoRedirect
        {
            get
            {
                return this.webRequest.AllowAutoRedirect;
            }

            set
            {
                this.webRequest.AllowAutoRedirect = value;
            }
        }

        public CookieContainer CookieContainer
        {
            get
            {
                return this.webRequest.CookieContainer;
            }

            set
            {
                this.webRequest.CookieContainer = value;
            }
        }

        public IWebProxy Proxy
        {
            get
            {
                return this.webRequest.Proxy;
            }

            set
            {
                this.webRequest.Proxy = value;
            }
        }

        public string Referer
        {
            get
            {
                return this.webRequest.Referer;
            }

            set
            {
                this.webRequest.Referer = Uri.EscapeUriString(value);
            }
        }

        public Uri Address
        {
            get
            {
                return this.webRequest.Address;
            }
        }

        public string Host
        {
            get
            {
                return this.webRequest.Host;
            }

            set
            {
                this.webRequest.Host = value;
            }
        }
        public X509CertificateCollection ClientCertificates
        {
            get
            {
                return this.webRequest.ClientCertificates;
            }
            set
            {
                this.webRequest.ClientCertificates = value;
            }
        }


        #endregion IHttpWebRequest Members
    }
}
