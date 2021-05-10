using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Modernization.Services.MappingProviders
{
    /// <summary>
    /// 
    /// </summary>
    public class MetadataMappingProviderOutput: MappingProviderOutput
    {
        /// <summary>
        /// Provides the value resulting from the mapping
        /// </summary>
        public object MappedValue { get; set; }
    }
}
