using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Services.Builder.Configuration;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// SharePoint implementation of <see cref="IMetadataMappingProvider"/>
    /// </summary>
    public class SharePointMetadataMappingProvider : IMetadataMappingProvider
    {
        private ILogger<SharePointMetadataMappingProvider> logger;
        private readonly IOptions<SharePointTransformationOptions> options;
        private readonly CorrelationService correlationService;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Main constructor for the mapping provider
        /// </summary>
        /// <param name="logger">Logger for tracing activities</param>
        /// <param name="options">Configuration options</param>
        /// <param name="correlationService">The Correlation Service</param>
        /// <param name="serviceProvider">Service provider</param>
        public SharePointMetadataMappingProvider(ILogger<SharePointMetadataMappingProvider> logger,
            IOptions<SharePointTransformationOptions> options,
            CorrelationService correlationService,
            IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Maps a Metadata Field Value from the source platform to the target platform
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token to use, if any</param>
        /// <returns>The output of the mapping activity</returns>
        public Task<MetadataMappingProviderOutput> MapMetadataFieldAsync(MetadataMappingProviderInput input, CancellationToken token = default)
        {
            logger.LogInformation(this.correlationService.CorrelateString(
                input.Context.Task.Id,
                $"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapMetadataFieldAsync"));
            return Task.FromResult(new MetadataMappingProviderOutput());
        }
    }
}
