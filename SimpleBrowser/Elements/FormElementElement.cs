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
        { }

        /// <summary>
        /// Gets the form associated with this form element. Null, if no associated form could be found.
        /// </summary>
        public HtmlElement OwningForm
        {
            get
            {
                // Look for the form attribute first.
                if (Element.HasAttributeCI("form"))
                {
                    XElement formAttribute = Element.Document.Descendants("form").Where(e => e.HasAttributeCI("id") && e.GetAttributeCI("id").Equals(Element.GetAttributeCI("form"))).FirstOrDefault();
                    if (formAttribute != null)
                    {
                        return OwningBrowser.CreateHtmlElement(formAttribute);
                    }
                }

                // Look for a parent form element.
                XElement formElement = Element.Ancestors("form").FirstOrDefault();
                if (formElement != null)
                {
                    return OwningBrowser.CreateHtmlElement(formElement);
                }

                // Look for a form preceeding this element.
                XElement previousElement = Element.ElementsBeforeSelf("form").LastOrDefault();
                if (previousElement != null)
                {
                    return OwningBrowser.CreateHtmlElement(previousElement);
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
            get => GetAttribute("name")?.Value;
        }

        /// <summary>
        /// Gets a value indicating whether the element is disabled.
        /// </summary>
        /// <remarks>
        /// The element is disabled if the element has a disabled attribute set to any value other than empty string.
        /// </remarks>
        public bool Disabled
        {
            get => GetAttribute("disabled") != null;
        }

        /// <summary>
        /// Gets or sets the value of the value attribute
        /// </summary>
        public virtual string Value
        {
            get => Element.Value;
            set => Element.Value = value;
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