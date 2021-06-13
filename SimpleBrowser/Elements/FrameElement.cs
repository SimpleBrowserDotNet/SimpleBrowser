// -----------------------------------------------------------------------
// <copyright file="FrameElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    internal class FrameElement : HtmlElement
    {
        public FrameElement(XElement element)
            : base(element)
        {
        }

        public Browser FrameBrowser { get; private set; }

        internal override string GetAttributeValue(string name)
        {
            if (name == "SimpleBrowser.WebDriver:frameWindowHandle")
            {
                return this.FrameBrowser.WindowHandle;
            }

            return base.GetAttributeValue(name);
        }

        public string Src
        {
            get
            {
                return Regex.Replace(this.Element.GetAttributeCI("src"), @"\s+", "");
            }
        }

        public string Name
        {
            get
            {
                return this.Element.GetAttributeCI("name");
            }
        }

        internal override Browser OwningBrowser
        {
            get
            {
                return base.OwningBrowser;
            }
            set
            {
                base.OwningBrowser = value;
                this.FrameBrowser = this.OwningBrowser.CreateChildBrowser(this.Name);
                this.FrameBrowser.NavigateAsync(new Uri(this.OwningBrowser.Url, this.Src)).GetAwaiter().GetResult();
            }
        }
    }
}