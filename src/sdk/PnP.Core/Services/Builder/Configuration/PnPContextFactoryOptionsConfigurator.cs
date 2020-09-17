using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace PnP.Core.Services.Builder.Configuration
{
    internal class PnPContextFactoryOptionsConfigurator: 
        IConfigureOptions<PnPContextFactoryOptions>,
        IConfigureOptions<AuthenticationProvidersOptions>,
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

        public void Configure(AuthenticationProvidersOptions options)
        {
            foreach (var (optionKey, optionValue) in pnpCoreOptions.Value.Credentials)
            {
                // Let's see if we have the configuration Type attribute for the current credential options
                string configurationTypeName;
                if (optionValue.TryGetValue("Type", out configurationTypeName))
                {
                    // Get the corresponding .NET type
                    var configurationType = Type.GetType(configurationTypeName, true);

                    // And try to create a new instance
                    IAuthenticationProviderConfiguration configuration = Activator.CreateInstance(configurationType) as IAuthenticationProviderConfiguration;
                    configuration.Name = optionKey;

                    // Configure the object with the remainder options
                    foreach (var (key, value) in optionValue)
                    {
                        if (key != "Type")
                        {
                            configuration.GetType().GetProperty(key).SetValue(configuration, value);
                        }
                    }

                    // Add the configuration to the Credentials options
                    options.Configurations.Add(configuration);
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
