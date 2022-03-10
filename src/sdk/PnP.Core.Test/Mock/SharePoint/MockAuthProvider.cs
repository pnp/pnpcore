using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Test.Mock.SharePoint
{
    internal class MockAuthProvider : IAuthenticationProvider
    {
        public string Token { get; set; }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task AuthenticateRequestAsync(Uri resource, HttpRequestMessage request)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
        }

        public Task<string> GetAccessTokenAsync(Uri resource, string[] scopes)
        {
            return Task.FromResult(Token);
        }

        public Task<string> GetAccessTokenAsync(Uri resource)
        {
            return Task.FromResult(Token);
        }
    }
}
