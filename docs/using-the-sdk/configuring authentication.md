
# Configuring authentication

The PnP Core SDK works with both SharePoint REST as Microsoft Graph in a transparent way, this also means that the authentication model used must work for both. The chosen authentication model is Azure Active Directory (a.k.a. Azure AD), using Azure Active Directory you can define an application and grant it permissions to access Microsoft 365 workloads like SharePoint, Teams,...**Configuring your own application is the recommended approach**, but you can also use a multi-tenant application that the PnP team created. Both options are detailed in the next chapters

## I want to configure my own Azure AD application (recommended)

When configuring your Azure AD application you'll need to also defined which delegated and/or application permissions your application needs. **It's recommended to use the minimal permissions needed for the application at hand**, for example if your app is not using the managed metadata features of the SDK, then there's no need to request TermStore permissions. In below setup instructions we assume your app wants to use the main features of PnP Core SDK, but the list of shown permissions can be to narrow or to wide depending on your actual application needs. When you want to experiment with the needed permissions then this will be the easiest on a tenant you're admin of, for example a [free Microsoft 365 developer tenant](https://developer.microsoft.com/en-us/microsoft-365/dev-program) is ideal for developing and testing.

### Delegated Permissions (acting in the name of the user)

In this section you can learn how to register an application in Azure Active Directory and how to use it in your .NET code, in order to use the PnP Core SDK with interactive login in a Console application, running your requests in the name of the authenticated user.

#### Configuring the application in Azure AD

In this step by step guide you will register an application in Azure Active Directory, in order to consume the PnP Core SDK in the name of the user connected to your app (i.e. with a delegated access token) from within a .NET Core Console application.
Follow below steps to configure an application in Azure AD:

1. Navigate to https://aad.portal.azure.com/
2. Click on **Azure Active Directory** from the left navigation
3. Click on **App registrations** in the **Manage** left navigation group
4. Click on **New registration**
5. Give the application a name (e.g. PnP Core SDK) and click on **Register**
6. Copy the **Application ID** (Client ID) from the **Overview** page, you'll need this GUID value later on
7. Copy the **Directory ID** (Tenant ID) from the **Overview** page, you'll need this GUID value later on
8. Click on the **API Permissions** in the **Manage** left navigation group
9. Click on **Add Permissions** and add the permissions you want to give to this application. Below list is a recommendation, you can grant less permissions but that might result in some PnP Core SDK calls to fail due getting access denied errors.

   - SharePoint -> Delegated Permissions -> AllSites -> AllSites.FullControl
   - SharePoint -> Delegated Permissions -> Sites -> Sites.Search.All
   - SharePoint -> Delegated Permissions -> TermStore -> TermStore.ReadWrite.All
   - SharePoint -> Delegated Permissions -> User -> User.ReadWrite.All
   - Microsoft Graph -> Delegated Permissions -> User -> User.Read
   - Microsoft Graph -> Delegated Permissions -> Directory -> Directory.ReadWrite.All
   - Microsoft Graph -> Delegated Permissions -> Directory -> Directory.AccessAsUser.All
   - Microsoft Graph -> Delegated Permissions -> Group -> Group.ReadWrite.All

10. Click on the **Grant admin consent for** button to consent to these permissions for the users in your organization
11. Click on **Authentication** in the **Manage** left navigation group
12. Change **Default client type** to **Treat application as public client** and hit **Save** (this step is optional and you should do that if and only if you are planning to use the `UsernamePasswordAuthenticationProvider` or the `CredentialManagerAuthenticationProvider` for authentication)

If you want to configure support for interactive login you should also configure the _Platform_ and the _redirect URI_ in the **Authentication** panel. You can read [further details here](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app#add-a-redirect-uri).

13. Click on **Authentication** and then click on **Add a platform**, choose **Mobile and desktop applications** and provide http://localhost as the **Redirect URI**

> [!Note]
> It's recommended to align the actually required permissions with the needs of your application.

#### Configuring PnP Core SDK to use the configured application

When you're configuring your application to use the PnP Core SDK you will have to configure the `PnP.Core` services and the `PnP.Core.Auth` services using the `AddPnPCore` and `AddPnPCoreAuthentication` methods, respectively. The `ClientId` and `TenantId` are those of the application that you just registered in Azure Active Directory. The value for the `CredentialManagerName` property is the name of the item stored in the Windows Credential Manager.

```csharp
// Add the PnP Core SDK library
services.AddPnPCore(options => {
    options.PnPContext.GraphFirst = true;
    options.HttpRequests.UserAgent = "ISV|Contoso|ProductX";

    options.Sites.Add("SiteToWorkWith", new PnPCoreSiteOptions
    {
        SiteUrl = "https://contoso.sharepoint.com/sites/pnp"
    });
});
services.AddPnPCoreAuthentication(
    options => {
        // Configure an Authentication Provider relying on Windows Credential Manager
        options.Credentials.Configurations.Add("interactive",
            new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = "{your_client_id}",
                TenantId = "{your_tenant_id}",
                Interactive = new PnPCoreAuthenticationInteractiveOptions
                {
                    RedirectUri = new Uri("http://localhost")
                }
            });

        // Configure the default authentication provider
        options.Credentials.DefaultConfiguration = "interactive";

        // Map the site defined in AddPnPCore with the 
        // Authentication Provider configured in this action
        options.Sites.Add("SiteToWorkWith",
            new PnPCoreAuthenticationSiteOptions
            {
                AuthenticationProviderName = "interactive"
            });
});
```

### Application Permissions (acting as an app account with app-only permissions)

In this section you can learn how to register an application in Azure Active Directory and how to use it in your .NET code, in order to use the PnP Core SDK within a background job/service/function, running your requests with an app account.

#### Configuring the application in Azure AD

The easiest way to register an application in Azure Active Directory for app-only is to use the [PnP PowerShell](https://docs.microsoft.com/en-us/powershell/sharepoint/sharepoint-pnp/sharepoint-pnp-cmdlets?view=sharepoint-ps) cmdlets. Specifically you can use the [`Register-PnPAzureADApp` command](https://docs.microsoft.com/en-us/powershell/module/sharepoint-pnp/register-pnpazureadapp?view=sharepoint-ps) with the following syntax:

```powershell
$app = Register-PnPAzureADApp -ApplicationName "PnP.Core.SDK.Consumer" -Tenant contoso.onmicrosoft.com -OutPath c:\temp -CertificatePassword (ConvertTo-SecureString -String "password" -AsPlainText -Force) -Scopes "MSGraph.Group.ReadWrite.All","MSGraph.User.ReadWrite.All","SPO.Sites.FullControl.All","SPO.TermStore.ReadWrite.All","SPO.User.ReadWrite.All" -Store CurrentUser -Interactive
```
With SharePoint PnP PowerShell Online cmdlets version 3.29.2101.0 and higher.
```powershell
$app = Register-PnPAzureADApp -Interactive -ApplicationName "PnP.Core.SDK.Consumer" -Tenant contoso.onmicrosoft.com -OutPath d:\temp -CertificatePassword (ConvertTo-SecureString -String "password" -AsPlainText -Force) -GraphApplicationPermissions "Group.ReadWrite.All, User.ReadWrite.All" -SharePointApplicationPermissions "Sites.FullControl.All, TermStore.ReadWrite.All, User.ReadWrite.All" -Store CurrentUser
```

> [!Note]
> It's recommended to align the actually required permissions with the needs of your application.

The above command will register for you in Azure Active Directory an app with name `PnP.Core.SDK.Consumer`, with a self-signed certificate that will be also saved on your filesystem under the `c:\temp` folder (remember to create the folder or to provide the path of an already existing folder), with a certificate password value of `password` (you should provide your own strong password, indeed). Remember to replace `contoso.onmicrosoft.com` with your Azure AD tenant name, which typically is `company.onmicrosoft.com`. The permissions granted to the app will be:

   - SharePoint -> Application Permissions -> Sites -> Sites.FullControl.All
   - SharePoint -> Application Permissions -> TermStore -> TermStore.ReadWrite.All
   - SharePoint -> Application Permissions -> User -> User.ReadWrite.All
   - Microsoft Graph -> Application Permissions -> User -> User.ReadWrite.All
   - Microsoft Graph -> Application Permissions -> Group -> Group.ReadWrite.All

Executing the command you will first have to authenticate against the target tenant, providing the credentials of a Global Tenant Admin. Then you will see a message like the following one:

```text
Waiting 60 seconds to launch consent flow in a browser window. This wait is required to make sure that Azure AD is able to initialize all required artifacts.........
```

Almost 60 seconds later, the command will prompt you for authentication again and to grant the selected permissions to the app you are registering. Once you have done that, in the `$app` variable you will find information about the just registered app. You can copy in your clipboard the **Application ID** (Client ID) executing the following command:

```powershell
$app.'AzureAppId/ClientId'| clip
```

And you can copy in your clipboard the thumbprint of the generated X.509 certificate executing the following command:

```powershell
$app.'Certificate Thumbprint' | clip
```

Paste this copied values in a safe place, because you will need them soon.
In the `c:\temp` folder (or whatever else folder you will choose) there will also be a file named `PnP.Core.SDK.Consumer.pfx`, which includes the private key of the self-signed certificate generated for you, as well as a file named `PnP.Core.SDK.Consumer.cer`, which includes the public key of the self-signed certificate generated for you.

> [!Note]
> If you can't use PowerShell to generate a self-signed certificate then checkout [Generate self-signed certificates with the .NET CLI](https://docs.microsoft.com/en-us/dotnet/core/additional-tools/self-signed-certificates-guide). It for example shows how to use OpenSSL on Linux to create a self-signed certificate.

#### Configuring PnP Core SDK to use the configured application

When you're configuring your application to use the PnP Core SDK you will have to configure the `PnP.Core` services and the `PnP.Core.Auth` services using the `AddPnPCore` and `AddPnPCoreAuthentication` methods, respectively. The `ClientId` and `TenantId` are those of the application that you just registered in Azure Active Directory..

```csharp
// Add the PnP Core SDK library
services.AddPnPCore(options => {
    options.PnPContext.GraphFirst = true;
    options.HttpRequests.UserAgent = "ISV|Contoso|ProductX";

    options.Sites.Add("SiteToWorkWith", new PnPCoreSiteOptions
    {
        SiteUrl = "https://contoso.sharepoint.com/sites/pnp"
    });
});
services.AddPnPCoreAuthentication(
    options => {
        // Configure an Authentication Provider relying on Windows Credential Manager
        options.Credentials.Configurations.Add("x509certificate",
            new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = "{your_client_id}",
                TenantId = "{your_tenant_id}",
                X509Certificate = new PnPCoreAuthenticationX509CertificateOptions
                {
                    StoreName = StoreName.My,
                    StoreLocation = StoreLocation.CurrentUser,
                    Thumbprint = "{certificate_thumbprint}"
                }
            });

        // Configure the default authentication provider
        options.Credentials.DefaultConfiguration = "x509certificate";

        // Map the site defined in AddPnPCore with the 
        // Authentication Provider configured in this action
        options.Sites.Add("SiteToWorkWith",
            new PnPCoreAuthenticationSiteOptions
            {
                AuthenticationProviderName = "x509certificate"
            });
});
```

> [!Note]
> If you're using PnP Core SDK on Linux you can use https://github.com/gsoft-inc/dotnet-certificate-tool as tool to import your certificate.

## Using the multi-tenant PnP Azure AD application

Azure AD has the concept of multi-tenant applications allowing you to re-use an application created in another Azure AD tenant. The PnP team did setup a general purpose Azure AD application (named "PnP Office 365 Management Shell") configured with the needed permissions, and you can reuse this application. It means that you don't need to create your own Azure AD application, instead you simply need to consent permissions to the already created multi-tenant application.

### Step 1: Consent to the PnP Office 365 Management Shell application

To consent permissions to the PnP multi-tenant application first update below content URL: replace contoso.onmicrosoft.com with your Azure AD tenant name, which typically is company.onmicrosoft.com.

```
https://login.microsoftonline.com/contoso.onmicrosoft.com/adminconsent?client_id=31359c7f-bd7e-475c-86db-fdb8c937548e&state=12345&redirect_uri=https://aka.ms/sppnp
```

Login to your Microsoft 365 tenant (e.g. by browsing to SharePoint Online), open a new browser tab and paste the URL you've just created. Azure AD will eventually ask you to login, and then it will prompt you to consent permissions to the app:

![PnP Multi-tenant app admin consent](../images/PnP%20admin%20consent.png)

Click on **Accept** to accept the requested permissions. At that point you will be redirected to the PnP Site (https://aka.ms/sppnp). You've now successfully registered the PnP multi-tenant application in your Azure AD environment and you can use it with the PnP Core SDK. The PnP Core SDK defaults to this application, so if you're not specifying any Azure AD application details when setting up authentication for the application, then the PnP Core SDK automatically uses the PnP application (application id 31359c7f-bd7e-475c-86db-fdb8c937548e).

> [!Note]
> If you get errors during this consent process it's most likely because you are not an Azure AD tenant administrator. Please contact your admins and check with them for further steps.

### Step 2: Configure your project authentication settings

If you're using the PnP Management Shell Azure AD application then you can leave out the application id and tenant id from your authentication setup. Below samples show how to use the `InteractiveAuthenticationProvider` in combination with the PnP Management Shell Azure AD application. Using code you can configure authentication like this:

```csharp
var host = 
    Host.CreateDefaultBuilder()                
        // Configure logging
        .ConfigureLogging((hostingContext, logging) =>
        {
            logging.AddEventSourceLogger();
            logging.AddConsole();
        })
        .ConfigureServices((hostingContext, services) =>
        {                
            // Add the PnP Core SDK library services
            services.AddPnPCore();
            // Add the PnP Core SDK library services configuration from the appsettings.json file
            services.Configure<PnPCoreOptions>(hostingContext.Configuration.GetSection("PnPCore"));
            // Add the PnP Core SDK Authentication Providers
            services.AddPnPCoreAuthentication(options =>
            {
                options.Credentials.Configurations.Add("Default", new PnPCoreAuthenticationCredentialConfigurationOptions
                {
                    Interactive = new PnPCoreAuthenticationInteractiveOptions { },
                });
                options.Credentials.DefaultConfiguration = "Default";
            });
        })
        // Let the builder know we're running in a console
        .UseConsoleLifetime()
        // Add services to the container
        .Build();
```

This snippet show the JSON authentication section to use:

```json
"Credentials": {
    "DefaultConfiguration": "myAuthConfig",
        "Configurations": {
            "myAuthConfig": {
                "Interactive": {
                "RedirectUri": "http://localhost"
                }
            }
    }
}
```

## Using the credential manager

Another supported option to authenticate to a created Azure AD application, configured for delegated permissions, is via username and password, through the `UsernamePasswordAuthenticationProvider`. To configure this in a secure way it's recommended to setup a credential manager entry and to use the `CredentialManagerAuthenticationProvider`. Below steps walk you through the setup on Windows, but a similar credential manager concepts exists on other platforms as well.

1. Click on the **Windows Start** button in the taskbar and type **credential manager**.
2. Click on the **Credential Manager** link.
3. Go to **Windows Credentials** and click on **Add a generic credential**.
4. Give the credential a name (e.g. Contoso), a user name (e.g. joe@contoso.onmicrosoft.com) and a password. Hit **OK** to save.
5. Use the credential manager name (Contoso in this example) in the settings of the `CredentialManagerAuthenticationProvider` provider.
