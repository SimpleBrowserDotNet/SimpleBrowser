// -----------------------------------------------------------------------
// <copyright file="ColorInputElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    internal class ColorInputElement : InputElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorInputElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public ColorInputElement(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Gets or sets the value of the input element value attribute.
        /// </summary>
        public override string Value
        {
            get
            {
                XAttribute attribute = this.GetAttribute("value");
                if (attribute == null)
                {
                    return string.Empty; // no value attribute means empty string
                }

                return attribute.Value;
            }

            set
            {
                // Don't set the value of a read only or disabled input
                if (this.ReadOnly || this.Disabled)
                {
                    return;
                }

                if (Regex.Match(value, "^#(?:[0-9a-fA-F]{3}){1,2}$").Success)
                {
                    this.Element.SetAttributeValue("value", value);
                }

                return;
            }
        }

        /// <summary>
        /// Gets the form values to submit for this input
        /// </summary>
        /// <param name="isClickedElement">True, if the action to submit the form was clicking this element. Otherwise, false.</param>
        /// <returns>A collection of <see cref="UserVariableEntry"/> objects.</returns>
        public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement, bool validate)
        {
            if (!string.IsNullOrEmpty(this.Name) && !this.Disabled)
            {
                yield return new UserVariableEntry() { Name = this.Name, Value = this.Value };
            }

            yield break;
        }
    }
}