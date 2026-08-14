using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Tenant storage entities - the key/value pairs SharePoint stores on the app catalog site.
    /// </summary>
    internal static class StorageEntities
    {
        /// <summary>
        /// Writes one storage entity.
        /// </summary>
        internal static async Task SetAsync(PnPContext appCatalog, string key, string value,
            string description, string comment)
        {
            string url = "_api/web/SetStorageEntity(" +
                $"key='{Escape(key)}'," +
                $"value='{Escape(value)}'," +
                $"description='{Escape(description)}'," +
                $"comments='{Escape(comment)}')";

            await appCatalog.Web.ExecuteRequestAsync(
                new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest, url, "{}")).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads one storage entity's value, or null when it is not set.
        /// </summary>
        internal static async Task<string> GetAsync(PnPContext appCatalog, string key)
        {
            ApiRequestResponse response = await appCatalog.Web.ExecuteRequestAsync(
                new ApiRequest(ApiRequestType.SPORest, $"_api/web/GetStorageEntity('{Escape(key)}')"))
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(response.Response))
            {
                return null;
            }

            using (JsonDocument document = JsonDocument.Parse(response.Response))
            {
                return VerboseOData.StringOf(VerboseOData.Unwrap(document.RootElement), "Value");
            }
        }

        internal static async Task RemoveAsync(PnPContext appCatalog, string key)
        {
            await appCatalog.Web.ExecuteRequestAsync(new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest,
                $"_api/web/RemoveStorageEntity(key='{Escape(key)}')", "{}")).ConfigureAwait(false);
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
