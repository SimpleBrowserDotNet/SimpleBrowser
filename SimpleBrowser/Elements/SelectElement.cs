// -----------------------------------------------------------------------
// <copyright file="SelectElement.cs" company="SimpleBrowser">
// See https://github.com/axefrog/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Implements an HTML select element
    /// </summary>
    internal class SelectElement : FormElementElement
    {
        /// <summary>
        /// A collection of <see cref="OptionElement"/> objects representing the selectable options of this select.
        /// </summary>
        private IEnumerable<OptionElement> options = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public SelectElement(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Gets or sets the value of the select element value attribute.
        /// </summary>
        public override string Value
        {
            get
            {
                var optionElement = this.Options.Where(d => d.Selected).FirstOrDefault() ?? this.Options.FirstOrDefault();
                if (optionElement == null)
                {
                    return null;
                }

                return optionElement.OptionValue;
            }

            set
            {
                // Don't set the value of a disabled select
                if (this.Disabled)
                {
                    return;
                }

                // todo: use Options and OptionValue
                foreach (XElement x in Element.Descendants("option"))
                {
                    var attr = GetAttribute(x, "value");
                    string val = attr == null ? x.Value.Trim() : attr.Value.Trim();
                    x.SetAttributeValue("selected", val == value.Trim() ? "selected" : null);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the select allows multiple selections.
        /// </summary>
        public bool MultiValued
        {
            get
            {
                var attribute = GetAttribute("multiple");
                if (attribute == null || string.IsNullOrWhiteSpace(attribute.Value))
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="OptionElement"/> objects, the options in the select.
        /// </summary>
        public IEnumerable<OptionElement> Options
        {
            get
            {
                if (this.options == null)
                {
                    var optionElements = Element.Descendants()
                        .Where(e => e.Name.LocalName.ToLower() == "option")
                        .Select(e => this.OwningBrowser.CreateHtmlElement<OptionElement>(e));
                    this.options = optionElements;
                }

                return this.options;
            }
        }

        /// <summary>
        /// Returns the values to send with a form submission for this form element
        /// </summary>
        /// <param name="isClickedElement">A value indicating whether the clicking of this element caused the form submission.</param>
        /// <returns>A collection of <see cref="UserVariableEntry"/></returns>
        public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement)
        {
            if (!string.IsNullOrEmpty(this.Name) && !this.Disabled)
            {
                foreach (var item in this.Options)
                {
                    if (item.Selected)
                    {
                        yield return new UserVariableEntry() { Name = this.Name, Value = item.OptionValue };
                    }
                }
            }

            yield break;
        }

        /// <summary>
        /// Determines if the option element is selected.
        /// </summary>
        /// <param name="optionElement">The <see cref="OptionElement"/> to test.</param>
        /// <returns>True if the option is selected, otherwise false.</returns>
        internal bool IsSelected(OptionElement optionElement)
        {
            if (this.MultiValued || this.Options.Any(o => o.GetAttributeValue("selected") != null))
            {
                return optionElement.GetAttributeValue("selected") != null;
            }
            else
            {
                return optionElement.Element == this.Options.First().Element;
            }
        }

        /// <summary>
        /// Selects or unselects the option in the select
        /// </summary>
        /// <param name="optionElement">The <see cref="OptionElement"/> to select.</param>
        /// <param name="selected">The selected state of the option. True to select, false to unselect.</param>
        internal void MakeSelected(OptionElement optionElement, bool selected)
        {
            if (!selected)
            {
                optionElement.Element.RemoveAttributeCI("selected");
            }
            else
            {
                optionElement.Element.SetAttributeValue(XName.Get("selected"), "selected");
                if (!this.MultiValued)
                {
                    foreach (var option in this.Options)
                    {
                        if (option.Element != optionElement.Element)
                        {
                            option.Element.RemoveAttributeCI("selected");
                        }
                    }
                }
            }
        }
    }
}
