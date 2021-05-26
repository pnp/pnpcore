using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Provides the basic interface for a Metadata mapping provider
    /// </summary>
    public interface IMetadataMappingProvider
    {
        /// <summary>
        /// Maps a Metadata Field Value from the source platform to the target platform
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The output of the mapping activity</returns>
        Task<MetadataMappingProviderOutput> MapMetadataFieldAsync(MetadataMappingProviderInput input, CancellationToken token = default);
    }
}
