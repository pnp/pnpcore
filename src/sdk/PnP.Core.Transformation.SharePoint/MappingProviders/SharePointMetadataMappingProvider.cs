using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PnP.Core.Transformation.Services.MappingProviders;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// SharePoint implementation of <see cref="IMetadataMappingProvider"/>
    /// </summary>
    public class SharePointMetadataMappingProvider : IMetadataMappingProvider
    {
        /// <summary>
        /// Maps a Metadata Field Value from the source platform to the target platform
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token to use</param>
        /// <returns>The output of the mapping activity</returns>
        public Task<MetadataMappingProviderOutput> MapMetadataFieldAsync(MetadataMappingProviderInput input, CancellationToken token)
        {
            return Task.FromResult(new MetadataMappingProviderOutput());
        }
    }
}
