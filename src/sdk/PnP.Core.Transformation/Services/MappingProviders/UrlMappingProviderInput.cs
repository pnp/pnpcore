using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Model.Classic;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a URL mapping provider
    /// </summary>
    public class UrlMappingProviderInput : MappingProviderInput
    {
        /// <summary>
        /// Creates an instance for the specified context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="url"></param>
        public UrlMappingProviderInput(PageTransformationContext context, Uri url) : base(context)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        /// <summary>
        /// Defines the source URL to map
        /// </summary>
        public Uri Url { get; }
    }
}
