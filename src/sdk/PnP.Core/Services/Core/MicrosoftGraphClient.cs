using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;

namespace PnP.Core.Services
{
    /// <summary>
    /// Client that handles all Microsoft Graph requests
    /// </summary>
    public class MicrosoftGraphClient
    {
        private readonly ILogger logger;
        private readonly PnPGlobalSettingsOptions globalSettings;

        /// <summary>
        /// Returns the configured Microsoft Graph http client
        /// </summary>
        public HttpClient Client { get; }

        /// <summary>
        /// Constructs the Microsoft Graph http client
        /// </summary>
        /// <param name="client">Http client instance</param>
        /// <param name="log">Logger</param>
        /// <param name="options">Settings to configure the http client</param>
        public MicrosoftGraphClient(HttpClient client, ILogger<MicrosoftGraphClient> log, IOptions<PnPGlobalSettingsOptions> options)
        {
            logger = log;
            globalSettings = options?.Value;

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (globalSettings == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            client.BaseAddress = PnPConstants.MicrosoftGraphBaseUri;
            client.DefaultRequestHeaders.Add("Accept", "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=true");

            if (!string.IsNullOrEmpty(globalSettings.HttpUserAgent))
            {
                client.DefaultRequestHeaders.Add("User-Agent", globalSettings.HttpUserAgent);
            }

            Client = client;
        }
    }
}
