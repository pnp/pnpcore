using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a page layout mapping provider
    /// </summary>
    public class PageLayoutMappingProviderInput : MappingProviderInput
    {
        /// <summary>
        /// Creates an instances for the specified context
        /// </summary>
        /// <param name="context"></param>
        public PageLayoutMappingProviderInput(PageTransformationContext context) : base(context)
        {
        }

        /// <summary>
        /// Defines the source Page Layout to map
        /// </summary>
        // public PageLayout PageLayout { get; }
    }
}
