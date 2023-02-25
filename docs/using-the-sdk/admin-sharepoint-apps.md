# Apps

The Core SDK Admin library provides SharePoint Apps related APIs for configuring app prerequisites for managing apps.

[!INCLUDE [SharePoint Admin setup](fragments/setup-admin-sharepoint.md)]

## Two types of app catalogs

There are two types of app catalogs available in SharePoint Online. One is global at the tenant level called tenant app catalog. It hosts all the apps that are available for all sites in your tenant. The other app catalog type is a site collection app catalog. It's aimed to host apps for the specific site collection only.  

`PnP.Core.Admin` package provides an explicit way of managing either tenant or site collection scoped apps. To do so, you should create an instance of the corresponding app manager using one of the extension methods `GetTenantAppManager` or `GetSiteCollectionAppManager`:

```csharp
// instantiate a class to work with various tenant app catalog related operations
var tenantAppManager = context.GetTenantAppManager();

// instantiate a class to work with various site collection app catalog related operations
var siteCollectionAppManager = context.GetSiteCollectionAppManager();
```

> [!Important]
> If you're willing to work with the site collection app catalog, you should call the `GetSiteCollectionAppManager` extension method using the site collection's app catalog context.

Depending on your requirements you will create any of the above objects to work with apps in your app catalog sites.

## Common app operations

Tenant and site collection app managers provide some common app operations like `GetAvailable`, `Add`, `Deploy`, `Install`, etc. These operations are available for both tenant and site collection app managers and are listed below.

In the samples below you will see a lot of `appManager` usages. `appManager` can be either tenant or site collection app catalog manager (depending on which extension method you use). As mentioned above, all the below methods apply to any of the app catalog managers.

### Get available app(s)

You can use the method `GetAvailable` to get either all or specific apps by their id or title. The app's id corresponds to the `UniqueId` field value inside the out-of-the-box `AppCatalog` list where all the apps are located.

```csharp
// gets all available apps for the app catalog
var allApps = await appManager.GetAvailableAsync();

// gets the app instance by it's unique id
var app = await appManager.GetAvailableAsync(new Guid("34427a18-486d-45d2-b4e5-9fd5324ede53"));

// get the app instance by it's title
var app = await appManager.GetAvailableAsync("title of my app");
```

### Add an app

To add an app to the app catalog you should use the `Add` method. It accepts a file path or a binary file as a bytes array:

```csharp
// adds app package to the app catalog and overwrites if there is an existing one
var app = await appManager.AddAsync("path/to/file.sppkg", true);

// adds app package using bytes array and app name
var bytes = System.IO.File.ReadAllBytes("path/to/file.sppkg");
var app = await appManager.AddAsync(bytes, "My App", true);
```

`Add` method returns an instance of the `App` object, which you can use to perform further operations.

### Deploy an app

When your app is added to the app catalog, it's not yet available to the sites. To make it available you should first deploy it using the corresponding method:

```csharp
var app = await appManager.AddAsync(packagePath, true);

// deploy the app using the app's unique id
var result = await appManager.DeployAsync(app.Id, false);
```

Upon deployment, you can specify whether to perform global deployment (when your app immediately becomes available for all the sites). In SharePoint Framework this feature is called "Skip feature deployment". When you provide `skipFeatureDeployment=true` for the `Deploy` method, the app will be deployed globally.

To deploy the app you can also use the app instance itself:

```csharp
var app = await appManager.AddAsync(packagePath, true);
// deploys app globally using app instance
await app.DeployAsync(true);
```

Under the hood, the app instance calls the `Deploy` method from the `appManager`.

### Install an app

To install the app into the specific site you need to call the `Install` method:

```csharp
// install the app to the site
await appManager.InstallAsync(app.Id);

// or use app instance
await app.InstallAsync();
```

> [!Important]
> Install operation is context-specific. It means that you should use the `PnPContext` instance from the corresponding site to install the app to that site.

### Upgrade an app

To upgrade the app on the specific site you need to call the `Upgrade` method:

```csharp
// upgrades the app on the site
await appManager.UpgradeAsync(app.Id);

// or use app instance
await app.UpgradeAsync();
```

> [!Important]
> Upgrade operation is context-specific. It means that you should use the `PnPContext` instance from the corresponding site to upgrade the app on that site.

