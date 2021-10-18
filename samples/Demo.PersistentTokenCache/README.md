# PnP Core SDK - Console Sample with PersistentTokenCache

This solution demonstrates how the PnP Core SDK can be extended to implement a persistent cache and reuse access tokens after application restart. After initial authentication the MSAL tokens are cached locally and when the application restarts there's no credential prompting needed anymore. This is realized by building a custom `IAuthenticationProvider`.

## Source code

> [!Note]
> This sample was authored by Stefano Driussi 💪🥇 and the original source code can be found [here](https://github.com/stefanodriussi/Demo.PersistentTokenCache/tree/main/Demo.PersistentTokenCache). This code was copied to the PnP Core SDK samples after approval of the author, see [this GitHub discussion](https://github.com/pnp/pnpcore/discussions/400) for the background.

You can find the sample source code here: [/samples/Demo.PersistentTokenCache](https://github.com/pnp/pnpcore/tree/dev/samples/Demo.PersistentTokenCache)

# Run the sample

## Register and configure an AAD app

In order for the user to authenticate on the App, A new app registration should be created on Azure Portal

- Go to [Azure Active Directory Portal](https://aad.portal.azure.com)

- In App registrations, click __New registration__

- Enter a name for your new app, make sure *Accounts in this organizational directory only* is selected. As the Redirect URI, change from Web Platform to "Mobile and Desktop Applications" use __http://localhost__ for the redirect URI (only needed if you want use an interactive authentication flow)

- Under __Implicit grant__ section, check __ID tokens__ and __Access tokens__

- Under __Advanced settings__ section, set __Allow public client flows__ to __yes__

- Go to __API permissions__ section , click __Add a permission__
  - Select __Microsoft Graph__ > __Delegated permissions__ > select __Directory.Read.All__
  - Select __Microsoft Graph__ > __Delegated permissions__ > select __User.Read__
  - Select __Microsoft Graph__ > __Delegated permissions__ > select __ChannelMessage.Read.All__
  - Select __Microsoft Graph__ > __Delegated permissions__ > select __ChannelMessage.Send__
  - Select __Microsoft Graph__ > __Delegated permissions__ > select __TeamSettings.ReadWrite.All__
  - Select __Microsoft Graph__ > __Delegated permissions__ > select __TeamsTab.ReadWrite.All__

- Click __Grand admin consent for {tenant}__

- From __Overview__,
  - copy the value of __Directory (tenant) ID__
  - copy the value of __Application (client) ID__

## Configure your application

- This demo application is configured directly inside `Program.cs`. Replace all parameters between curly braces with actual values from your SharePoint/Azure app.
Be sure to have a Team in Microsoft Teams backing the modern team site in the above site collection

## Execute

Hit F5 in Visual studio to execute the console app. The app will prompt for an interactive login (via a browser window).
Execute the application a second time and the code will fetch the last access token without requiring any user interaction.
