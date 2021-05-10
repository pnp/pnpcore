using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Modernization.Model.Classic;

namespace PnP.Core.Modernization.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a Web Part mapping provider
    /// </summary>
    public class WebPartMappingProviderInput : MappingProviderInput
    {
        /// <summary>
        /// Defines the source Web Part to map
        /// </summary>
        public WebPart WebPart { get; set; }
    }
}
