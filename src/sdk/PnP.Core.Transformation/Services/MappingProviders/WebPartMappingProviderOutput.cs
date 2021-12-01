using PnP.Core.Transformation.Model;

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
