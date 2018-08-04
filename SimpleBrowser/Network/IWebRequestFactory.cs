// -----------------------------------------------------------------------
// <copyright file="IWebRequestFactory.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Network
{
    using System;

    // TODO Review
    //   1) consider adding XML comments (documentation) to all public members

    public interface IWebRequestFactory
    {
        IHttpWebRequest GetWebRequest(Uri url);
    }

    public class DefaultRequestFactory : IWebRequestFactory
    {
        public IHttpWebRequest GetWebRequest(Uri url)
        {
            return new WebRequestWrapper(url);
        }
    }
}