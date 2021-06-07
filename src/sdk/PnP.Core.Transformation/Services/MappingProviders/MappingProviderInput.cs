using PnP.Core.Transformation.Services.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a mapping provider
    /// </summary>
    public class MappingProviderInput
    {
        /// <summary>
        /// Creates an instance with the context specified
        /// </summary>
        /// <param name="context"></param>
        public MappingProviderInput(PageTransformationContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Provides information about the current transformation
        /// </summary>
        public PageTransformationContext Context { get; }
    }
}
