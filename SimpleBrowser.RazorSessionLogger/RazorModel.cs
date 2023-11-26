// -----------------------------------------------------------------------
// <copyright file="RazorModel.cs" company="SimpleBrowser">
// Copyright © 2010 - 2023, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.RazorSessionLogger
{
    /// <summary>
    /// A data model to pass to RazorLight for rendering.
    /// </summary>
    public class RazorModel
    {
        /// <summary>
        /// Gets or sets a date time representing the time the log entry was rendered.
        /// </summary>
        public DateTime? CaptureDate { get; set; }

        /// <summary>
        /// Gets or sets a title for the rendered page.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets a total duration of the session being rendered.
        /// </summary>
        public TimeSpan? TotalDuration { get; set; }

        /// <summary>
        /// Gets or sets a collection of log entries.
        /// </summary>
        public List<LogItem>? Logs { get; set; }

        /// <summary>
        /// Gets or sets a count of requests.
        /// </summary>
        public int? RequestsCount { get; set; }
    }
}
