using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PnP.Core.Transformation.Services.MappingProviders;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// SharePoint implementation of <see cref="IUrlMappingProvider"/>
    /// </summary>
    public class SharePointUrlMappingProvider : IUrlMappingProvider
    {
        /// <summary>
        /// Maps a URL from classic to modern
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <returns>The output of the mapping activity</returns>
        public Task<UrlMappingProviderOutput> MapUrlAsync(UrlMappingProviderInput input)
        {
            return Task.FromResult(new UrlMappingProviderOutput());
        }
    }
}
