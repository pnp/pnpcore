using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a URL mapping provider
    /// </summary>
    public class UrlMappingProviderOutput : MappingProviderOutput
    {
        /// <summary>
        /// Creates an instance with the specified url
        /// </summary>
        /// <param name="url"></param>
        public UrlMappingProviderOutput(Uri url)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        /// <summary>
        /// Defines the target URL from the mapping
        /// </summary>
        public Uri Url { get; }
    }
}
