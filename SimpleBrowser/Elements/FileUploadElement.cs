// -----------------------------------------------------------------------
// <copyright file="FileUploadElement.cs" company="SimpleBrowser">
// See https://github.com/axefrog/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml.Linq;
    using SimpleBrowser.Internal;

    /// <summary>
    /// Implements an HTML file upload element
    /// </summary>
    internal class FileUploadElement : InputElement, IHasRawPostData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploadElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public FileUploadElement(XElement element)
            : base(element)
        {
        }

        #region IHasRawPostData Members
        /// <summary>
        /// Gets the file for upload from disk
        /// </summary>
        /// <returns>The encoded file</returns>
        public string GetPostData()
        {
            string filename = this.Value;
            if (File.Exists(filename))
            {
                // Todo: create a mime type for extensions
                string extension = new FileInfo(filename).Extension;
                string contentType = string.Format(
                    "Content-Type: {0}\r\nContent-Transfer-Encoding: binary\r\n\r\n",
                    ApacheMimeTypes.MimeForExtension(extension));
                byte[] allBytes = File.ReadAllBytes(filename);
                return contentType + Encoding.GetEncoding(28591).GetString(allBytes);
            }

            return string.Empty;
        }

        #endregion
    }
}
