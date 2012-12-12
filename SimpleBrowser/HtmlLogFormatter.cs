using System;
using System.Collections.Generic;
using System.Linq;
using RazorHosting;
using SimpleBrowser.Properties;

namespace SimpleBrowser
{
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
			var engine = new RazorEngine<RazorTemplateBase>();
			var html = engine.RenderTemplate(Resources.HtmlLogTemplate, new[] { typeof(Browser).Assembly.Location, "System.Web.dll" }, new RazorModel {
				CaptureDate = DateTime.UtcNow,
				TotalDuration = logs.Count == 0 ? TimeSpan.MinValue : logs.Last().ServerTime - logs.First().ServerTime,
				Title = title,
				Logs = logs,
				RequestsCount = logs.Count(l => l is HttpRequestLog)
			});
			return html ?? engine.ErrorMessage;
		}
	}
}
