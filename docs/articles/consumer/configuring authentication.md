
# Configuring authentication

The PnP Core SDK works with both SharePoint REST as Microsoft Graph in a transparent way, this also means that the authentication model used must work for both. The chosen authentication model is Azure Active Directory (a.k.a. Azure AD), using Azure Active Directory you can define an application and grant it permissions to access Microsoft 365 workloads like SharePoint, Teams,...**Configuring your own application is the recommended approach**, but you can also use a multi-tenant application that the PnP team created. Both options are detailed in the next chapters

## I want to configure my own Azure AD application (recommended)

> [!Note]
> The current preview supports delegated access tokens only. We are working on extending the SDK to support app-only access tokens.

### Configuring the application in Azure AD

Follow below steps to configure an application in Azure AD:

1. Navigate to https://aad.portal.azure.com/
2. Click on **Azure Active Directory** from the left navigation
3. Click on **App registrations** in the **Manage** left navigation group
4. Click on **New registration**
5. Give the application a name (e.g. PnP Core SDK) and click on **Register**
6. Copy the **Application ID** from the **Overview** page, you'll need this GUID value later on
7. Click on the **API Permissions** in the **Manage** left navigation group
8. Click on **Add Permissions** and add the permissions you want to give to this application. Below list is a recommendation, you can grant less permissions but that might result in some PnP Core SDK calls to fail due getting access denied errors.

   - SharePoint -> Delegated Permissions -> AllSites -> AllSites.FullControl
   - SharePoint -> Delegated Permissions -> Sites -> Sites.Search.All
   - SharePoint -> Delegated Permissions -> TermStore -> TermStore.ReadWrite.All
   - SharePoint -> Delegated Permissions -> User -> User.ReadWrite.All
   - Microsoft Graph -> Delegated Permissions -> User -> User.Read
   - Microsoft Graph -> Delegated Permissions -> Directory -> Directory.ReadWrite.All
   - Microsoft Graph -> Delegated Permissions -> Directory -> Directory.AccessAsUser.All
   - Microsoft Graph -> Delegated Permissions -> Group -> Group.ReadWrite.All

9. Click on the **Grant admin consent for** button to consent to these permissions for the users in your organization
10. Click on **Authentication** in the **Manage** left navigation group
11. Change **Default client type** to **Treat application as public client** and hit **Save**

### Configuring PnP Core SDK to use the configured application

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
        options.Credentials.Configurations.Add("credentialmanager",
            new PnPCoreAuthenticationCredentialConfigurationOptions
            {
                ClientId = "{your_client_id}",
                TenantId = "{your_tenant_id}",
                CredentialManager = new PnPCoreAuthenticationCredentialManagerOptions
                {
                    CredentialManagerName = "mycreds"
                }
            });

        // Configure the default authentication provider
        options.Credentials.DefaultConfiguration = "credentialmanager";

        // Map the site defined in AddPnPCore with the 
        // Authentication Provider configured in this action
        options.Sites.Add("SiteToWorkWith",
            new PnPCoreAuthenticationSiteOptions
            {
                AuthenticationProviderName = "credentialmanager"
            });
});
```

## Using the multi-tenant PnP Azure AD application

Azure AD has the concept of multi-tenant applications allowing you to re-use an application created in another Azure AD tenant. The PnP team did setup a general purpose Azure AD application (named "PnP Office 365 Management Shell") configured with the needed permissions, and you can reuse this application. It means that you don't need to create your own Azure AD application, instead you simply need to consent permissions to the already created multi-tenant application.

### Step 1

To consent permissions to the PnP multi-tenant application first update below content URL: replace contoso.onmicrosoft.com with your Azure AD tenant name, which typically is company.onmicrosoft.com.

```
https://login.microsoftonline.com/contoso.onmicrosoft.com/adminconsent?client_id=31359c7f-bd7e-475c-86db-fdb8c937548e&state=12345&redirect_uri=https://aka.ms/sppnp
```

### Step 2

Login to your Microsoft 365 tenant (e.g. by browsing to SharePoint Online), open a new browser tab and paste the URL you've just created. Azure AD will eventually ask you to login, and then it will prompt you to consent permissions to the app:

![PnP Multi-tenant app admin consent](../../images/PnP%20admin%20consent.png)

Click on **Accept** to accept the requested permissions. At that point you will be redirected to the PnP Site (https://aka.ms/sppnp). You've now successfully registered the PnP multi-tenant application in your Azure AD environment and you can use it with the PnP Core SDK. The PnP Core SDK defaults to this application, so if you're not specifying any Azure AD application details when setting up authentication for the application, then the PnP Core SDK automatically uses the PnP application (application id 31359c7f-bd7e-475c-86db-fdb8c937548e).

> [!Note]
> If you get errors during this consent process it's most likely because you are not an Azure AD tenant administrator. Please contact your admins and check with them for further steps.

## Using the credential manager

Currently the only supported option to authenticate to a created Azure AD application is via username and password. To configure this in a secure way it's recommended to setup a credential manager entry. Below steps walk you through the setup on Windows, but a similar credential manager concepts exists on other platforms as well.

1. Click on the **Windows Start** button in the taskbar and type **credential manager**.
2. Click on the **Credential Manager** link.
3. Go to **Windows Credentials** and click on **Add a generic credential**.
4. Give the credential a name (e.g. Contoso), a user name (e.g. joe@contoso.onmicrosoft.com) and a password. Hit **OK** to save.
5. Use the credential manager name (Contoso in this example) in your `OAuthCredentialManagerConfiguration` setup.

