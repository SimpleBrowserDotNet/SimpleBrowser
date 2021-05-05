// -----------------------------------------------------------------------
// <copyright file="HtmlElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using SimpleBrowser.Elements;

    /// <summary>
    /// Implements an HTML element corresponding with an XElement from the DOM
    /// </summary>
    internal class HtmlElement
    {
        /// <summary>
        /// The XElement corresponding to this HTML element.
        /// </summary>
        private readonly XElement correspondingElement;

        /// <summary>
        /// Gets the XElement corresponding to this HTML element.
        /// </summary>
        public XElement XElement
        {
            get { return correspondingElement; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this instance.</param>
        public HtmlElement(XElement element)
        {
            correspondingElement = element;
        }

        /// <summary>
        /// Returns the requested attribute.
        /// </summary>
        /// <param name="name">The name of the attribute to return.</param>
        /// <returns>The <see cref="XAttribute"/> requested, null if the attribute was not found.</returns>
        protected XAttribute GetAttribute(string name)
        {
            return GetAttribute(Element, name);
        }

        /// <summary>
        /// Returns the requested attribute.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> to search for attributes.</param>
        /// <param name="name">The name of the attribute to find.</param>
        /// <returns>The <see cref="XAttribute"/> requested, null if the attribute was not found.</returns>
        protected XAttribute GetAttribute(XElement element, string name)
        {
            return element.Attributes().Where(h => h.Name.LocalName.ToLower() == name.ToLower()).FirstOrDefault();
        }

        /// <summary>
        /// Returns the requested attribute value.
        /// </summary>
        /// <param name="name">The name of the attribute to find.</param>
        /// <returns>The value requested, null if the attribute was not found.</returns>
        internal virtual string GetAttributeValue(string name)
        {
            return GetAttributeValue(Element, name);
        }

        /// <summary>
        /// Returns the requested attribute value.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> to search for attributes.</param>
        /// <param name="name">The name of the attribute to find.</param>
        /// <returns>The value requested, null if the attribute was not found.</returns>
        protected string GetAttributeValue(XElement element, string name)
        {
            return GetAttribute(element, name)?.Value;
        }

        /// <summary>
        /// Gets the tag name
        /// </summary>
        public string TagName
        {
            get { return Element.Name.LocalName; }
        }

        public bool Valid { get; private set; } = true;

        public void Invalidate()
        {
            this.Valid = false;
        }

        public class NavigationArgs
        {
            /// <summary>
            /// This can be a full Url, but also a relative url that can be combined with the current url of the Browser object
            /// </summary>
            public string Uri;

            public string Target;
            public string Method = "GET";
            public NameValueCollection UserVariables = new NameValueCollection();
            public string PostData = string.Empty;
            public string ContentType = string.Empty;
            public string EncodingType = string.Empty;
            public int TimeoutMilliseconds;

            /// <summary>
            /// Used to pass name value pairs that should NOT be passed as submitted request data (like the UserVariables member)
            /// </summary>
            public NameValueCollection NavigationAttributes = new NameValueCollection();
        }

        public class UserVariableEntry
        {
            public string Name;
            public string Value;
        }

        public event Func<NavigationArgs, Task<bool>> NavigationRequested;


        [Obsolete("Use ClickAsync instead")]
        public virtual ClickResult Click()
        {
            return ClickResult.SucceededNoOp;
        }

        [Obsolete("Use ClickAsync instead")]
        public virtual ClickResult Click(uint x, uint y)
        {
            return Click();
        }

        public virtual async Task<ClickResult> ClickAsync(uint x, uint y)
        {
            await Task.Delay(1);
            return ClickResult.SucceededNoOp;
        }

        public virtual async Task<ClickResult> ClickAsync()
        {
            await Task.Delay(1);
            return ClickResult.SucceededNoOp;
        }

       

        internal static HtmlElement CreateFor(XElement element)
        {
            HtmlElement result;
            switch (element.Name.LocalName.ToLower())
            {
                case "form":
                    result = new FormElement(element);
                    break;

                case "input":
                    string type = element.GetAttribute("type") ?? "";
                    switch (type.ToLower())
                    {
                        case "radio":
                            result = new RadioInputElement(element);
                            break;

                        case "checkbox":
                            result = new CheckboxInputElement(element);
                            break;

                        case "image":
                            result = new ImageInputElement(element);
                            break;

                        case "submit":
                        case "button":
                            result = new ButtonInputElement(element);
                            break;

                        case "file":
                            result = new FileUploadElement(element);
                            break;

                        case "email":
                            result = new EmailInputElement(element);
                            break;

                        case "url":
                            result = new UrlInputElement(element);
                            break;

                        case "datetime-local":
                        case "date":
                        case "month":
                        case "week":
                        case "time":
                            result = new DateTimeInputElement(element);
                            break;

                        case "number":
                        case "range":
                            result = new NumberInputElement(element);
                            break;

                        case "color":
                            result = new ColorInputElement(element);
                            break;

                        default:
                            result = new InputElement(element);
                            break;
                    }
                    break;

                case "textarea":
                    result = new TextAreaElement(element);
                    break;

                case "select":
                    result = new SelectElement(element);
                    break;

                case "option":
                    result = new OptionElement(element);
                    break;

                case "iframe":
                case "frame":
                    var src = element.GetAttributeCI("src");
                    if (!string.IsNullOrWhiteSpace(src))
                    {
                        result = new FrameElement(element);
                    }
                    else
                    {
                        result = default;
                    }
                    break;

                case "a":
                    result = new AnchorElement(element);
                    break;

                case "label":
                    result = new LabelElement(element);
                    break;

                case "button":
                    result = new ButtonInputElement(element);
                    break;

                default:
                    result = new HtmlElement(element);
                    break;
            }
            return result;
        }

        protected virtual async Task<bool> RequestNavigation(NavigationArgs args)
        {
            if (NavigationRequested != null)
                return await NavigationRequested(args);
            else
                return false;
        }


        [Obsolete("Use Async version instead")]
        public virtual bool SubmitForm(string url = null, HtmlElement clickedElement = null)
        {
            return SubmitFormAsync(url, clickedElement).GetAwaiter().GetResult();
        }
        public virtual async Task<bool> SubmitFormAsync(string url = null, HtmlElement clickedElement = null)
        {
            XElement formElement = null;
            if (this.Element.HasAttributeCI("form"))
            {
                formElement = this.Element.Document.Descendants().Where(e => e.HasAttributeCI("id") && e.GetAttributeCI("id").Equals(this.Element.GetAttributeCI("form"))).First();
            }
            else
            {
                formElement = this.Element.GetAncestorCI("form");
            }

            if (formElement != null)
            {
                FormElement form = this.OwningBrowser.CreateHtmlElement<FormElement>(formElement);
                return await form.SubmitFormAsync(url, clickedElement);
            }
            return false;
        }


        [Obsolete("Use Async version instead")]
        public ClickResult DoAspNetLinkPostBack()
        {
            return DoAspNetLinkPostBackAsync().GetAwaiter().GetResult();
        }

        public async Task<ClickResult> DoAspNetLinkPostBackAsync()
        {
            if (this is AnchorElement)
            {
                return await this.ClickAsync();
            }
            throw new InvalidOperationException("This method must only be called on <a> elements having a __doPostBack javascript call in the href attribute");
        }

        internal XElement Element
        {
            get { return correspondingElement; }
        }

        internal virtual Browser OwningBrowser { get; set; }
    }
}