// -----------------------------------------------------------------------
// <copyright file="DateTimeInputElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml.Linq;

    /// <summary>
    /// A class implementing the datetime-local, date, month, week, and time input element types.
    /// </summary>
    /// <remarks>
    /// All of these input element types involve dates and/or times. While they could have been
    /// implemented as their own input element classes, for simplicity, they have been implemented
    /// as a single input element class. If there is a need to break out this class into individual
    /// classes, please do.
    /// -
    /// For the purposes of SimpleBrowser, to set the value of any of these input types, set the
    /// value of the input to a date time string that is able to be parsed by System.DateTime.TryParse.
    /// </remarks>
    internal class DateTimeInputElement : InputElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeInputElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public DateTimeInputElement(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// The minimum date time value as defined by the HTML5.2 specification.
        /// </summary>
        private static DateTime htmlDateTimeMinimumValue = new DateTime(1970, 1, 1);

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

                if (DateTime.TryParse(value, out DateTime datetime))
                {
                    this.Element.SetAttributeValue("value", value);
                }
            }
        }

        /// <summary>
        /// Gets the form values to submit for this input
        /// </summary>
        /// <param name="isClickedElement">True, if the action to submit the form was clicking this element. Otherwise, false.</param>
        /// <returns>A collection of <see cref="UserVariableEntry"/> objects.</returns>
        public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement, bool validate)
        {
            string pattern = string.Empty;
            string message = string.Empty;

            if (this.InputType.ToLowerInvariant() == "datetime-local")
            {
                message = "{0} is an invalid date time.";
                pattern = "yyyy-MM-ddTHH:mm";
            }
            else if (this.InputType.ToLowerInvariant() == "date")
            {
                message = "{0} is an invalid date.";
                pattern = "yyyy-MM-dd";
            }
            else if (this.InputType.ToLowerInvariant() == "time")
            {
                message = "{0} is an invalid time.";
                pattern = "HH:mm";
            }
            else if (this.InputType.ToLowerInvariant() == "month")
            {
                message = "{0} is an invalid month.";
                pattern = "yyyy-MM";
            }
            else if (this.InputType.ToLowerInvariant() == "week")
            {
                message = "{0} is an invalid week.";
                pattern = "yyyy-W{0}";
            }

            if (validate)
            {
                if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(pattern))
                {
                    throw new FormElementValidationException(string.Format("Unknown element type: {0}", this.InputType));
                }

                if (this.IsValidDateTime(this.Value, base.Required) == false)
                {
                    throw new FormElementValidationException(string.Format(message, this.Value));
                }

                try
                {
                    // Apply min value validation
                    this.ValidateMinimumDateTimeValue();

                    // Apply min value validation
                    this.ValidateMaximumDateTimeValue();

                    // Apply pattern validation
                    this.ValidatePattern();

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
                DateTime.TryParse(this.Value, out DateTime dateTime);
                if (this.InputType.ToLowerInvariant() == "week")
                {
                    pattern = string.Format(pattern, this.GetWeekOfYear(dateTime));
                }

                if (string.IsNullOrWhiteSpace(this.Value))
                {
                    yield return new UserVariableEntry() { Name = Name, Value = string.Empty };
                }
                else
                {
                    yield return new UserVariableEntry() { Name = Name, Value = dateTime.ToString(pattern) };
                }
            }

            yield break;
        }

        /// <summary>
        /// Validates a string as a date time value.
        /// </summary>
        /// <param name="value">The datetime to validate</param>
        /// <returns>True if the string is a valid date time value. Otherwise, returns false.</returns>
        private bool IsValidDateTime(string value, bool required)
        {
            if (required == false && string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            try
            {
                return DateTime.TryParse(value, out DateTime dateTime);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Calculates the week of the year based on the browser's culture.
        /// </summary>
        /// <param name="dateTime">The date time to calculate</param>
        /// <returns>The week of the year</returns>
        private int GetWeekOfYear(DateTime dateTime)
        {
            return this.OwningBrowser.Culture.Calendar.GetWeekOfYear(
                dateTime,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }

        /// <summary>
        /// Validates that any value entered in this element is a value that corresponds to the defined step.
        /// </summary>
        /// <remarks>
        /// This method throws a <see cref="FormElementValidationException"></see> to indicate a validation failure.
        /// That is, if no exception is thrown, the validation succeeds.
        /// </remarks>
        private void ValidateStep()
        {
            string stepAttributeValue = this.GetAttributeValue("step");
            if (string.IsNullOrWhiteSpace(stepAttributeValue))
            {
                return;
            }

            //  "... if the attribute’s value is an ASCII case-insensitive match for the string "any", then there is no allowed value step."
            if (stepAttributeValue.CaseInsensitiveCompare("any"))
            {
                return;
            }

            // "... let step value be the result of running the rules for parsing floating-point number values, when they are applied to the step attribute’s value."
            // A null return value indicates a parsing error to be handled by each input type below.
            decimal? stepValue = this.ParseFloatingPointAttribute("step", null);

            string inputType = this.GetAttributeValue("type");
            try
            {
                if (inputType.CaseInsensitiveCompare("datetime-local") ||
                    inputType.CaseInsensitiveCompare("time"))
                {
                    this.ValidateDateTimeStep(stepValue);
                }
                else if (inputType.CaseInsensitiveCompare("date"))
                {
                    this.ValidateDateStep(stepValue);
                }
                else if (inputType.CaseInsensitiveCompare("month"))
                {
                    this.ValidateMonthStep(stepValue);
                }
                else if (inputType.CaseInsensitiveCompare("week"))
                {
                    this.ValidateWeekStep(stepValue);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// For any type that has a millisecond step scale factor, this method validates that the date time
        /// value entered corresponds to the defined step.
        /// </summary>
        /// <param name="stepValue">The value of the step attribute</param>
        /// <param name="stepScaleFactor">The step scale factor</param>
        /// <returns></returns>
        private ValidationResult ValidateStepValue(decimal stepValue, decimal stepScaleFactor)
        {
            // The allowed value step is step value multiplied by the step scale factor.
            long allowedStepValue = (long)(stepValue * stepScaleFactor);

            if (string.IsNullOrWhiteSpace(this.Value))
            {
                return new ValidationResult()
                {
                    Success = true
                };
            }

            if (DateTime.TryParse(this.Value, out DateTime value) == false)
            {
                throw new FormElementValidationException("Invalid element value.");
            }

            TimeSpan span;
            string min = this.GetAttributeValue("min");
            if (string.IsNullOrWhiteSpace(min) == false && DateTime.TryParse(min, out DateTime minDate))
            {
                span = value - minDate;
            }
            else
            {
                span = value - htmlDateTimeMinimumValue;
            }

            double differenceFromAllowedStepValue = span.TotalMilliseconds % allowedStepValue;

            return new ValidationResult()
            {
                AllowedStepValue = allowedStepValue,
                AllowedStepValueOffset = differenceFromAllowedStepValue,
                Success = differenceFromAllowedStepValue == 0,
                Value = value
            };
        }

        /// <summary>
        /// Validates a date time with a step.
        /// </summary>
        /// <param name="stepValue">The value of the step attribute</param>
        private void ValidateDateTimeStep(decimal? stepValue)
        {
            decimal stepScaleFactor = 1000; // milliseconds in 1 second
            if (stepValue.HasValue == false || stepValue.Value <= 0)
            {
                stepValue = 60; // 60 seconds
            }

            ValidationResult result = this.ValidateStepValue(stepValue.Value, stepScaleFactor);
            if (result.Success == false)
            {
                DateTime previousDate = result.Value - new TimeSpan(0, 0, 0, 0, (int)result.AllowedStepValueOffset);
                DateTime nextDate = previousDate + new TimeSpan(0, 0, 0, 0, (int)result.AllowedStepValue);
                throw new FormElementValidationException(string.Format("Please enter a valid value. The two nearest valid values are {0} and {1}.", previousDate.ToString("MM/dd/yyyy hh:mm:ss.fff tt"), nextDate.ToString("MM/dd/yyyy hh:mm:ss.fff tt")));
            }
        }

        /// <summary>
        /// Validates a date with a step.
        /// </summary>
        /// <param name="stepValue">The value of the step attribute</param>.\
        private void ValidateDateStep(decimal? stepValue)
        {
            decimal stepScaleFactor = 86400000; // milliseconds in one day
            if (stepValue.HasValue == false || stepValue.Value <= 0)
            {
                stepValue = 1; // 1 day
            }

            ValidationResult result = this.ValidateStepValue(stepValue.Value, stepScaleFactor);
            if (result.Success == false)
            {
                DateTime previousDate = result.Value - new TimeSpan((int)(result.AllowedStepValueOffset / (double)stepScaleFactor), 0, 0, 0);
                DateTime nextDate = previousDate + new TimeSpan((int)(result.AllowedStepValue / stepScaleFactor), 0, 0, 0);
                throw new FormElementValidationException(string.Format("Please enter a valid value. The two nearest valid values are {0} and {1}.", previousDate.ToString("MM/dd/yyyy"), nextDate.ToString("MM/dd/yyyy")));
            }
        }

        /// <summary>
        /// Validates a month with a step.
        /// </summary>
        /// <param name="stepValue">The value of the step attribute</param>
        private void ValidateMonthStep(decimal? stepValue)
        {
            if (string.IsNullOrWhiteSpace(this.Value))
            {
                return;
            }

            decimal stepScaleFactor = 1; // one month
            if (stepValue.HasValue == false || stepValue.Value <= 0)
            {
                stepValue = 1; // one month
            }

            // The allowed value step is step value multiplied by the step scale factor.
            long allowedStepValue = (long)(stepValue * stepScaleFactor);

            if (DateTime.TryParse(this.Value, out DateTime value) == false)
            {
                throw new FormElementValidationException("Invalid element value.");
            }

            DateTime valueDateTime = new DateTime(value.Year, value.Month, 1);

            string min = this.GetAttributeValue("min");
            if (string.IsNullOrWhiteSpace(min) == true || DateTime.TryParse(min, out DateTime minimumDate) == false)
            {
                minimumDate = htmlDateTimeMinimumValue;
            }

            int months = ((valueDateTime.Year - minimumDate.Year) * 12) + valueDateTime.Month - minimumDate.Month;

            double differenceFromAllowedStepValue = months % allowedStepValue;
            if (differenceFromAllowedStepValue != 0)
            {
                DateTime previousDate = valueDateTime.AddMonths(-(int)differenceFromAllowedStepValue);
                DateTime nextDate = valueDateTime.AddMonths((int)(((double)stepValue.Value) - differenceFromAllowedStepValue));
                throw new FormElementValidationException(string.Format("Please enter a valid value. The two nearest valid values are {0} and {1}.", previousDate.ToString("MMMM yyyy"), nextDate.ToString("MMMM yyyy")));
            }
        }

        /// <summary>
        /// Validates a date time with a step.
        /// </summary>
        /// <param name="stepValue">The value of the step attribute</param>
        private void ValidateWeekStep(decimal? stepValue)
        {
            if (string.IsNullOrWhiteSpace(this.Value))
            {
                return;
            }

            decimal stepScaleFactor = 1; // one week
            if (stepValue.HasValue == false || stepValue.Value <= 0)
            {
                stepValue = 1; // one month
            }

            // The allowed value step is step value multiplied by the step scale factor.
            long allowedStepValue = (long)(stepValue * stepScaleFactor);

            if (DateTime.TryParse(this.Value, out DateTime value) == false)
            {
                throw new FormElementValidationException("Invalid element value.");
            }

            string min = this.GetAttributeValue("min");
            if (string.IsNullOrWhiteSpace(min) == true || DateTime.TryParse(min, out DateTime minimumDate) == false)
            {
                minimumDate = htmlDateTimeMinimumValue;
            }

            int numberOfWeeksInRange = 0;
            GregorianCalendar cal = new GregorianCalendar();
            for (int year = minimumDate.Year; year <= value.Year; year++)
            {
                int startWeekNumber = 0;
                int endWeekNumber = 0;

                if (year == minimumDate.Year)
                {
                    // start date through the end of the year
                    startWeekNumber = cal.GetWeekOfYear(minimumDate,
                        CalendarWeekRule.FirstDay, DayOfWeek.Thursday);

                    endWeekNumber = cal.GetWeekOfYear((
                                           new DateTime(year + 1, 1, 1).AddDays(-1)),
                        CalendarWeekRule.FirstDay, DayOfWeek.Thursday);
                }
                else if (year == value.Year)
                {
                    // start of the given year through the end date
                    startWeekNumber = cal.GetWeekOfYear((new DateTime(year, 1, 1)),
                        CalendarWeekRule.FirstDay, DayOfWeek.Thursday);

                    endWeekNumber = cal.GetWeekOfYear(value,
                        CalendarWeekRule.FirstDay, DayOfWeek.Thursday);
                }
                else
                {
                    // calculate the number of weeks in a full year
                    startWeekNumber = cal.GetWeekOfYear(new DateTime(year, 1, 1),
                        CalendarWeekRule.FirstDay, DayOfWeek.Thursday);

                    endWeekNumber = cal.GetWeekOfYear((
                                            new DateTime(year + 1, 1, 1).AddDays(-1)),
                        CalendarWeekRule.FirstDay, DayOfWeek.Thursday);
                }

                numberOfWeeksInRange += endWeekNumber - startWeekNumber;
            }

            double differenceFromAllowedStepValue = numberOfWeeksInRange % allowedStepValue;
            if (differenceFromAllowedStepValue != 0)
            {
                DateTime previousDate = value.AddDays((-(int)differenceFromAllowedStepValue) * 7);
                DateTime nextDate = value.AddDays(((int)(((double)stepValue.Value) - differenceFromAllowedStepValue)) * 7);
                throw new FormElementValidationException(string.Format("Please enter a valid value. The two nearest valid values are {0} and {1}.", previousDate.ToString("MMMM yyyy"), nextDate.ToString("MMMM yyyy")));
            }
        }

        /// <summary>
        /// Implements a validation result
        /// </summary>
        private class ValidationResult
        {
            /// <summary>
            /// Gets or sets a value indicating success
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// Gets or sets the allowed step value
            /// </summary>
            public long AllowedStepValue { get; set; }

            /// <summary>
            /// Gets or sets the offset from the allowed step value
            /// </summary>
            public double AllowedStepValueOffset { get; set; }

            /// <summary>
            /// Gets or sets the validated date time value
            /// </summary>
            public DateTime Value { get; set; }
        }
    }
}