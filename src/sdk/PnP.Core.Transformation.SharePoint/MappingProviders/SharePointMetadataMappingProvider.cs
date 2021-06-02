using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PnP.Core.Transformation.Services.MappingProviders;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// SharePoint implementation of <see cref="IMetadataMappingProvider"/>
    /// </summary>
    public class SharePointMetadataMappingProvider : IMetadataMappingProvider
    {
        private ILogger<SharePointMetadataMappingProvider> logger;

        public SharePointMetadataMappingProvider(ILogger<SharePointMetadataMappingProvider> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Maps a Metadata Field Value from the source platform to the target platform
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token to use</param>
        /// <returns>The output of the mapping activity</returns>
        public Task<MetadataMappingProviderOutput> MapMetadataFieldAsync(MetadataMappingProviderInput input, CancellationToken token)
        {
            logger.LogInformation($"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapMetadataFieldAsync");
            return Task.FromResult(new MetadataMappingProviderOutput());
        }
    }
}
