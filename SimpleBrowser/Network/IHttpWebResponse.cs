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

    public interface IHttpWebResponse : IDisposable
    {
        Stream GetResponseStream();
        string CharacterSet { get; set; }
        string ContentType { get; set; }
        WebHeaderCollection Headers { get; set; }
        HttpStatusCode StatusCode { get; set; }
    }
}