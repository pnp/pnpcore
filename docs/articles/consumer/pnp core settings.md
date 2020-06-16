# Configuring the PnP Core SDK via settings

The PnP Core SDK uses a default configuration and as such you're not required to provide specific settings. However, if you want to change the settings, then providing the custom settings via a settings service (e.g. settings file) is the way to go.

## Sample settings file

Below snippet shows the settings which are used by the PnP Core SDK, you can simply include this snippet in your application settings file next to your custom settings.

```json
{
  "PnPCore": {
    "DisableTelemetry": false,
    "PnPContext": {
      "GraphFirst": true,
      "GraphAlwaysUseBeta": false,
      "GraphCanUseBeta": true
    },
    "HttpRequests": {
      "UserAgent": "ISV|Contoso|ProductX",
      "SharePointRest": {
        "UseRetryAfterHeader": false,
        "MaxRetries": 10,
        "DelayInSeconds": 3,
        "IncrementalDelay": true
      },
      "MicrosoftGraph": {
        "UseRetryAfterHeader": true,
        "MaxRetries": 10,
        "DelayInSeconds": 3,
        "IncrementalDelay": true
      }
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
PnPCore:PnPContext:GraphAlwaysUsesBeta | false | The library by default uses the production v1.0 Microsoft Graph API. Use this setting to default it to the beta Microsoft Graph API.
PnPCore:PnPContext:GraphCanUseBeta | true | When you ask for data that can only be provided via the Microsoft Graph beta API the PnP Core SDK will use the beta endpoint for that specific request. All other requests will still use the v1.0 endpoint. If you set this to false, then any request that requires Microsoft Graph beta will not provide any result.
PnPCore:HttpRequests:UserAgent | NONISV&#124;SharePointPnP&#124;PnPCoreSDK | Value set as user agent when the request is sent to Microsoft 365.
PnPCore:HttpRequests:SharePointRest:UseRetryAfterHeader | Use retry-after http header when calculating the wait time in seconds for SharePoint Rest request retry. Defaults to false.
PnPCore:HttpRequests:SharePointRest:MaxRetries | Maximum number of retries before retrying a SharePoint Rest request throws an exception. Defaults to 10.
PnPCore:HttpRequests:SharePointRest:DelayInSeconds | Delay in seconds between SharePoint Rest request retries. Defaults to 3.
PnPCore:HttpRequests:SharePointRest:IncrementalDelay | Delays get incrementally longer with each retry. Defaults to true.
PnPCore:HttpRequests:MicrosoftGraph:UseRetryAfterHeader | Use retry-after http header when calculating the wait time in seconds for Microsoft Graph request retry. Defaults to true.
PnPCore:HttpRequests:MicrosoftGraph:MaxRetries | Maximum number of retries before retrying a Microsoft Graph request throws an exception. Defaults to 10.
PnPCore:HttpRequests:MicrosoftGraph:DelayInSeconds | Delay in seconds between Microsoft Graph request retries. Defaults to 3.
PnPCore:HttpRequests:MicrosoftGraph:IncrementalDelay | Delays get incrementally longer with each retry. Defaults to true.
PnPCore:DisableTelemetry | false | Allows to turn off telemetry being sent. Telemetry is used to improve this open source library and it's recommended to keep it on, but you can disable it, if required.
