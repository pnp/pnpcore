using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly PnPGlobalSettingsOptions globalSettings;

        /// <summary>
        /// Http client which needs to be used for making a SharePoint REST call
        /// </summary>
        public HttpClient Client { get; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="client">Http Client coming from the .Net http client factory</param>
        /// <param name="log">Logger service</param>
        /// <param name="options">Options service</param>
        public SharePointRestClient(HttpClient client, ILogger<SharePointRestClient> log, IOptions<PnPGlobalSettingsOptions> options)
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

            client.DefaultRequestHeaders.Add("Accept", "application/json;odata=nometadata");
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

            if (!string.IsNullOrEmpty(globalSettings.VersionTag))
            {
                client.DefaultRequestHeaders.Add("X-ClientService-ClientTag", globalSettings.VersionTag);
            }

            Client = client;
        }
    }
}
