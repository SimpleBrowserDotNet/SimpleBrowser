// -----------------------------------------------------------------------
// <copyright file="HtmlLogFormatter.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

#if NET40
    using RazorHosting;
#endif

    using SimpleBrowser.Properties;

    public class HtmlLogFormatter
    {
        public class RazorModel
        {
            public DateTime CaptureDate { get; set; }
            public string Title { get; set; }
            public TimeSpan TotalDuration { get; set; }
            public List<LogItem> Logs { get; set; }
            public int RequestsCount { get; set; }
        }

        public string Render(List<LogItem> logs, string title)
        {
            RazorModel model = new RazorModel
            {
                CaptureDate = DateTime.UtcNow,
                TotalDuration = logs.Count == 0 ? TimeSpan.MinValue : logs.Last().ServerTime - logs.First().ServerTime,
                Title = title,
                Logs = logs,
                RequestsCount = logs.Count(l => l is HttpRequestLog)
            };

#if NET40
            RazorEngine<RazorTemplateBase> engine = new RazorEngine<RazorTemplateBase>();
            string html = engine.RenderTemplate(Resources.HtmlLogTemplate, new[] { typeof(Browser).Assembly.Location, "System.Web.dll" }, model);
            return html ?? engine.ErrorMessage;
#else
            var engine = new RazorLight.RazorLightEngineBuilder()
                .UseMemoryCachingProvider()
                .Build();

            return engine.CompileRenderAsync("HtmlLog", Resources.HtmlLogTemplateNetStandard, model).Result;
#endif
        }
    }
}