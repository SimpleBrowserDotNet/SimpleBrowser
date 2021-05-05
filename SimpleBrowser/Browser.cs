// -----------------------------------------------------------------------
// <copyright file="Browser.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml.Linq;
    using SimpleBrowser.Internal;
    using SimpleBrowser.Network;
    using SimpleBrowser.Parser;
    using SimpleBrowser.Query;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    public class Browser : IDisposable
    {
        private const string TARGET_SELF = "_self";
        internal const string TARGET_BLANK = "_blank";
        private const string TARGET_PARENT = "_parent";

        private readonly List<Browser> _allWindows;


        public void Dispose()
        {
            foreach(var frame in Frames)
            {
                frame.Dispose();
            }
            this.Close();
            
        }


        private HashSet<string> _extraHeaders = new HashSet<string>();
        private List<NavigationState> navigationHistory = new List<NavigationState>();
        private int navigationHistoryPosition = -1;
        private int maximumNavigationHistoryCount = 20;
        public int MaximumNavigationHistoryCount
        {
            get
            {
                return maximumNavigationHistoryCount;
            }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException();
                }

                maximumNavigationHistoryCount = value;
                navigationHistoryPosition = value - 1;

                while (navigationHistory.Count > this.maximumNavigationHistoryCount)
                {
                    navigationHistory.RemoveAt(navigationHistory.Count - 1);
                }
            }
        }

        private IWebProxy _proxy;
        private int _timeoutMilliseconds = 30000;
        private NameValueCollection _includeFormValues;
        private XDocument _doc;
        private HttpRequestLog _lastRequestLog;
        private List<LogItem> _logs = new List<LogItem>();
        private readonly IWebRequestFactory _reqFactory;
        private readonly Dictionary<string, BasicAuthenticationToken> _basicAuthenticationTokens;
        private NameValueCollection _navigationAttributes = null;
        private X509CertificateCollection _clientCertificates;
        
        public Encoding ResponseEncoding { get; set; }

        static Browser()
        {
            // Chrome no longer supports SSL. Chrome supports TLS 1.0, 1.1, 1.2, and 1.3.
            // .NET Standard 2.1 does not support TLS 1.3.
            // This sets the default SimpleBrowser security protocol to TLS 1.0, 1.1, or 1.2 by default. 
            // This site shows what security protocols are supported by any given browser: https://www.ssllabs.com/ssltest/viewMyClient.html
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            if (ServicePointManager.Expect100Continue)
            {
                ServicePointManager.Expect100Continue = false;
            }

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public Browser(IWebRequestFactory requestFactory = null, string name = null, List<Browser> context = null)
        {
            _allWindows = context ?? new List<Browser>();
            AutoRedirect = true;
            UserAgent = "SimpleBrowser/0.6 (Windows NT 10.0; Win64; x64)";
            RetainLogs = true;
            UseGZip = true;
            Cookies = new CookieContainer();
            if (requestFactory == null)
            {
                requestFactory = new DefaultRequestFactory();
            }

            _reqFactory = requestFactory;
            _basicAuthenticationTokens = new Dictionary<string, BasicAuthenticationToken>();
            WindowHandle = name;
            this.Register(this);
            this.RefererMode = RefererModes.NoneWhenDowngrade;
            this.Culture = CultureInfo.CurrentCulture;

            // Chrome no longer supports SSL. Chrome supports TLS 1.0, 1.1, 1.2, and 1.3 (Experimental).
            // .NET Standard 2.1 does not support TLS 1.3.
            // This sets the default SimpleBrowser security protocol to TLS 1.0, 1.1, or 1.2. 
            // This site shows what security protocols are supported by any given browser: https://www.ssllabs.com/ssltest/viewMyClient.html
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        public event Action<Browser, string> MessageLogged;

        public event Action<Browser, HttpRequestLog> RequestLogged;

        public event Action<Browser, Browser> NewWindowOpened;

        #region public properties

        public SecurityProtocolType SecurityProtocol
        {
            get
            {
                return ServicePointManager.SecurityProtocol;
            }

            set
            {
                ServicePointManager.SecurityProtocol = value;
            }
        }

        public string Accept { get; set; }

        /// <summary>
        /// Gets or sets the default browser time out in milliseconds.
        /// </summary>
        /// <remarks>
        /// The default browser time out must be greater than 0.
        /// </remarks>
        public int Timeout
        {
            get
            {
                return this._timeoutMilliseconds;
            }

            set
            {
                if (value > 0)
                {
                    this._timeoutMilliseconds = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the browser culture.
        /// </summary>
        /// <remarks>
        /// The default culture is the culture of the user or system on which the browser is running.
        /// </remarks>
        public CultureInfo Culture { get; set; }

        public string ContentType
        {
            get
            {
                return CurrentState == null ? string.Empty : CurrentState.ContentType;
            }
        }

        public string DocumentType
        {
            get
            {
                return CurrentState == null ? string.Empty : CurrentState.XDocument.DocumentType.ToString();
            }
        }

        public CookieContainer Cookies { get; set; }

        public string CurrentHtml
        {
            get
            {
                return CurrentState == null ? string.Empty : CurrentState.Html;
            }
        }

        public KeyStateOption KeyState { get; set; }

        /// <summary>
        /// Gets the collection of extra form values.
        /// </summary>
        /// <remarks>
        /// This collection allows you to specify additional key/value pairs that will be sent in the next request. Some
        /// websites use JavaScript or other dynamic methods to dictate what is submitted to the next page and these
        /// cannot be determined automatically from the originating HTML. In those cases, investigate the process using
        /// an HTTP sniffer, such as the HttpFox plugin for Firefox, and determine what values are being sent in the
        /// form submission. The additional unexpected values can then be automated by populating this property. Any
        /// values specified here are cleared after each request.
        /// </remarks>
        public NameValueCollection ExtraFormValues
        {
            get
            {
                return _includeFormValues ?? (_includeFormValues = new NameValueCollection());
            }
        }

        public IEnumerable<Browser> Frames
        {
            get
            {
                var doc = this.XDocument; // this will force the document to be parsed. It could result in more windows
                return this.Windows.Where(b => b.ParentWindow == this);
            }
        }

        public WebException LastWebException { get; private set; }

        /// <summary>
        /// Gets a dictionary reflecting the current navigation history. The keys are integers reflecting the position in the history,
        /// where the current page equals 0, the history has negative numbers and the future (pages you can navigate forward to) have positive
        /// numbers.
        /// </summary>
        public IDictionary<int, Uri> NavigationHistory
        {
            get
            {
                CheckDisposed();
                return navigationHistory.Select((s, i) => new { Index = i, State = s })
                    .ToDictionary((i) => i.Index - navigationHistoryPosition, (i) => i.State.Uri);
            }
        }
        /// <summary>
        /// An enumeration of the defined Referrer Policy States.
        /// </summary>
        /// <remarks>
        /// Source: http://www.w3.org/TR/referrer-policy/#referrer-policy-states
        /// </remarks>
        public enum RefererModes
        {
            /// <summary>
            /// The simplest policy is None, which specifies that no referrer information is to be sent along with requests made
            /// from a particular global environment to any origin. The header will be omitted entirely.
            /// </summary>
            /// <remarks>
            /// See: http://www.w3.org/TR/referrer-policy/#referrer-policy-state-none
            /// </remarks>
            None,

            /// <summary>
            /// Navigations from secure (https) URLs to insecure (http) URLs do not include the $referer$ header. Other navigations
            /// (secure to secure and insecure to insecure) to include the $referer$ header.
            /// Per the W3C policy:
            /// "This is a user agent’s default behavior, if no policy is otherwise specified."
            /// </summary>
            /// <remarks>
            /// See: http://www.w3.org/TR/referrer-policy/#referrer-policy-state-none-when-downgrade
            /// </remarks>
            NoneWhenDowngrade,

            /// <summary>
            /// Replace the referrer-header-value with the origin of the referring document.
            /// That is, https://www.google.com/webhp?sourceid=chrome-instant&ion=1&espv=2&ie=UTF-8
            /// becomes https://www.google.com/. Transition from secure to nonsecure transport has no effect on this setting.
            /// </summary>
            /// <remarks>
            /// See: http://www.w3.org/TR/referrer-policy/#referrer-policy-state-origin
            /// </remarks>
            Origin,

            /// <summary>
            /// If the domain of the request is the same as the referrer, the $referer$ header is the full URL of the referrer.
            /// If the domain is different, the $referer$ header behaves as if it were the Origin referrer policy.
            /// </summary>
            /// <remarks>
            /// See: http://www.w3.org/TR/referrer-policy/#referrer-policy-state-origin-when-cross-origin
            /// </remarks>
            OriginWhenCrossOrigin,

            /// <summary>
            /// The Unsafe URL policy specifies that a full URL, stripped for use as a referrer, is sent along with both
            /// cross-origin requests and same-origin requests made from a particular global environment.
            /// </summary>
            /// <remarks>
            /// See: http://www.w3.org/TR/referrer-policy/#referrer-policy-state-unsafe-url
            /// </remarks>
            UnsafeUrl,
        }

        /// <summary>
        /// Gets or sets the $referer$ header handling mode. (See <see cref="RefererModes"/>).
        /// This setting is overridden by the referrer meta tag, if received from the client.
        /// </summary>
        public RefererModes RefererMode { get; set; }

        /// <summary>
        /// Gets the Http Referrer
        /// </summary>
        public Uri Referer
        {
            get
            {
                return CurrentState?.Referer;
            }
        }

        [Obsolete("Use the CurrentHtml property instead.", true)]
        public string ResponseText
        {
            get
            {
                return CurrentState.Html /*TODO What is the difference here?*/;
            }
        }

        public bool AutoRedirect { get; set; }

        public bool RetainLogs { get; set; }

        public string Text
        {
            get
            {
                return XDocument.Root.Value;
            }
        }

        public Uri Url
        {
            get
            {
                return CurrentState?.Uri;
            }
        }

        public string UserAgent { get; set; }

        public bool UseGZip { get; set; }

        public string WindowHandle { get; private set; }

        public IEnumerable<Browser> Windows
        {
            get
            {
                return _allWindows;
            }
        }

        /// <summary>
        /// Gets the current HTML document parsed and converted to a valid XDocument object. Note that the
        /// originating HTML does not have to be valid XML; the parser will use a variety of methods to convert any
        /// invalid markup to valid XML.
        /// </summary>
        public XDocument XDocument
        {
            get
            {
                if (CurrentState.XDocument == null)
                {
                    try
                    {
                        CurrentState.XDocument = CurrentHtml.ParseHtml();
                        _lastRequestLog.ParsedHtml = CurrentState.XDocument.ToString();
                    }
                    catch (Exception ex)
                    {
                        Log("Error converting HTML to XML for URL " + Url, LogMessageType.Error);
                        Log(ex.Message, LogMessageType.Error);
                        Log("<b>Exception Stack Trace:</b><br />" + ex.StackTrace.Replace(Environment.NewLine, "<br />"), LogMessageType.StackTrace);
                        CurrentState.XDocument = HtmlParser.CreateBlankHtmlDocument();
                    }

                    // check if we need to create sub-browsers for iframes
                    foreach (var frame in this.FindAll("iframe"))
                    {
                        Log("found iframe +" + frame.CurrentElement.GetAttributeValue("name"));
                    }

                    // check if we need to create sub-browsers for frames
                    foreach (var frame in this.FindAll("frame"))
                    {
                        Log("found frame +" + frame.CurrentElement.GetAttributeValue("name"));
                    }
                }

                return CurrentState.XDocument;
            }
        }

        #endregion public properties

        #region internal properties

        internal NavigationState CurrentState
        {
            get
            {
                CheckDisposed();
                if (navigationHistoryPosition == -1)
                {
                    return null;
                }

                return navigationHistory[navigationHistoryPosition];
            }
        }

        #endregion internal properties

        #region private properties

        private Browser ParentWindow { get; set; }

        #endregion private properties

        #region public methods

        public void ClearException()
        {
            LastWebException = null;
        }

        public void ClearWindowsInContext()
        {
            foreach (var window in _allWindows.ToArray())
            {
                window.Close();
            }

            _allWindows.Clear();
        }

        public static void ClearWindows()
        {
            foreach (var list in _allContexts.ToArray())
            {
                foreach (var window in list.ToArray())
                {
                    window.Close();
                }
            }

            _allContexts.Clear();
        }

        public void Close()
        {
            navigationHistory = null;

            if (_allWindows.Contains(this))
                _allWindows.Remove(this);
        }

        /// <summary>
        /// Performs a culture-invariant text search on the current document, ignoring whitespace, html elements and case, which reduces the
        /// </summary>
        /// <param name="text">The text to search for</param>
        /// <returns>A value indicating whether the text was found</returns>
        public bool ContainsText(string text)
        {
            Log("Looking for text: " + text, LogMessageType.Internal);
            text = HttpUtility.HtmlDecode(text);
            string source = HttpUtility.HtmlDecode(XDocument.Root.Value).Replace("&nbsp;", " ");
            var found = new Regex(Regex.Replace(Regex.Escape(text).Replace(@"\ ", " "), @"\s+", @"\s+"), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).IsMatch(source);
            Log("&rarr; Text " + (found ? "" : "NOT ") + "found!", LogMessageType.Internal);
            return found;
        }

        public Browser CreateReferenceView()
        {
            Browser b = new Browser(_reqFactory, context: _allWindows)
            {
                Cookies = Cookies,
                _doc = _doc,
                _extraHeaders = _extraHeaders,
                _includeFormValues = _includeFormValues,
                _lastRequestLog = _lastRequestLog,
                _logs = _logs,
                _proxy = _proxy,
                _timeoutMilliseconds = _timeoutMilliseconds,
                Accept = Accept,
                LastWebException = LastWebException,
                RetainLogs = RetainLogs,
                UserAgent = UserAgent,
                AutoRedirect = AutoRedirect,
            };
            b.MessageLogged = MessageLogged;
            b.AddNavigationState(this.CurrentState);
            return b;
        }

        public Browser GetWindowByName(string name)
        {
            return Windows.FirstOrDefault(b => b.WindowHandle == name);
        }

        #region Finding

        public HtmlResult Find(ElementType elementType, FindBy findBy, string value)
        {
            return GetHtmlResult(FindElement(elementType, findBy, value));
        }

        public HtmlResult Find(string tagName, FindBy findBy, string value)
        {
            return GetHtmlResult(FindElement(FindElements(tagName), findBy, value));
        }

        public HtmlResult Find(string id)
        {
            return GetHtmlResult(FilterElementsByAttribute(XDocument.Descendants().ToList(), "id", id, false));
        }

        public HtmlResult Find(ElementType elementType, string attributeName, string attributeValue)
        {
            var elementsOfCorrectType = FindElements(elementType);
            return GetHtmlResult(FilterElementsByAttribute(elementsOfCorrectType, attributeName, attributeValue, false));
        }

        public HtmlResult Find(ElementType elementType, object elementAttributes)
        {
            var list = FindElements(elementType);
            foreach (var p in elementAttributes.GetType().GetProperties())
            {
                object o = p.GetValue(elementAttributes, null);
                if (o == null)
                {
                    continue;
                }

                list = FilterElementsByAttributeName(list, p, o.ToString(), false);
            }

            return GetHtmlResult(list);
        }

        public HtmlResult Find(string tagName, object elementAttributes)
        {
            var list = FindElements(tagName);
            foreach (var p in elementAttributes.GetType().GetProperties())
            {
                object o = p.GetValue(elementAttributes, null);
                if (o == null)
                {
                    continue;
                }

                list = FilterElementsByAttribute(list, p.Name, o.ToString(), false);
            }

            return GetHtmlResult(list);
        }

        public HtmlResult FindAll(string tagName)
        {
            return GetHtmlResult(FindElements(tagName));
        }

        public HtmlResult FindClosestAncestor(HtmlResult element, string ancestorTagName, object elementAttributes = null)
        {
            XElement anc = element.CurrentElement.Element;
            for (; ; )
            {
                anc = anc.GetAncestorOfSelfCI(ancestorTagName);
                if (elementAttributes == null)
                {
                    break;
                }

                bool succeeded = true;
                foreach (var p in elementAttributes.GetType().GetProperties())
                {
                    object o = p.GetValue(elementAttributes, null);
                    if (o == null)
                    {
                        continue;
                    }

                    var attr = GetAttribute(anc, p.Name);
                    if (attr == null || attr.Value.ToLower() != o.ToString().ToLower())
                    {
                        succeeded = false;
                        break;
                    }
                }

                if (succeeded)
                {
                    break;
                }

                anc = anc.Parent;
            }

            return GetHtmlResult(anc);
        }

        #endregion Finding

        #region Logging

        public void Log(string message, LogMessageType type = LogMessageType.User)
        {
            if (RetainLogs)
            {
                _logs.Add(new LogMessage(message, type));
            }

            MessageLogged?.Invoke(this, message);
        }

        public void LogRequestData()
        {
            HttpRequestLog log = RequestData();
            if (RetainLogs)
            {
                _logs.Add(log);
            }

            if (log != null && RequestLogged != null)
            {
                RequestLogged(this, log);
            }
        }

        #endregion Logging

        internal void RaiseNewWindowOpened(Browser newWindow)
        {
            this.NewWindowOpened?.Invoke(this, newWindow);
        }

        [Obsolete("Use Async version instead")]
        public bool Navigate(string url)
        {
            return Navigate(new Uri(url));
        }

        public async Task<bool> NavigateAsync(string url)
        {
            return await NavigateAsync(new Uri(url));
        }

        [Obsolete("Use Async version instead")]
        public bool Navigate(string url, string Method)
        {
            return DoRequest(new Uri(url), Method, null, null, null, null, _timeoutMilliseconds);
        }

        public async Task<bool> NavigateAsync(string url, string Method)
        {
            return await DoRequestAsync(new Uri(url), Method, null, null, null, null, _timeoutMilliseconds);
        }

        [Obsolete("Use Async version instead")]
        public bool Navigate(string url, int timeoutMilliseconds)
        {
            return Navigate(new Uri(url), timeoutMilliseconds);
        }

        public async Task<bool> NavigateAsync(string url, int timeoutMilliseconds)
        {
            return await NavigateAsync(new Uri(url), timeoutMilliseconds);
        }

        [Obsolete("Use Async version instead")]
        public bool Navigate(Uri url)
        {
            return DoRequest(url, "GET", null, null, null, null, _timeoutMilliseconds);
        }

        public async Task<bool> NavigateAsync(Uri url)
        {
            return await DoRequestAsync(url, "GET", null, null, null, null, _timeoutMilliseconds);
        }

        [Obsolete("Use Async version instead")]
        public bool Navigate(Uri url, string postData, string contentType)
        {
            return DoRequest(url, "POST", null, postData, contentType, null, _timeoutMilliseconds);
        }

        public async Task<bool> NavigateAsync(Uri url, string postData, string contentType)
        {
            return await DoRequestAsync(url, "POST", null, postData, contentType, null, _timeoutMilliseconds);
        }

        [Obsolete("Use Async version instead")]
        public bool Navigate(Uri url, NameValueCollection postData, string contentType = null, string encodingType = null)
        {
            return DoRequest(url, "POST", postData, null, contentType, encodingType, _timeoutMilliseconds);
        }

        public async Task<bool> NavigateAsync(Uri url, NameValueCollection postData, string contentType = null, string encodingType = null)
        {
            return await DoRequestAsync(url, "POST", postData, null, contentType, encodingType, _timeoutMilliseconds);
        }


        [Obsolete("Use Async version instead")]
        public bool Navigate(Uri url, int timeoutMilliseconds)
        {
            _timeoutMilliseconds = timeoutMilliseconds;
            return Navigate(url);
        }

        public async Task<bool> NavigateAsync(Uri url, int timeoutMilliseconds)
        {
            _timeoutMilliseconds = timeoutMilliseconds;
            return await NavigateAsync(url);
        }


        public bool NavigateBack()
        {
            CheckDisposed();
            if (navigationHistoryPosition > 0)
            {
                navigationHistoryPosition--;
                InvalidateAllActiveElements();
                return true;
            }

            return false;
        }

        public bool NavigateForward()
        {
            CheckDisposed();
            if (navigationHistory.Count > navigationHistoryPosition + 1)
            {
                navigationHistoryPosition++;
                InvalidateAllActiveElements();
                return true;
            }

            return false;
        }

        public void RemoveHeader(string header)
        {
            _extraHeaders.Remove(header);
        }

        public string RenderHtmlLogFile(HtmlLogFormatter.IViewRenderService renderservice, string title = "SimpleBrowser Session Log")
        {
            var formatter = new HtmlLogFormatter();
            return formatter.Render(_logs, title, renderservice);
        }

        /// <summary>
        /// Return the information related to the last request
        /// </summary>
        /// <returns>The <see cref="HttpRequestLog"/> for the last navigation request.</returns>
        public HttpRequestLog RequestData()
        {
            return _lastRequestLog;
        }

        /// <summary>
        /// This is an alternative to Find and allows the use of jQuery selector syntax to locate elements on the page.
        /// </summary>
        /// <param name="query">The query to use to locate elements</param>
        /// <returns>An HtmlResult object containing zero or more matches</returns>
        public HtmlResult Select(string query)
        {
            var result = new HtmlResult(XQuery.Execute(query, XDocument).Select(CreateHtmlElement).ToList(), this);
            Log("Selected " + result.TotalElementsFound + " element(s) via jQuery selector: " + query, LogMessageType.Internal);
            return result;
        }

        /// <summary>
        /// Overwrites the CurrentHtml property with new content, allowing it to be queried and analyzed as though it
        /// was navigated to in the last request.
        /// </summary>
        /// <param name="content">A string containing a</param>
        public void SetContent(string content)
        {
            AddNavigationState(new NavigationState()
            {
                Html = content,
                ContentType = "text/html",
                Uri = new Uri("app://simplebrowser/setcontent"),
                Referer = null,
            });

            CurrentState.XDocument = CurrentHtml.ParseHtml();
        }

        public void SetHeader(string header)
        {
            _extraHeaders.Add(header);
        }

        public void SetProxy(IWebProxy webProxy)
        {
            _proxy = webProxy;
        }

        public void SetProxy(string host, int port)
        {
            _proxy = new WebProxy(host, port);
        }

        public void SetProxy(string host, int port, string username, string password)
        {
            SetProxy(host, port);
            _proxy.Credentials = new NetworkCredential(username, password);
        }

        public void BasicAuthenticationLogin(string domain, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentNullException("domain");
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException("username");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException("password");
            }

            _basicAuthenticationTokens[domain] = new BasicAuthenticationToken(domain, username, password);
        }
        
        public void SetCertificate(System.Security.Cryptography.X509Certificates.X509CertificateCollection clientCertificates)
        {
            _clientCertificates = clientCertificates;
        }
        
        public void BasicAuthenticationLogout(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentNullException("domain");
            }

            _basicAuthenticationTokens.Remove(domain);
        }

        #endregion public methods

        #region internal methods

        internal void AddNavigationState(NavigationState state)
        {
            while (navigationHistory.Count > navigationHistoryPosition + 1)
            {
                navigationHistory.Remove(navigationHistory.Last());
            }

            navigationHistoryPosition++;
            navigationHistory.Add(state);
            this.InvalidateAllActiveElements();
            while (navigationHistory.Count > maximumNavigationHistoryCount)
            {
                navigationHistory.RemoveAt(0);
                navigationHistoryPosition--;
            }
        }

        internal Browser CreateChildBrowser(string name = null)
        {
            Browser child = new Browser(_reqFactory, name, _allWindows)
            {
                ParentWindow = this
            };

            // no RaiseNewWindowOpened here, because it is not really a new window. It can be navigated to using
            // the frames collection of the parent
            return child;
        }

        internal HtmlElement CreateHtmlElement(XElement element)
        {
            var htmlElement = HtmlElement.CreateFor(element);
            if (htmlElement != null)
            {
                this.CurrentState.Elements.Add(htmlElement);
                htmlElement.OwningBrowser = this;
                htmlElement.NavigationRequested += HtmlElement_NavigationRequested;
            }

            return htmlElement;
        }

        internal T CreateHtmlElement<T>(XElement element) where T : HtmlElement
        {
            var result = CreateHtmlElement(element);
            if (result is T)
            {
                return (T)result;
            }

            throw new InvalidOperationException("The element was not of the corresponding type");
        }

        [Obsolete("This methods execute sync over async and should be avoided")]
        public bool DoRequest(Uri uri, string method, NameValueCollection userVariables, string postData, string contentType, string encodingType, int timeoutMilliseconds)
        {
            return DoRequestAsync(uri, method, userVariables, postData, contentType, encodingType, timeoutMilliseconds).GetAwaiter().GetResult();
        }

        public async Task<bool> DoRequestAsync(Uri uri, string method, NameValueCollection userVariables, string postData, string contentType, string encodingType, int timeoutMilliseconds)
        {
            string html;
            string referer = null;

            if (uri.IsFile)
            {
                using (var reader = new StreamReader(uri.AbsolutePath))
                {
                    html = await reader.ReadToEndAsync();
                    reader.Close();
                }

                _lastRequestLog = new HttpRequestLog
                {
                    Method = "GET",
                    Url = uri,
                    Text = html
                };
            }
            else
            {
                bool handle3xxRedirect = false;
                int maxRedirects = 10; // Per RFC2068, Section 10.3 it should be 5. However some sites have abused this 
                string postBody = string.Empty;
                do
                {
                    Debug.WriteLine(uri.ToString());
                    if (maxRedirects-- < 0)
                    {
                        Log("Too many 3xx redirects", LogMessageType.Error);
                        return false;
                    }

                    handle3xxRedirect = false;
                    IHttpWebRequest req = null;

                    try
                    {
                        req = PrepareRequestObject(uri, method, contentType, timeoutMilliseconds);
                    }
                    catch (NotSupportedException)
                    {
                        // Happens when the URL cannot be parsed (example: 'javascript:')
                        return false;
                    }

                    foreach (var header in _extraHeaders)
                    {
                        if (header.StartsWith("host:", StringComparison.OrdinalIgnoreCase))
                        {
                            req.Host = header.Split(':')[1];
                        }
                        else
                        {
                            req.Headers.Add(header);
                        }
                    }

                    if (!string.IsNullOrEmpty(encodingType) && method != "GET")
                    {
                        req.Headers.Add(HttpRequestHeader.ContentEncoding, encodingType);
                    }

                    // Remove all expired basic authentication tokens
                    List<BasicAuthenticationToken> expired =
                        _basicAuthenticationTokens.Values.Where(t => DateTime.Now > t.Expiration).ToList();
                    foreach (var expiredToken in expired)
                    {
                        _basicAuthenticationTokens.Remove(expiredToken.Domain);
                    }

                    // If an authentication token exists for the domain, add the authorization header.
                    foreach (var token in _basicAuthenticationTokens.Values)
                    {
                        if (req.Host.Contains(token.Domain))
                        {
                            // Extend the expiration.
                            token.UpdateExpiration();

                            // Add the authentication header.
                            req.Headers.Add(string.Format(
                                "Authorization: Basic {0}",
                                token.Token));
                        }
                    }

                    if (_includeFormValues != null)
                    {
                        if (userVariables == null)
                        {
                            userVariables = _includeFormValues;
                        }
                        else
                        {
                            userVariables.Add(_includeFormValues);
                        }
                    }

                    if (userVariables != null)
                    {
                        if (method == "POST" || method == "PUT")
                        {
                            postBody = StringUtil.MakeQueryString(userVariables);
                            byte[] data = Encoding.GetEncoding(28591).GetBytes(postBody);
                            req.ContentLength = data.Length;
                            using (Stream stream = await req.GetRequestStreamAsync())
                            {
                                await stream.WriteAsync(data, 0, data.Length);
                            }
                        }
                        else
                        {
                            uri = new Uri(
                                uri.Scheme + "://" + uri.Host + ":" + uri.Port + uri.AbsolutePath
                                + ((userVariables.Count > 0) ? "?" + StringUtil.MakeQueryString(userVariables) : "")
                                );
                            req = PrepareRequestObject(uri, method, contentType, timeoutMilliseconds);
                        }
                    }
                    else if (postData != null)
                    {
                        if (method == "GET")
                        {
                            throw new InvalidOperationException("Cannot call DoRequest with method GET and non-null postData");
                        }

                        postBody = postData;

                        // 28591 corresponds to ISO-8859-1, the default encoding of an HTTP POST.
                        // http://msdn.microsoft.com/en-us/library/system.text.encodinginfo.getencoding%28v=vs.110%29.aspx
                        // In the event that this value ever changes, including no longer being hard coded,
                        // update the encoding being sent with a form submission with a hidden input named _charset_
                        // in InputElement.cs.
                        byte[] data = Encoding.GetEncoding(28591).GetBytes(postData);
                        req.ContentLength = data.Length;
                        using (Stream stream = await req.GetRequestStreamAsync())
                        {
                            await stream.WriteAsync(data, 0, data.Length);
                        }
                    }

                    referer = req.Referer;

                    if (contentType != null)
                    {
                        req.ContentType = contentType;
                    }

                    _lastRequestLog = new HttpRequestLog
                    {
                        Method = method,
                        PostData = userVariables,
                        PostBody = postBody,
                        RequestHeaders = req.Headers,
                        Url = uri,
                        Address = req.Address,
                        Host = req.Host
                    };
                    if (_clientCertificates != null)
                    {
                        req.ClientCertificates = _clientCertificates;
                    }
                    
                    try
                    {
                        System.Threading.Thread.Sleep(100);

                        using (IHttpWebResponse response = await req.GetResponseAsync())
                        {
                            Encoding responseEncoding = ResponseEncoding ?? Encoding.UTF8; //default
                            if (ResponseEncoding == null &&
                                ((response.Headers.AllKeys.Contains("Content-Type", StringComparer.OrdinalIgnoreCase) &&
                                 response.Headers["Content-Type"].IndexOf("charset", 0, StringComparison.OrdinalIgnoreCase) > -1) ||
                                 !string.IsNullOrWhiteSpace(response.CharacterSet)))
                            {
                                try
                                {
                                    responseEncoding = Encoding.GetEncoding(response.CharacterSet);
                                }
                                catch (ArgumentException)
                                {
                                    responseEncoding = Encoding.UTF8; // try using utf8
                                }
                            }

                            //ensure the stream is disposed
                            using (Stream rs = response.GetResponseStream())
                            {
                                using (StreamReader reader = new StreamReader(rs, responseEncoding))
                                {
                                    html = reader.ReadToEnd();
                                }
                            }

                            _doc = null;
                            _includeFormValues = null;

                            _lastRequestLog.Text = html;
                            _lastRequestLog.ResponseHeaders = response.Headers;
                            _lastRequestLog.ResponseCode = (int)response.StatusCode;

                            if (method == "GET" && uri.Query.Length > 0 && uri.Query != "?")
                            {
                                _lastRequestLog.QueryStringData = HttpUtility.ParseQueryString(uri.Query);
                            }

                            if (AutoRedirect == true &&
                                (((int)response.StatusCode == 300 || // Not entirely supported. If provided, the server's preference from the Location header is honored.
                                (int)response.StatusCode == 301 ||
                                (int)response.StatusCode == 302 ||
                                (int)response.StatusCode == 303 ||

                                // 304 - Unsupported, conditional Get requests are not supported (mostly because SimpleBrowser does not cache content)
                                // 305 - Unsupported, possible security threat
                                // 306 - No longer used, per RFC2616, Section 10.3.7
                                (int)response.StatusCode == 307 ||
                                (int)response.StatusCode == 308) &&
                                response.Headers.AllKeys.Contains("Location")))
                            {
                                uri = new Uri(uri, response.Headers["Location"]);
                                handle3xxRedirect = true;
                                Debug.WriteLine("Redirecting to: " + uri);
                                method = "GET";
                                postData = null;
                                userVariables = null;
                            }

                            if (response.Headers.AllKeys.Contains("Set-Cookie"))
                            {
                                var cookies = SetCookieHeaderParser.GetAllCookiesFromHeader(uri.Host, response.Headers["Set-Cookie"]);
                                Cookies.Add(cookies);
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        if (ex.Response != null)
                        {
                            _lastRequestLog.ResponseHeaders = ex.Response.Headers;

                            //ensure the stream is disposed
                            using (Stream rs = ex.Response.GetResponseStream())
                            {
                                using (StreamReader reader = new StreamReader(rs))
                                {
                                    html = reader.ReadToEnd();
                                }
                            }

                            _lastRequestLog.Text = html;
                            _lastRequestLog.ResponseCode = (int)((HttpWebResponse)ex.Response).StatusCode;
                        }

                        LastWebException = ex;

                        switch (ex.Status)
                        {
                            case WebExceptionStatus.Timeout:
                                Log("A timeout occurred while trying to load the web page", LogMessageType.Error);
                                break;

                            case WebExceptionStatus.ReceiveFailure:
                                Log("The response was cut short prematurely", LogMessageType.Error);
                                break;

                            default:
                                Log("An exception was thrown while trying to load the page: " + ex.Message, LogMessageType.Error);
                                break;
                        }

                        return false;
                    }
                    finally
                    {
                        LogRequestData();
                    }
                }
                while (handle3xxRedirect);
            }

            this._navigationAttributes = null;
            this.RemoveChildBrowsers(); //Any frames contained in the previous state should be removed. They will be recreated if we ever navigate back
            this.AddNavigationState(
                new NavigationState()
                {
                    Html = html,
                    Uri = uri,
                    ContentType = contentType,
                    Referer = string.IsNullOrEmpty(referer) ? null : new Uri(Uri.UnescapeDataString(referer))
                });

            return true;
        }

        internal void RemoveChildBrowsers()
        {
            _allWindows.RemoveAll((b) => b.ParentWindow == this);
        }

        #endregion internal methods

        #region private methods

        private void CheckDisposed()
        {
            if (navigationHistory == null)
            {
                throw new ObjectDisposedException("This browser has been closed. You cannot access the content or history after closing.");
            }
        }

        #region Finding

        private List<XElement> FindElements(string tagName)
        {
            return XDocument.Descendants()
                .Where(h => h.Name.LocalName.ToLower() == tagName.ToLower())
                .ToList();
        }

        private static readonly string[] knownInputTypes = new string[] { "submit", "image", "checkbox", "radio", "button" };

        private List<XElement> FindElements(ElementType elementType)
        {
            List<XElement> list;
            switch (elementType)
            {
                case ElementType.Anchor:
                    list = XDocument.Descendants()
                        .Where(h => h.Name.LocalName.ToLower() == "a" && h.Attributes()
                                                                            .Where(k => k.Name.LocalName.ToLower() == "href").Count() > 0)
                        .ToList();
                    break;

                case ElementType.Button:
                    list = XDocument.Descendants()
                        .Where(h => h.Name.LocalName.ToLower() == "button" ||
                                    (h.Name.LocalName.ToLower() == "input" && h.Attributes()
                                                                                .Where(k => k.Name.LocalName.ToLower() == "type"
                                                                                            && (k.Value.ToLower() == "submit" || k.Value.ToLower() == "image")).Count() > 0))
                        .ToList();
                    break;

                case ElementType.TextField:
                    list = XDocument.Descendants()
                        .Where(h => h.Name.LocalName.ToLower() == "textarea" ||
                                    (h.Name.LocalName.ToLower() == "input" && h.Attributes()
                                                                                .Where(k => k.Name.LocalName.ToLower() == "type" && !knownInputTypes.Contains(k.Value.ToLower())).Count() > 0))
                        .ToList();
                    list.AddRange(XDocument.Descendants() // also add input elements with no "type" attribute (they default to type="text")
                                    .Where(h => h.Name.LocalName.ToLower() == "input" && h.Attributes()
                                                                                            .Where(k => k.Name.LocalName.ToLower() == "type").Count() == 0));
                    break;

                case ElementType.Checkbox:
                    list = XDocument.Descendants()
                        .Where(h => h.Name.LocalName.ToLower() == "input" && h.Attributes()
                                                                                .Where(k => k.Name.LocalName.ToLower() == "type" && k.Value.ToLower() == "checkbox").Count() > 0)
                        .ToList();
                    break;

                case ElementType.RadioButton:
                    list = XDocument.Descendants()
                        .Where(h => h.Name.LocalName.ToLower() == "input" && h.Attributes()
                                                                                .Where(k => k.Name.LocalName.ToLower() == "type" && k.Value.ToLower() == "radio").Count() > 0)
                        .ToList();
                    break;

                case ElementType.SelectBox:
                    list = XDocument.Descendants()
                        .Where(h => h.Name.LocalName.ToLower() == "select")
                        .ToList();
                    break;

                case ElementType.Script:
                    list = XDocument.Descendants()
                        .Where(h => h.Name.LocalName.ToLower() == "script")
                        .ToList();
                    break;

                default:
                    list = new List<XElement>();
                    break;
            }

            return list;
        }

        private List<XElement> FilterElementsByAttribute(List<XElement> elements, string attributeName, string value, bool allowPartialMatch)
        {
            if (allowPartialMatch)
            {
                return elements.Where(h => h.Attributes()
                                            .Where(k => k.Name.LocalName.ToLower() == attributeName.ToLower()
                                                        && k.Value.ToLower().Contains(value.ToLower())).Count() > 0)
                    .ToList();
            }
            else
            {
                return elements.Where(h => h.Attributes()
                                            .Where(k => k.Name.LocalName.ToLower() == attributeName.ToLower()
                                                        && k.Value.ToLower() == value.ToLower()).Count() > 0)
                    .ToList();
            }
        }

        private List<XElement> FilterElementsByAttributeName(List<XElement> list, PropertyInfo p, string value, bool allowPartialMatch)
        {
            var matchesByAttribute = FilterElementsByAttribute(list, p.Name, value, allowPartialMatch);
            if (!matchesByAttribute.Any())
            {
                if (p.Name.Contains('_'))
                {
                    var attributeName = p.Name.Replace('_', '-');
                    matchesByAttribute = FilterElementsByAttribute(list, attributeName, value, allowPartialMatch);
                }
            }

            list = matchesByAttribute;
            return list;
        }

        private List<XElement> FilterElementsByAttributeNameToken(List<XElement> elements, string attributeName, string value, bool allowPartialMatch)
        {
            return elements.Where(elm =>
            {
                string attrValue = elm.GetAttribute(attributeName);
                if (attrValue == null)
                {
                    return false;
                }

                string[] tokens = attrValue.ToLower().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (allowPartialMatch)
                {
                    return tokens.Any(t => t.Contains(value.ToLower()));
                }
                else
                {
                    return tokens.Any(t => t == value.ToLower());
                }
            }).ToList();
        }

        private List<XElement> FilterElementsByInnerText(List<XElement> elements, string tagName, string value, bool allowPartialMatch)
        {
            if (allowPartialMatch)
            {
                return elements.Where(h => (tagName == null || h.Name.LocalName.ToLower() == tagName.ToLower())
                                           && h.Value.ToLower().Trim().Contains(value.ToLower().Trim()))
                    .ToList();
            }
            else
            {
                return elements.Where(h => (tagName == null || h.Name.LocalName.ToLower() == tagName.ToLower())
                                           && h.Value.ToLower().Trim() == value.ToLower().Trim())
                    .ToList();
            }
        }

        private List<XElement> FindElement(ElementType elementType, FindBy findBy, string value)
        {
            return FindElement(FindElements(elementType), findBy, value);
        }

        private List<XElement> FindElement(List<XElement> elements, FindBy findBy, string value)
        {
            switch (findBy)
            {
                case FindBy.Text:
                    return FilterElementsByInnerText(elements, null, value, false);

                case FindBy.Class:
                    return FilterElementsByAttributeNameToken(elements, "class", value, false);

                case FindBy.Id:
                    return FilterElementsByAttribute(elements, "id", value, false);

                case FindBy.Name:
                    return FilterElementsByAttribute(elements, "name", value, false);

                case FindBy.Value:
                    {
                        var newlist = FilterElementsByAttribute(elements, "value", value, false);
                        newlist.AddRange(FilterElementsByInnerText(elements, "textarea", value, false));
                        newlist.AddRange(FilterElementsByInnerText(elements, "button", value, false));
                        return newlist;
                    }

                case FindBy.PartialText:
                    return FilterElementsByInnerText(elements, null, value, true);

                case FindBy.PartialClass:
                    return FilterElementsByAttributeNameToken(elements, "class", value, true);

                case FindBy.PartialId:
                    return FilterElementsByAttribute(elements, "id", value, true);

                case FindBy.PartialName:
                    return FilterElementsByAttribute(elements, "name", value, true);

                case FindBy.PartialValue:
                    {
                        var newlist = FilterElementsByAttribute(elements, "value", value, true);
                        newlist.AddRange(FilterElementsByInnerText(elements, "textarea", value, true));
                        newlist.AddRange(FilterElementsByInnerText(elements, "button", value, true));
                        return newlist;
                    }

                default:
                    return null;
            }
        }

        #endregion Finding

        private XAttribute GetAttribute(XElement element, string name)
        {
            return element.Attributes().Where(h => h.Name.LocalName.ToLower() == name.ToLower()).FirstOrDefault();
        }

        private HtmlResult GetHtmlResult(List<XElement> list)
        {
            List<HtmlElement> xlist = new List<HtmlElement>();
            foreach (var e in list)
            {
                var element = CreateHtmlElement(e);
                if (element != null)
                {
                    xlist.Add(element);
                }
            }

            return new HtmlResult(xlist, this);
        }

        private HtmlResult GetHtmlResult(XElement e)
        {
            return new HtmlResult(CreateHtmlElement(e), this);
        }

        private async Task<bool> HtmlElement_NavigationRequested(HtmlElement.NavigationArgs args)
        {
            Uri fullUri = new Uri(this.Url, args.Uri);
            if (args.TimeoutMilliseconds <= 0)
            {
                args.TimeoutMilliseconds = _timeoutMilliseconds;
            }

            Browser browserToNav = null;
            if (args.Target == TARGET_SELF || String.IsNullOrEmpty(args.Target))
            {
                browserToNav = this;
            }
            else if (args.Target == TARGET_BLANK)
            {
                browserToNav = new Browser(_reqFactory, context: _allWindows);
                RaiseNewWindowOpened(browserToNav);
            }
            else if (args.Target == TARGET_PARENT)
            {
                browserToNav = this.ParentWindow ?? (this);
            }
            else
            {
                browserToNav = this.Windows.FirstOrDefault(b => b.WindowHandle == args.Target);
                if (browserToNav == null)
                {
                    browserToNav = new Browser(this._reqFactory, args.Target, _allWindows);
                    RaiseNewWindowOpened(browserToNav);
                }
            }

            this._navigationAttributes = args.NavigationAttributes;

            return await browserToNav.DoRequestAsync(fullUri, args.Method, args.UserVariables, args.PostData, args.ContentType, args.EncodingType, args.TimeoutMilliseconds);
        }

        private void InvalidateAllActiveElements()
        {
            navigationHistory.ForEach(h => h.Invalidate());
        }

        private IHttpWebRequest PrepareRequestObject(Uri url, string method, string contentType, int timeoutMilliseconds)
        {
            IHttpWebRequest req = _reqFactory.GetWebRequest(url);
            req.Method = method;
            req.ContentType = contentType;
            req.UserAgent = UserAgent;
            req.Accept = Accept ?? "*/*";
            req.Timeout = timeoutMilliseconds;
            req.AllowAutoRedirect = false;
            req.CookieContainer = Cookies;

            if (UseGZip)
            {
                req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            }

            if (_proxy != null)
            {
                req.Proxy = _proxy;
            }

            // Start by using the browser setting for the referrer mode.
            RefererModes refererMode = this.RefererMode;

            // Allow the referrer meta tag to override the global browser mode.
            if (this.CurrentState != null)
            {
                var metatag = this.Find("meta", new { name = "referrer" });
                if (metatag.Exists)
                {
                    string content = metatag.GetAttribute("content").Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        if (content.Equals("never", StringComparison.OrdinalIgnoreCase) || // Legacy value
                            content.Equals("none", StringComparison.OrdinalIgnoreCase)) // Modern value
                        {
                            refererMode = RefererModes.None;
                        }
                        else if (content.Equals("origin", StringComparison.OrdinalIgnoreCase))
                        {
                            refererMode = RefererModes.Origin;
                        }
                        else if (content.Equals("default", StringComparison.OrdinalIgnoreCase) || // Legacy value
                            content.Equals("none-when-downgrade", StringComparison.OrdinalIgnoreCase)) // Modern value
                        {
                            refererMode = RefererModes.NoneWhenDowngrade;
                        }
                        else if (content.Equals("origin-when-crossorigin", StringComparison.OrdinalIgnoreCase) || // Current value, with typo in early document versions
                            content.Equals("origin-when-cross-origin", StringComparison.OrdinalIgnoreCase)) // Modern value
                        {
                            refererMode = RefererModes.OriginWhenCrossOrigin;
                        }
                        else if (content.Equals("always", StringComparison.OrdinalIgnoreCase) || // Legacy value
                            content.Equals("unsafe-url", StringComparison.OrdinalIgnoreCase)) // Modern value
                        {
                            refererMode = RefererModes.UnsafeUrl;
                        }
                    }
                }
            }

            // Allow the anchor rel attribute to override both the browser setting and meta tag.
            if (this._navigationAttributes != null && this._navigationAttributes.AllKeys.Contains("rel") && this._navigationAttributes["rel"] == "noreferrer")
            {
                refererMode = RefererModes.None;
            }

            switch (refererMode)
            {
                case RefererModes.None:
                    {
                        req.Referer = string.Empty;
                        break;
                    }

                case RefererModes.Origin:
                    {
                        if (this.CurrentState != null && !string.IsNullOrEmpty(this.CurrentState.Uri.ToString()))
                        {
                            req.Referer = string.Format("{0}://{1}", this.CurrentState.Uri.Scheme, this.CurrentState.Uri.Host);
                        }

                        break;
                    }

                case RefererModes.OriginWhenCrossOrigin:
                    {
                        if (this.CurrentState != null && !string.IsNullOrEmpty(this.CurrentState.Uri.ToString()))
                        {
                            if (this.CurrentState.Uri.Host.Equals(url.Host, StringComparison.InvariantCultureIgnoreCase))
                            {
                                req.Referer = this.CurrentState.Uri.ToString();
                            }
                            else
                            {
                                req.Referer = string.Format("{0}://{1}", this.CurrentState.Uri.Scheme, this.CurrentState.Uri.Host);
                            }
                        }

                        break;
                    }

                case RefererModes.UnsafeUrl:
                    {
                        if (this.CurrentState != null && !string.IsNullOrEmpty(this.CurrentState.Uri.ToString()))
                        {
                            req.Referer = this.CurrentState.Uri.ToString();
                        }

                        break;
                    }

                case RefererModes.NoneWhenDowngrade:
                    {
                        if (this.CurrentState != null &&
                            !string.IsNullOrEmpty(this.CurrentState.Uri.ToString()))
                        {
                            if (this.CurrentState.Uri.Scheme == "https" && url.Scheme == "http")
                            {
                                req.Referer = string.Empty;
                            }
                            else
                            {
                                req.Referer = this.CurrentState.Uri.ToString();
                            }
                        }

                        break;
                    }
            }

            return req;
        }

        private void Register(Browser browser)
        {
            _allWindows.Add(browser);
            if (!_allContexts.Contains(_allWindows))
            {
                _allContexts.Add(_allWindows);
            }

            if (browser.WindowHandle == null)
            {
                browser.WindowHandle = Guid.NewGuid().ToString().Substring(0, 8);
            }
        }

        private static readonly List<List<Browser>> _allContexts = new List<List<Browser>>();

        #endregion private methods
    }
}
