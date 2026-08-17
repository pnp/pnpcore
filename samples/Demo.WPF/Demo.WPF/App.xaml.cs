using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private static IHost? _host = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                var environmentNameFromEnvFile = GetEnvironmentNameFromEnvFile();

                if (!string.IsNullOrEmpty(environmentNameFromEnvFile))
                {
                    var currentEnvironment = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
                    if (!string.Equals(currentEnvironment, environmentNameFromEnvFile, StringComparison.OrdinalIgnoreCase))
                    {
                        System.Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", environmentNameFromEnvFile);
                    }
                }
            }

            // Use ApplicationBuilder to support multiple environments
            //var args = Environment.GetCommandLineArgs();
            var builder = Host.CreateApplicationBuilder(e.Args);

            builder.Configuration.AddUserSecrets<App>();

            
            // Add the PnP Core SDK library services
            builder.Services.AddPnPCore();
            // Add the PnP Core SDK library services configuration from the appsettings.json file
            builder.Services.Configure<PnPCoreOptions>(builder.Configuration.GetSection("PnPCore"));
            // Add the PnP Core SDK Authentication Providers
            builder.Services.AddPnPCoreAuthentication();
            // Add the PnP Core SDK Authentication Providers configuration from the appsettings.json file
            builder.Services.Configure<PnPCoreAuthenticationOptions>(builder.Configuration.GetSection("PnPCore"));


            builder.Services.AddTransient<MainWindow>();

            _host = builder.Build();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
          if (_host != null)
          {
            await _host.StopAsync();
            _host.Dispose();
            _host = null;
          }
          base.OnExit(e);
        }

        public static string? GetEnvironmentNameFromEnvFile()
        {
            var testEnvironmentFile = Path.Combine("..", "..", "..", "env.txt");

            if (File.Exists(testEnvironmentFile))
            {
                var content = File.ReadAllText(testEnvironmentFile);
                if (!string.IsNullOrEmpty(content))
                {
                    return content.Trim();
                }
            }

            return null;
        }
    }
}
