# Configuring the environment to run PnP.Core.Auth tests
The PnP.Core.Auth library has some requirements in order to properly execute its automated tests (PnP.Core.Auth.Test).
Specifically, you need to:
- Register in Azure Active Directory a generic test application for authentication tests 
- Register in Azure Active Directory a frontend application for the OnBehalfOf authentication tests
- Register in Azure Active Directory a backend application for the OnBehalfOf authentication tests
- Enable the frontend application to consume the backend application
- Configure a set of credentials in the Windows Credential Manager
- Configure the appsettings.json file of the PnP.Core.Auth.Test project

## Register in AAD a generic test application for authentication tests
In order to execute both tests with application and delegated access tokens, you will need to register an application in AAD both as app-only and delegated.
The easiest way to register an application in Azure Active Directory for app-only is to use the [PnP PowerShell](https://docs.microsoft.com/en-us/powershell/sharepoint/sharepoint-pnp/sharepoint-pnp-cmdlets?view=sharepoint-ps) command lets. Specifically you can use the [`Initialize-PnPPowerShellAuthentication` command](https://docs.microsoft.com/en-us/powershell/module/sharepoint-pnp/initialize-pnppowershellauthentication?view=sharepoint-ps) with the following syntax:

```powershell
$app = Initialize-PnPPowerShellAuthentication -ApplicationName "PnP.Core.Test" -Tenant contoso.onmicrosoft.com -OutPath c:\temp  -CertificatePassword (ConvertTo-SecureString -String "password" -AsPlainText -Force) -Scopes "MSGraph.Group.ReadWrite.All","MSGraph.Directory.ReadWrite.All","SPO.Sites.FullControl.All","SPO.TermStore.ReadWrite.All","SPO.User.ReadWrite.All" -Store CurrentUser
```

The above command will register for you in Azure Active Directory an app with name `PnP.Core.Test`, with a self-signed certificate that will be also saved on your filesystem under the `c:\temp` folder (remember to create the folder or to provide the path of an already existing folder), with a certificate password value of `password` (you should provide your own strong password, indeed). Remember to replace `contoso.onmicrosoft.com` with your Azure AD tenant name, which typically is `company.onmicrosoft.com`. The permissions granted to the app will be:

   - SharePoint -> Application Permissions -> Sites -> Sites.FullControl.All
   - SharePoint -> Application Permissions -> TermStore -> TermStore.ReadWrite.All
   - SharePoint -> Application Permissions -> User -> User.ReadWrite.All
   - Microsoft Graph -> Application Permissions -> Group -> Group.ReadWrite.All
   - Microsoft Graph -> Application Permissions -> Directory -> Directory.ReadWrite.All

Executing the command you will first have to authenticate against the target tenant, providing the credentials of a Global Tenant Admin. Then you will see a message like the following one:

```text
Waiting 60 seconds to launch consent flow in a browser window. This wait is required to make sure that Azure AD is able to initialize all required artifacts.........
```

Almost 60 seconds later, the command will prompt you for authentication again and to grant the selected permissions to the app you are registering. Once you have done that, in the `$app` variable you will find information about the just registered app. You can copy in your clipboard the **Application ID** (Client ID) executing the following command:

```powershell
$app.AzureAppId | clip
```

And you can copy in your clipboard the thumbprint of the generated X.509 certificate executing the following command:

```powershell
$app.'Certificate Thumbprint' | clip
```

Paste this copied values in a safe place, because you will need them soon.
Now, configure the same application also for delegated access tokens following the below steps:

1. Navigate to https://aad.portal.azure.com/
2. Click on **Azure Active Directory** from the left navigation
3. Click on **App registrations** in the **Manage** left navigation group
4. Write the Client ID of the just created application in the search box 
5. Select the application with name `PnP.Core.Test` in the list of results
6. Click on the **API Permissions** in the **Manage** left navigation group
7. Click on **Add Permissions** and add the following permissions to this application:

   - SharePoint -> Delegated Permissions -> AllSites -> AllSites.FullControl
   - SharePoint -> Delegated Permissions -> TermStore -> TermStore.ReadWrite.All
   - SharePoint -> Delegated Permissions -> User -> User.ReadWrite.All
   - Microsoft Graph -> Delegated Permissions -> Group -> Group.ReadWrite.All
   - Microsoft Graph -> Delegated Permissions -> User -> User.Read
   - Microsoft Graph -> Delegated Permissions -> Directory -> Directory.ReadWrite.All

At the end, the overall list of permissions for the application will be the following one:

   - SharePoint -> Delegated Permissions -> AllSites -> AllSites.FullControl
   - SharePoint -> Delegated Permissions -> TermStore -> TermStore.ReadWrite.All
   - SharePoint -> Delegated Permissions -> User -> User.ReadWrite.All
   - SharePoint -> Application Permissions -> Sites -> Sites.FullControl.All
   - SharePoint -> Application Permissions -> TermStore -> TermStore.ReadWrite.All
   - SharePoint -> Application Permissions -> User -> User.ReadWrite.All
   - Microsoft Graph -> Delegated Permissions -> Group -> Group.ReadWrite.All
   - Microsoft Graph -> Delegated Permissions -> User -> User.Read
   - Microsoft Graph -> Delegated Permissions -> Directory -> Directory.ReadWrite.All
   - Microsoft Graph -> Application Permissions -> Group -> Group.ReadWrite.All
   - Microsoft Graph -> Application Permissions -> Directory -> Directory.ReadWrite.All

10. Click on the **Grant admin consent for** button to consent to these permissions for the users in your organization
11. Click on **Authentication** in the **Manage** left navigation group
12. Change **Default client type** to **Treat application as public client** and hit **Save**
13. Click on **Authentication** and then click on **Add a platform**, choose **Mobile and desktop applications** and provide http://localhost as the **Redirect URI**

## Register in AAD a frontend application for the OnBehalfOf authentication tests
You need to register an application that will be the frontend for the OnBehalfOf flow.
Follow the below steps:

1. Navigate to https://aad.portal.azure.com/
2. Click on **Azure Active Directory** from the left navigation
3. Click on **App registrations** in the **Manage** left navigation group
4. Click on **New registration**
5. Give the application a name (e.g. `PnP.Core.Test.OnBehalfOf.Frontend`) and click on **Register**
6. Copy the **Application ID** (Client ID) from the **Overview** page, you'll need this GUID value later on
7. Copy the **Directory ID** (Tenant ID) from the **Overview** page, you'll need this GUID value later on
8. Click on **Authentication** and then click on **Add a platform**, choose **Mobile and desktop applications** and provide http://localhost as the **Redirect URI**

## Register in AAD a backend application for the OnBehalfOf authentication tests
You need to register an application that will be the backend for the OnBehalfOf flow.
Follow the below steps:

1. Navigate to https://aad.portal.azure.com/
2. Click on **Azure Active Directory** from the left navigation
3. Click on **App registrations** in the **Manage** left navigation group
4. Click on **New registration**
5. Give the application a name (e.g. `PnP.Core.Test.OnBehalfOf.Backend`) and click on **Register**
6. Copy the **Application ID** (Client ID) from the **Overview** page, you'll need this GUID value later on
7. Copy the **Directory ID** (Tenant ID) from the **Overview** page, you'll need this GUID value later on
8. Click on **API Permissions** in the **Manage** left navigation group
9. Click on **Add Permissions** and add the permissions you want to give to this application. Below list is a recommendation, you can grant less permissions but that might result in some PnP Core SDK calls to fail due getting access denied errors.

   - SharePoint -> Delegated Permissions -> AllSites -> AllSites.Read
   - Microsoft Graph -> Delegated Permissions -> Sites -> Sites.Read.All
   - Microsoft Graph -> Delegated Permissions -> User -> User.Read

10. Click on the **Grant admin consent for** button to consent to these permissions for the users in your organization
11. Click on the **Expose an API** in the **Manage** left navigation group
12. Configure the **Application ID URI** with a value like `api://pnp.core.test.onbehalfof.backend`
13. Click on **Add a scope** and configure a new scope with name `Backend.Consume` with `Admins Only` consent. Provide a display name and a description for admins and click **Add scope**.
14. Click on the **Manifest** in the **Manage** left navigation group
15. In the JSON file representing the manifest of the application search for the **knownClientApplications** value. It is an array of IDs and assign to it an array made of a single item, which is the Client ID of the application with name `PnP.Core.Test.OnBehalfOf.Frontend`. Save the updated manifest
16. Click on **Certificates & secrets** in the **Manage** left navigation group
17. Click on **New client secret** to create a new client secret for the backend application. Choose a name (for example `ClientSecret`) and a lifetime (for example 2 years).
18. Click on **Add** and store the Client Secret value in a safe place
19. Click on **Upload certificate**, browse your file system for the .CER file that you created when you registered the application with PowerShell (the path should `c:\temp`, unless you changed it, and the file should be `PnP.Core.Test.CER`)

## Enable the frontend application to consume the backend application
Now, you need to go back to the application registered with name `PnP.Core.Test.OnBehalfOf.Frontend` and update its setting accordingly to the following steps:

1. Navigate to https://aad.portal.azure.com/
2. Click on **Azure Active Directory** from the left navigation
3. Click on **App registrations** in the **Manage** left navigation group
4. Write `PnP.Core.Test.OnBehalfOf.Frontend` in the search box 
5. Select the application with name `PnP.Core.Test.OnBehalfOf.Frontend` in the list of results
6. Click on the **API Permissions** in the **Manage** left navigation group
7. Click on **Add Permissions**, select **My APIs** and choose `PnP.Core.Test.OnBehalfOf.Backend`
8. Select the permission with name `Backend.Consume`
9. Click on the **Grant admin consent for** button to consent to these permissions for the users in your organization

Now the frontend application is enabled to consume the backend application.

## Configure a set of credentials in the Windows Credential Manager
In order to test the CredentialManagerAuthenticationProvider you need to register a set of credentials in the Windows Credential Manager, by following the below steps.

1. Click on the **Windows Start** button in the taskbar and type **credential manager**.
2. Click on the **Credential Manager** link.
3. Go to **Windows Credentials** and click on **Add a generic credential**.
4. Give the credential a name (e.g. Contoso), a user name (e.g. joe@contoso.onmicrosoft.com) and a password. Hit **OK** to save.

Copy in a safe place the name you provided to the Credential Manager item, because you will reuse it shortly.

## Configure the appsettings.json file of the PnP.Core.Auth.Test project
Copy the file `appsettings.copyme.json` defined in the root folder of the `PnP.Core.Auth.Test` project and paste it to create a new copy. Rename the copied file with a name like `appsettings.yourname.json`, where yourname should be replaced with something the identifies you (like your name or your company name).
Now copy the file `env.sample` defined in the root folder of the `PnP.Core.Auth.Test` project and paste it to create a new copy. Rename the copied file with name `env.txt`.
Open the file `env.txt` and write in its content the value that you choose for the `yourname` token in the name of the appsettings.json file. For example, if you renamed the settings file as `appsettings.contoso.json` you will have to write `contoso` as the unique content of the `env.txt` file.
Open the `appsettings.yourname.json` file, it will look like the following one.

```json
{
  "PnPCore": {
    "DisableTelemetry": "true",
    "PnPContext": {
      "GraphFirst": "true",
      "GraphCanUseBeta": "true",
      "GraphAlwaysUseBeta": "false"
    },
    "Credentials": {
      "DefaultConfiguration": "credentialManager",
      "Configurations": {
        "usernamePassword": {
          "ClientId": "{PnP.Core.Test-ClientId}",
          "TenantId": "{TenantId}",
          "UsernamePassword": {
            "Username": "{Test-Username}",
            "Password": "{Test-Password}"
          }
        },
        "credentialManager": {
          "ClientId": "{PnP.Core.Test-ClientId}",
          "TenantId": "{TenantId}",
          "CredentialManager": {
            "CredentialManagerName": "{Credential-Manager}"
          }
        },
        "onBehalfOf": {
          "ClientId": "{PnP.Core.Test.OnBehalfOf.Backend-ClientID}",
          "TenantId": "{TenantId}",
          "OnBehalfOf": {
            "StoreName": "My",
            "StoreLocation": "CurrentUser",
            "Thumbprint": "{Backend-Certificate-Thumbprint}",
            "ClientSecret": "{Backend-Client-Secret}"
          }
        },
        "interactive": {
          "ClientId": "{PnP.Core.Test-ClientId}",
          "TenantId": "{TenantId}",
          "Interactive": {
            "RedirectUri": "http://localhost"
          }
        },
        "x509Certificate": {
          "ClientId": "{PnP.Core.Test-ClientId}",
          "TenantId": "{TenantId}",
          "X509Certificate": {
            "StoreName": "My",
            "StoreLocation": "CurrentUser",
            "ThumbPrint": "{Certificate-Thumbprint}"
          }
        },
        "onBehalfFrontEnd": {
          "ClientId": "{PnP.Core.Test.OnBehalfOf.Frontend-ClientID}",
          "TenantId": "{TenantId}",
          "Interactive": {
            "RedirectUri": "http://localhost"
          }
        },
        "deviceCode": {
          "ClientId": "{PnP.Core.Test-ClientId}",
          "TenantId": "{TenantId}",
          "DeviceCode": {
            "RedirectUri": "http://localhost"
          }
        },
        "externalRealProvider": {
          "ClientId": "{PnP.Core.Test-ClientId}",
          "TenantId": "{TenantId}",
          "CredentialManager": {
            "CredentialManagerName": "{Credential-Manager}"
          }
        }
      }
    },
    "Sites": {
      "TestSiteCredentialManager": {
        "SiteUrl": "https://contoso.sharepoint.com/sites/pnpcoresdktestgroup",
        "AuthenticationProviderName": "credentialManager"
      },
      "TestSiteUsernamePassword": {
        "SiteUrl": "https://contoso.sharepoint.com/sites/pnpcoresdktestgroup",
        "AuthenticationProviderName": "usernamePassword"
      },
      "TestSiteInteractive": {
        "SiteUrl": "https://contoso.sharepoint.com/sites/pnpcoresdktestgroup",
        "AuthenticationProviderName": "interactive"
      },
      "TestSiteOnBehalfOf": {
        "SiteUrl": "https://contoso.sharepoint.com/sites/pnpcoresdktestgroup",
        "AuthenticationProviderName": "onBehalfOf"
      },
      "TestSiteX509Certificate": {
        "SiteUrl": "https://contoso.sharepoint.com/sites/pnpcoresdktestgroup",
        "AuthenticationProviderName": "x509Certificate"
      },
      "TestSiteDeviceCode": {
        "SiteUrl": "https://contoso.sharepoint.com/sites/pnpcoresdktestgroup",
        "AuthenticationProviderName": "deviceCode"
      }
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

In the file replace:
- `{PnP.Core.Test-ClientId}` with the ClientId of the PnP.Core.Test application that you registered before
- `{TenantId}` with the TenantId of your target tenant
- `{Test-Username}` with the username (UPN) of a user that you want to use for tests and that must have access to the target test SharePoint sites and Teams. It might be a user account dedicated to tests.
- `{Test-Password}` with the password of the username declared in the previous step
- `{Credential-Manager}` with the name of the Credential Manager item that you created before
- `{Certificate-Thumbprint}` with the thumbprint of the certificate created before

Lastly, replace all the occurrences of the URL of the site `https://contoso.sharepoint.com/sites/pnpcoresdktestgroup` with the URL of a site that you want to use for testing purposes.