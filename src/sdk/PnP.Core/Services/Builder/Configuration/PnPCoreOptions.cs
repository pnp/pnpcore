using System;
using System.Collections.Generic;

namespace PnP.Core.Services.Builder.Configuration
{
    /// <summary>
    /// Options for configuring PnP Core SDK
    /// </summary>
    public class PnPCoreOptions
    {
        /// <summary>
        /// Turns on/off telemetry, can be customized via configuration. Defaults to false.
        /// </summary>
        public bool DisableTelemetry { get; set; }

        /// <summary>
        /// The Microsoft 365 cloud environment that's used
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Returns the Microsoft Graph authority (e.g. graph.microsoft.com) to use when <see cref="Environment"/> is set to <see cref="Microsoft365Environment.Custom"/>
        /// </summary>
        public string MicrosoftGraphAuthority { get; set; }

        /// <summary>
        /// Returns the Azure AD Login authority (e.g. login.microsoftonline.com) to use when <see cref="Environment"/> is set to <see cref="Microsoft365Environment.Custom"/>
        /// </summary>
        public string AzureADLoginAuthority { get; set; }

        /// <summary>
        /// AAD tenant id, used for telemetry purposes. Can be customized via configuration
        /// </summary>
        public Guid AADTenantId { get; set; }

        /// <summary>
        /// The global HTTP requests settings
        /// </summary>
        public PnPCoreHttpRequestsOptions HttpRequests { get; set; } = new PnPCoreHttpRequestsOptions();

        /// <summary>
        /// The global PnPContext options
        /// </summary>
        public PnPCoreContextOptions PnPContext { get; set; } = new PnPCoreContextOptions();

        /// <summary>
        /// The sites options
        /// </summary>
        public PnPCoreSitesOptions Sites { get; } = new PnPCoreSitesOptions();

        /// <summary>
        /// The default Authentication Provider for the sites
        /// </summary>
        public IAuthenticationProvider DefaultAuthenticationProvider { get; set; }
    }

    /// <summary>
    /// Http request global settings
    /// </summary>
    public class PnPCoreHttpRequestsOptions
    {
        /// <summary>
        /// User agent value, can be customized via configuration 
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// HTTP request timeout in seconds to use when making calls to SharePoint or Microsoft Graph
        /// </summary>
        public int Timeout { get; set; } = 100;

        /// <summary>
        /// When set (e.g. to 10%) then RateLimit response headers coming from SharePoint and Graph are used: if there's less 
        /// then the set value (e.g. 10%) capacity left before getting throttled the pending request will be delayed until
        /// the 1 minute window is reset. Defaults to 10, set to 0 to disable.
        /// </summary>
        public int RateLimiterMinimumCapacityLeft { get; set; } = 10;

        /// <summary>
        /// SharePoint Online REST options
        /// </summary>
        public PnPCoreHttpRequestsSharePointRestOptions SharePointRest { get; set; } = new PnPCoreHttpRequestsSharePointRestOptions();

        /// <summary>
        /// Microsoft Graph REST options
        /// </summary>
        public PnPCoreHttpRequestsGraphOptions MicrosoftGraph { get; set; } = new PnPCoreHttpRequestsGraphOptions();

        /// <summary>
        /// Azure AD options
        /// </summary>
        public PnPCoreHttpRequestsAzureActiveDirectoryOptions AzureActiveDirectory { get; set; } = new PnPCoreHttpRequestsAzureActiveDirectoryOptions();
    }

    /// <summary>
    /// SharePoint Online REST options
    /// </summary>
    public class PnPCoreHttpRequestsSharePointRestOptions
    {
        /// <summary>
        /// Use the Retry-After header for calculating the delay in case of a retry. Defaults to true
        /// </summary>
        public bool UseRetryAfterHeader { get; set; } = true;

        /// <summary>
        /// When not using retry-after, how many times can a retry be made. Defaults to 10
        /// </summary>
        public int MaxRetries { get; set; } = 10;

        /// <summary>
        /// How many seconds to wait for the next retry attempt. Defaults to 3
        /// </summary>
        public int DelayInSeconds { get; set; } = 3;

        /// <summary>
        /// Use an incremental strategy for the delay: each retry doubles the previous delay time. Defaults to true
        /// </summary>
        public bool UseIncrementalDelay { get; set; } = true;

        /// <summary>
        /// Defines the default page size for querying items in SharePoint REST
        /// </summary>
        public int DefaultPageSize { get; set; } = 100;
    }

    /// <summary>
    /// Microsoft Graph REST options
    /// </summary>
    public class PnPCoreHttpRequestsGraphOptions
    {
        /// <summary>
        /// Use the Retry-After header for calculating the delay in case of a retry. Defaults to true
        /// </summary>
        public bool UseRetryAfterHeader { get; set; } = true;

        /// <summary>
        /// When not using retry-after, how many times can a retry be made. Defaults to 10
        /// </summary>
        public int MaxRetries { get; set; } = 10;

        /// <summary>
        /// How many seconds to wait for the next retry attempt. Defaults to 3
        /// </summary>
        public int DelayInSeconds { get; set; } = 3;

        /// <summary>
        /// Use an incremental strategy for the delay: each retry doubles the previous delay time. Defaults to true
        /// </summary>
        public bool UseIncrementalDelay { get; set; } = true;
    }

    /// <summary>
    /// Microsoft Graph REST options
    /// </summary>
    public class PnPCoreHttpRequestsAzureActiveDirectoryOptions
    {
        /// <summary>
        /// Use the Retry-After header for calculating the delay in case of a retry. Defaults to true
        /// </summary>
        public bool UseRetryAfterHeader { get; set; } = true;

        /// <summary>
        /// When not using retry-after, how many times can a retry be made. Defaults to 10
        /// </summary>
        public int MaxRetries { get; set; } = 10;

        /// <summary>
        /// How many seconds to wait for the next retry attempt. Defaults to 3
        /// </summary>
        public int DelayInSeconds { get; set; } = 3;

        /// <summary>
        /// Use an incremental strategy for the delay: each retry doubles the previous delay time. Defaults to true
        /// </summary>
        public bool UseIncrementalDelay { get; set; } = true;
    }

    /// <summary>
    /// Microsoft Graph global settings
    /// </summary>
    public class PnPCoreContextOptions
    {
        /// <summary>
        /// Controls whether the library will try to use Microsoft Graph over REST whenever that's defined in the model
        /// </summary>
        public bool GraphFirst { get; set; } = true;

        /// <summary>
        /// If true than the Graph beta endpoint is used when there's no other option, default approach stays using the v1 endpoint
        /// </summary>
        public bool GraphCanUseBeta { get; set; } = true;

        /// <summary>
        /// If true than all requests to Microsoft Graph use the beta endpoint
        /// </summary>
        public bool GraphAlwaysUseBeta { get; set; }
    }

    /// <summary>
    /// Options for configuring PnP Core SDK
    /// </summary>
    public class PnPCoreSitesOptions : Dictionary<string, PnPCoreSiteOptions>
    {
    }

    /// <summary>
    /// Options for configuring a single site in PnP Core SDK
    /// </summary>
    public class PnPCoreSiteOptions
    {
        /// <summary>
        /// The URL of the target site
        /// </summary>
        public string SiteUrl { get; set; }

        /// <summary>
        /// The Authentication Provider
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; set; }
    }
}
