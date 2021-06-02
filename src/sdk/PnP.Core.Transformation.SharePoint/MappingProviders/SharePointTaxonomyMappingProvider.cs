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
    /// SharePoint implementation of <see cref="ITaxonomyMappingProvider"/>
    /// </summary>
    public class SharePointTaxonomyMappingProvider : ITaxonomyMappingProvider
    {
        private ILogger<SharePointTaxonomyMappingProvider> logger;

        public SharePointTaxonomyMappingProvider(ILogger<SharePointTaxonomyMappingProvider> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Maps a Taxonomy Term from the source platform to the target platform
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token to use</param>
        /// <returns>The output of the mapping activity</returns>
        public Task<TaxonomyMappingProviderOutput> MapTermAsync(TaxonomyMappingProviderInput input, CancellationToken token)
        {
            logger.LogInformation($"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapTermAsync");
            return Task.FromResult(new TaxonomyMappingProviderOutput());
        }
    }
}
