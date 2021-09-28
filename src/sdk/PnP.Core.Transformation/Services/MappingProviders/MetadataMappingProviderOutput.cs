using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a metadata mapping provider
    /// </summary>
    public class MetadataMappingProviderOutput: BaseMappingProviderOutput
    {
        /// <summary>
        /// Provides the value resulting from the mapping
        /// </summary>
        public object Value { get; set; }
    }
}
