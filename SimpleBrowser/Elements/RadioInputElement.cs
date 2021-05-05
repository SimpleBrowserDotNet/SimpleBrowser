// -----------------------------------------------------------------------
// <copyright file="RadioInputElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// Implements an input element of type radio.
    /// </summary>
    internal class RadioInputElement : SelectableInputElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RadioInputElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public RadioInputElement(XElement element)
            : base(element)
        { }

        /// <summary>
        /// Gets or sets the selected (checked) state of the radio
        /// </summary>
        public override bool Selected
        {
            get
            {
                return this.GetAttribute("checked") != null;
            }

            set
            {
                if (this.Disabled)
                {
                    return;
                }

                if (value)
                {
                    this.Element.SetAttributeValue("checked", "checked");
                    foreach (RadioInputElement other in this.Siblings)
                    {
                        if (other.Element != this.Element)
                        {
                            other.Selected = false;
                        }
                    }
                }
                else
                {
                    this.Element.RemoveAttributeCI("checked");
                }
            }
        }

        /// <summary>
        /// Gets a collection of the other radio buttons in the same group as this radio input.
        /// </summary>
        public IEnumerable<RadioInputElement> Siblings
        {
            get
            {
                IEnumerable<RadioInputElement> others = this.Element.Ancestors(XName.Get("form")).Descendants(XName.Get("input"))
                    .Where(e => e.GetAttributeCI("type") == "radio" && e.GetAttributeCI("name") == this.Name)
                    .Select(e => this.OwningBrowser.CreateHtmlElement<RadioInputElement>(e));
                return others;
            }
        }


        [Obsolete("Use Async version instead")]
        public override ClickResult Click()
        {
            return ClickAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Perform a click action on the radio input element.
        /// </summary>
        /// <returns>The <see cref="ClickResult"/> of the operation.</returns>
        public override async Task<ClickResult> ClickAsync()
        {
            if (this.Disabled)
            {
                return ClickResult.SucceededNoOp;
            }

            await base.ClickAsync();
            if (!this.Selected)
            {
                this.Selected = true;
            }

            return ClickResult.SucceededNoNavigation;
        }
    }
}