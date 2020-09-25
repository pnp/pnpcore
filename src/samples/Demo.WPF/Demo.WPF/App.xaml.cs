using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services.Builder.Configuration;
using System;
using System.IO;
using System.Windows;

namespace Demo.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public IServiceProvider ServiceProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

        /// <summary>
        /// See https://marcominerva.wordpress.com/2019/03/06/using-net-core-3-0-dependency-injection-and-service-provider-with-wpf/ for the inspiration on this approach
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Add the PnP Core SDK library services
            services.AddPnPCore();
            // Add the PnP Core SDK library services configuration from the appsettings.json file
            services.Configure<PnPCoreOptions>(Configuration.GetSection("PnPCore"));
            // Add the PnP Core SDK Authentication Providers
            services.AddPnPCoreAuthentication();
            // Add the PnP Core SDK Authentication Providers configuration from the appsettings.json file
            services.Configure<PnPCoreAuthenticationOptions>(Configuration.GetSection("PnPCore"));

            services.AddTransient(typeof(MainWindow));
        }
    }
}
