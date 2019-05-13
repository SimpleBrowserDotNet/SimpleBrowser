// -----------------------------------------------------------------------
// <copyright file="IHttpWebResponse.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Network
{
    using System;
    using System.IO;
    using System.Net;

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