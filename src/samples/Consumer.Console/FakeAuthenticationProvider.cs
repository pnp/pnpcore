using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    public class FakeAuthenticationProvider : IAuthenticationProvider
    {
        private readonly ILogger log;

        public FakeAuthenticationProvider(
            ILogger<FakeAuthenticationProvider> logger)
        {
            log = logger;
        }

        public Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        public void Configure(IAuthenticationProviderConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAccessTokenAsync(Uri resource, string[] scopes)
        {
            throw new NotImplementedException();
        }
    }
}
