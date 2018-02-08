// -----------------------------------------------------------------------
// <copyright file="CheckboxInputElement.cs" company="SimpleBrowser">
// See https://github.com/axefrog/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Collections.Generic;
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
                return GetAttribute("checked") != null;
            }

            set
            {
                if (Disabled)
                {
                    return;
                }

                if (value)
                {
                    Element.SetAttributeValue("checked", "checked");
                }
                else
                {
                    Element.RemoveAttributeCI("checked");
                }
            }
        }

        /// <summary>
        /// Perform a click action on the checkbox input element.
        /// </summary>
        /// <returns>The <see cref="ClickResult"/> of the operation.</returns>
        public override ClickResult Click()
        {
            if (Disabled)
            {
                return ClickResult.SucceededNoOp;
            }

            base.Click();
            Selected = !Selected;
            return ClickResult.SucceededNoNavigation;
        }

        /// <summary>
        /// Gets the form values to submit for this input
        /// </summary>
        /// <param name="isClickedElement">True, if the action to submit the form was clicking this element. Otherwise, false.</param>
        /// <returns>A collection of <see cref="UserVariableEntry"/> objects.</returns>
        public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement)
        {
            if (XElement.HasAttributeCI("checked") && !string.IsNullOrEmpty(Name) && !Disabled)
            {
                yield return new UserVariableEntry()
                {
                    Name = Name, Value = string.IsNullOrEmpty(Value) ? "on" : Value
                };
            }

            yield break;
        }
    }
}