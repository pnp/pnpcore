using System;
using System.Text.RegularExpressions;

namespace PnP.Core.Model
{
    /// <summary>
    /// Helper class to manage API calls URLs
    /// </summary>
    internal static class ApiHelper
    {
        internal static string ParseApiCall(TransientObject pnpObject, string apiCall, bool useGraph)
        {
            var metadataBasedObject = pnpObject as IMetadataExtensible;

            if (!useGraph && metadataBasedObject.Metadata.ContainsKey("uri"))
            {
                return metadataBasedObject.Metadata["uri"];
            }

            // No tokens, so nothing to do parse
            if (!apiCall.Contains("{"))
            {
                return apiCall;
            }

            // Parse api call to replace tokens
            apiCall = ParseApiRequest(metadataBasedObject, apiCall);

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
