using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Modernization.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a metadata mapping provider
    /// </summary>
    public class MetadataMappingProviderOutput: MappingProviderOutput
    {
        /// <summary>
        /// Provides the value resulting from the mapping
        /// </summary>
        public object Value { get; set; }
    }
}
