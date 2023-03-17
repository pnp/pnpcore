# Legacy SharePoint AddIns

The Core SDK Admin library contains APIs to enumerate the [SharePoint AddIns](https://learn.microsoft.com/en-us/sharepoint/dev/sp-add-ins/sharepoint-add-ins) that are installed in a site collection.

[!INCLUDE [SharePoint Admin setup](fragments/setup-admin-sharepoint.md)]

> [!Note]
> If your tenant is using vanity URL's then you'll need to populate the `VanityUrlOptions` class and pass it to any method that allows it.

## Enumerating the SharePoint AddIns installed in a site collection

SharePoint AddIns are installed in sites, enumerating them then starts from the site collection you want to get the installed SharePoint AddIns from. To do so use on of the `GetSiteCollectionSharePointAddIns` methods which will return a collection of `ISharePointAddIn` object instances.

```csharp
var addIns = await context.GetSiteCollectionManager().GetSiteCollectionSharePointAddInsAsync();

foreach(addIn in addIns)
{
    // do something with the AddIn
}
```

> [!Note]
> SharePoint AddIns are typically installed per site, but sometimes the SharePOint AddIn can be installed in the corporate app catalog and deployed tenant wide. In the returned data you can see that by looking at the `Installed...` properties versus the `Current...` properties: when these are the same then the SharePoint AddIn was installed in the current site, if not then the SharePoint was installed via an app catalog and the `Installed...` properties will give you information about the app catalog where the SharePoint AddIn was installed.

## Uninstalling a SharePoint AddIn

Coming soon.
