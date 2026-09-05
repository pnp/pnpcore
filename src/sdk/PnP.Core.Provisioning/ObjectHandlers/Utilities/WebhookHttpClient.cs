using System;
using System.Net.Http;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// The <see cref="HttpClient"/> used to deliver provisioning webhooks.
    /// </summary>
    public static class WebhookHttpClient
    {
        private static readonly Lazy<HttpClient> defaultClient = new Lazy<HttpClient>(() => new HttpClient(), true);
        private static HttpClient instance;

        /// <summary>
        /// The client webhooks are sent with. Assign your own to route them through an
        /// <c>IHttpClientFactory</c> managed client; assign <c>null</c> to go back to the default.
        /// </summary>
        public static HttpClient Instance
        {
            get => instance ?? defaultClient.Value;
            set => instance = value;
        }
    }
}
