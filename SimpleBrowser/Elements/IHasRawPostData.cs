// -----------------------------------------------------------------------
// <copyright file="IHasRawPostData.cs" company="SimpleBrowser">
// See https://github.com/axefrog/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    /// <summary>
    /// An interface for uploading files as string data
    /// </summary>
    public interface IHasRawPostData
    {
        /// <summary>
        /// Gets the file for upload from disk
        /// </summary>
        /// <returns>The encoded file</returns>
        string GetPostData();
    }
}
