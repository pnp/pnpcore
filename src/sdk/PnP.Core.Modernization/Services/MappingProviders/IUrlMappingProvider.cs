using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Services.MappingProviders
{
    /// <summary>
    /// Provides the basic interface for a URL mapping provider
    /// </summary>
    public interface IUrlMappingProvider
    {
        /// <summary>
        /// Maps a URL from classic to modern
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <returns>The output of the mapping activity</returns>
        Task<UrlMappingProviderOutput> MapUrlAsync(UrlMappingProviderInput input);
    }
}
