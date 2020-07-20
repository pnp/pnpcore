using PnP.Core.Model;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Helper class to manage API calls URLs
    /// </summary>
    internal static class ApiHelper
    {
        internal async static Task<string> ParseApiCallAsync(TransientObject pnpObject, string apiCall)
        {
            // No tokens, so nothing to do parse
            if (!apiCall.Contains("{"))
            {
                return apiCall;
            }

            // Parse api call to replace tokens
            return await ParseApiRequestAsync(pnpObject as IMetadataExtensible, apiCall).ConfigureAwait(false);
        }

        internal async static Task<string> ParseApiRequestAsync(IMetadataExtensible pnpObject, string input)
        {
            return await TokenHandler.ResolveTokensAsync(pnpObject, input).ConfigureAwait(false);
        }
    }
}
