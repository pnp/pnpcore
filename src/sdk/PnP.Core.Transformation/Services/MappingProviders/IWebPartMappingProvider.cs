using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PnP.Core.Transformation.Model.Classic;
using PnP.Core.Transformation.Model.Modern;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Provides the basic interface for a Web Part mapping provider
    /// </summary>
    public interface IWebPartMappingProvider
    {
        /// <summary>
        /// Maps a classic Web Part into a modern Web Part
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <returns>The output of the mapping activity</returns>
        Task<WebPartMappingProviderOutput> MapWebPartAsync(WebPartMappingProviderInput input);
    }
}
