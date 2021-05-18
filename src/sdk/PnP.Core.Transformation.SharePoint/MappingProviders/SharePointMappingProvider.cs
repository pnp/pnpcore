using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// Default implementation of <see cref="IMappingProvider"/>
    /// </summary>
    public class SharePointMappingProvider : IMappingProvider
    {
        private readonly IOptions<SharePointTransformationOptions> options;

        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="options"></param>
        public SharePointMappingProvider(IOptions<SharePointTransformationOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Maps an item from the source platform to the target platform
        /// </summary>
        /// <param name="input">The input for the mapping</param>
        /// <returns>The output of the mapping</returns>
        public async Task<MappingProviderOutput> MapAsync(MappingProviderInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var options = this.options.Value;
            if (options.WebPartMappingProvider != null)
            {
                var webPartInput = new WebPartMappingProviderInput
                {
                    Context = input.Context
                };
                var output = await options
                    .WebPartMappingProvider
                    .MapWebPartAsync(webPartInput)
                    .ConfigureAwait(false);
            }

            if (options.HtmlMappingProvider != null)
            {
                var htmlInput = new HtmlMappingProviderInput
                {
                    Context = input.Context
                };
                var output = await options
                    .HtmlMappingProvider
                    .MapHtmlAsync(htmlInput)
                    .ConfigureAwait(false);
            }

            if (options.MetadataMappingProvider != null)
            {
                var metadataInput = new MetadataMappingProviderInput
                {
                    Context = input.Context
                };
                var output = await options
                    .MetadataMappingProvider
                    .MapMetadataFieldAsync(metadataInput)
                    .ConfigureAwait(false);
            }

            if (options.PageLayoutMappingProvider != null)
            {
                var pageLayoutInput = new PageLayoutMappingProviderInput
                {
                    Context = input.Context
                };
                var output = await options
                    .PageLayoutMappingProvider
                    .MapPageLayoutAsync(pageLayoutInput)
                    .ConfigureAwait(false);
            }

            if (options.TaxonomyMappingProvider != null)
            {
                var taxonomyInput = new TaxonomyMappingProviderInput
                {
                    Context = input.Context
                };
                var output = await options
                    .TaxonomyMappingProvider
                    .MapTermAsync(taxonomyInput)
                    .ConfigureAwait(false);
            }

            if (options.UserMappingProvider != null)
            {
                var userInput = new UserMappingProviderInput
                {
                    Context = input.Context
                };
                var output = await options
                    .UserMappingProvider
                    .MapUserAsync(userInput)
                    .ConfigureAwait(false);
            }

            return new MappingProviderOutput();
        }
    }
}
