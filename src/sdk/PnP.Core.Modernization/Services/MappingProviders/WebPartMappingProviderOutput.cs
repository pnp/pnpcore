using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Modernization.Model.Modern;

namespace PnP.Core.Modernization.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a Web Part mapping provider
    /// </summary>
    public class WebPartMappingProviderOutput : MappingProviderOutput
    {
        /// <summary>
        /// Defines the target Web Part from the mapping
        /// </summary>
        public ClientSideWebPart WebPart { get; set; }
    }
}
