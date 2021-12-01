using Microsoft.Identity.Client;
using System.Net.Http;

namespace PnP.Core.Auth.Services.Http
{
    /// <summary>
    /// Factory responsible for creating HttpClient as .NET recommends to use a single instance of HttpClient.
    /// </summary>
    public sealed class MsalHttpClientFactory : IMsalHttpClientFactory
    {
        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// Default Constructor 
        /// </summary>
        /// <param name="httpClientFactory">Client factory that will handle the <see cref="HttpClient"/> creation</param>
        public MsalHttpClientFactory(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Returns the configured <see cref="HttpClient"/>
        /// </summary>
        /// <returns>The configured <see cref="HttpClient"/></returns>
        public HttpClient GetHttpClient()
        {
            return httpClientFactory.CreateClient("MsalHttpClient");
        }
    }
}
