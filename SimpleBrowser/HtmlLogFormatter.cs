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

        //public string Render(List<LogItem> logs, string title)
        //{
        //    RazorModel model = new RazorModel
        //    {
        //        CaptureDate = DateTime.UtcNow,
        //        TotalDuration = logs.Count == 0 ? TimeSpan.MinValue : logs.Last().ServerTime - logs.First().ServerTime,
        //        Title = title,
        //        Logs = logs,
        //        RequestsCount = logs.Count(l => l is HttpRequestLog)
        //    };

        //    var engine = new RazorLight.RazorLightEngineBuilder()
        //        .UseMemoryCachingProvider()
        //        .Build();

        //    return engine.CompileRenderAsync("HtmlLog", Resources.HtmlLogTemplate, model).Result;
        //}

        public interface IViewRenderService
        {
            Task<string> RenderToStringAsync(string viewName);
            Task<string> RenderToStringAsync<TModel>(string viewName, TModel model);
            string RenderToString<TModel>(string viewPath, TModel model);
            string RenderToString(string viewPath);
        }

        //public class ViewRenderService : IViewRenderService
        //{
        //    private readonly IRazorViewEngine _viewEngine;
        //    private readonly ITempDataProvider _tempDataProvider;
        //    private readonly IServiceProvider _serviceProvider;
        //    private readonly IHttpContextAccessor _httpContextAccessor;

        //    public ViewRenderService(IRazorViewEngine viewEngine, IHttpContextAccessor httpContextAccessor,
        //        ITempDataProvider tempDataProvider,
        //        IServiceProvider serviceProvider)
        //    {
        //        _viewEngine = viewEngine;
        //        _httpContextAccessor = httpContextAccessor;
        //        _tempDataProvider = tempDataProvider;
        //        _serviceProvider = serviceProvider;
        //    }

        //    public string RenderToString<TModel>(string viewPath, TModel model)
        //    {
        //        try
        //        {
        //            var viewEngineResult = _viewEngine.GetView("~/", viewPath, false);

        //            if (!viewEngineResult.Success)
        //            {
        //                throw new InvalidOperationException($"Couldn't find view {viewPath}");
        //            }

        //            var view = viewEngineResult.View;

        //            using (var sw = new StringWriter())
        //            {
        //                var viewContext = new ViewContext()
        //                {
        //                    HttpContext = _httpContextAccessor.HttpContext ?? new DefaultHttpContext { RequestServices = _serviceProvider },
        //                    ViewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model },
        //                    Writer = sw
        //                };
        //                view.RenderAsync(viewContext).GetAwaiter().GetResult();
        //                return sw.ToString();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception("Error ending email.", ex);
        //        }
        //    }

        //    public async Task<string> RenderToStringAsync<TModel>(string viewName, TModel model)
        //    {
        //        var httpContext = _httpContextAccessor.HttpContext ?? new DefaultHttpContext { RequestServices = _serviceProvider };
        //        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        //        using (var sw = new StringWriter())
        //        {
        //            var viewResult = _viewEngine.FindView(actionContext, viewName, false);

        //            // Fallback - the above seems to consistently return null when using the EmbeddedFileProvider
        //            if (viewResult.View == null)
        //            {
        //                viewResult = _viewEngine.GetView("~/", viewName, false);
        //            }

        //            if (viewResult.View == null)
        //            {
        //                throw new ArgumentNullException($"{viewName} does not match any available view");
        //            }

        //            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        //            {
        //                Model = model
        //            };

        //            var viewContext = new ViewContext(
        //                actionContext,
        //                viewResult.View,
        //                viewDictionary,
        //                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
        //                sw,
        //                new HtmlHelperOptions()
        //            );

        //            await viewResult.View.RenderAsync(viewContext);
        //            return sw.ToString();
        //        }
        //    }

        //    public string RenderToString(string viewPath)
        //    {
        //        return RenderToString(viewPath, string.Empty);
        //    }

        //    public Task<string> RenderToStringAsync(string viewName)
        //    {
        //        return RenderToStringAsync<string>(viewName, string.Empty);
        //    }
        //}
    }
}