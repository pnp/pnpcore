using Microsoft.Identity.Client;
using PnP.Core.Services;
using System.Net.Http;

namespace PnP.Core.Auth.Services.Http
{
    public class MsalHttpClientFactory : IMsalHttpClientFactory
    {
        private IHttpClientFactory _httpClientFactory;

        public MsalHttpClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public HttpClient GetHttpClient()
        {
            return _httpClientFactory.CreateClient("MsalHttpClient");
        }
    }
}
