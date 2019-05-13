// -----------------------------------------------------------------------
// <copyright file="FormElementValidationException.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;

    /// <summary>
    /// Implements a custom exception indicating a form valudation error that interrupts a form submission.
    /// </summary>
    public class FormElementValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormElementValidationException"/> class.
        /// </summary>
        public FormElementValidationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormElementValidationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FormElementValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormElementValidationException"/> with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public FormElementValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}