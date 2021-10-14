using PnP.Core.Transformation.Services.Core;
using System;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a HTML mapping provider
    /// </summary>
    public class HtmlMappingProviderInput : BaseMappingProviderInput
    {
        /// <summary>
        /// Creates an instance for the specified context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="htmlContent"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HtmlMappingProviderInput(PageTransformationContext context, string htmlContent) : base(context)
        {
            HtmlContent = htmlContent ?? throw new ArgumentNullException(nameof(htmlContent));
        }

        /// <summary>
        /// The source HTML content to map
        /// </summary>
        public string HtmlContent { get; }

        /// <summary>
        /// Gets or sets if placeholders should be used for img and iframe
        /// </summary>
        public bool UsePlaceHolders { get; set; }
    }
}
