using System;
using System.IO;
using System.Text;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.SharePoint.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using PnP.Core.Auth;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services.Builder.Configuration;
using PnP.Core.Transformation.Poc;
using PnP.Core.Transformation.Poc.Implementations;

[assembly: FunctionsStartup(typeof(Startup))]

namespace PnP.Core.Transformation.Poc
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton(p => CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage")));
            builder.Services.AddSingleton(p =>
            {
                var account = p.GetRequiredService<CloudStorageAccount>();
                return account.CreateCloudTableClient();
            });
            builder.Services.AddSingleton(p =>
            {
                var account = p.GetRequiredService<CloudStorageAccount>();
                return account.CreateCloudQueueClient();
            });

            // Configure PnP Core and PnP Core Auth options
            builder.Services.AddOptions<PnPCoreOptions>()
                .Configure<IConfiguration>((options, configuration) => configuration.GetSection("PnPCore").Bind(options));
            builder.Services.AddOptions<PnPCoreAuthenticationOptions>()
                .Configure<IConfiguration>((options, configuration) => configuration.GetSection("PnPCore").Bind(options));

            // Register PnP Core and PnP Core Auth services
            builder.Services.AddPnPCoreAuthentication();
            builder.Services.AddPnPCore();

            // Register the PnP Core Auth providers
            // Workaround: DryIoc (used by functions choose wrong ctor)
            builder.Services.RemoveAll<X509CertificateAuthenticationProvider>();
            builder.Services.RemoveAll<CredentialManagerAuthenticationProvider>();
            builder.Services.AddTransient(p =>
                new CredentialManagerAuthenticationProvider(
                    p.GetRequiredService<ILogger<OAuthAuthenticationProvider>>()));
            builder.Services.AddTransient(p =>
                new X509CertificateAuthenticationProvider(
                    p.GetRequiredService<ILogger<OAuthAuthenticationProvider>>()));

            // Register the PnP Transformation Framework services
            // for a SharePoint to SharePoint transformation
            builder.Services.AddPnPSharePointTransformation()
                .WithTransformationStateManager<AzureTableTransformationStateManager>()
                .WithTransformationExecutor<AzureQueueTransformationExecutor>();

            // Register the CSOM ClientContext for the data source
            builder.Services.AddTransient(p => {

                var clientContext = new ClientContext(Environment.GetEnvironmentVariable("SourceSite"));
                clientContext.ExecutingWebRequest += (sender, args) =>
                {
                    var resource = $"https://{new Uri(Environment.GetEnvironmentVariable("SourceSite")).Authority}";

                    var clientId = Environment.GetEnvironmentVariable("PnPCore:Credentials:Configurations:CredentialManager:ClientId");
                    var tenantId = Environment.GetEnvironmentVariable("PnPCore:Credentials:Configurations:CredentialManager:TenantId");
                    var credentialManager = Environment.GetEnvironmentVariable("PnPCore:Credentials:Configurations:CredentialManager:CredentialManager:CredentialManagerName");

                    var cmap = new CredentialManagerAuthenticationProvider(clientId, tenantId, credentialManager);
                    if (cmap != null)
                    {
                        args.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + cmap.GetAccessTokenAsync(new Uri(resource)).GetAwaiter().GetResult();
                    }
                };
                return clientContext;
            });
        }


    }
}