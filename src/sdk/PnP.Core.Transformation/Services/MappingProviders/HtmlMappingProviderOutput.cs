using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a HTML mapping provider
    /// </summary>
    public class HtmlMappingProviderOutput : BaseMappingProviderOutput
    {
        /// <summary>
        /// Creates an instance for the content
        /// </summary>
        /// <param name="htmlContent"></param>
        public HtmlMappingProviderOutput(string htmlContent)
        {
            HtmlContent = htmlContent ?? throw new ArgumentNullException(nameof(htmlContent));
        }

        /// <summary>
        /// Defines the HTML content resulting from the mapping
        /// </summary>
        public string HtmlContent { get; }
    }
}
