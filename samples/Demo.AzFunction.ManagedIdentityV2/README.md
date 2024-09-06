# PnP Core SDK - Azure Function with Managed Identity Sample


> [!Important]
> Beginning **10 November 2026**, the **in-process model** for .NET apps in Azure Functions will **no longer be supported**. To ensure that your apps that use this model continue being supported, you'll need to transition to the isolated worker model by that date. [Retirement: Support for the in-process model for .NET apps in Azure Functions ends 10 November 2026.](https://azure.microsoft.com/en-us/updates/retirement-support-for-the-inprocess-model-for-net-apps-in-azure-functions-ends-10-november-2026/)
> This sample is a new version of the **Azure V4 Function using managed identity (in-process)** using in-process model.

This solution demonstrates how to build Azure function that connects to a SPO site using:

- System-Managed Identity, when running in Azure and
- Azure Ad application (App Registration) during local development.

The Authentication Provider is selected based on the presence of the `MSI_SECRET` token, which is only available if Managed Identity is enabled.

When the function is executed in Azure, it uses custom _ManagedIdentityTokenProvider_ authentication provider, which invokes `Azure.Identity.ManagedIdentityCredential` class to attempt authentication using a managed identity that has been assigned to the Azure Function.

For local development, certificate-based authentication with _X509CertificateAuthenticationProvider_ is required.
> App-only authentication against SharePoint Online requires certificate based authentication for calling the "classic" SharePoint REST/CSOM APIs. The SharePoint Graph calls can work with clientid+secret, but since PnP Core SDK requires both type of APIs (as not all features are exposed via the Graph APIs) you need to use certificate based auth.

This solution follows the principle of least privilege, by using `Sites.Selected` application permissions for Graph and SharePoint APIs, and `Read`/`Write`/`FullControl` permissions granted to a specific SPO site.
> Always choose minimum required permissions.

## Source code

> [!Note]
> This sample was authored by [Kinga Kazala](https://github.com/kkazala) 💪🥇.

You can find the sample source code here: [/samples/Demo.AzFunction.ManagedIdentityV2](https://github.com/pnp/pnpcore/tree/dev/samples/Demo.AzFunction.ManagedIdentityV2)

> [!Note]
> This sample was created with [Visual Studio Code](https://code.visualstudio.com/) using [.NET 8.0](https://dotnet.microsoft.com/) and has been created as an [Azure Function v4](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process) running in the [isolated worker model](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows).

## Sample configuration

> [!Note]
> For the sample setup, you will need to have a recent version of [PnP.PowerShell](https://pnp.github.io/powershell/) installed on your machine.

### Create and configure the Azure AD applications

The Azure Function must have System-Managed Identity enabled. Before you execute the next steps, make sure you [enable System-Managed Identity](https://learn.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=portal%2Chttp#add-a-system-assigned-identity).

The configuration script:

- Creates a new App Registration with a name `{$appName}-LocalDev` and generates a new self-signed certificate
- Grants `Sites.Selected` API Permissions to the `{$appName}` Managed Identity and the `{$appName}-LocalDev` Azure AD application
- Grants permissions defined in `Permissions` parameter to the `{$appName}` Managed Identity and the `{$appName}-LocalDev` Azure AD application
- saves the site and tenant information, client id and the certificate thumbprint to the `local.settings.json` configuration file.

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "SiteUrl": "",
    "TenantId": "[TENANT ID]",
    "ClientId": "[CLIENT ID]",
    "CertificateThumbPrint": "[CERTIFICATE THUMBPRINT]",
    "WEBSITE_LOAD_CERTIFICATES": "[CERTIFICATE THUMBPRINT]"
  }
}
```

> [!Important]
> Currently, you cannot **manually** grant API Permissions to the System-Managed Identity

Execute the [Configure.ps1](https://github.com/pnp/pnpcore/tree/dev/samples/Demo.AzFunction.ManagedIdentity/Tools/Configure.ps1), defining the `Permissions` level that your application requires.

This sample requires `FullControl` permissions because it creates a new list.
> [!Note]
>You may decide to only grant `Write` permissions to see the REST errors once the code reaches method requiring FullControl permissions.
>
> ![access denied](docs-images/accessdenied.png)

```bash
.\Configure.ps1 -SiteUrl $siteUrl -TenantId $tenantId -AzureADAppName $appName -Permissions FullControl -CertificatePwd ""
```

> [!Important]
>
> - To consent to the application permissions, make sure you are an Azure AD admin or a global admin in your tenant

## Run the sample locally

> [!Note]
> Before you get started, make sure to [configure your environment](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process#configure-your-environment).

To test your code locally, follow the [Run the function locally](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process#run-the-function-locally) procedure.

Observe the output printed to the **Terminal**. It will confirm you are running your code locally ("_Local DEV using cert auth_"), authenticate to the SharePoint site and attempt to execute functions requiring Read, Write and FullControl permissions.

In case you only granted `Write` permissions to the app, you will see an error message when the function attempts to create a new list.
> ![terminal output error](docs-images/terminalError.png)

If you granted `FullControl` permissions, all the steps will be completed successfully.
> ![terminal output success](docs-images/terminalOK.png)

## Deploy the sample to Azure

### Create Azure Function App

Go to the [Azure Portal](https://portal.azure.com/) and create a new Function App (consumption plan) using following settings:

- Runtime stack: **.NET**
- Version: **8 (LTS), isolated worked model**
- Region: pick the region that works best for you
- Operating System: **Windows**

> Note: This sample has only been tested with the above configuration.


Click **Review + create**, verify the settings and click **Create**. Now your function is provisioned in Azure.

### Configure the Function App

Once the Function App has been created navigate to **Settings** -> **Configuration** and add the following **Application setting**:

Name | Value
-----|------
SiteUrl | the Url of your SPO site, e.g. <https://contoso.sharepoint.com/sites/subsiteName>

Click **Save** to persist the changes.

> [!Note]
> You don't need `TenantId`, `ClientId`, `CertificateThumbPrint` or `WEBSITE_LOAD_CERTIFICATES` application settings when using Managed Identity authentication.

Under **Function runtime settings** verify the Runtime version is set to **~4**.

### Deploy the Function App code from Visual Studio

To deploy the project to Azure:

- [Sign to Azure](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process#sign-in-to-azure)
- [Deploy the project to Azure](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process#deploy-the-project-to-azure)

### Test your Function App in Azure

Test the function in Azure using the [run the function in Azure](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp) procedure.

### Performance optimizations
See [Performance optimizations](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#performance-optimizations) for changes you can implement to improve performance of your Azure Function.