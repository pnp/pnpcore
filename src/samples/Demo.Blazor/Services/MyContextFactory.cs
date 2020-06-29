using Microsoft.Extensions.Configuration;
using PnP.Core.Services;
using System;

namespace Demo.Blazor.Services
{
    public interface IMyPnPContextFactory
    {
        public PnPContext GetContext();
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

        public PnPContext GetContext()
        {
            string siteUrl = _configuration.GetValue<string>("SharePoint:SiteUrl");
            return _contextFactory.Create(new Uri(siteUrl), Program.AuthProviderName);
        }
    }
}
