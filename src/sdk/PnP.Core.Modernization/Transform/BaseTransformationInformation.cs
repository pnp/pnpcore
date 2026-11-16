using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using PnPCore = PnP.Core.Model.SharePoint;

namespace PnP.Core.Modernization.Transform
{
    /// <summary>
    /// Information used to configure the page transformation process which applies to all types of page transformations
    /// </summary>
    public abstract class BaseTransformationInformation
    {

        #region Page Properties
        /// <summary>
        /// Source wiki/webpart page we want to transform
        /// </summary>
        public ListItem SourcePage { get; set; }

        /// <summary>
        /// File to convert - used for web part pages living outside a library as these pages do not have an associated list item
        /// </summary>
        public File SourceFile { get; set; }

        /// <summary>
        /// Overwrite the target page if it already exists?
        /// </summary>
        public bool Overwrite { get; set; }

        /// <summary>
        /// Name for the transformed page
        /// </summary>
        public string TargetPageName { get; set; }

        /// <summary>
        /// Target page folder
        /// </summary>
        public string TargetPageFolder { get; set; }

        /// <summary>
        /// When a target page folder is set, it overwrites the potentially default folder path (e.g. if the source page lived in a folder then that folder is in the default folder path)
        /// </summary>
        public bool TargetPageFolderOverridesDefaultFolder { get; set; }

        /// <summary>
        /// Apply the item level page permissions on to the target page, defaults to true
        /// </summary>
        public bool KeepPageSpecificPermissions { get; set; }

        /// <summary>
        /// Removes empty sections and columns to optimize screen real estate
        /// </summary>
        public bool RemoveEmptySectionsAndColumns { get; set; }
        #endregion

        #region Webpart replacement configuration
        /// <summary>
        /// Splits text around eligible iframes when true; otherwise appends video parts.
        /// Does not affect image handling.
        /// </summary>
        public bool HandleWikiImagesAndVideos { get; set; }

        /// <summary>
        /// Retained for API compatibility; ignored.
        /// </summary>
        public bool AddTableListImageAsImageWebPart { get; set; }

        /// <summary>
        /// Property bag for adding properties that will be exposed to the functions and selectors in the web part mapping file.
        /// These properties are used to condition the transformation process.
        /// </summary>
        public Dictionary<string, string> MappingProperties { get; set; }

        /// <summary>
        /// Set this property in case you want to retain the page's Author/Editor/Created/Modified fields. Note that a page publish will always set Editor/Modified
        /// </summary>
        public bool KeepPageCreationModificationInformation { get; set; }

        /// <summary>
        /// Should the created page be immediately published (default = true)
        /// </summary>
        public bool PublishCreatedPage { get; set; }

        /// <summary>
        /// Post the created page as news
        /// </summary>
        public bool PostAsNews { get; set; }

        /// <summary>
        /// Disable page comments on the created page
        /// </summary>
        public bool DisablePageComments { get; set; }

        /// <summary>
        /// Skip URL rewriting
        /// </summary>
        public bool SkipUrlRewrite { get; set; }

        /// <summary>
        /// Skip default URL rewriting, custom URL rewriting using a URL mapping file is still handled
        /// </summary>
        public bool SkipDefaultUrlRewrite { get; set; }

        /// <summary>
        /// Url to an URL mapping file
        /// </summary>
        public string UrlMappingFile { get; set; }

        /// <summary>
        /// Don't transform hidden web parts
        /// </summary>
        public bool SkipHiddenWebParts { get; set; }
        #endregion

        #region Term Mapping Properties

        /// <summary>
        /// URL to an Term Store Mapping File
        /// </summary>
        public string TermMappingFile { get; set; }

        /// <summary>
        /// Turn off term store mapping
        /// </summary>
        public bool SkipTermStoreMapping { get; set; }
        
        #endregion

        #region User Mapping Properties
        /// <summary>
        /// Don't perform user mapping
        /// </summary>
        public bool SkipUserMapping { get; set; }

        /// <summary>
        /// User Mapping file
        /// </summary>
        public string UserMappingFile { get; set; }

        /// <summary>
        /// Searches for users within a specific LDAP Connection String, if not specified domain LDAP will be calculated
        /// </summary>
        /// <example>
        /// LDAP://OU=Test,DC=ALPHADELTA,DC=LOCAL
        /// </example>
        public string LDAPConnectionString { get; set; }        
        #endregion

        #region Override properties
        /// <summary>
        /// Custom function callout that can be triggered to provide a tailored page title
        /// </summary>
        public Func<string, string> PageTitleOverride { get; set; }
        /// <summary>
        /// Custom layout transformator to be used for this page
        /// </summary>
        public Func<PnPCore.IPage, ILayoutTransformator> LayoutTransformatorOverride { get; set; }
        /// <summary>
        /// Custom content transformator to be used for this page
        /// </summary>
        public Func<PnPCore.IPage, PageTransformation, IContentTransformator> ContentTransformatorOverride { get; set; }
        #endregion

        #region General properties
        /// <summary>
        /// Disable telemetry: we use telemetry to make this tool better by sending anonymous data, but you're free to disable this
        /// </summary>
        public bool SkipTelemetry { get; set; }
        #endregion

        #region Internal fields, not settable by 3rd party
        /// <summary>
        /// Folder where the page to transform lives in
        /// </summary>
        internal string Folder { get; set; }

        /// <summary>
        /// Indicates if this transformation spans farms (on-prem to online tenant, online tenant A to online tenant B)
        /// </summary>
        internal bool IsCrossFarmTransformation { get; set; }

        /// <summary>
        /// Indicates if the transformation spans site collections or whether it's an in-place page transformation
        /// </summary>
        internal bool IsCrossSiteTransformation { get; set; }
        
        /// <summary>
        /// SharePoint version of the source 
        /// </summary>
        internal SPVersion SourceVersion { get; set; }

        /// <summary>
        /// SharePoint version number of the source 
        /// </summary>
        internal string SourceVersionNumber { get; set; }

        /// <summary>
        /// SharePoint version of the target 
        /// </summary>
        internal SPVersion TargetVersion { get; set; }

        /// <summary>
        /// SharePoint version number of the target 
        /// </summary>
        internal string TargetVersionNumber { get; set; }
        #endregion

    }
}
