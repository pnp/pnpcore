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
    /// SharePoint implementation of <see cref="IPageLayoutMappingProvider"/>
    /// </summary>
    public class SharePointPageLayoutMappingProvider : IPageLayoutMappingProvider
    {
        private ILogger<SharePointPageLayoutMappingProvider> logger;

        public SharePointPageLayoutMappingProvider(ILogger<SharePointPageLayoutMappingProvider> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Maps a classic Page Layout into a modern Page Layout
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token to use</param>
        /// <returns>The output of the mapping activity</returns>
        public Task<PageLayoutMappingProviderOutput> MapPageLayoutAsync(PageLayoutMappingProviderInput input, CancellationToken token)
        {
            logger.LogInformation($"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapPageLayoutAsync");
            return Task.FromResult(new PageLayoutMappingProviderOutput());
        }
    }
}
