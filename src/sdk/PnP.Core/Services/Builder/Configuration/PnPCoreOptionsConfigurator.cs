using Microsoft.Extensions.Options;
using System;

namespace PnP.Core.Services.Builder.Configuration
{
    internal class PnPCoreOptionsConfigurator :
        IConfigureOptions<PnPContextFactoryOptions>,
        IConfigureOptions<PnPGlobalSettingsOptions>
    {
        private readonly IOptions<PnPCoreOptions> pnpCoreOptions;

        public PnPCoreOptionsConfigurator(IOptions<PnPCoreOptions> pnpCoreOptions)
        {
            this.pnpCoreOptions = pnpCoreOptions;
        }

        public void Configure(PnPContextFactoryOptions options)
        {
            //foreach (var (optionKey, optionValue) in pnpCoreOptions.Value.Sites)
            foreach (var siteOption in pnpCoreOptions.Value.Sites)
            {
                var optionKey = siteOption.Key;
                var optionValue = siteOption.Value;

                options.Configurations.Add(new PnPContextFactoryOptionsConfiguration
                {
                    Name = optionKey,
                    SiteUrl = new Uri(optionValue.SiteUrl),
                    AuthenticationProvider = optionValue.AuthenticationProvider
                });
            }

            // Configure the global Microsoft Graph settings
            options.GraphFirst = pnpCoreOptions.Value.PnPContext.GraphFirst;
            options.GraphCanUseBeta = pnpCoreOptions.Value.PnPContext.GraphCanUseBeta;
            options.GraphAlwaysUseBeta = pnpCoreOptions.Value.PnPContext.GraphAlwaysUseBeta;
            options.DefaultAuthenticationProvider = pnpCoreOptions.Value.DefaultAuthenticationProvider;
        }

        public void Configure(PnPGlobalSettingsOptions options)
        {
            options.DisableTelemetry = pnpCoreOptions.Value.DisableTelemetry;
            options.AADTenantId = pnpCoreOptions.Value.AADTenantId;

            if (!string.IsNullOrEmpty(pnpCoreOptions.Value.Environment))
            {
                if (Enum.TryParse(pnpCoreOptions.Value.Environment, out Microsoft365Environment environment))
                {
                    options.Environment = environment;
                }
            }

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
                    options.HttpSharePointRestDefaultPageSize = pnpCoreOptions.Value.HttpRequests.SharePointRest.DefaultPageSize;
                }
                if (pnpCoreOptions.Value?.HttpRequests?.AzureActiveDirectory != null)
                {
                    options.HttpAzureActiveDirectoryDelayInSeconds = pnpCoreOptions.Value.HttpRequests.AzureActiveDirectory.DelayInSeconds;
                    options.HttpAzureActiveDirectoryMaxRetries = pnpCoreOptions.Value.HttpRequests.AzureActiveDirectory.MaxRetries;
                    options.HttpAzureActiveDirectoryUseIncrementalDelay = pnpCoreOptions.Value.HttpRequests.AzureActiveDirectory.UseIncrementalDelay;
                    options.HttpAzureActiveDirectoryUseRetryAfterHeader = pnpCoreOptions.Value.HttpRequests.AzureActiveDirectory.UseRetryAfterHeader;
                }
                options.HttpUserAgent = pnpCoreOptions.Value.HttpRequests.UserAgent;
                options.HttpTimeout = pnpCoreOptions.Value.HttpRequests.Timeout;
                options.HttpRateLimiterMinimumCapacityLeft = pnpCoreOptions.Value.HttpRequests.RateLimiterMinimumCapacityLeft;
            }
        }
    }
}
