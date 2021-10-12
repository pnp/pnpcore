using PnP.Core.Transformation.Services.MappingProviders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Provides the basic interface for a SPO modern Page Generator
    /// </summary>
    public interface IPageGenerator
    {
        /// <summary>
        /// Generates a page from the output of a data source mapping to SPO modern
        /// </summary>
        /// <param name="context">The context of the transformation process</param>
        /// <param name="mappingOutput">The output of the initial mapping from the data source</param>
        /// <param name="targetPageUri">The URI of the target page to create</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The output of the transformed page</returns>
        Task<PageGeneratorOutput> GenerateAsync(PageTransformationContext context, MappingProviderOutput mappingOutput, Uri targetPageUri, CancellationToken token = default);
    }
}
