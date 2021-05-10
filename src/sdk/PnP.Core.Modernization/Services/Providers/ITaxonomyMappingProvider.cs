using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Services.Providers
{
    /// <summary>
    /// Provides the basic interface for a Taxonomy mapping provider
    /// </summary>
    public interface ITaxonomyMappingProvider
    {
        /// <summary>
        /// Maps a Taxonomy Term from the source platform to the target platform
        /// </summary>
        /// <param name="source">The Taxonomy Term ID in the source platform</param>
        /// <returns>The Taxonomy Term ID in the target platform</returns>
        Task<string> MapTermAsync(string source);
    }
}
