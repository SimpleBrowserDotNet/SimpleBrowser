// -----------------------------------------------------------------------
// <copyright file="EmailElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Xml.Linq;

    internal class EmailInputElement : InputElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailInputElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public EmailInputElement(XElement element)
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
            // Is the multiple attribute present?
            XAttribute attribute = base.GetAttribute("multiple");
            if (attribute == null)
            {
                if (validate && this.IsValidEmail(this.Value, base.Required) == false)
                {
                    throw new FormElementValidationException(string.Format("{0} is an invalid e-mail address.", this.Value));
                }
            }
            else
            {
                string[] addresses = this.Value.Split(',');
                foreach (string address in addresses)
                {
                    if (validate && this.IsValidEmail(address, base.Required) == false)
                    {
                        throw new FormElementValidationException(string.Format("{0} is an invalid e-mail address.", address));
                    }
                }
            }

            if (validate)
            {
                try
                {
                    // Apply minimum length validation
                    this.ValidateMinimumLength();
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
        /// Validates a string as an e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address to validate</param>
        /// <returns>True if the string is a valid e-mail address. Otherwise, returns false.</returns>
        private bool IsValidEmail(string email, bool required)
        {
            if (required == false && string.IsNullOrWhiteSpace(email))
            {
                return true;
            }

            try
            {
                MailAddress addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}