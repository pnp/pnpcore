using PnP.Core.Transformation.Services.Core;
using System;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a URL mapping provider
    /// </summary>
    public class UrlMappingProviderInput : BaseMappingProviderInput
    {
        /// <summary>
        /// Creates an instance for the specified context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="text"></param>
        public UrlMappingProviderInput(PageTransformationContext context, string text) : base(context)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        /// <summary>
        /// Defines the text to use for URL mapping
        /// </summary>
        public string Text { get; }
    }
}
