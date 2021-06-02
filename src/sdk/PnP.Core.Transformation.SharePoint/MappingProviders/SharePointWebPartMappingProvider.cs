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
    /// SharePoint implementation of <see cref="IWebPartMappingProvider"/>
    /// </summary>
    public class SharePointWebPartMappingProvider : IWebPartMappingProvider
    {
        private ILogger<SharePointWebPartMappingProvider> logger;

        public SharePointWebPartMappingProvider(ILogger<SharePointWebPartMappingProvider> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Maps a classic Web Part into a modern Web Part
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token to use</param>
        /// <returns>The output of the mapping activity</returns>
        public Task<WebPartMappingProviderOutput> MapWebPartAsync(WebPartMappingProviderInput input, CancellationToken token)
        {
            logger.LogInformation($"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapWebPartAsync");
            return Task.FromResult(new WebPartMappingProviderOutput());
        }
    }
}
