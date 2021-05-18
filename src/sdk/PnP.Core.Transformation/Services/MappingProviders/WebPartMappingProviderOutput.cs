using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Model.Modern;

namespace PnP.Core.Transformation.Services.MappingProviders
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
