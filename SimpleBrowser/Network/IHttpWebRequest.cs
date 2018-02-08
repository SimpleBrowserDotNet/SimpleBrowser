using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SimpleBrowser.Network
{
    // TODO Review 
    //   1) consider adding XML comments (documentation) to all public members

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
        Uri Address { get; }
        string Host { get; set; }
    }
}