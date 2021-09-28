// -----------------------------------------------------------------------
// <copyright file="ImageInputElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
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
        { }

        /// <summary>
        /// Gets the form values to submit for this input
        /// </summary>
        /// <param name="isClickedElement">True, if the action to submit the form was clicking this element. Otherwise, false.</param>
        /// <returns>A collection of <see cref="UserVariableEntry"/> objects.</returns>
        public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement, bool validate)
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
                        yield return new UserVariableEntry() { Name = Name, Value = Value };
                    }
                }
            }

            yield break;
        }

        [Obsolete("Use ClickAsync instead")]
        public override ClickResult Click(uint x, uint y)
        {
            return ClickAsync(x, y).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Perform a click action on the label element.
        /// </summary>
        /// <param name="x">The x-coordinate of the location clicked</param>
        /// <param name="y">The y-coordinate of the location clicked</param>
        /// <returns>The <see cref="ClickResult"/> of the operation.</returns>
        public override async Task<ClickResult> ClickAsync(uint x, uint y)
        {
            if (this.Disabled)
            {
                return ClickResult.SucceededNoOp;
            }

            this.x = x;
            this.y = y;

            await base.ClickAsync();
            if (await this.SubmitFormAsync(clickedElement: this))
            {
                return ClickResult.SucceededNavigationComplete;
            }

            return ClickResult.SucceededNavigationError;
        }
    }
}