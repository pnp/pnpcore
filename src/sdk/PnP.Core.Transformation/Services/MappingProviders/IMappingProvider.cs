using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.MappingProviders
{    
    /// <summary>
    /// Provides the basic interface for a generic mapping provider
    /// </summary>
    public interface IMappingProvider
    {
        /// <summary>
        /// Maps an item from the source platform to the target platform
        /// </summary>
        /// <param name="input">The input for the mapping</param>
        /// <returns>The output of the mapping</returns>
        Task<MappingProviderOutput> MapAsync(MappingProviderInput input);
    }
}
