using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Model.Modern;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a HTML mapping provider
    /// </summary>
    public class HtmlMappingProviderOutput : MappingProviderOutput
    {
        /// <summary>
        /// Defines the HTML content resulting from the mapping
        /// </summary>
        public string HtmlContent { get; set; }
    }
}
