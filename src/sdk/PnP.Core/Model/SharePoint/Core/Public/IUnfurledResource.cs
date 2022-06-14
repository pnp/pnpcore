using System;
using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines the data coming back from unfurling a link
    /// </summary>
    public interface IUnfurledResource
    {
        /// <summary>
        /// Type of resource that the unfurled link is referring to
        /// </summary>
        UnfurlLinkType LinkType { get; }

        /// <summary>
        /// SharePoint resource url for the unfurled link
        /// </summary>
        Uri Resource { get; }

        /// <summary>
        /// Id of the site collection containing the unfurled resource
        /// </summary>
        Guid SiteId { get; }

        /// <summary>
        /// Id of the web containing the unfurled resource
        /// </summary>
        Guid WebId { get; }

        /// <summary>
        /// Url of the web containing the unfurled resource
        /// </summary>
        Uri WebUrl { get; }

        /// <summary>
        /// Id of the parent list of the resource
        /// </summary>
        Guid ListId { get; }

        /// <summary>
        /// Display name of the parent list of the resource
        /// </summary>
        string ListDisplayName { get; }

        /// <summary>
        /// Url of the parent list of the resource
        /// </summary>
        Uri ListUrl { get; }

        /// <summary>
        /// ListItem id of the resource (if it's a file, listitem or page)
        /// </summary>
        int ListItemId { get; }

        /// <summary>
        /// Graph Drive Id of the resource (if it's a file)
        /// </summary>
        string FileDriveId { get; }

        /// <summary>
        /// Graph DriveItem id of the resource (if it's a file)
        /// </summary>
        string FileDriveItemId { get; }

        /// <summary>
        /// Unique ID of the resource (if it's a file)
        /// </summary>
        Guid FileUniqueId { get; }

        /// <summary>
        /// Name of the resource
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Size of the resource (if it's a file or page)
        /// </summary>
        long Size { get; }

        /// <summary>
        /// When was the resource last modified
        /// </summary>
        DateTime LastModified { get; }

        /// <summary>
        /// Who last modified the resource
        /// </summary>
        string LastModifiedBy { get; }

        /// <summary>
        /// Thumbnails of the resource (only for files at the moment)
        /// </summary>
        List<IThumbnail> Thumbnails { get; }
    }
}
