using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Model.Modern;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a URL mapping provider
    /// </summary>
    public class UrlMappingProviderOutput : MappingProviderOutput
    {
        /// <summary>
        /// Defines the target URL from the mapping
        /// </summary>
        public Uri Url { get; set; }
    }
}
