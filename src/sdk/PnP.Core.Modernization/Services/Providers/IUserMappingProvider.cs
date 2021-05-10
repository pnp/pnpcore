using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Services.Providers
{
    /// <summary>
    /// Provides the basic interface for a User mapping provider
    /// </summary>
    public interface IUserMappingProvider
    {
        /// <summary>
        /// Maps a user UPN from the source platform to the target platform
        /// </summary>
        /// <param name="source">The UPN of the user in the source platform</param>
        /// <returns>The UPN of the user in the target platform</returns>
        Task<string> MapUserAsync(string source);
    }
}
