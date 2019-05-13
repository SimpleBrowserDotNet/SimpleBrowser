// -----------------------------------------------------------------------
// <copyright file="UrlInputElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    internal class UrlInputElement : InputElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlInputElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public UrlInputElement(XElement element)
        : base(element)
        {
        }

        /// <summary>
        /// Gets the form values to submit for this input
        /// </summary>
        /// <param name="isClickedElement">True, if the action to submit the form was clicking this element. Otherwise, false.</param>
        /// <returns>A collection of <see cref="UserVariableEntry"/> objects.</returns>
        public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement, bool validate)
        {
            if (validate)
            {
                if (this.IsValidUrl(this.Value, base.Required) == false)
                {
                    throw new FormElementValidationException(string.Format("{0} is an invalid URL.", this.Value));
                }

                try
                {
                    // Apply minimum length validation
                    this.ValidateMinimumLength();

                    // Apply pattern validation
                    this.ValidatePattern();
                }
                catch
                {
                    throw;
                }
            }

            if (!string.IsNullOrEmpty(this.Name) && !this.Disabled)
            {
                yield return new UserVariableEntry() { Name = Name, Value = Value };
            }

            yield break;
        }

        /// <summary>
        /// Validates a string as a URL.
        /// </summary>
        /// <param name="url">The URL to validate</param>
        /// <returns>True if the string is a valid URL. Otherwise, returns false.</returns>
        private bool IsValidUrl(string url, bool required)
        {
            if (required == false && string.IsNullOrWhiteSpace(url))
            {
                return true;
            }

            try
            {
                new Uri(url);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}