using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PnP.Core.Transformation.Services.MappingProviders;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// SharePoint implementation of <see cref="IHtmlMappingProvider"/>
    /// </summary>
    public class SharePointHtmlMappingProvider : IHtmlMappingProvider
    {
        /// <summary>
        /// Maps HTML content from classic to modern
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The output of the mapping activity</returns>
        public Task<HtmlMappingProviderOutput> MapHtmlAsync(HtmlMappingProviderInput input, CancellationToken token = default)
        {
            return Task.FromResult(new HtmlMappingProviderOutput());
        }
    }
}
