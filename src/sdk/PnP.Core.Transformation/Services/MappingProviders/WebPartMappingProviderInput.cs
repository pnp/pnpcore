using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Model;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a Web Part mapping provider
    /// </summary>
    public class WebPartMappingProviderInput : BaseMappingProviderInput
    {
        /// <summary>
        /// Creates an instance for the specified context and web part
        /// </summary>
        /// <param name="context">The transformation context</param>
        public WebPartMappingProviderInput(PageTransformationContext context) : base(context)
        {
        }

        /// <summary>
        /// Defines the type of the source component
        /// </summary>
        public string SourceComponentType { get; set; }

        /// <summary>
        /// Property bag of the source component
        /// </summary>
        public Dictionary<string, string> SourceProperties { get; set; }

        /// <summary>
        /// The actual TXT/XML/JSON content of the source component
        /// </summary>
        public string SourceComponentRawContent { get; set; }

        /// <summary>
        /// The web part to transform
        /// </summary>
        public WebPartEntity WebPart { get; set; }

        /// <summary>
        /// Defines whether the transformation is cross-site
        /// </summary>
        public bool IsCrossSiteTransformation { get; set; }
    }
}
