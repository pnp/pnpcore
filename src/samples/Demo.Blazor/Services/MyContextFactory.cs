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

        public MyContextFactory(IPnPContextFactory contextFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _contextFactory = contextFactory;
        }

        public async Task<PnPContext> GetContextAsync()
        {
            string siteUrl = _configuration.GetValue<string>("SharePoint:SiteUrl");
            return await _contextFactory.CreateAsync(new Uri(siteUrl), Program.AuthProviderName);
        }
    }
}
