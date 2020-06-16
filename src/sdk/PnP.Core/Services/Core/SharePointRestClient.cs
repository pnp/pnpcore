using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace PnP.Core.Services
{
    /// <summary>
    /// Client that handles all SharePoint REST requests
    /// </summary>
    public class SharePointRestClient
    {
        private readonly ILogger logger;
        private readonly ISettings settings;

        /// <summary>
        /// Http client which needs to be used for making a SharePoint REST call
        /// </summary>
        public HttpClient Client { get; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="client">Http Client coming from the .Net http client factory</param>
        /// <param name="log">Logger service</param>
        /// <param name="settingsClient">Settings service</param>
        public SharePointRestClient(HttpClient client, ILogger<SharePointRestClient> log, ISettings settingsClient)
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

            client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");

            if (!string.IsNullOrEmpty(settings.HttpUserAgent))
            {
                client.DefaultRequestHeaders.Add("User-Agent", settings.HttpUserAgent);
            }

            if (!string.IsNullOrEmpty(settings.VersionTag))
            {
                client.DefaultRequestHeaders.Add("X-ClientService-ClientTag", settings.VersionTag);
            }

            Client = client;
        }
    }
}
