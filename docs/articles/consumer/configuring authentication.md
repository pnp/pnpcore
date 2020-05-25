
# Configuring authentication

The PnP Core SDK works with both SharePoint REST as Microsoft Graph in a transparent way, this also means that the authentication model used must work for both. The chosen authentication model is Azure Active Directory (a.k.a. Azure AD), using Azure Active Directory you can define an application and grant it permissions to access Microsoft 365 services like SharePoint, Teams,...Configuring your own application is the recommended approach, but you can also use an multi-tenant application that the PnP team created. Both options are detailed in the next chapters

## I want to configure my own Azure AD application


## Using the multi-tenant PnP Azure AD application

Azure AD has the concept of multi-tenant applications allowing you to re-use an application created in another Azure AD tenant. The PnP team did setup an general purpose Azure AD application (named "PnP Office 365 Management Shell") configured with the needed permissions and you can reuse this application. This will mean you don't have to create you're own Azure AD application but instead you consent to the already created multi-tenant application.

### Step 1

To consent to the PnP multi-tenant application first update below content URL: replace contoso.onmicrosoft.com with your Azure AD name which typically is company.onmicrosoft.com.

```
https://login.microsoftonline.com/contoso.onmicrosoft.com/adminconsent?client_id=31359c7f-bd7e-475c-86db-fdb8c937548e&state=12345&redirect_uri=https://aka.ms/sppnp
```

### Step 2

Login to your Microsoft 365 tenant (e.g. by going to SharePoint Online), open a new browser tab and paste the URL you've just created. Azure AD will ask you to login and then it will prompt you to consent to the permissions the app requests:

![PnP Multi-tenant app admin consent](../../images/PnP%20admin%20consent.png)

Click on **Accept** to accept the requested permissions and that point you're being redirected to the PnP Site (https://aka.ms/sppnp). At this point you've registered the PnP multi-tenant application in your Azure AD environment and you can use it with the PnP Core SDK. The PnP Core SDK defaults to this application, so if you're not specifying any Azure AD application details when setting up authentication for the application then the PnP Core SDK automatically uses the PnP application (application id 31359c7f-bd7e-475c-86db-fdb8c937548e).

> [!Note]
> If you get errors during this consent process it's most likely because your Azure AD tenant administrators have blocked you from registering multi-tenant applications. Please contact your admins and check with them.
