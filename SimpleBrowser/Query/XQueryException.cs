// -----------------------------------------------------------------------
// <copyright file="XQueryException.cs" company="SimpleBrowser">
// See https://github.com/axefrog/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Query
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Implements an XQuery exception
    /// </summary>
    [Serializable]
    public class XQueryException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XQueryException"/> class.
        /// </summary>
        /// <param name="message">A descriptive error message</param>
        /// <param name="query">The query causing the exception to be thrown</param>
        /// <param name="index">The location of the error in the query</param>
        /// <param name="length">The length of the query</param>
        public XQueryException(string message, string query, int index, int length)
            : base(message)
        {
            this.Query = query;
            if (index >= query.Length)
            {
                index = query.Length - 1;
            }

            this.Index = index;
            if (index + length >= query.Length)
            {
                length = query.Length - index;
            }

            this.Length = length;
        }

        /// <summary>
        /// Gets or sets the query causing the exception to be thrown
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the index of the error in the query
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the length of the query
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> to populate/></param>
        /// <param name="context">The <see cref="StreamingContext"/> of the <see cref="SerializationInfo"/></param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Query", this.Query);
            info.AddValue("Index", this.Index);
            info.AddValue("Length", this.Length);
        }
    }
}
