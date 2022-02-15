# Using the PnP Core SDK Event hub

When certain key events happen in PnP Core SDK your code can be informed of that event via subscribing to the event of interest via the PnP Core SDK `EventHub`. A typical use case for this is throttling, if your code is getting throttled you might want to take action by lowering the load sent to Microsoft 365, next to the default throttling wait/retry mechanism that PnP Core SDK brings.

## Using the EventHub

The event hub is implemented via the `EventHub` class which is added a singleton to your dependency injection container when you configure PnP Core SDK. To subscribe to an event you first need to get a reference to the `EventHub` by using dependency injection. Once you've a reference then you can implement the needed events. Below sample shows a typical use case:

```csharp
public MyClass(IPnPContextFactory pnpContextFactory, EventHub eventHub)
{
    eventHub.RequestRetry = (retryEvent) => 
    {  
      // request got retried due to throttling or socket exception...    
    };
}
```

## Throttling handling via the event hub

When a request is retried due to either throttling or due to a socket exception the `RequestRetry` is fired providing you with information about the retry. Above sample already showed the code to hook up to the event, in this paragraph you'll find more details about the received `RetryEvent` instance. Following information is passed via this instance:

- **Request uri**: the full request URL of the request that will be retried
- **HttpStatusCode**: the HTTP status code that led to the retry. This will be 429, 503 or 503 in case of throttling event
- **Exception**: when the retry is triggered due to a socket exception this property will be set. Use the presence of this property to distinguish between retry due to throttling or network exceptions
- **WaitTime**: time in seconds execution is waited before the retry is made. The `RequestRetry` is fired before the actual wait starts
- **PnpContextProperties**: by default there's a property named `WebUrl` contained the URI of the SharePoint web (if available) for which the request was made. If the `PnPContext` instance that triggered the request contained custom properties (see [Advanced PnPContext use](basics-context.md) for details) then these are added here as well
