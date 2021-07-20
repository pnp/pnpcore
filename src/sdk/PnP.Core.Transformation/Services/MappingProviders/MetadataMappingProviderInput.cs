using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a metadata mapping provider
    /// </summary>
    public class MetadataMappingProviderInput : BaseMappingProviderInput
    {
        /// <summary>
        /// Creates an instances for the specified context
        /// </summary>
        /// <param name="context"></param>
        public MetadataMappingProviderInput(PageTransformationContext context) : base(context)
        {
        }

        /// <summary>
        /// Defines the internal name of the source field
        /// </summary>
        public string SourceFieldName { get; set; }

        /// <summary>
        /// Defines the internal name of the target field
        /// </summary>
        public string TargetFieldName { get; set; }

        /// <summary>
        /// Provides the value of the source field
        /// </summary>
        public object SourceValue { get; set; }

    }
}
