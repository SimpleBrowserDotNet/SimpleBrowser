// -----------------------------------------------------------------------
// <copyright file="Browser.cs" company="SimpleBrowser">
// Copyright © 2010 - 2024, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser
{
    using System.Collections.Generic;

    /// <summary>
    /// An interface to a session render service.
    /// </summary>
    public interface ISessionRenderService
    {
        // Task<string> RenderToStringAsync(string viewName);

        // Task<string> RenderToStringAsync<TModel>(string viewName, TModel model);

        // string RenderToString<TModel>(string viewPath, TModel model);

        string Render(List<LogItem> logs, string viewPath);
    }
}