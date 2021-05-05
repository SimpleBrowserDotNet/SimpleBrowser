// -----------------------------------------------------------------------
// <copyright file="IHttpWebRequest.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Network
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    // TODO Review
    //   1) consider adding XML comments (documentation) to all public members

    public interface IHttpWebRequest
    {
        Task<Stream> GetRequestStreamAsync();

        Task<IHttpWebResponse> GetResponseAsync();

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
        Uri Address { get; }
        string Host { get; set; }
        X509CertificateCollection ClientCertificates { get; set; }
    }
}
