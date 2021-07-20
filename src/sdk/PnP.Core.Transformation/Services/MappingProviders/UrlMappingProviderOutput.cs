using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a URL mapping provider
    /// </summary>
    public class UrlMappingProviderOutput : BaseMappingProviderOutput
    {
        /// <summary>
        /// Creates an instance with the specified text
        /// </summary>
        /// <param name="text">The text with mapped URLs</param>
        public UrlMappingProviderOutput(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        /// <summary>
        /// Defines the text after the URL mapping
        /// </summary>
        public string Text { get; }
    }
}
