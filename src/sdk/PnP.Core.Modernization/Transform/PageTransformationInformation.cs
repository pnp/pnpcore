using Microsoft.SharePoint.Client;
using System.Collections.Generic;
using PnPCore = PnP.Core.Model.SharePoint;

namespace PnP.Core.Modernization.Transform
{
    /// <summary>
    /// Information used to configure the wiki and web part page transformation process
    /// </summary>
    public class PageTransformationInformation: BaseTransformationInformation
    {
        #region Construction
        /// <summary>
        /// Instantiates the page transformation class
        /// </summary>
        /// <param name="sourcePage">Page we want to transform</param>
        public PageTransformationInformation(ListItem sourcePage) : this(sourcePage, null, false)
        {
        }

        /// <summary>
        /// Instantiates the page transformation class
        /// </summary>
        /// <param name="sourcePage">Page we want to transform</param>
        /// <param name="targetPageName">Name of the target page</param>
        public PageTransformationInformation(ListItem sourcePage, string targetPageName) : this(sourcePage, targetPageName, false)
        {
        }

        /// <summary>
        /// Instantiates the page transformation class
        /// </summary>
        /// <param name="sourcePage">Page we want to transform</param>
        /// <param name="targetPageName">Name of the target page</param>
        /// <param name="overwrite">Do we overwrite the target page if it already exists</param>
        public PageTransformationInformation(ListItem sourcePage, string targetPageName, bool overwrite)
        {
            SourcePage = sourcePage;
            TargetPageName = targetPageName;
            Overwrite = overwrite;
            HandleWikiImagesAndVideos = true;
            AddTableListImageAsImageWebPart = true;
            TargetPageTakesSourcePageName = false;
            KeepPageSpecificPermissions = true;
            CopyPageMetadata = false;
            SkipTelemetry = false;
            RemoveEmptySectionsAndColumns = true;
            PublishCreatedPage = true;
            KeepPageCreationModificationInformation = false;
            PostAsNews = false;
            DisablePageComments = false;
            SkipUserMapping = false;
            TargetPageFolderOverridesDefaultFolder = false;
            SetDefaultTargetPagePrefix();
            SetDefaultSourcePagePrefix();
            // Populate with OOB mapping properties
            MappingProperties = new Dictionary<string, string>(5)
            {
                { Constants.UseCommunityScriptEditorMappingProperty, "false" },
                { Constants.SummaryLinksToQuickLinksMappingProperty, "true" }
            };
        }
        #endregion

        #region Page Properties        
        /// <summary>
        /// Target page will get the source page name, source page will be renamed. Used in conjunction with SourcePagePrefix
        /// </summary>
        public bool TargetPageTakesSourcePageName { get; set; }
        
        /// <summary>
        /// Prefix used to name the target page
        /// </summary>
        public string TargetPagePrefix { get; set; }

        /// <summary>
        /// Prefix used to name the source page. Used in conjunction with TargetPageTakesSourcePageName
        /// </summary>
        public string SourcePagePrefix { get; set; }

        /// <summary>
        /// Copy the page metadata (if any) to the created modern client side page. Defaults to false
        /// </summary>
        public bool CopyPageMetadata { get; set; }

        /// <summary>
        /// Configuration of the page header to apply
        /// </summary>
        public PnPCore.IPageHeader PageHeader { get; set; }

        /// <summary>
        /// Sets the page author in the page header similar to the original page author
        /// </summary>
        public bool SetAuthorInPageHeader { get; set; }

        /// <summary>
        /// Configuration driven by the presence of a modernization center
        /// </summary>
        public ModernizationCenterInformation ModernizationCenterInformation { get; set; }
        #endregion

        #region Webpart replacement configuration
        /// <summary>
        /// If the page to be transformed is the web's home page then replace with stock modern home page instead of transforming it
        /// </summary>
        public bool ReplaceHomePageWithDefaultHomePage { get; set; }
        #endregion

        #region Functionality
        /// <summary>
        /// Set's default target page prefix
        /// </summary>
        public void SetDefaultTargetPagePrefix()
        {
            this.TargetPagePrefix = "Migrated_";
        }

        /// <summary>
        /// Set's default source page prefix
        /// </summary>
        public void SetDefaultSourcePagePrefix()
        {
            this.SourcePagePrefix = "Previous_";
        }
        #endregion

    }
}
