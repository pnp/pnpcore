namespace PnP.Core.Transformation.SharePoint.Builder.Configuration
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
    }
}
