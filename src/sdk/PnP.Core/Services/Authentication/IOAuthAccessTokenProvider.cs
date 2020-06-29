using System;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// OAuth access token provider
    /// </summary>
    public interface IOAuthAccessTokenProvider
    {
        /// <summary>
        /// Returns an access token for a given resource
        /// </summary>
        /// <param name="resourceUri">Resource to return the access token for</param>
        /// <returns>Access token</returns>
        Task<string> GetAccessTokenAsync(Uri resourceUri);
    }
}