### Uninstall an app

To uninstall the app from the specific site you need to call the `Uninstall` method:

```csharp
// uninstalls the app from the site
await appManager.uninstallAsync(app.Id);

// or use app instance
await app.UninstallAsync();
```

> [!Important]
> Uninstall operation is context-specific. It means that you should use the `PnPContext` instance from the corresponding site to uninstall the app from that site.

### Retract an app

Retract operation is the opposite to Deploy. If you don't want your app to be available for sites, you should call `Retract`. Retract command does not delete the app from the app catalog.

```csharp
// retracts app
await appManager.RetractAsync(app.Id);

// or use app instance
await app.RetractAsync();
```

### Remove an app

To completely remove the app from the app catalog you should call the `Remove` method:

```csharp
// removes the app from the corresponding app catalog
await appManager.RemoveAsync(app.Id);

// or use app instance
await app.RemoveAsync();
```

## List, approve or reject the permissions requests for an app

Some apps can request permissions to call additional APIs by adding a `webApiPermissionRequests` element in their `package-solution.json` file. Below snippet shows a part of such a file:

```json
{
    "solution": {
    "name": "apicalltest-client-side-solution",
    "id": "da4e941c-a64e-401a-b63d-664e5bf62bdc",
    "version": "1.0.0.0",
    "includeClientSideAssets": true,
    "skipFeatureDeployment": true,
    "isDomainIsolated": false,
    "webApiPermissionRequests": [
      {
        "resource": "Microsoft Graph",
        "scope": "Calendars.Read"
      },
      {
        "resource": "Microsoft Graph",
        "scope": "User.ReadBasic.All"
      }
    ]
}
```

