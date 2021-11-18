# PnP Core SDK - Azure Function v4 Sample using out-of-process with .NET 6

This solution demonstrates how to build a simple backend API in the form of an HTTP Trigger Azure Function. The sample contains a `ProvisionSite` function that creates a new site collection including a new site page containing some text and an image. This Azure function is using application permissions (app-only).

## Source code

You can find the sample source code here: [/samples/Demo.AzureFunction.OutOfProcess.AppOnly](https://github.com/pnp/pnpcore/tree/dev/samples/Demo.AzureFunction.OutOfProcess.AppOnly)

> [!Note]
> This sample was created with Visual Studio 2022 using .NET 6.0 and has been tested with a Windows Azure Function v4 running out-of-process (so using process isolation).

## Sample configuration

> [!Note]
> For the sample setup, you will need to have a recent version of [PnP.PowerShell](https://pnp.github.io/powershell/) installed on your machine.

### Create and configure the Azure AD application

Using PnP PowerShell this becomes really simple. Below cmdlet will create a new Azure AD application, will create a new self-signed certificate and will configure that cert with the Azure AD application. Finally the right permissions are configured and you're prompted to consent these permissions.

```powershell
# Ensure you replace contoso.onmicrosoft.com with your Azure AD tenant name
# Ensure you replace joe@contoso.onmicrosoft.com with the user id that's an Azure AD admin (or global admin)

Register-PnPAzureADApp -ApplicationName FunctionDemoSiteProvisiong -Tenant contoso.onmicrosoft.com -Store CurrentUser -GraphApplicationPermissions "Sites.FullControl.All" -SharePointApplicationPermissions "Sites.FullControl.All" -Username "joe@contoso.onmicrosoft.com" -Interactive
```

> [!Important]
> Once this cmdlet is done you do need to to copy **certificate thumbprint** and **ClientId** values as these will be needed for the configuration step

If you prefer to manually create and configure the Azure AD application then follow these steps:

1. Create a new (self-signed) certificate
2. Create a new Azure AD application and take note of the shown **Application (client) ID**
3. Under **Certificates & secrets** upload your certificate and take note of the shown **thumbprint**
4. Under **API permissions** add these two **application** permissions:
   1. Microsoft Graph -> `Sites.FullControl.All` application permission
   2. SharePoint -> `Sites.FullControl.All` application permission
5. Click on **Grant consent for** to consent the permissions for the application

### Configure the sample's configuration file

The demo function needs a configuration file named `local.settings.json`, copy the `local.settings.copyme.json` file into a file named `local.settings.json` and in the file properties set `Copy to output directory` to `Copy if newer`. 

Once that's done you do have this file in project:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "SiteUrl": "<base site collection url to connect to>",
    "TenantId": "<tenant id>",
    "ClientId": "<application client id>",
    "CertificateThumbPrint": "<thumbprint>",
    "WEBSITE_LOAD_CERTIFICATES": "<thumbprint>"
  },
  "Host": {
    "CORS": "*"
  }
}
```

After filling in the configuration data your file will look like this:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "SiteUrl": "https://contoso.sharepoint.com",
    "TenantId": "9bd71689-66cb-4560-bb09-ab908ec21437",
    "ClientId": "8bb62681-cddd-41e0-bfdf-ab908ec8a3c3",
    "CertificateThumbPrint": "1C3342BA9B5269FDBCDCAB5D6334F1A60C73B184",
    "WEBSITE_LOAD_CERTIFICATES": "1C3342BA9B5269FDBCDCAB5D6334F1A60C73B184"
  },
  "Host": {
    "CORS": "*"
  }
}
```

## Run the sample

If you're using Visual Studio then press F5 to launch the sample, if you're using command line then use `func start`.

> [!Note]
> To use command line you first have to install the [Azure function core tools version 4.x](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash%2Ckeda#v2)

With the sample running go to your browser and load this url: `http://localhost:7071/api/CreateSite?owner=joe@contoso.onmicrosoft.com&sitename=azurefunctiondemo001`, doing this will call into your Azure function passing the owner of the site collection that will be created and the site collection name. You have to update the url parameters accordingly, above sample will result in a site collection with url https://contoso.sharepoint.com/sites/azurefunctiondemo001 having joe@contoso.onmicrosoft.com set as owner.

