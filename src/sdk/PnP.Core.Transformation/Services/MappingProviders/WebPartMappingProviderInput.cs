using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Model.Classic;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a Web Part mapping provider
    /// </summary>
    public class WebPartMappingProviderInput : MappingProviderInput
    {
        /// <summary>
        /// Creates an instance for the specified context and web part
        /// </summary>
        /// <param name="context"></param>
        /// <param name="webPart"></param>
        public WebPartMappingProviderInput(PageTransformationContext context, WebPart webPart) : base(context)
        {
            WebPart = webPart ?? throw new ArgumentNullException(nameof(webPart));
        }

        /// <summary>
        /// Defines the source Web Part to map
        /// </summary>
        public WebPart WebPart { get; }
    }
}
