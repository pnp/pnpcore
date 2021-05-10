using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Modernization.Services.Core
{
    /// <summary>
    /// Defines the options to transform a page
    /// </summary>
    public class PageTransformationOptions : TransformationOptions
    {
        #region Ctor

        /// <summary>
        /// Creates a new instance with default values
        /// </summary>
        public PageTransformationOptions()
        {
            
        }

        /// <summary>
        /// Creates a new instance copying settings from another
        /// </summary>
        /// <param name="options"></param>
        public PageTransformationOptions(PageTransformationOptions options) : base(options)
        {
            TargetPageName = options.TargetPageName;
            TargetPageFolder = options.TargetPageFolder;
            SetAuthorInPageHeader = options.SetAuthorInPageHeader;
            TargetPagePrefix = options.TargetPagePrefix;
            SourcePagePrefix = options.SourcePagePrefix;
            CopyPageMetadata = options.CopyPageMetadata;
        }

        #endregion

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
        public string TargetPagePrefix { get; set; }

        /// <summary>
        /// Prefix used to rename the source page. Used in conjunction with TargetPageName.
        /// </summary>
        public string SourcePagePrefix { get; set; }

        /// <summary>
        /// Copy the page metadata (if any) to the created modern client side page. Defaults to false.
        /// </summary>
        public bool CopyPageMetadata { get; set; }

    }
}
