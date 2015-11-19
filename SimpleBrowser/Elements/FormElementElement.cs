// -----------------------------------------------------------------------
// <copyright file="FormElementElement.cs" company="SimpleBrowser">
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// An abstract base class implementing a form input element.
    /// </summary>
    internal abstract class FormElementElement : HtmlElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormElementElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public FormElementElement(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Gets the form associated with this form element. Null, if no associated form could be found.
        /// </summary>
        public HtmlElement OwningForm
        {
            get
            {
                // Look for the form attribute first.
                if (this.Element.HasAttributeCI("form"))
                {
                    XElement formAttribute = this.Element.Document.Descendants("form").Where(e => e.HasAttributeCI("id") && e.GetAttributeCI("id").Equals(this.Element.GetAttributeCI("form"))).FirstOrDefault();
                    if (formAttribute != null)
                    {
                        return this.OwningBrowser.CreateHtmlElement(formAttribute);
                    }
                }

                // Look for a parent form element.
                XElement formElement = this.Element.Ancestors("form").FirstOrDefault();
                if (formElement != null)
                {
                    return this.OwningBrowser.CreateHtmlElement(formElement);
                }

                // Look for a form preceeding this element.
                XElement previousElement = this.Element.ElementsBeforeSelf("form").LastOrDefault();
                if (previousElement != null)
                {
                    return this.OwningBrowser.CreateHtmlElement(previousElement);
                }

                /// Null, if not found. In which case, this input never submits values with the form.
                return null;
            }
        }

        /// <summary>
        /// Gets the value of the name attribute
        /// </summary>
        public string Name
        {
            get
            {
                var attr = GetAttribute("name");
                if (attr == null)
                {
                    return null;
                }

                return attr.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the element is disabled.
        /// </summary>
        /// <remarks>
        /// The element is disabled if the element has a disabled attribute set to any value other than empty string.
        /// </remarks>
        public bool Disabled
        {
            get
            {
                return this.GetAttribute("disabled") != null;
            }
        }

        /// <summary>
        /// Gets or sets the value of the value attribute
        /// </summary>
        public virtual string Value
        {
            get
            {
                return this.Element.Value;
            }

            set
            {
                this.Element.Value = value;
            }
        }

        /// <summary>
        /// Returns the values to send with a form submission for this form element
        /// </summary>
        /// <param name="isClickedElement">A value indicating whether the clicking of this element caused the form submission.</param>
        /// <returns>An empty collection of <see cref="UserVariableEntry"/></returns>
        public virtual IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement)
        {
            return new UserVariableEntry[0];
        }
    }
}
