using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Services.Core
{
    /// <summary>
    /// Implements the concret PageTransformator (this is the core of PnP Modernization Framework)
    /// </summary>
    public class PageTransformator : IPageTransformator
    {
        private readonly ILogger<PageTransformator> _logger;

        /// <summary>
        /// Constructor with DI support
        /// </summary>
        /// <param name="logger">The logger interface</param>
        public PageTransformator(ILogger<PageTransformator> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Transforms a page from classic to modern
        /// </summary>
        /// <param name="options">The options to use while transforming the page, optional</param>
        /// <param name="task">The context of the transformation process</param>
        /// <returns>The URL of the transformed page</returns>
        public async Task<Uri> TransformAsync(PageTransformationTask task, Action<PageTransformationOptions> options = null)
        {
            this._logger.LogInformation("TransformAsync");

            return default(Uri);
        }
    }
}
