using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// A thumbnail for a file in SharePoint, OneDrive or Teams
    /// </summary>
    public interface IThumbnail
    {
        /// <summary>
        /// Thumbnail size identifier
        /// </summary>
        string Size { get; }

        /// <summary>
        /// Thumbnail URL
        /// </summary>
        Uri Url { get; }

        /// <summary>
        /// Thumbnail width
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Thumbnail height
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Thumbnail belongs to thumnail set with this id (not always populated)
        /// </summary>
        string SetId { get; }
    }
}
