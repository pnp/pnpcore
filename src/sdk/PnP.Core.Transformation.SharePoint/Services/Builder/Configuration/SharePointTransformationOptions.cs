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
        /// Defines whether to include the TitleBarWebPart in the list of web parts to process. Defaults to false.
        /// </summary>
        public bool IncludeTitleBarWebPart { get; set; }

        /// <summary>
        /// Skip URL rewriting. Defaults to false.
        /// </summary>
        public bool SkipUrlRewrite { get; set; }

        /// <summary>
        /// Defines whether to remove empty sections and columns on target. Defaults to false.
        /// </summary>
        public bool RemoveEmptySectionsAndColumns { get; set; }

        /// <summary>
        /// Defines whether to map users or not. Defaults to true.
        /// </summary>
        public bool ShouldMapUsers { get; set; } = true;

        /// <summary>
        /// If true images and videos embedded in wiki text will be transformed to actual image/video web parts, 
        /// else they'll get a placeholder and will be added as separate web parts at the end of the page. Defaults to true.
        /// </summary>
        public bool HandleWikiImagesAndVideos { get; set; } = true;

        /// <summary>
        /// Defines whether to transform hidden web parts or not. Defaults to false.
        /// </summary>
        public bool SkipHiddenWebParts { get; set; }

        /// <summary>
        /// When an image lives inside a table (or list) then also add it as a separate image web part. Defaults to true.
        /// </summary>
        public bool AddTableListImageAsImageWebPart { get; set; } = true;

        /// <summary>
        /// Property bag for adding properties that will be exposed to the functions and selectors in the web part mapping file.
        /// These properties are used to condition the transformation process.
        /// </summary>
        public Dictionary<string, string> MappingProperties { get; set; }

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
