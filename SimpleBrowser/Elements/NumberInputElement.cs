// -----------------------------------------------------------------------
// <copyright file="NumberInputlElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    internal class NumberInputElement : InputElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberInputElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public NumberInputElement(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Gets or sets the value of the input element value attribute.
        /// </summary>
        public override string Value
        {
            get
            {
                XAttribute attribute = this.GetAttribute("value");
                if (attribute == null)
                {
                    return string.Empty; // no value attribute means empty string
                }

                return attribute.Value;
            }

            set
            {
                // Don't set the value of a read only or disabled input
                if (this.ReadOnly || this.Disabled)
                {
                    return;
                }

                if ((value.Contains('e') || value.Contains('E')) && decimal.TryParse(value, NumberStyles.AllowExponent, this.OwningBrowser.Culture, out decimal exponentDecimalValue))
                {
                    this.Element.SetAttributeValue("value", exponentDecimalValue.ToString(CultureInfo.InvariantCulture));
                }
                else if (decimal.TryParse(value, 
					NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, 
					CultureInfo.InvariantCulture, out decimal decimalValue))
                {
                    this.Element.SetAttributeValue("value", decimalValue.ToString(CultureInfo.InvariantCulture));
                }

                return;
            }
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
                if (string.IsNullOrWhiteSpace(this.Value) == false && decimal.TryParse(this.Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal decimalValue) == false)
                {
                    throw new FormElementValidationException(string.Format("{0} is an invalid number.", this.Value));
                }

                try
                {
                    // Apply minimum length validation
                    this.ValidateMinimumValue();

                    // Apply minimum length validation
                    this.ValidateMaximumValue();

                    // Apply step validation
                    this.ValidateStep();
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

        private void ValidateMinimumValue()
        {
            string minimum = this.GetAttributeValue("min");
            if (string.IsNullOrWhiteSpace(minimum) == true || double.TryParse(minimum, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double minimumValue) == false)
            {
                return;
            }

            if (double.TryParse(this.Value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double decimalValue))
            {
                if (minimumValue > decimalValue)
                {
                    throw new FormElementValidationException(string.Format("Value must be greater than or equal to {0}.", minimum));
                }
            }
        }

        private void ValidateMaximumValue()
        {
            string maximum = this.GetAttributeValue("max");
            if (string.IsNullOrWhiteSpace(maximum) == true || decimal.TryParse(maximum, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal maximumValue) == false)
            {
                return;
            }

            if (decimal.TryParse(this.Value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal decimalValue))
            {
                if (decimalValue > maximumValue)
                {
                    throw new FormElementValidationException(string.Format("Value must be less than or equal to {0}.", maximum));
                }
            }
        }

        private void ValidateStep()
        {
            if (string.IsNullOrWhiteSpace(this.Value))
            {
                return;
            }

            string stepAttributeValue = this.GetAttributeValue("step");
            if (string.IsNullOrWhiteSpace(stepAttributeValue))
            {
                return;
            }

            if (decimal.TryParse(this.Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal value) == false)
            {
                throw new FormElementValidationException("Invalid element value.");
            }

            decimal? stepValue = this.ParseFloatingPointAttribute("step", null);
            decimal stepScaleFactor = 1; // "The step scale factor is 1"
            if (stepValue.HasValue == false || stepValue.Value <= 0)
            {
                stepValue = 1; // "The default step is 1 (allowing only integers to be selected by the user, unless the step base has a non-integer value)."
            }

            // The allowed value step is step value multiplied by the step scale factor.
            decimal allowedStepValue = stepValue.Value * stepScaleFactor;

            decimal stepBase = 1;
            string min = this.GetAttributeValue("min");
            if (string.IsNullOrWhiteSpace(min) == false && decimal.TryParse(min, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal minimumValue))
            {
                stepBase = minimumValue;
            }

            decimal allowedStepValueOffset = (value - stepBase) % allowedStepValue;

            if (allowedStepValueOffset != 0)
            {
                decimal previousValue = value - allowedStepValueOffset;
                decimal nextValue = previousValue + allowedStepValue;
                throw new FormElementValidationException(string.Format("Please enter a valid value. The two nearest valid values are {0} and {1}.", previousValue, nextValue));
            }
        }
    }
}