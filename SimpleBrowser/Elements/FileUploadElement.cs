// -----------------------------------------------------------------------
// <copyright file="FileUploadElement.cs" company="SimpleBrowser">
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml.Linq;
    using SimpleBrowser.Internal;

    /// <summary>
    /// Implements an HTML file upload element
    /// </summary>
    internal class FileUploadElement : InputElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploadElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public FileUploadElement(XElement element)
            : base(element)
        {
        }

        /// <summary>
        /// Returns the values to send with a form submission for this form element
        /// </summary>
        /// <param name="isClickedElement">A value indicating whether the clicking of this element caused the form submission.</param>
        /// <returns>An empty collection of <see cref="UserVariableEntry"/></returns>
        public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement)
        {
            string filename = string.Empty;
            string extension = string.Empty;
            string contentType = string.Empty;

            if (File.Exists(this.Value))
            {
                // Todo: create a mime type for extensions
                filename = this.Value;
                byte[] allBytes = allBytes = File.ReadAllBytes(filename);

                FileInfo fileInfo = new FileInfo(filename);
                extension = fileInfo.Extension;
                filename = fileInfo.Name;

                contentType = string.Format(
                    "Content-Type: {0}\r\nContent-Transfer-Encoding: binary\r\n\r\n{1}",
                    ApacheMimeTypes.MimeForExtension(extension),
                    Encoding.GetEncoding(28591).GetString(allBytes));
            }
            else
            {
                contentType = string.Format(
                    "Content-Type: {0}\r\n\r\n\r\n",
                    ApacheMimeTypes.MimeForExtension(extension));
            }

            yield return new UserVariableEntry() { Name = filename, Value = contentType };
        }
    }
}