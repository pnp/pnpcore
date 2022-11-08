# Configuring the PnP Core SDK via settings

The PnP Core SDK uses a default configuration and as such you're not required to provide specific settings. However, if you want to change the settings, then providing the custom settings via a settings service (e.g. settings file) is the way to go.

## Sample settings file

Below snippet shows the settings which are used by the PnP Core SDK, you can simply include this snippet in your application settings file next to your custom settings.

```json
{
  "PnPCore": {
    "DisableTelemetry": "false",
    "Environment": "Production",
    "HttpRequests": {
      "UserAgent": "ISV|Contoso|ProductX",
      "Timeout": "100",
      "SharePointRest": {
        "UseRetryAfterHeader": "false",
        "MaxRetries": "10",
        "DelayInSeconds": "3",
        "UseIncrementalDelay": "true",
        "DefaultPageSize": 100
      },
      "MicrosoftGraph": {
        "UseRetryAfterHeader": "true",
        "MaxRetries": "10",
        "DelayInSeconds": "3",
        "UseIncrementalDelay": "true"
      }
    },
    "PnPContext": {
      "GraphFirst": "true",
      "GraphCanUseBeta": "true",
      "GraphAlwaysUseBeta": "false"
    },
    "Credentials": {
      "CredentialManagerAuthentication": {
        "CredentialManagerName": "mycreds"
      }
    },
    "Sites": {
      "SiteToWorkWith": {
        "SiteUrl": "https://contoso.sharepoint.com/sites/pnp",
        "AuthenticationProviderName": "CredentialManagerAuthentication"
      },
    }
  },

  // Not really library related, but since the library assumes logging is connected it's being shown here
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

## Settings overview

Setting | Default value | Description
--------|---------------|------------
Logging:LogLevel:Default | Information | Allows you to change log level. See the [.Net Logging article](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1#log-level) for more details. Common levels are Debug and Information.
PnPCore:PnPContext:GraphFirst | true | If for a given request the library can choose between a SharePoint REST or a Microsoft Graph call then it will favor the Microsoft Graph call. Settings this to false will prefer SharePoint REST for all SharePoint related API calls.
PnPCore:PnPContext:GraphCanUseBeta | true | When you ask for data that can only be provided via the Microsoft Graph beta API the PnP Core SDK will use the beta endpoint for that specific request. All other requests will still use the v1.0 endpoint. If you set this to false, then any request that requires Microsoft Graph beta will not provide any result.
PnPCore:PnPContext:GraphAlwaysUsesBeta | false | The library by default uses the production v1.0 Microsoft Graph API. Use this setting to default it to the beta Microsoft Graph API.
PnPCore:HttpRequests:UserAgent | NONISV&#124;SharePointPnP<br />&#124;PnPCoreSDK | Value set as user agent when the request is sent to Microsoft 365.
PnPCore:HttpRequests:Timeout | 100 | Timeout in seconds for HTTP requests. Set higher if you need to for example download large files. Setting to -1 will result in an infinite timeout.
PnPCore:HttpRequests:RateLimiterMinimumCapacityLeft | 0 | Setting this value between 1 and 20 will result in request delaying when RateLimit headers indicate the application is about to get throttled
PnPCore:HttpRequests:SharePointRest:UseRetryAfterHeader | false | Use retry-after http header when calculating the wait time in seconds for SharePoint Rest request retry.
PnPCore:HttpRequests:SharePointRest:MaxRetries | 10 | Maximum number of retries before retrying a SharePoint Rest request throws an exception.
PnPCore:HttpRequests:SharePointRest:DelayInSeconds | 3 | Delay in seconds between SharePoint Rest request retries.
PnPCore:HttpRequests:SharePointRest:IncrementalDelay | true | Delays get incrementally longer with each retry.
PnPCore:HttpRequests:SharePointRest:DefaultPageSize | 100 | Page size using when paging is automatically applied during data querying via the PnP Core SDK LINQ support.
PnPCore:HttpRequests:MicrosoftGraph:UseRetryAfterHeader | true | Use retry-after http header when calculating the wait time in seconds for Microsoft Graph request retry.
PnPCore:HttpRequests:MicrosoftGraph:MaxRetries | 10 | Maximum number of retries before retrying a Microsoft Graph request throws an exception.
PnPCore:HttpRequests:MicrosoftGraph:DelayInSeconds | 3 | Delay in seconds between Microsoft Graph request retries.
PnPCore:HttpRequests:MicrosoftGraph:IncrementalDelay | true | Delays get incrementally longer with each retry.
PnPCore:Environment | Production | Use this setting if you are using a cloud environment **different** from the standard production cloud: possible values are `Production`, `PreProduction`, `USGovernment` (a.k.a GCC), `USGovernmentHigh` (a.k.a GCC High), `USGovernmentDoD` (a.k.a DoD), `China` and `Germany`. **Important:** use the correct casing when using these values.
PnPCore:DisableTelemetry | false | Allows to turn off telemetry being sent. Telemetry is used to improve this open source library and it's recommended to keep it on, but you can disable it, if required.
PnPCore:Credentials | | This section defines the settings for the Authentication Providers and it will be updated in the near future.
PnPCore:Sites | | This section defines the site collections to consume using the PnP Core SDK. Every single item of the array has a name, which can then be used with the PnPContextFactory to retrieve an instance of PnPContext for that specific site, a _SiteUrl_ and the _AuthenticationProviderName_ that maps to the corresponding Authentication Provider to use for accessing the target site.