using Microsoft.Extensions.Logging;
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
        private readonly ISettings settings;

        /// <summary>
        /// Returns the configured Microsoft Graph http client
        /// </summary>
        public HttpClient Client { get; }

        /// <summary>
        /// Constructs the Microsoft Graph http client
        /// </summary>
        /// <param name="client">Http client instance</param>
        /// <param name="log">Logger</param>
        /// <param name="settingsClient">Settings to configure the http client</param>
        public MicrosoftGraphClient(HttpClient client, ILogger<MicrosoftGraphClient> log, ISettings settingsClient)
        {
            logger = log;
            settings = settingsClient;

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (settingsClient == null)
            {
                throw new ArgumentNullException(nameof(settingsClient));
            }

            client.BaseAddress = PnPConstants.MicrosoftGraphBaseUri;
            client.DefaultRequestHeaders.Add("Accept", "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=true");

            if (!string.IsNullOrEmpty(settings.HttpUserAgent))
            {
                client.DefaultRequestHeaders.Add("User-Agent", settings.HttpUserAgent);
            }

            Client = client;
        }
    }
}
