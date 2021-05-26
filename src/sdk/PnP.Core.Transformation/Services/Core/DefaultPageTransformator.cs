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
    /// Implements the concret PageTransformator (this is the core of PnP Transformation Framework)
    /// </summary>
    public class DefaultPageTransformator : IPageTransformator
    {
        private readonly ILogger<DefaultPageTransformator> logger;
        private readonly IMappingProvider mappingProvider;
        private readonly IEnumerable<IPagePreTransformation> pagePreTransformations;
        private readonly IEnumerable<IPagePostTransformation> pagePostTransformations;
        private readonly IOptions<PageTransformationOptions> defaultPageTransformationOptions;

        /// <summary>
        /// Constructor with DI support
        /// </summary>
        /// <param name="logger">The logger interface</param>
        /// <param name="mappingProvider">The mapping provider interface</param>
        /// <param name="pagePreTransformations">The list of post transformations to call</param>
        /// <param name="pagePostTransformations">The list of pre transformations to call</param>
        /// <param name="pageTransformationOptions">The options</param>
        public DefaultPageTransformator(
            ILogger<DefaultPageTransformator> logger,
            IMappingProvider mappingProvider,
            IEnumerable<IPagePreTransformation> pagePreTransformations,
            IEnumerable<IPagePostTransformation> pagePostTransformations,
            IOptions<PageTransformationOptions> pageTransformationOptions)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mappingProvider = mappingProvider ?? throw new ArgumentNullException(nameof(mappingProvider));
            this.pagePreTransformations = pagePreTransformations ?? throw new ArgumentNullException(nameof(pagePreTransformations));
            this.pagePostTransformations = pagePostTransformations ?? throw new ArgumentNullException(nameof(pagePostTransformations));
            this.defaultPageTransformationOptions = pageTransformationOptions ?? throw new ArgumentNullException(nameof(pageTransformationOptions));
        }

        /// <summary>
        /// Transforms a page from classic to modern
        /// </summary>
        /// <param name="task">The context of the transformation process</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The URL of the transformed page</returns>
        public async Task<Uri> TransformAsync(PageTransformationTask task, CancellationToken token = default)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            logger.LogInformation("Transforming task {id} from {sourceItemId} to {targetPageUri}", task.Id, task.SourceItem.Id.Id, task.TargetPageUri);

            // Call pre transformations
            var preContext = new PagePreTransformationContext(task);
            foreach (var pagePreTransformation in pagePreTransformations)
            {
                await pagePreTransformation.PreTransformAsync(preContext, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
            }

            var context = new PageTransformationContext(task);
            var input = new MappingProviderInput
            {
                Context = context
            };
            MappingProviderOutput output = await mappingProvider.MapAsync(input, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            // Call post transformations
            var postContext = new PagePostTransformationContext(task);
            foreach (var pagePostTransformation in pagePostTransformations)
            {
                await pagePostTransformation.PostTransformAsync(postContext, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
            }

            return task.TargetPageUri;
        }
    }
}
