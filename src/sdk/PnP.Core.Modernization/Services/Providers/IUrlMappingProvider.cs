using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Services.Providers
{
    /// <summary>
    /// Provides the basic interface for a URL mapping provider
    /// </summary>
    public interface IUrlMappingProvider
    {
        /// <summary>
        /// Maps a URL from classic to modern
        /// </summary>
        /// <param name="source">The URL in the source platform</param>
        /// <returns>The URL in the target platform</returns>
        Task<Uri> MapUrlAsync(Uri source);
    }
}
