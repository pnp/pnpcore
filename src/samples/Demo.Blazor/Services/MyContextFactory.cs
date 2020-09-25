using Microsoft.Extensions.Configuration;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace Demo.Blazor.Services
{
    public interface IMyPnPContextFactory
    {
        public Task<PnPContext> GetContextAsync();
    }

    public class MyContextFactory : IMyPnPContextFactory
    {
        private readonly IPnPContextFactory _contextFactory;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationProvider _msalAuthProvider;

        public MyContextFactory(IPnPContextFactory contextFactory, IConfiguration configuration, IAuthenticationProvider msalAuthProvider)
        {
            _configuration = configuration;
            _contextFactory = contextFactory;
            _msalAuthProvider = msalAuthProvider;
        }

        public async Task<PnPContext> GetContextAsync()
        {
            string siteUrl = _configuration.GetValue<string>("SharePoint:SiteUrl");
            return await _contextFactory.CreateAsync(new Uri(siteUrl), _msalAuthProvider);
        }
    }
}
