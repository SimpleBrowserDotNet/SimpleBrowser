// -----------------------------------------------------------------------
// <copyright file="SelectableInputElement.cs" company="SimpleBrowser">
// See https://github.com/axefrog/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    /// <summary>
    /// Implements an abstract selectable input element, not corresponding with any specific input type.
    /// </summary>
    internal abstract class SelectableInputElement : InputElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectableInputElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public SelectableInputElement(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the selectable input element is selected.
        /// </summary>
        public virtual bool Selected { get; set; }

        /// <summary>
        /// Gets the form values to submit for this input
        /// </summary>
        /// <param name="isClickedElement">True, if the action to submit the form was clicking this element. Otherwise, false.</param>
        /// <returns>A collection of <see cref="UserVariableEntry"/> objects.</returns>
        public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement)
        {
            if (this.Selected && !string.IsNullOrEmpty(this.Name) && !this.Disabled)
            {
                yield return new UserVariableEntry() { Name = this.Name, Value = this.Value };
            }

            yield break;
        }
    }
}