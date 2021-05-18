using System;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Default implementation of <see cref="IMappingProvider"/>
    /// </summary>
    public class DefaultMappingProvider : IMappingProvider
    {
        private readonly IServiceProvider services;

        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="services"></param>
        public DefaultMappingProvider(IServiceProvider services)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Maps an item from the source platform to the target platform
        /// </summary>
        /// <param name="input">The input for the mapping</param>
        /// <returns>The output of the mapping</returns>
        public async Task<MappingProviderOutput> MapAsync(MappingProviderInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var options = input.Context.Options;
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
