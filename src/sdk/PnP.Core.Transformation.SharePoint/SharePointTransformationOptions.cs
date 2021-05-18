using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Services.MappingProviders;

namespace PnP.Core.Transformation.SharePoint
{
    
    /// <summary>
    /// Options used for SharePoint transformations
    /// </summary>
    public class SharePointTransformationOptions
    {
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

        /// <summary>
        /// Defines a custom HTML mapping provider
        /// </summary>
        public IHtmlMappingProvider HtmlMappingProvider { get; set; }
    }
}
