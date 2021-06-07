using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Builder.Configuration;
using PnP.Core.Transformation.SharePoint.Model;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// Default implementation of <see cref="IMappingProvider"/>
    /// </summary>
    public class SharePointMappingProvider : IMappingProvider
    {
        private ILogger<SharePointMappingProvider> logger;
        private readonly IOptions<SharePointTransformationOptions> options;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Main constructor for the mapping provider
        /// </summary>
        /// <param name="logger">Logger for tracing activities</param>
        /// <param name="options">Configuration options</param>
        /// <param name="serviceProvider">Service provider</param>
        public SharePointMappingProvider(ILogger<SharePointMappingProvider> logger, IOptions<SharePointTransformationOptions> options, IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Maps an item from the source platform to the target platform
        /// </summary>
        /// <param name="input">The input for the mapping</param>
        /// <param name="token">The cancellation token to use, if any</param>
        /// <returns>The output of the mapping</returns>
        public async Task<MappingProviderOutput> MapAsync(MappingProviderInput input, CancellationToken token = default)
        {
            logger.LogInformation($"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapAsync");

            if (input == null) throw new ArgumentNullException(nameof(input));

            var sourceItem = input.Context.SourceItem as SharePointSourceItem;

            if (sourceItem == null) throw new ApplicationException(SharePointTransformationResources.Error_MissiningSharePointInputItem);

            //var webPartMappingProvider = serviceProvider.GetService<IWebPartMappingProvider>();
            //if (webPartMappingProvider != null)
            //{
            //    // Extract the list of Web Parts to process
            //    var webPartsToProcess = ExtractWebParts(null);

            //    // TODO: prepare webpart
            //    var webPartInput = new WebPartMappingProviderInput(input.Context);
            //    var output = await webPartMappingProvider
            //        .MapWebPartAsync(webPartInput, token)
            //        .ConfigureAwait(false);
            //}

            var htmlMappingProvider = serviceProvider.GetService<IHtmlMappingProvider>();
            if (htmlMappingProvider != null)
            {
                // TODO: get the html content
                var htmlInput = new HtmlMappingProviderInput(input.Context, "TODO");
                var output = await htmlMappingProvider
                    .MapHtmlAsync(htmlInput, token)
                    .ConfigureAwait(false);
            }

            var metadataMappingProvider = serviceProvider.GetService<IMetadataMappingProvider>();
            if (metadataMappingProvider != null)
            {
                // TODO: prepare input
                var metadataInput = new MetadataMappingProviderInput(input.Context);
                var output = await metadataMappingProvider
                    .MapMetadataFieldAsync(metadataInput, token)
                    .ConfigureAwait(false);
            }

            var urlMappingProvider = serviceProvider.GetService<IUrlMappingProvider>();
            if (urlMappingProvider != null)
            {
                // TODO: prepare uri
                var metadataInput = new UrlMappingProviderInput(input.Context, new Uri("http://dummy"));
                var output = await urlMappingProvider
                    .MapUrlAsync(metadataInput, token)
                    .ConfigureAwait(false);
            }

            var pageLayoutMappingProvider = serviceProvider.GetService<IPageLayoutMappingProvider>();
            if (pageLayoutMappingProvider != null)
            {
                // TODO: prepare page layout
                var pageLayoutInput = new PageLayoutMappingProviderInput(input.Context);
                var output = await pageLayoutMappingProvider
                    .MapPageLayoutAsync(pageLayoutInput, token)
                    .ConfigureAwait(false);
            }

            var taxonomyMappingProvider = serviceProvider.GetService<ITaxonomyMappingProvider>();
            if (taxonomyMappingProvider != null)
            {
                // TODO: prepare term id
                var taxonomyInput = new TaxonomyMappingProviderInput(input.Context, "");
                var output = await taxonomyMappingProvider
                    .MapTermAsync(taxonomyInput, token)
                    .ConfigureAwait(false);
            }

            var userMappingProvider = serviceProvider.GetService<IUserMappingProvider>();
            if (userMappingProvider != null)
            {
                // TODO: prepare user
                var userInput = new UserMappingProviderInput(input.Context, "");
                var output = await userMappingProvider
                    .MapUserAsync(userInput, token)
                    .ConfigureAwait(false);
            }

            return new MappingProviderOutput();
        }

        //private List<WebPartPlaceHolder> ExtractWebParts(File page)
        //{
        //    // Load web parts on web part page
        //    // Note: Web parts placed outside of a web part zone using SPD are not picked up by the web part manager. There's no API that will return those,
        //    //       only possible option to add parsing of the raw page aspx file.
        //    var limitedWPManager = page.GetLimitedWebPartManager(PersonalizationScope.Shared);
        //    cc.Load(limitedWPManager);

        //    IEnumerable<WebPartDefinition> webParts = cc.LoadQuery(limitedWPManager.WebParts.IncludeWithDefaultProperties(wp => wp.Id, wp => wp.ZoneId, wp => wp.WebPart.ExportMode, wp => wp.WebPart.Title, wp => wp.WebPart.ZoneIndex, wp => wp.WebPart.IsClosed, wp => wp.WebPart.Hidden, wp => wp.WebPart.Properties));
        //    cc.ExecuteQueryRetry();
        //}
    }
}
