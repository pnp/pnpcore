using PnP.Core.Transformation.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a Web Part mapping provider
    /// </summary>
    public class WebPartMappingProviderOutput : BaseMappingProviderOutput
    {
        /// <summary>
        /// The Web Part Entity resulting after the mapping
        /// </summary>
        public WebPartEntity WebPart { get; set; }
    }
}
