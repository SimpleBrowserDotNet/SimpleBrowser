// -----------------------------------------------------------------------
// <copyright file="TextAreaElement.cs" company="SimpleBrowser">
// See https://github.com/axefrog/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    /// <summary>
    /// Implements a text area HTML element
    /// </summary>
    internal class TextAreaElement : FormElementElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextAreaElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public TextAreaElement(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the element is readonly.
        /// </summary>
        /// <remarks>
        /// The element is readonly if the element has a readonly attribute set to any value other than empty string.
        /// </remarks>
        public bool ReadOnly
        {
            get
            {
                return this.GetAttribute("readonly") != null;
            }
        }

        /// <summary>
        /// Gets or sets the value of the input element value attribute.
        /// </summary>
        public override string Value
        {
            get
            {
                return base.Value;
            }

            set
            {
                // Don't set the value of a read only or disabled text area
                if (this.ReadOnly || this.Disabled)
                {
                    return;
                }

                int maxLength = int.MaxValue;
                if (Element.HasAttributeCI("maxlength"))
                {
                    string maxLengthStr = Element.GetAttributeCI("maxlength");
                    try
                    {
                        int length = Convert.ToInt32(maxLengthStr);
                        if (length >= 0)
                        {
                            maxLength = length;
                        }
                        //// Do nothing (implicitly) if the value of maxlength is negative, per the HTML5 spec.
                    }
                    catch
                    {
                        //// Do nothing if the value of the maxlength is not a valid integer value, per the HTML5 spec.
                    }
                }

                Element.RemoveNodes();

                // If the length of the value being assigned is too long, truncate it.
                if (value.Length > maxLength)
                {
                    Element.AddFirst(value.Substring(0, maxLength));
                }
                else
                {
                    Element.SetAttributeValue("value", value);
                    Element.AddFirst(value);
                }
            }
        }

        /// <summary>
        /// Gets the form values to submit for this input
        /// </summary>
        /// <param name="isClickedElement">True, if the action to submit the form was clicking this element. Otherwise, false.</param>
        /// <returns>A collection of <see cref="UserVariableEntry"/> objects.</returns>
        public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement)
        {
            if (!string.IsNullOrEmpty(this.Name) && !this.Disabled)
            {
                yield return new UserVariableEntry() { Name = this.Name, Value = this.Value };
            }

            yield break;
        }
    }
}
