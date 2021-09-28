using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Provides the basic interface for a HTML mapping provider
    /// </summary>
    public interface IHtmlMappingProvider
    {
        /// <summary>
        /// Maps HTML content from classic to modern
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The output of the mapping activity</returns>
        Task<HtmlMappingProviderOutput> MapHtmlAsync(HtmlMappingProviderInput input, CancellationToken token = default);
    }
}
