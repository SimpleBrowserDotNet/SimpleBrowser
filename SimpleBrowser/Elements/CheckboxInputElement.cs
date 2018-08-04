// -----------------------------------------------------------------------
// <copyright file="CheckboxInputElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System.Xml.Linq;

    /// <summary>
    /// Implements an input element of type checkbox.
    /// </summary>
    internal class CheckboxInputElement : SelectableInputElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckboxInputElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public CheckboxInputElement(XElement element)
            : base(element)
        { }

        /// <summary>
        /// Gets or sets the selected (checked) state of the checkbox
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
                }
                else
                {
                    this.Element.RemoveAttributeCI("checked");
                }
            }
        }

        /// <summary>
        /// Perform a click action on the checkbox input element.
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