After adding and deploying an app to the app catalog these API permissions need to be approved by an admin (e.g. via https://contoso-admin.sharepoint.com/_layouts/15/online/AdminHome.aspx#/webApiPermissionManagement) or via code. The code approach can be implemented using the `IServicePrincipal` class and the `GetPermissionRequests`, `ApprovePermissionRequest` and `DenyPermissionRequest` methods:

```csharp
// List the permission requests that are pending approval or rejection
var appManager = context.GetTenantAppManager();
List<IPermissionRequest> permissionRequests = await appManager.ServicePrincipal.GetPermissionRequestsAsync();

// Approve a permission request
var result = await appManager.ServicePrincipal.ApprovePermissionRequestAsync(permissionRequests.First().Id.ToString());

// Deny a permission request
await appManager.ServicePrincipal.DenyPermissionRequestAsync(permissionRequests.First().Id.ToString());
```
## Enable or disable the SharePoint app service principal

The SharePoint app service can be enabled or disabled via Azure Active Directory portal or via code. The `Enable` and `Disable` operations can be found on the `IServicePrincipal` interface:

```csharp
// Enable the ServicePrincipal
var appManager = context.GetTenantAppManager();
IServicePrincipalProperties result = await appManager.ServicePrincipal.Enable();

// Disable the ServicePrincipal
var appManager = context.GetTenantAppManager();
IServicePrincipalProperties result = await appManager.ServicePrincipal.Disable();
```

## Tenant app catalog specific operations

Some methods are available only for the tenant app catalog. They are listed below.

### Getting the url for the tenant app catalog site

When you're running setup tasks you need to ensure there's an app catalog site setup, using the `GetTenantAppCatalogUri` methods you can get the current tenant app catalog site url:

```csharp
// Get the tenant app catalog url, returns null if there's none setup
var url = await context.GetTenantAppManager().GetTenantAppCatalogUriAsync();
```

### Ensuring there's a tenant app catalog

If you want to ensure there's a tenant app catalog because you need to deploy an app to it, you can use the `EnsureTenantAppCatalog` methods. If the tenant app catalog site exists the methods return false, if there was no app catalog it will be setup using the default path of `sites/appcatalog` and the method returns true.

```csharp
// Get the tenant app catalog url, returns null if there's none setup
if (await context.GetTenantAppManager().EnsureTenantAppCatalogAsync())
{
    // App catalog site was missing, but now added as /sites/appcatalog
}
else
{
    // The app catalog site was already available
}
```

Note that you have to use the `GetTenantAppCatalogUri` to get the actual app catalog site url, even when there was no app catalog site and it was created by calling `EnsureTenantAppCatalog` it's still recommended to get the actual url.

### List all site collection app catalogs

Using the `GetSiteCollectionAppCatalogs` you can get all site collection app catalogs from the whole tenant:

```csharp
var tenantAppManager = context.GetTenantAppManager();
var siteAppCatalogList = await tenantAppManager.GetSiteCollectionAppCatalogsAsync();
```

The result includes site collection app catalog metadata like absolute url and unique id.

### Ensuring there's a site collection app catalog

To ensure a site collection app catalog does exist you can use the `EnsureSiteCollectionAppCatalog` methods, they check if there's an app catalog and if not one is created by calling the `AddSiteCollectionAppCatalog` methods.

```csharp
var tenantAppManager = context.GetTenantAppManager();
await tenantAppManager.EnsureSiteCollectionAppCatalogAsync("https://contoso.sharepoint.com/sites/sitethatneedsacatalog");
```

### Removing a site collection app catalog

If the app catalog of a site is not needed anymore then it can be removed using the `RemoveSiteCollectionAppCatalog` methods:

```csharp
var tenantAppManager = context.GetTenantAppManager();
await tenantAppManager.RemoveSiteCollectionAppCatalogAsync("https://contoso.sharepoint.com/sites/sitethatneedsacatalog");
```

### Check whether the solution contains MS Teams component

SharePoint Framework solutions might extend MS Teams as well. You can check, whether a particular SPFx app extends MS Teams or not:

```csharp
// get tenant app manager 
var tenantAppManager = context.GetTenantAppManager();

// get app
var app = await tenantAppManager.GetAvailableAsync("My App Title");

// check it
var containsTeams = await tenantAppManager.SolutionContainsTeamsComponentAsync(app.Id);
```

For example, If your SPFx solution lists any webpart with `TeamsPersonalApp` or `TeamsTab` as supported hosts, the method above will return *true*.

### Get all apps, acquired from SharePoint Store

Your tenant app catalog hosts not only custom apps, but also apps installed from the SharePoint store. To easily get all third-party apps from the SharePoint Store, use the method below:

```csharp
var tenantAppManager = context.GetTenantAppManager();
var storeApps = await tenantAppManager.GetStoreAppsAsync();
```

`storeApps` is a collection of App instances, where you can do all common operations like `Install`, `UnInstall`, etc.

### Automate SharePoint Store apps deployment and installation

With tenant app manager you can also automate the process of installing apps from the SharePoint Store. To deploy an app from SharePoint Store you should know the unique store asset id. You can easily find it in the query string on the app home page. Usually the url has a format `https://appsource.microsoft.com/en-us/product/office/WA200001111` or `<your tenant>sites/appcatalog/_layouts/15/appStore.aspx/appDetail/WA200001111`. The last part of the url is your asset id, i.e. `WA200001111`. Then you can use the below code to add the app:

```csharp
var tenantAppManager = context.GetTenantAppManager();
var app = tenantAppManager.AddAndDeployStoreApp("WA200001111", CultureInfo.GetCultureInfo(1033).Name, false, true);
```

The method returns an App instance so that you can further install it to any SharePoint site using apps API.

### Check if an upgrade for an app is available

Use below API to check whether the upgrade is available for the specific app on a site:

```csharp
var tenantAppManager = context.GetTenantAppManager();
var app = await tenantAppManager.GetAvailableAsync("My App Title");
var result = await tenantAppManager.IsAppUpgradeAvailableAsync(app.Id);
```

The method returns *true* if the app can be upgraded from the app catalog using the most recent version.

### Download MS Teams solution from SharePoint Framework app

SharePoint Framework solutions might extend MS Teams as well. If you want to download Teams specific component from SharePoint Framework solution, you should use `DownloadTeamsSolution` method:

```csharp
var tenantAppManager = context.GetTenantAppManager();
var app = await tenantAppManager.GetAvailableAsync("My App Title");

// download MS Teams's solution stream
using (var stream = await tenantAppManager.DownloadTeamsSolutionAsync(app.Id))
using (var outputFileStream = new FileStream("teams app.zip", FileMode.Create))
{
    // save stream to a file
    stream.CopyTo(outputFileStream);
}
```
