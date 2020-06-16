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

        public HttpClient Client { get; }

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
