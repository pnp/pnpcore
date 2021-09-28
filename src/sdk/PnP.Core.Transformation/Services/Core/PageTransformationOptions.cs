using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Defines the options to transform a page into a SharePoint Online modern page
    /// </summary>
    public class PageTransformationOptions
    {
        #region Generic transformation properties

        /// <summary>
        /// Defines whether the target content should be overwritten. Defaults to true.
        /// </summary>
        public bool Overwrite { get; set; } = true;

        /// <summary>
        /// Defines whether to keep item level page permissions on target or not. Defaults to true.
        /// </summary>
        public bool KeepPermissions { get; set; } = true;

        /// <summary>
        /// Set this property to true in case you want to retain the page's Author/Editor/Created/Modified fields. Note that a page publish will always set Editor/Modified. Defaults to false.
        /// </summary>
        public bool KeepPageCreationModificationInformation { get; set; }

        /// <summary>
        /// Defines whether the target page will be automatically published. Defaults to true.
        /// </summary>
        public bool PublishPage { get; set; }

        /// <summary>
        /// Defines whether the target page will have page comments enabled or disabled. Defaults to false (i.e. comments enabled).
        /// </summary>
        public bool DisablePageComments { get; set; }

        /// <summary>
        /// Defines whether to post the created page as news. Defaults to false.
        /// </summary>
        public bool PostAsNews { get; set; }

        /// <summary>
        /// If the page to be transformed is the web's home page, then replace with stock modern home page instead of transforming it
        /// </summary>
        public bool ReplaceHomePageWithDefaultHomePage { get; set; }

        #endregion

        #region Other page transformation properties

        /// <summary>
        /// Defines if the target page will get the name of the source page
        /// </summary>
        public bool TargetPageTakesSourcePageName { get; set; }

        /// <summary>
        /// Name of the transformed page. Leave it blank to reuse the source page name.
        /// </summary>
        public string TargetPageName { get; set; }

        /// <summary>
        /// Target page folder.
        /// </summary>
        public string TargetPageFolder { get; set; }

        /// <summary>
        /// Sets the page author in the page header similar to the original page author. Default to false.
        /// </summary>
        public bool SetAuthorInPageHeader { get; set; }

        /// <summary>
        /// Prefix used to name the target page.
        /// </summary>
        public string TargetPagePrefix { get; set; } = "Migrated_";

        /// <summary>
        /// Copy the page metadata (if any) to the created modern client side page. Defaults to false.
        /// </summary>
        public bool CopyPageMetadata { get; set; }

        #endregion
    }
}
