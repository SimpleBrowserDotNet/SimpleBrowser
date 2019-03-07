// -----------------------------------------------------------------------
// <copyright file="InputElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    /// <summary>
    /// Implements an input of types text, hidden, password, of null (unspecified) type, or an unknown or unimplemented type.
    /// </summary>
    /// <remarks>Per the HTML specification, any input of unknown or unspecified type is a considered a text input.</remarks>
    internal class InputElement : FormElementElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public InputElement(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the element is readonly.
        /// </summary>
        /// <remarks>
        /// The element is readonly if the element has a readonly attribute.
        /// </remarks>
        public bool ReadOnly
        {
            get
            {
                return this.GetAttribute ("readonly") != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the element is required.
        /// </summary>
        /// <remarks>
        /// The element is required if the element has a required attribute.
        /// </remarks>
        public bool Required
        {
            get
            {
                return this.GetAttribute ("required") != null;
            }
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

                int? maxLength = base.ParseNonNegativeIntegerAttribute("maxlength", int.MaxValue);

                // Apply maximum length validation
                // If the length of the value being assigned is too long, truncate it.
                if (value.Length > maxLength)
                {
                    this.Element.SetAttributeValue("value", value.Substring(0, maxLength.Value));
                }
                else
                {
                    this.Element.SetAttributeValue("value", value);
                }
            }
        }

        /// <summary>
        /// Gets the value of the input element type attribute.
        /// </summary>
        public string InputType
        {
            get
            {
                return this.GetAttributeValue ("type");
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
                if (validate)
                {
                    try
                    {
                        this.ValidateMinimumLength();
                        this.ValidatePattern();
                    }
                    catch
                    {
                        throw;
                    }
                }

                if (this.Name.Equals("_charset_") && string.IsNullOrEmpty(this.Value) && this.InputType.Equals("hidden", StringComparison.OrdinalIgnoreCase))
                {
                    yield return new UserVariableEntry() { Name = Name, Value = "iso-8859-1" };
                }
                else
                {
                    yield return new UserVariableEntry() { Name = Name, Value = Value };

                    XAttribute dirNameAttribute = this.GetAttribute("dirname");
                    if (dirNameAttribute != null && this.OwningBrowser.Culture != null && this.OwningBrowser.Culture.TextInfo != null)
                    {
                        yield return new UserVariableEntry() { Name = dirNameAttribute.Value, Value = this.OwningBrowser.Culture.TextInfo.IsRightToLeft ? "rtl" : "ltr" };
                    }
                }
            }

            yield break;
        }
    }
}