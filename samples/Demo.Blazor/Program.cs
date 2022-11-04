using Demo.Blazor.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Demo.Blazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
                // Provide a default scope, need to get Msal.js to work
                options.ProviderOptions.DefaultAccessTokenScopes = new List<string>() { "https://graph.microsoft.com/.default" };

                //https://github.com/dotnet/aspnetcore/issues/39104#issuecomment-1117082810
                // Temp workaround for now...the auth popup otherwise doesn't close
                options.ProviderOptions.LoginMode = "redirect";
            });

            // Add the PnP Core SDK library
            builder.Services.AddPnPCore();

            var config = new ConfigurationBuilder().Build();
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
