// -----------------------------------------------------------------------
// <copyright file="HtmlLogFormatter.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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

        public string Render(List<LogItem> logs, string title, IViewRenderService renderservice)
        {
            RazorModel model = new RazorModel
            {
                CaptureDate = DateTime.UtcNow,
                TotalDuration = logs.Count == 0 ? TimeSpan.MinValue : logs.Last().ServerTime - logs.First().ServerTime,
                Title = title,
                Logs = logs,
                RequestsCount = logs.Count(l => l is HttpRequestLog)
            };

            //var engine = new RazorLight.RazorLightEngineBuilder()
            //    .UseMemoryCachingProvider()
            //    .Build();

            return renderservice.RenderToString(Resources.HtmlLogTemplate, model.Title, model);

        }

        public interface IViewRenderService
        {
           
            string RenderToString<TModel>(string template, string title, TModel model);
        }

    }
}