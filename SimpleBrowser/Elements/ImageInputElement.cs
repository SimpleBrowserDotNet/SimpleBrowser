// -----------------------------------------------------------------------
// <copyright file="ImageInputElement.cs" company="SimpleBrowser">
// See https://github.com/axefrog/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    /// <summary>
    /// Implements an HTML image input element
    /// </summary>
    internal class ImageInputElement : ButtonInputElement
    {
        /// <summary>
        /// The X-coordinate of the location clicked.
        /// </summary>
        private uint x = 0;

        /// <summary>
        /// The X-coordinate of the location clicked.
        /// </summary>
        private uint y = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageInputElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public ImageInputElement(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Gets the form values to submit for this input
        /// </summary>
        /// <param name="isClickedElement">True, if the action to submit the form was clicking this element. Otherwise, false.</param>
        /// <returns>A collection of <see cref="UserVariableEntry"/> objects.</returns>
        public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement)
        {
            if (isClickedElement && !this.Disabled)
            {
                if (string.IsNullOrEmpty(this.Name))
                {
                    yield return new UserVariableEntry() { Name = "x", Value = this.x.ToString() };
                    yield return new UserVariableEntry() { Name = "y", Value = this.y.ToString() };
                }
                else
                {
                    yield return new UserVariableEntry() { Name = string.Format("{0}.x", this.Name), Value = this.x.ToString() };
                    yield return new UserVariableEntry() { Name = string.Format("{0}.y", this.Name), Value = this.y.ToString() };
                    if (!string.IsNullOrEmpty(this.Value))
                    {
                        yield return new UserVariableEntry() { Name = this.Name, Value = this.Value };
                    }
                }
            }

            yield break;
        }

        /// <summary>
        /// Perform a click action on the label element.
        /// </summary>
        /// <param name="x">The x-coordinate of the location clicked</param>
        /// <param name="y">The y-coordinate of the location clicked</param>
        /// <returns>The <see cref="ClickResult"/> of the operation.</returns>
        public override ClickResult Click(uint x, uint y)
        {
            if (this.Disabled)
            {
                return ClickResult.SucceededNoOp;
            }

            this.x = x;
            this.y = y;

            if (this.SubmitForm(clickedElement: this))
            {
                return ClickResult.SucceededNavigationComplete;
            }

            return ClickResult.SucceededNavigationError;
        }
    }
}
