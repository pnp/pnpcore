using Demo.Blazor.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace Demo.Blazor
{
    public class Program
    {
        public const string AuthProviderName = "AccessTokenAuth";

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            var config = new ConfigurationBuilder().Build();

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
            });

            builder.Services
                .AddScoped<IOAuthAccessTokenProvider, MsalWrappedTokenProvider>()
                .AddSingleton<IConfiguration>(config)
                .AddAuthenticationProviderFactory(options => options.Configurations.Add(new OAuthAccessTokenConfiguration() { Name = AuthProviderName }))
                .AddPnPContextFactory(options => options.Configurations.Add(new PnPContextFactoryOptionsConfiguration()
                {
                    AuthenticationProviderName = AuthProviderName,
                    //SiteUrl = new
                }))
                .AddScoped<IMyPnPContextFactory, MyContextFactory>();

            await builder.Build().RunAsync();
        }

    }
}
