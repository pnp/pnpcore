using Microsoft.Extensions.Options;
using System;

namespace PnP.Core.Services.Builder.Configuration
{
    internal class PnPContextFactoryOptionsConfigurator: 
        IConfigureOptions<PnPContextFactoryOptions>, 
        IConfigureOptions<OAuthAuthenticationProviderOptions>,
        IConfigureOptions<PnPGlobalSettingsOptions>
    {
        private readonly IOptions<PnPCoreOptions> pnpCoreOptions;

        public PnPContextFactoryOptionsConfigurator(IOptions<PnPCoreOptions> pnpCoreOptions)
        {
            this.pnpCoreOptions = pnpCoreOptions;
        }

        public void Configure(PnPContextFactoryOptions options)
        {
            foreach (var (optionKey, optionValue) in pnpCoreOptions.Value.Sites)
            {
                options.Configurations.Add(new PnPContextFactoryOptionsConfiguration { 
                    Name = optionKey,
                    SiteUrl = new Uri(optionValue.SiteUrl),
                    AuthenticationProviderName = optionValue.AuthenticationProviderName
                });
            }

            // Configure the global Microsoft Graph settings
            options.GraphFirst = pnpCoreOptions.Value.PnPContext.GraphFirst;
            options.GraphCanUseBeta = pnpCoreOptions.Value.PnPContext.GraphCanUseBeta;
            options.GraphAlwaysUseBeta = pnpCoreOptions.Value.PnPContext.GraphAlwaysUseBeta;
        }

        public void Configure(OAuthAuthenticationProviderOptions options)
        {
            foreach (var (optionKey, optionValue) in pnpCoreOptions.Value.Credentials)
            {
                if (!String.IsNullOrEmpty(optionValue.CredentialManagerName))
                {
                    options.Configurations.Add(new OAuthCredentialManagerConfiguration {
                        Name = optionKey,
                        ClientId = optionValue.ClientId,
                        CredentialManagerName = optionValue.CredentialManagerName
                    });
                }
                else if (!String.IsNullOrEmpty(optionValue.CertificateThumbprint))
                {
                    options.Configurations.Add(new OAuthCertificateConfiguration
                    {
                        Name = optionKey,
                        ClientId = optionValue.ClientId,
                        Certificate = null, // TODO: Load certificate from thumbprint
                    });
                }
            }
        }

        public void Configure(PnPGlobalSettingsOptions options)
        {
            options.DisableTelemetry = pnpCoreOptions.Value.DisableTelemetry;
            options.AADTenantId = pnpCoreOptions.Value.AADTenantId;
            if (pnpCoreOptions.Value?.HttpRequests != null)
            {
                if (pnpCoreOptions.Value?.HttpRequests?.MicrosoftGraph != null)
                {
                    options.HttpMicrosoftGraphDelayInSeconds = pnpCoreOptions.Value.HttpRequests.MicrosoftGraph.DelayInSeconds;
                    options.HttpMicrosoftGraphMaxRetries = pnpCoreOptions.Value.HttpRequests.MicrosoftGraph.MaxRetries;
                    options.HttpMicrosoftGraphUseIncrementalDelay = pnpCoreOptions.Value.HttpRequests.MicrosoftGraph.UseIncrementalDelay;
                    options.HttpMicrosoftGraphUseRetryAfterHeader = pnpCoreOptions.Value.HttpRequests.MicrosoftGraph.UseRetryAfterHeader;
                }
                if (pnpCoreOptions.Value?.HttpRequests?.SharePointRest != null)
                {
                    options.HttpSharePointRestDelayInSeconds = pnpCoreOptions.Value.HttpRequests.SharePointRest.DelayInSeconds;
                    options.HttpSharePointRestMaxRetries = pnpCoreOptions.Value.HttpRequests.SharePointRest.MaxRetries;
                    options.HttpSharePointRestUseIncrementalDelay = pnpCoreOptions.Value.HttpRequests.SharePointRest.UseIncrementalDelay;
                    options.HttpSharePointRestUseRetryAfterHeader = pnpCoreOptions.Value.HttpRequests.SharePointRest.UseRetryAfterHeader;
                }
                options.HttpUserAgent = pnpCoreOptions.Value.HttpRequests.UserAgent;
            }
        }
    }
}
