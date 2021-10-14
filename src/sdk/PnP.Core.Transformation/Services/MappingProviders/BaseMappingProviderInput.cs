using PnP.Core.Transformation.Services.Core;
using System;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the basic input for a mapping provider
    /// </summary>
    public abstract class BaseMappingProviderInput
    {
        /// <summary>
        /// Creates an instance with the context specified
        /// </summary>
        /// <param name="context"></param>
        public BaseMappingProviderInput(PageTransformationContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Provides information about the current transformation
        /// </summary>
        public PageTransformationContext Context { get; }
    }
}
