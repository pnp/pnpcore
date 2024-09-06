using System.Security.Cryptography.X509Certificates;
using Demo.AzFunction.ManagedIdentity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PnP.Core.Auth;
using PnP.Core.Services.Builder.Configuration;

var appConfig = new AppConfig();

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder => { }, options =>
    {
        // By default, exceptions thrown by your code can end up wrapped in an RpcException.
        // To remove this extra layer, set the EnableUserCodeException property to "true" as part of configuring the builder
        options.EnableUserCodeException = true;
    })
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // Configure the isolated process application to emit logs directly to Application Insights.
        // This behavior replaces the default behavior of relaying logs through the host, and is recommended because it gives you control over how those logs are emitted.
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddPnPCore(options =>
        {
            // Disable telemetry because of mixed versions on AppInsights dependencies
            options.DisableTelemetry = true;

            //If Managed Identity is configured
            if (appConfig.isMSI)
            {
                var authProvider = new ManagedIdentityTokenProvider();

                // And set it as default
                options.DefaultAuthenticationProvider = authProvider;

                // Add a default configuration with the site configured in app settings
                options.Sites.Add("Default",
                    new PnPCoreSiteOptions
                    {
                        SiteUrl = AppConfig.SiteUrl,
                        AuthenticationProvider = authProvider
                    });
            }
            else
            {
                var appConfigCert = new AppConfigCert();
                // Configure an authentication provider with certificate (Required for app only)
                // App-only authentication against SharePoint Online requires certificate based authentication for calling the "classic" SharePoint REST/CSOM APIs. The SharePoint Graph calls can work with clientid+secret, but since PnP Core SDK requires both type of APIs (as not all features are exposed via the Graph APIs) you need to use certificate based auth.
                var authProvider = new X509CertificateAuthenticationProvider(AppConfigCert.ClientId,
                    AppConfigCert.TenantId,
                    StoreName.My,
                    StoreLocation.CurrentUser,
                    AppConfigCert.CertificateThumbprint);
                // And set it as default
                options.DefaultAuthenticationProvider = authProvider;

                // Add a default configuration with the site configured in app settings
                options.Sites.Add("Default",
                       new PnP.Core.Services.Builder.Configuration.PnPCoreSiteOptions
                       {
                           SiteUrl = AppConfig.SiteUrl,
                           AuthenticationProvider = authProvider
                       });
            }
        });
    })
    .ConfigureLogging(logging =>
    {
        // By default, the Application Insights SDK adds a logging filter that instructs the logger to capture only warnings and more severe logs.
        // If you want to disable this behavior, remove the filter rule as part of service configuration.

        // Azure Functions integrates with Application Insights by storing telemetry events in Application Insights tables.
        // If you set a category log level to any value different from Information, it prevents the telemetry from flowing to those tables, and you won't be able to see related data in the Application Insights and Function Monitor tabs.
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            LoggerFilterRule defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider")!;
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    })
    .Build();

host.Run();
