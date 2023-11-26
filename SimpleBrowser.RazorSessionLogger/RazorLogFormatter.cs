// -----------------------------------------------------------------------
// <copyright file="RazorLogFormatter.cs" company="SimpleBrowser">
// Copyright © 2010 - 2023, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.RazorSessionLogger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using SimpleBrowser.RazorSessionLogger.Properties;

    /// <summary>
    /// A log formatter supporting RazorLight.
    /// </summary>
    public class RazorLogFormatter : ISessionRenderService
    {
        /// <summary>
        /// Render the log file.
        /// </summary>
        /// <param name="logs">A collection of log entries to render.</param>
        /// <param name="title">A title.</param>
        /// <returns>A string containing the rendered content.</returns>
        public string Render(List<LogItem> logs, string title)
        {
            RazorModel model = new ()
            {
                CaptureDate = DateTime.UtcNow,
                TotalDuration = logs.Count == 0 ? TimeSpan.MinValue : logs.Last().ServerTime - logs.First().ServerTime,
                Title = title,
                Logs = logs,
                RequestsCount = logs.Count(l => l is HttpRequestLog),
            };

            var references = new MetadataReference[]
                {
                    MetadataReference.CreateFromFile("SimpleBrowser.dll"),
                };

            var engine = new RazorLight.RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(RazorLogFormatter))
                .AddMetadataReferences(references)
                .SetOperatingAssembly(typeof(RazorLogFormatter).Assembly)
                .AddDefaultNamespaces(["SimpleBrowser", "System.Web", "SimpleBrowser.RazorSessionLogger"])
                .UseMemoryCachingProvider()
                .Build();

            string result = engine.CompileRenderStringAsync("ServerTime", Resources.HtmlLogTemplate, model).Result;

            return result;
        }
    }
}