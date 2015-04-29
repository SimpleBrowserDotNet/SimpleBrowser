// -----------------------------------------------------------------------
// <copyright file="OptionElement.cs" company="SimpleBrowser">
// See https://github.com/axefrog/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Implements an HTML option element
    /// </summary>
    internal class OptionElement : FormElementElement
    {
        /// <summary>
        /// The parent select of this option element.
        /// </summary>
        private SelectElement owner = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public OptionElement(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Gets the option value of the option element
        /// </summary>
        public string OptionValue
        {
            get
            {
                var attr = GetAttribute("value");
                if (attr == null)
                {
                    return this.Element.Value.Trim();
                }

                return attr.Value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the value of the option element
        /// </summary>
        public override string Value
        {
            get
            {
                return this.Element.Value.Trim();
            }

            set
            {
                throw new InvalidOperationException("Cannot change the value for an option element. Set the value attibute.");
            }
        }

        /// <summary>
        /// Gets the parent select element of this option element
        /// </summary>
        public SelectElement Owner
        {
            get
            {
                if (this.owner == null)
                {
                    var selectElement = Element.Ancestors().First(e => e.Name.LocalName.ToLower() == "select");
                    this.owner = this.OwningBrowser.CreateHtmlElement<SelectElement>(selectElement);
                }

                return this.owner;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this option is selected
        /// </summary>
        public bool Selected
        {
            get
            {
                // Being selected is more complicated than it seems. If a selectbox is single-valued,
                // the first option is selected when none of the options has a selected-attribute. The
                // selected state is therefor managed at the selectbox level
                return this.Owner.IsSelected(this);
            }

            set
            {
                this.Owner.MakeSelected(this, value);
            }
        }

        /// <summary>
        /// Perform a click action on the option element.
        /// </summary>
        /// <returns>The <see cref="ClickResult"/> of the operation.</returns>
        public override ClickResult Click()
        {
            if (this.Disabled)
            {
                return ClickResult.SucceededNoOp;
            }

            base.Click();
            this.Selected = !this.Selected;
            return ClickResult.SucceededNoNavigation;
        }
    }
}
