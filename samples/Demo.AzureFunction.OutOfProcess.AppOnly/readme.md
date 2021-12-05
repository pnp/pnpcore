# PnP Core SDK - Azure Function v4 Sample using out-of-process with .NET 6

This solution demonstrates how to build a simple backend API in the form of an HTTP Trigger Azure Function. The sample contains a `CreateSite` function that creates a new site collection including a new site page containing some text and an image. This Azure function is using application permissions (app-only).

## Source code

You can find the sample source code here: [/samples/Demo.AzureFunction.OutOfProcess.AppOnly](https://github.com/pnp/pnpcore/tree/dev/samples/Demo.AzureFunction.OutOfProcess.AppOnly)

> [!Note]
> This sample was created with [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) using [.NET 6.0](https://dotnet.microsoft.com/) and has been created as an [Azure Function v4](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process) running out-of-process (so using process isolation).

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
>
> - Approving application permissions requires you to use a user which is Azure AD admin or global admin in your tenant
> - Once this cmdlet is done you do need to to copy **certificate thumbprint** and **ClientId** values as these will be needed for the configuration step

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

If you're using Visual Studio then press **F5** to launch the sample, if you're using command line then use `func start`.

> [!Note]
> To use command line you first have to install the [Azure function core tools version 4.x](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash%2Ckeda#v2)

With the sample running go to your browser and load this url: `http://localhost:7071/api/CreateSite?owner=joe@contoso.onmicrosoft.com&sitename=azurefunctiondemo001`, doing this will call into your Azure function passing the owner of the site collection that will be created and the site collection name. You have to update the url parameters accordingly, above sample will result in a site collection with url https://contoso.sharepoint.com/sites/azurefunctiondemo001 having joe@contoso.onmicrosoft.com set as owner.

## Deploy the sample to Azure

### Create Azure Function App

Go to the [Azure Portal](https://portal.azure.com/) and create a new Function App (consumption plan) using following settings:

- Publish: **Code**
- Runtime stack: **.NET**
- Version: **6**
- Region: pick the region that works best for you

Click **Review + create**, verify the settings and click **Create**. Now your function is provisioned in Azure.

### Configure the Function App

Once the Function App has been created navigate to **Settings** -> **Configuration** and add the following **Application settings**:

Name | Value
-----|------
SiteUrl | base site collection url to connect to, e.g. https://contoso.sharepoint.com
TenantId | tenant id, e.g. 9bd71689-66cb-4560-bb09-ab908ec21437
ClientId | application client id, e.g. 8bb62681-cddd-41e0-bfdf-ab908ec8a3c3

Click **Save** to persist the changes.

Under **Function runtime settings** verify the Runtime version is set to **~4**.

### Deploying the certificate - use KeyVault

Final configuration step needed is ensuring the Azure Function App can use the configured certificate. Quite often you want to reuse your certificate across multiple Azure resources and then consolidating all secrets and certificates in an Azure KeyVault is a commonly used scenario. So let's explain how to make that happen.

- Ensure the managed identity of the function is on: click on **Settings** -> **Identity** and ensure System assigned status is set to **On**
- Navigate to your Azure KeyVault or create a new one if you've none available
- In KeyVault click on **Settings** -> **Certificates** -> **Generate/Import** -> select **Import** in the dropdown and provide a name and path the PFX file you've created earlier on via the `Register-PnPAzureADApp` Powershell cmdlet. Click **Create** to add the certificate
- In KeyVault click on **Settings** -> **Access Policies** -> **+ Add Access Policy**:
  - Select Secret Permission **Get**
  - Select Certificate Permission **Get**
  - Select principal:
    - Click on **none selected**
    - In the **Principal** page enter the name of the Azure Function App, this selects the managed identity that you've enabled before. Click **Select** to pick that principal
  - Click **Add** to add the new Access Policy
  - Click **Save** to persist the changes

Using above steps you've now uploaded your certificate in a KeyVault and you've enabled your Function App to read secrets from the vault using it's managed identity. Final step is letting the Azure Function App now which certificate to pick. For that follow these steps:

- In KeyVault click on your certificate and then click on the **Current version**. The certificate details will be shown, copy the **Certificate Identifier** e.g. `https://mykeyvault.vault.azure.net/certificates/PnPCoreSDKDemo/6c752ad7218248b0976f387ef288523a`
- In the Function App navigate to **Settings** -> **Configuration** and add the following **Application setting**. Note the certificate version identifier has been dropped from the URL.

Name | Value
-----|------
CertificateFromKeyVault | @Microsoft.KeyVault(SecretUri=https://mykeyvault.vault.azure.net/certificates/PnPCoreSDKDemo/)

Click **Save** to persist the changes.

### Deploying the certificate - local upload to Function App

Previous steps showed you how to manage your certificate via KeyVault, but you can also opt to simply upload the certificate to the Azure Function App.

- Navigate to **Settings** -> **TLS/SSL Settings** and click on **Private Key Certificates (.pfx)**
- Click on **Upload Certificate**, browse to the location of the PFX file you've created earlier on via the `Register-PnPAzureADApp` Powershell cmdlet. Click **Upload** to upload the certificate
- Navigate to **Settings** -> **Configuration** and add the following **Application settings**

Name | Value
-----|------
CertificateThumbPrint | thumbprint, e.g. 1C3342BA9B5269FDBCDCAB5D6334F1A60C73B184
WEBSITE_LOAD_CERTIFICATES | thumbprint, e.g. 1C3342BA9B5269FDBCDCAB5D6334F1A60C73B184

### Deploy the Function App code from Visual Studio

Final step now that the Function App is configured is to deploy our bits.

- Right click the project in Visual Studio and choose **Publish...**
- Chose the **Import Profile** option:
  - Navigate to your Azure Function App in Azure Portal and click on **Get publish profile** from the **Overview** page
  - Select the downloaded profile
- Click on **Publish** to push your project to the Azure Function App

### Test your Function App in Azure

To test your Function App you now need to build an URL that points to your Azure Function App + it's function authorization key. To that URL the `owner` and `siteName` URL parameters have to be added. To get your Azure Function URL follow these steps:

- Navigate to your Azure Function App in Azure Portal and Navigate to **Functions** -> **Functions**
- Click on the **CreateSite** function
- Click on **Get Function Url** and copy the proposed URL
- Append `&owner=joe@contoso.onmicrosoft.com&sitename=azurefunctiondemo001`

The final result will be something along these lines:

`https://myfunctionapphost.azurewebsites.net/api/CreateSite?code=OerO/tVAIGbCacM1e6PYi6MsH5rsHmzpjUmZMlKTYayDhcYMJ9zjZw==&owner=joe@contoso.onmicrosoft.com&sitename=azurefunctiondemo001`
