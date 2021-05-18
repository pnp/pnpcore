using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Provides the basic interface for a Taxonomy mapping provider
    /// </summary>
    public interface ITaxonomyMappingProvider
    {
        /// <summary>
        /// Maps a Taxonomy Term from the source platform to the target platform
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <returns>The output of the mapping activity</returns>
        Task<TaxonomyMappingProviderOutput> MapTermAsync(TaxonomyMappingProviderInput input);
    }
}
