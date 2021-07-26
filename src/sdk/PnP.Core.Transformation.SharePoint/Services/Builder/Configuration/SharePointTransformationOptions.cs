using System;
using System.Collections.Generic;

namespace PnP.Core.Transformation.SharePoint.Services.Builder.Configuration
{
    
    /// <summary>
    /// Options used for SharePoint transformations
    /// </summary>
    public class SharePointTransformationOptions
    {
        /// <summary>
        /// Defines the path for the Web Part Mapping File
        /// </summary>
        public string WebPartMappingFile { get; set; }

        /// <summary>
        /// Defines the path for the Page Layout Mapping File
        /// </summary>
        public string PageLayoutMappingFile { get; set; }

        /// <summary>
        /// Defines whether to include the TitleBarWebPart in the list of web parts to process
        /// </summary>
        public bool IncludeTitleBarWebPart { get; set; }

        /// <summary>
        /// Defines whether to keep specific permissions for the page or not
        /// </summary>
        public bool KeepPageSpecificPermissions { get; set; }

        /// <summary>
        /// Defines whether the target page takes the source page name or not
        /// </summary>
        public bool TargetPageTakesSourcePageName { get; set; }

        /// <summary>
        /// Skip URL rewriting
        /// </summary>
        public bool SkipUrlRewrite { get; set; }

        /// <summary>
        /// Defines whether to remove empty sections and columns on target
        /// </summary>
        public bool RemoveEmptySectionsAndColumns { get; set; }

        /// <summary>
        /// Defines whether to copy metadata onto the target page
        /// </summary>
        public bool CopyPageMetadata { get; set; }

        /// <summary>
        /// Defines whether to map users or not
        /// </summary>
        public bool ShouldMapUsers { get; set; }

        /// <summary>
        /// Custom URL mappings
        /// </summary>
        public List<UrlMapping> UrlMappings { get; set; }

        /// <summary>
        /// Custom URL mappings
        /// </summary>
        public List<UserMapping> UserMappings { get; set; }
    }

    /// <summary>
    /// Defines a custom URL mapping item
    /// </summary>
    public class UrlMapping
    {
        /// <summary>
        /// Url to be replaced
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// Url replacement value
        /// </summary>
        public string TargetUrl { get; set; }
    }

    /// <summary>
    /// Defines a custom User mapping item
    /// </summary>
    public class UserMapping
    {
        /// <summary>
        /// User to be replaced
        /// </summary>
        public string SourceUser { get; set; }

        /// <summary>
        /// User replacement value
        /// </summary>
        public string TargetUser { get; set; }
    }
}
