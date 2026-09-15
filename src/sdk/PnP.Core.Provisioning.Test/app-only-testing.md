# Running the live tests with application permissions

The live tests normally run as a signed-in user. Running them **app-only** exercises a different
code path in the engine and finds defects delegated runs cannot: there is no current user, and
some APIs answer differently to an application token.

## 1. Azure AD app registration

Create (or reuse) an app registration and add **application** permissions — not delegated —
then **Grant admin consent**:

| API | Permission |
|---|---|
| SharePoint | `Sites.FullControl.All` |
| SharePoint | `User.Read.All` |
| Microsoft Graph | `Sites.FullControl.All` |
| Microsoft Graph | `Group.ReadWrite.All` |
| Microsoft Graph | `User.Read.All` |
| **Microsoft Graph** | **`TermStore.ReadWrite.All`** |

The Graph term store permission is easy to miss because SharePoint has a permission of the same
name. PnP Core's taxonomy model is Graph based, so the **Graph** one is the one that counts.
Without it, 8 of the 13 taxonomy tests fail with `HTTP 403 accessDenied` — and the term group
sweep in teardown fails too, leaving groups behind in the tenant term store.

## 2. Certificate

Azure AD app-only against SharePoint **requires a certificate**. A client secret does not work:
SharePoint rejects app-only tokens obtained with one, and PnP Core ships no client-secret
provider (`ClientSecret` exists only on the On-Behalf-Of provider, which is delegated).

```powershell
$c = New-SelfSignedCertificate -Subject "CN=PnPProvisioningAppOnly" `
    -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable `
    -KeySpec Signature -NotAfter (Get-Date).AddYears(2)
Export-Certificate -Cert $c -FilePath "$env:USERPROFILE\Desktop\PnPProvisioningAppOnly.cer"
$c.Thumbprint
```

Upload the `.cer` under **Certificates & secrets → Certificates**.

## 3. Test configuration

`env.txt` selects the settings file: its contents name `appsettings.<name>.json`.

```
apponly
```

`appsettings.apponly.json`:

```json
{
  "AppOnly": {
    "SiteOwner": "someone@yourtenant.onmicrosoft.com"
  },
  "PnPCore": {
    "Credentials": {
      "DefaultConfiguration": "AppOnly",
      "Configurations": {
        "AppOnly": {
          "ClientId": "<client id>",
          "TenantId": "<tenant id>",
          "X509Certificate": {
            "StoreName": "My",
            "StoreLocation": "CurrentUser",
            "Thumbprint": "<thumbprint>"
          }
        }
      }
    },
    "Sites": { "...": "AuthenticationProviderName must be AppOnly on every site" }
  }
}
```

`AppOnly:SiteOwner` has no delegated equivalent. App-only has no current user, so a site created
without an explicit owner is a site nobody can administer. `LiveTestBase.SiteOwnerAsync` uses the
current user when running delegated and this setting when running app-only, and stops the test
with a reason rather than creating an ownerless site if it is missing.

## 4. Running

The live tests carry `[Ignore]`. Remove it to run them, and put it back afterwards.

```powershell
dotnet test PnP.Core.Provisioning.Test --filter "TestCategory=Taxonomy" --logger "console;verbosity=detailed"
```

**Use `verbosity=detailed`.** At `normal`, `Assert.Inconclusive` messages are not printed, and
these tests deliberately skip with a reason when a capability looks unavailable — at `normal` a
run of skips tells you nothing about why.

## Detecting app-only in code

Do not infer it from configuration or from the authentication provider type. PnP Core asks the
token:

```csharp
await context.GetMicrosoft365Admin().AccessTokenUsesApplicationPermissionsAsync();
```

`SiteCreationOptions.UsingApplicationPermissions` is `bool?` and `SiteCollectionCreator` fills it
in with exactly that call when it is left null. **Leave it null.** Setting it explicitly overrides
the SDK's own detection — which is what made hierarchy site creation fail app-only with
`siteStatus = 3`.
