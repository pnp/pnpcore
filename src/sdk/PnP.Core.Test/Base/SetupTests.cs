using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using PnP.Core.Services.Builder.Configuration;
using System;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class SetupTests
    {

        [TestMethod]
        public void DefaultServices()
        {
            var services = new ServiceCollection();
            services.AddPnPCore();
            services.AddPnPCoreAuthentication();

            var provider = services.BuildServiceProvider();

            Assert.IsInstanceOfType(provider.GetRequiredService<IPnPContextFactory>(), typeof(IPnPContextFactory));
            Assert.IsInstanceOfType(provider.GetRequiredService<IAuthenticationProvider>(), typeof(IAuthenticationProvider));
        }

        [TestMethod]
        public void DefaultServices2()
        {
            var provider = new ServiceCollection()
                    .AddPnPCore().Services
                    .AddPnPCoreAuthentication()
                .BuildServiceProvider();

            Assert.IsInstanceOfType(provider.GetRequiredService<IPnPContextFactory>(), typeof(IPnPContextFactory));
            Assert.IsInstanceOfType(provider.GetRequiredService<IAuthenticationProvider>(), typeof(IAuthenticationProvider));

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                ServiceCollection services = null;
                services.AddPnPCore();
            });
        }

        [TestMethod]
        public void ServicesWithOptions()
        {
            var services = new ServiceCollection();
            services.AddPnPCore((options) =>
            {
                options.DisableTelemetry = false;
                options.Environment = "China";
                options.Sites.Add("DemoSite", new PnPCoreSiteOptions
                  {
                      SiteUrl = "https://contoso.sharepoint.com/sites/demo",                      
                  });
            }); 
            services.AddPnPCoreAuthentication();

            var provider = services.BuildServiceProvider();

            Assert.IsInstanceOfType(provider.GetRequiredService<IPnPContextFactory>(), typeof(IPnPContextFactory));
            Assert.IsInstanceOfType(provider.GetRequiredService<IAuthenticationProvider>(), typeof(IAuthenticationProvider));
        }

        [TestMethod]
        public void PnPContextFactory()
        {
            var services = new ServiceCollection();
            services.AddPnPContextFactory((options) =>
            {
                options.GraphFirst = false;
            });

            var provider = services.BuildServiceProvider();

            Assert.IsInstanceOfType(provider.GetRequiredService<IPnPContextFactory>(), typeof(IPnPContextFactory));

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                ServiceCollection services = null;
                services.AddPnPContextFactory();
            });

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                ServiceCollection services = null;
                services.AddPnPContextFactory((options) =>
                {
                    options.GraphFirst = false;
                });
            });

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                ServiceCollection services = new ServiceCollection();
                services.AddPnPContextFactory(null);
            });
        }
    }
}
