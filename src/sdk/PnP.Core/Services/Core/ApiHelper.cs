using PnP.Core.Model;

namespace PnP.Core.Services
{
    /// <summary>
    /// Helper class to manage API calls URLs
    /// </summary>
    internal static class ApiHelper
    {
        internal static string ParseApiCall(TransientObject pnpObject, string apiCall)
        {
            // No tokens, so nothing to do parse
            if (!apiCall.Contains("{"))
            {
                return apiCall;
            }

            // Parse api call to replace tokens
            apiCall = ParseApiRequest(pnpObject as IMetadataExtensible, apiCall);

            return apiCall;
        }

        internal static string ParseApiRequest(IMetadataExtensible pnpObject, string input)
        {
            var result = TokenHandler.ResolveTokensAsync(pnpObject, input)
                .GetAwaiter().GetResult();

            return result;
        }
    }
}
