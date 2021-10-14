using PnP.Core.Transformation.Services.Core;
using System;

namespace PnP.Core.Transformation.SharePoint
{
    /// <summary>
    /// SharePoint implementation of <see cref="ISourceItemId"/>
    /// </summary>
    public class SharePointSourceItemId : ISourceItemId
    {
        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="uri">The URI of the item</param>
        public SharePointSourceItemId(Uri uri)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        /// <summary>
        /// The URI of the item
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Gets the string representation of the item
        /// </summary>
        public string Id => Uri.ToString();

        /// <summary>
        /// Gets the string representation of the Server Relative Url of the item
        /// </summary>
        public string ServerRelativeUrl => Uri.AbsolutePath; 
    }
}
