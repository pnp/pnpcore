using Demo.Blazor.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Services;
using PnP.Core.Services.Builder.Configuration;
using System.Configuration;
using System.Threading.Tasks;

namespace Demo.Blazor
{
    public class Program
    {
        //public const string AuthProviderName = "AccessTokenAuth";

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            var config = new ConfigurationBuilder().Build();

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
            });

            // Add the PnP Core SDK library
            builder.Services.AddPnPCore();
            
            builder.Services
                // Add our custom IAuthenticationProvider implementation
                .AddScoped<IAuthenticationProvider, MsalWrappedTokenProvider>()
                // Load our configuration
                .AddSingleton<IConfiguration>(config)
                // Load our context factory
                .AddScoped<IMyPnPContextFactory, MyContextFactory>();

            await builder.Build().RunAsync();
        }

    }
}
