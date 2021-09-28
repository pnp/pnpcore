using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PnP.Core.Transformation.Services.MappingProviders;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Implements the concrete PageTransformator (this is the core of PnP Transformation Framework)
    /// </summary>
    public class DefaultPageTransformator : IPageTransformator
    {
        private readonly ILogger<DefaultPageTransformator> logger;
        private readonly IMappingProvider mappingProvider;
        private readonly ITargetPageUriResolver targetPageUriResolver;
        private readonly IEnumerable<IPagePreTransformation> pagePreTransformations;
        private readonly IEnumerable<IPagePostTransformation> pagePostTransformations;
        private readonly IOptions<PageTransformationOptions> defaultPageTransformationOptions;
        private readonly IPageGenerator pageGenerator;

        /// <summary>
        /// Constructor with DI support
        /// </summary>
        /// <param name="logger">The logger interface</param>
        /// <param name="mappingProvider">The mapping provider interface</param>
        /// <param name="targetPageUriResolver">The target page uri resolver</param>
        /// <param name="pagePreTransformations">The list of post transformations to call</param>
        /// <param name="pagePostTransformations">The list of pre transformations to call</param>
        /// <param name="pageTransformationOptions">The options</param>
        /// <param name="pageGenerator">The page generator to create the actual SPO modern page</param>
        public DefaultPageTransformator(
            ILogger<DefaultPageTransformator> logger,
            IMappingProvider mappingProvider,
            ITargetPageUriResolver targetPageUriResolver,
            IEnumerable<IPagePreTransformation> pagePreTransformations,
            IEnumerable<IPagePostTransformation> pagePostTransformations,
            IOptions<PageTransformationOptions> pageTransformationOptions,
            IPageGenerator pageGenerator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mappingProvider = mappingProvider ?? throw new ArgumentNullException(nameof(mappingProvider));
            this.targetPageUriResolver = targetPageUriResolver ?? throw new ArgumentNullException(nameof(targetPageUriResolver));
            this.pagePreTransformations = pagePreTransformations ?? throw new ArgumentNullException(nameof(pagePreTransformations));
            this.pagePostTransformations = pagePostTransformations ?? throw new ArgumentNullException(nameof(pagePostTransformations));
            this.defaultPageTransformationOptions = pageTransformationOptions ?? throw new ArgumentNullException(nameof(pageTransformationOptions));
            this.pageGenerator = pageGenerator ?? throw new ArgumentNullException(nameof(pageGenerator));
        }

        /// <summary>
        /// Transforms a page from the configured data source to a modern SharePoint Online page
        /// </summary>
        /// <param name="task">The context of the transformation process</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The URL of the transformed page</returns>
        public virtual async Task<Uri> TransformAsync(PageTransformationTask task, CancellationToken token = default)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            logger.LogInformation("Running transformation task {id} on data source item {sourceItemId}", task.Id, task.SourceItemId.Id);

            // Get the source item by id
            var sourceItem = await task.SourceProvider.GetItemAsync(task.SourceItemId, token).ConfigureAwait(false);

            // Resolve the target page uri
            var targetPageUri = await targetPageUriResolver.ResolveAsync(sourceItem, task.TargetContext, token).ConfigureAwait(false);

            // Call pre transformations handlers
            var preContext = new PagePreTransformationContext(task, sourceItem, targetPageUri);
            foreach (var pagePreTransformation in pagePreTransformations)
            {
                await pagePreTransformation.PreTransformAsync(preContext, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
            }

            // Invoke the configured main mapping provider
            var context = new PageTransformationContext(task, sourceItem, targetPageUri);
            var input = new MappingProviderInput(context);
            MappingProviderOutput output = await mappingProvider.MapAsync(input, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            // Here we generate the actual SPO modern page in SharePoint Online
            var generatedPageUri = await pageGenerator.GenerateAsync(context, output, targetPageUri, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            // Call post transformations handlers
            var postContext = new PagePostTransformationContext(task, sourceItem, generatedPageUri);
            foreach (var pagePostTransformation in pagePostTransformations)
            {
                await pagePostTransformation.PostTransformAsync(postContext, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
            }

            return generatedPageUri;
        }
    }
}
