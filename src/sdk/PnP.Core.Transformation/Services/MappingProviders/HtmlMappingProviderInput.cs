using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Model.Classic;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a HTML mapping provider
    /// </summary>
    public class HtmlMappingProviderInput : MappingProviderInput
    {
        /// <summary>
        /// The source HTML content to map
        /// </summary>
        public string HtmlContent { get; set; }
    }
}
