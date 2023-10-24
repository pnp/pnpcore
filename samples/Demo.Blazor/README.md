# PnP Core SDK - Blazor Sample

This solution demonstrates how the PnP Core SDK can be used in a Blazor WebAssembly app

## Source code

You can find the sample source code here: [/samples/Demo.Blazor](https://github.com/pnp/pnpcore/tree/dev/samples/Demo.Blazor)

# Run the sample

## Register and configure an AAD app

In order for the user to authenticate on the App, A new app registration should be created on Azure Portal

- Go to [Azure Active Directory Portal](https://aad.portal.azure.com)

- In App registrations, click __New registration__

- Enter a name for your new app, make sure *Accounts in this organizational directory only* is selected. As the Redirect URI, in Web platform enter __https://localhost:44349/authentication/login-callback__ (The port may vary according to your Visual Studio)

- Make sure that the added Redirect URI is for a Single-Page Application

- Under __Implicit grant__ section, check __Access tokens__ and __ID tokens__

- Go to __API permissions__ section , click __Add a permission__
-- Select __SharePoint__ > __Delegated permissions__ > select __AllSites.FullControl__
-- Select __Microsoft Graph__ > __Delegated permissions__ > select __email__, __openid__ and __Sites.FullControl.All__

- Click __Grant admin consent for {tenant}__

- From __Overview__,
-- copy the value of __Directory (tenant) ID__
-- copy the value of __Application (client) ID__

## Configure your application

- Replace `{sharepoint_url}` the URL of your SharePoint site in app setting
- in the file `wwwroot/appsettings.json`, replace the `{client_id}` and the `{tenant_id}` accordingly with the values from above

## Execute

  Hit F5 in Visual studio to execute the Blazor app.
  When trying to access one of the sections, the applications prompts you for signing in

  ![preview image of the running app](preview.png)
