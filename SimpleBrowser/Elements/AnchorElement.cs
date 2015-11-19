// -----------------------------------------------------------------------
// <copyright file="AnchorElement.cs" company="SimpleBrowser">
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    /// <summary>
    /// Implements an anchor (hyperlink) HTML element
    /// </summary>
    internal class AnchorElement : HtmlElement
    {
        /// <summary>
        /// A regular expression used to recognize JavaScript post back URLs.
        /// </summary>
        private static Regex postbackRecognizer = new Regex(@"javascript\:__doPostBack\('([^\']*)\'", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public AnchorElement(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Gets the value of the $href$ attribute
        /// </summary>
        public string Href
        {
            get
            {
                return this.Element.GetAttributeCI("href");
            }
        }

        /// <summary>
        /// Gets the value of the target attribute
        /// </summary>
        public string Target
        {
            get
            {
                return this.Element.GetAttributeCI("target");
            }
        }

        /// <summary>
        /// Gets the value of the $rel$ attribute
        /// </summary>
        public string Rel
        {
            get
            {
                return this.Element.GetAttributeCI("rel");
            }
        }

        /// <summary>
        /// Perform a click action on the anchor element.
        /// </summary>
        /// <returns>The <see cref="ClickResult"/> of the operation.</returns>
        public override ClickResult Click()
        {
            base.Click();
            var match = postbackRecognizer.Match(this.Href);
            if (match.Success)
            {
                var name = match.Groups[1].Value;
                var eventTarget = this.OwningBrowser.Select("input[name=__EVENTTARGET]");

                // IIS does browser sniffing. If using the default SimpleBrowser user agent string,
                // IIS will not render the hidden __EVENTTARGET input. If, for whatever reason,
                // the __EVENTTARGET input is not present, create it.
                if (!eventTarget.Exists)
                {
                    var elt = new XElement("input");
                    elt.SetAttributeCI("type", "hidden");
                    elt.SetAttributeCI("name", "__EVENTTARGET");
                    elt.SetAttributeCI("id", "__EVENTTARGET");
                    elt.SetAttributeCI("value", name);

                    this.XElement.AddBeforeSelf(elt);
                    eventTarget = this.OwningBrowser.Select("input[name=__EVENTTARGET]");
                }

                if (!eventTarget.Exists)
                {
                    // If the element is still not found abort.
                    return ClickResult.Failed;
                }

                eventTarget.Value = name;

                if (this.SubmitForm())
                {
                    return ClickResult.SucceededNavigationComplete;
                }
                else
                {
                    return ClickResult.Failed;
                }
            }

            string url = this.Href;
            string target = this.Target;
            string queryStringValues = null;

            if ((OwningBrowser.KeyState & (KeyStateOption.Ctrl | KeyStateOption.Shift)) != KeyStateOption.None)
            {
                target = Browser.TARGET_BLANK;
            }

            if (url != null)
            {
                string[] querystring = url.Split(new[] { '?' });
                if (querystring.Length > 1)
                {
                    queryStringValues = querystring[1];
                }
            }

            NavigationArgs navArgs = new NavigationArgs()
            {
                Uri = url,
                Target = target,
                UserVariables = StringUtil.MakeCollectionFromQueryString(queryStringValues)
            };

            if (this.Rel == "noreferrer")
            {
                navArgs.NavigationAttributes.Add("rel", "noreferrer");
            }

            if (this.RequestNavigation(navArgs))
            {
                return ClickResult.SucceededNavigationComplete;
            }
            else
            {
                return ClickResult.SucceededNavigationError;
            }
        }
    }
}
