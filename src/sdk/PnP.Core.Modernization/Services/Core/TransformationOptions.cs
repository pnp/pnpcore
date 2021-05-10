using PnP.Core.Modernization.Services.MappingProviders;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Modernization.Services.Core
{
    /// <summary>
    /// Defines the options to transform a page
    /// </summary>
    public abstract class TransformationOptions
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
        /// Defines whether to remove empty sections and columns to optimize screen real estate or not. Defaults to false.
        /// </summary>
        public bool RemoveEmptySectionsAndColumns { get; set; }

        /// <summary>
        /// Defines whether to transform hidden web parts or not. Defaults to false.
        /// </summary>
        public bool SkipHiddenWebParts { get; set; }

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
        /// If true images and videos embedded in wiki text will be transformed to actual image/video web parts, 
        /// else they'll get a placeholder and will be added as separate web parts at the end of the page. Defaults to false.
        /// </summary>
        public bool HandleWikiImagesAndVideos { get; set; }

        /// <summary>
        /// When an image lives inside a table (or list) then also add it as a separate image web part. Defaults to false.
        /// </summary>
        public bool AddTableListImageAsImageWebPart { get; set; }

        #endregion

        #region Mapping Providers

        /// <summary>
        /// Defines a custom Web Part mapping provider
        /// </summary>
        public IWebPartMappingProvider WebPartMappingProvider { get; set; }

        /// <summary>
        /// Defines a custom Page Layout mapping provider
        /// </summary>
        public IPageLayoutMappingProvider PageLayoutMappingProvider { get; set; }

        /// <summary>
        /// Defines a custom Taxonomy mapping provider
        /// </summary>
        public ITaxonomyMappingProvider TaxonomyMappingProvider { get; set; }

        /// <summary>
        /// Defines a custom Metadata mapping provider
        /// </summary>
        public IMetadataMappingProvider MetadataMappingProvider { get; set; }

        /// <summary>
        /// Defines a custom Url mapping provider
        /// </summary>
        public IUrlMappingProvider UrlMappingProvider { get; set; }

        /// <summary>
        /// Defines a custom User mapping provider
        /// </summary>
        public IUserMappingProvider UserMappingProvider { get; set; }

        #endregion
    }
}
