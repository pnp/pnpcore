using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Builder.Configuration;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// Default implementation of <see cref="IMappingProvider"/>
    /// </summary>
    public class SharePointMappingProvider : IMappingProvider
    {
        private readonly IOptions<SharePointTransformationOptions> options;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="options"></param>
        /// <param name="serviceProvider"></param>
        public SharePointMappingProvider(IOptions<SharePointTransformationOptions> options, IServiceProvider serviceProvider)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Maps an item from the source platform to the target platform
        /// </summary>
        /// <param name="input">The input for the mapping</param>
        /// <param name="token">The cancellation token to use</param>
        /// <returns>The output of the mapping</returns>
        public async Task<MappingProviderOutput> MapAsync(MappingProviderInput input, CancellationToken token)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            
            var webPartMappingProvider = serviceProvider.GetService<IWebPartMappingProvider>();
            if (webPartMappingProvider != null)
            {
                var webPartInput = new WebPartMappingProviderInput
                {
                    Context = input.Context
                };
                var output = await webPartMappingProvider
                    .MapWebPartAsync(webPartInput, token)
                    .ConfigureAwait(false);
            }

            var htmlMappingProvider = serviceProvider.GetService<IHtmlMappingProvider>();
            if (htmlMappingProvider != null)
            {
                var htmlInput = new HtmlMappingProviderInput
                {
                    Context = input.Context
                };
                var output = await htmlMappingProvider
                    .MapHtmlAsync(htmlInput, token)
                    .ConfigureAwait(false);
            }

            var metadataMappingProvider = serviceProvider.GetService<IMetadataMappingProvider>();
            if (metadataMappingProvider != null)
            {
                var metadataInput = new MetadataMappingProviderInput
                {
                    Context = input.Context
                };
                var output = await metadataMappingProvider
                    .MapMetadataFieldAsync(metadataInput, token)
                    .ConfigureAwait(false);
            }

            var urlMappingProvider = serviceProvider.GetService<IUrlMappingProvider>();
            if (urlMappingProvider != null)
            {
                var metadataInput = new UrlMappingProviderInput
                {
                    Context = input.Context
                };
                var output = await urlMappingProvider
                    .MapUrlAsync(metadataInput, token)
                    .ConfigureAwait(false);
            }

            var pageLayoutMappingProvider = serviceProvider.GetService<IPageLayoutMappingProvider>();
            if (pageLayoutMappingProvider != null)
            {
                var pageLayoutInput = new PageLayoutMappingProviderInput
                {
                    Context = input.Context
                };
                var output = await pageLayoutMappingProvider
                    .MapPageLayoutAsync(pageLayoutInput, token)
                    .ConfigureAwait(false);
            }

            var taxonomyMappingProvider = serviceProvider.GetService<ITaxonomyMappingProvider>();
            if (taxonomyMappingProvider != null)
            {
                var taxonomyInput = new TaxonomyMappingProviderInput
                {
                    Context = input.Context
                };
                var output = await taxonomyMappingProvider
                    .MapTermAsync(taxonomyInput, token)
                    .ConfigureAwait(false);
            }

            var userMappingProvider = serviceProvider.GetService<IUserMappingProvider>();
            if (userMappingProvider != null)
            {
                var userInput = new UserMappingProviderInput
                {
                    Context = input.Context
                };
                var output = await userMappingProvider
                    .MapUserAsync(userInput, token)
                    .ConfigureAwait(false);
            }

            return new MappingProviderOutput();
        }
    }
}
