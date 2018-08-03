// -----------------------------------------------------------------------
// <copyright file="FormElementElementValidator.cs" company="SimpleBrowser">
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A static class implementing a minimum length validator
    /// </summary>
    /// <remarks>
    /// This class defines extension methods for validating form elements when a form is submitted. This class throws a <see cref="FormElementValidationException"/>. The caller should catch and rethrow this exception.
    /// </remarks>
    internal static class FormElementElementValidator
    {
        private static DateTime htmlDateTimeMinimumValue = new DateTime(1970, 1, 1);

        /// <summary>
        /// An extension method for minimum length validation
        /// </summary>
        /// <param name="element"></param>
        internal static void ValidateMinimumLength(this FormElementElement element)
        {
            int? minLength = element.ParseNonNegativeIntegerAttribute("minlength", 0);

            if (element.Value.Length < minLength)
            {
                throw new FormElementValidationException(string.Format("Please lengthen this text to {0} characters or more (you are currently using {1} characters)", minLength, element.Value.Length));
            }
        }

        internal static void ValidatePattern(this FormElementElement element)
        {
            string pattern = element.GetAttributeValue("pattern");
            if (!string.IsNullOrWhiteSpace(pattern))
            {
                Regex expression = new Regex(pattern);
                MatchCollection matches = expression.Matches(element.Value);
                if (matches.Count == 0)
                {
                    string title = element.GetAttributeValue("title");
                    string message = string.Empty;
                    if (title != null)
                    {
                        message = string.Format("\r\n{0}", title);
                    }

                    throw new FormElementValidationException(string.Format("Please match the requested format.{0}", message));
                }
            }
        }

        internal static void ValidateMinimumDateTimeValue(this DateTimeInputElement element)
        {
            string minimumValue = element.GetAttributeValue("min");
            if (!string.IsNullOrWhiteSpace(minimumValue))
            {
                if (DateTime.TryParse(minimumValue, out DateTime minimumDateTime) == false)
                {
                    return;
                }

                if (DateTime.TryParse(element.Value, out DateTime elementDateTime) == false)
                {
                    throw new FormElementValidationException("Invalid element value.");
                }

                if (minimumDateTime > elementDateTime)
                {
                    throw new FormElementValidationException(string.Format("Value must be {0} or later.", minimumValue));
                }
            }
        }

        internal static void ValidateMaximumDateTimeValue(this DateTimeInputElement element)
        {
            string maximumValue = element.GetAttributeValue("max");
            if (!string.IsNullOrWhiteSpace(maximumValue))
            {
                if (DateTime.TryParse(maximumValue, out DateTime maximumDateTime) == false)
                {
                    return;
                }

                if (DateTime.TryParse(element.Value, out DateTime elementDateTime) == false)
                {
                    throw new FormElementValidationException("Invalid element value.");
                }

                if (elementDateTime > maximumDateTime)
                {
                    throw new FormElementValidationException(string.Format("Value must be {0} or earlier.", maximumValue));
                }
            }
        }

        internal static void ValidateMinimumNumericValue(this FormElementElement element)
        {
            string minimumValue = element.GetAttributeValue("min");
            decimal minimumNumericValue;
            try
            {
                minimumNumericValue = decimal.Parse(minimumValue);
            }
            catch
            {
                return;
            }

            decimal elementValue;
            try
            {
                elementValue = decimal.Parse(element.Value);
            }
            catch
            {
                throw new FormElementValidationException("Invalid element value.");
            }

            if (minimumNumericValue < elementValue)
            {
                throw new FormElementValidationException(string.Format("Value must be greater than or equal to {0}.", minimumValue));
            }
        }

        internal static void ValidateMaximumNumericValue(this FormElementElement element)
        {
            string maximumValue = element.GetAttributeValue("max");
            decimal maximumNumericValue;
            try
            {
                maximumNumericValue = decimal.Parse(maximumValue);
            }
            catch
            {
                return;
            }

            decimal elementValue;
            try
            {
                elementValue = decimal.Parse(element.Value);
            }
            catch
            {
                throw new FormElementValidationException("Invalid element value.");
            }

            if (elementValue > maximumNumericValue)
            {
                throw new FormElementValidationException(string.Format("Value must be less than or equal to {0}.", maximumValue));
            }
        }
    }
}