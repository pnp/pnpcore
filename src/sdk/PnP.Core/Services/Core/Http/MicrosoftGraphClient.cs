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
        private bool baseAddressWasSet = false;

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

            if (globalSettings.Logger == null)
            {
                globalSettings.Logger = logger;
            }

            client.BaseAddress = PnPConstants.MicrosoftGraphBaseUri;
            client.DefaultRequestHeaders.Add("Accept", "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=true");

#if NET5_0_OR_GREATER
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
#else
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
#endif

            client.Timeout = globalSettings.GetHttpTimeout();

            if (!string.IsNullOrEmpty(globalSettings.HttpUserAgent))
            {
                client.DefaultRequestHeaders.Add("User-Agent", globalSettings.HttpUserAgent);
            }

            Client = client;
        }

        internal void UpdateBaseAddress(string authority)
        {
            if (!baseAddressWasSet && !string.IsNullOrEmpty(authority))
            {
                // Only set base address when there's a different authority
                var currentBaseAddress = Client.BaseAddress;
                if (currentBaseAddress != null) 
                { 
                    if (!string.IsNullOrEmpty(currentBaseAddress.Authority) && currentBaseAddress.Authority.Equals(authority, StringComparison.InvariantCultureIgnoreCase))
                    {
                        baseAddressWasSet = true;
                        return;
                    }
                }

                Client.BaseAddress = new Uri($"https://{authority}/");
                baseAddressWasSet = true;
            }
        }
    }
}
