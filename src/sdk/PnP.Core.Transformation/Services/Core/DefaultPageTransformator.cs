using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary>
        /// Constructor with DI support
        /// </summary>
        /// <param name="logger">The logger interface</param>
        /// <param name="mappingProvider">The mapping provider interface</param>
        public DefaultPageTransformator(ILogger<DefaultPageTransformator> logger, IMappingProvider mappingProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mappingProvider = mappingProvider ?? throw new ArgumentNullException(nameof(mappingProvider));
        }

        /// <summary>
        /// Transforms a page from classic to modern
        /// </summary>
        /// <param name="options">The options to use while transforming the page, optional</param>
        /// <param name="task">The context of the transformation process</param>
        /// <returns>The URL of the transformed page</returns>
        public async Task<Uri> TransformAsync(PageTransformationTask task, Action<PageTransformationOptions> options = null)
        {
            this.logger.LogInformation("TransformAsync");

            //var context = new PageTransformationContext(task, options);
            var input = new MappingProviderInput
            {

            };
            await this.mappingProvider.MapAsync(input).ConfigureAwait(false);

            return default(Uri);
        }
    }
}
