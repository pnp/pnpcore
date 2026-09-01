# Demo.Console.Provisioning

An interactive console for the **PnP Core provisioning engine**: extract a template from a SharePoint
site, look at what it captured, and apply it to a different site.

---

## Before you start

You need:

- **.NET 10 SDK** or later
- A SharePoint Online site you can administer — **two** if you want to try the full round trip
- An account that can sign in interactively (a browser window opens; there is no stored password)

## App registration

In order to use this app you will need to register an app in Azure AD and grant it the following permissions:

- Go to [Azure Active Directory Portal](https://aad.portal.azure.com)

- In App registrations, click __New registration__

- Enter a name for your new app, make sure *Accounts in this organizational directory only* is selected. As the Redirect URI, change from Web Platform to "Mobile and Desktop Applications" use __http://localhost__ for the redirect URI (only needed if you want use an interactive authentication flow)

- Under __Implicit grant__ section, check __ID tokens__ and __Access tokens__

- Under __Advanced settings__ section, set __Allow public client flows__ to __yes__

- Go to __API permissions__ section , click __Add a permission__
  - Select __Microsoft Graph__ > __Delegated permissions__ > select __Group.ReadWrite.All__
  - Select __Microsoft Graph__ > __Delegated permissions__ > select __openid__
  - Select __Microsoft Graph__ > __Delegated permissions__ > select __profile__
  - Select __Microsoft Graph__ > __Delegated permissions__ > select __Sites.Manage.All__
  - Select __Microsoft Graph__ > __Delegated permissions__ > select __User.Read.All__
  - Select __SharePoint__ > __Delegated permissions__ > select __AllSites.FullControl__
  - Select __SharePoint__ > __Delegated permissions__ > select __AllSites.Manage__
  - Select __SharePoint__ > __Delegated permissions__ > select __TermStore.ReadWrite.All__
  - Select __SharePoint__ > __Delegated permissions__ > select __User.Read.All__

- Click __Grant admin consent for {tenant}__

- From __Overview__,
  - copy the value of __Directory (tenant) ID__
  - copy the value of __Application (client) ID__

---

## Running it

There are two ways in. **Apply one template and exit:**

```powershell
cd pnpcore/samples/Demo.Console.Provisioning
dotnet run -- https://contoso.sharepoint.com/sites/target Templates/site.xml
```

**Or with no arguments, for the menu:**

```powershell
dotnet run
```

```
=== PnP Core provisioning ===
  1  Extract a template from a site
  2  List saved templates
  3  Show what a saved template contains
  4  Apply a saved template to a site
  5  Export a site to a .pnp package
  6  Apply a .pnp package to a site
  0  Exit
```

Site urls are typed in when they are needed rather than configured, precisely so that extracting from
one site and applying to another does not mean editing a settings file in between.

---

## Extracting from the command line

```
dotnet run -- extract <site-url> <output.xml> [options]
dotnet run -- extract <site-url> <output.pnp> [options]    as a package, see Packages (.pnp)
```

A template is **structure only** by default — columns, content types, lists, security,
navigation, branding, but none of the content sitting in them. Content is the expensive part
of an extract, so you opt into it:

| Option | |
|---|---|
| `--items` | Include the items of **every** list on the site |
| `--items=A,B` | Include the items of these lists only |
| `--files` | Export the files held in **every** document library |
| `--files=A,B` | Export the files of these document libraries only |
| `--pages` | Include the site's client side pages and their contents |
| `--hidden-lists` | Include hidden lists in the structure |

```powershell
# structure only
dotnet run -- extract https://contoso.sharepoint.com/sites/marketing out.xml

# structure, every list's items, and the pages
dotnet run -- extract https://contoso.sharepoint.com/sites/marketing out.xml --items --pages

# just the two lists you care about
dotnet run -- extract https://contoso.sharepoint.com/sites/marketing out.xml --items="Tasks,Announcements"
```

## Applying from the command line

```
dotnet run -- <site-url> <template.xml>
dotnet run -- apply <site-url> <template.xml>    the same, spelled out
```

| Option | |
|---|---|
| `-v`, `--verbose` | Log the SDK's own requests as well. Reach for this when a failure needs the traffic behind it. |
| `-h`, `--help` | Usage. |

**Exit codes**, so a script can tell the three outcomes apart:

| Code | Meaning |
|---|---|
| `0` | Applied, nothing reported |
| `2` | Applied, **but warnings or errors were reported** |
| `1` | Failed |

`0` and `2` are genuinely different. The engine reports a problem and carries on rather than
stopping, so a run can finish having quietly skipped part of the template — see
[Reading the output](#reading-the-output).

Example:

```
Template: D:\templates\marketing.xml
Target:   https://contoso.sharepoint.com/sites/target

  Contains: 14 site columns, 3 content types, 6 lists

Connecting - a sign in window may appear...
Applying...
  1/20  Initializing engine
  [Warning] ListInstance Style Library is a Style Library of a site with NoScript enabled and will be skipped
  2/20  Regional Settings
  ...

Applied, but 1 thing(s) were reported:
  [Warning] ListInstance Style Library is a Style Library of a site with NoScript enabled and will be skipped
```

---

## How to use the menu

### 1. Extract

Choose **1**, then paste a site url:

```
Extract from which site? (full url, blank to cancel): https://contoso.sharepoint.com/sites/marketing
Save as (file name, blank for an automatic one):

Structure - columns, content types, lists, security - is always included.
Include list items as well? (y/N): y
  Which lists (comma separated, blank for all):
  Document libraries are skipped - see the note after the extract.
Include the site's pages and their contents? (y/N): y
Include hidden lists? (y/N): n
```

A browser window opens for sign in. Then:

```
Extracting...
  1/12  Initializing engine
  2/12  Site Fields
  ...
Saved Templates\sites-marketing-20260811-142233.xml

  Contains: 14 site columns, 3 content types, 6 lists, 1 client side page, 2 property bag entries
```

Templates land in a `Templates` folder beside the executable.

### 2. Look at it

Choose **3** and pick the file. You get the same summary, and can print the raw XML.

**This is worth doing before you apply anything.** An extract captures what a site *has*, which is
usually more than you meant to copy — and the summary is the quickest way to notice that.

### 3. Apply

Choose **4**, pick the template, then give a **different** site:

```
Apply to which site? (full url, blank to cancel): https://contoso.sharepoint.com/sites/marketing-copy

About to apply sites-marketing-20260811-142233.xml to https://contoso.sharepoint.com/sites/marketing-copy

  Contains: 14 site columns, 3 content types, 6 lists, 1 client side page, 2 property bag entries

Type the site's name to confirm, or anything else to cancel: marketing-copy
```

**Applying changes a real site and there is no undo**, which is why it asks you to type the name
rather than press y. The previous prompt was a url, and urls are easy to mistype.

### 4. Export a site as a .pnp package

Choose **5**. It asks the same questions as option 1, but writes a single **`.pnp` package**
instead of an `.xml` file:

```
Extract from which site? (full url, blank to cancel): https://contoso.sharepoint.com/sites/marketing
Save as (package file name, blank for an automatic one): marketing

Structure - columns, content types, lists, security - is always included.
Include list items as well? (y/N): y
Export the files held in document libraries? (y/N): y
  Which libraries (comma separated, blank for all): Documents
Include the site's pages and their contents? (y/N): y
```

```
Saved Templates\marketing.pnp
```

### 5. Apply a .pnp package

Choose **6**. It lists the `.pnp` files in the template folder rather than the `.xml` ones, then
asks for the target site and confirms exactly as option 4 does.

---

## Packages (.pnp)

A `.pnp` package is a **single file holding the template and everything it references** — the
provisioning XML plus the documents exported from libraries. It is an OPC package, the same
container format as `.docx`, so it can be copied, versioned and handed over as one artefact.

**Use a package when the template ships content.** An `.xml` template records file entries but
leaves the bytes beside it on disk, so moving the template to another machine without its
neighbouring files silently produces an apply that creates empty libraries. A `.pnp` cannot come
apart that way.

### From the command line

The **file extension decides the format** — there is no separate switch:

```powershell
# export a site as a package, with the items and the documents in it
dotnet run -- extract https://contoso.sharepoint.com/sites/marketing marketing.pnp --items --files

# apply that package to another site
dotnet run -- apply https://contoso.sharepoint.com/sites/marketing-copy marketing.pnp
```

```powershell
# the same thing as loose files
dotnet run -- extract https://contoso.sharepoint.com/sites/marketing marketing.xml --items --files
dotnet run -- apply https://contoso.sharepoint.com/sites/marketing-copy marketing.xml
```

Everything that works for `.xml` works for `.pnp` — `--items`, `--files`, `--pages`,
`--hidden-lists`, the exit codes, and the warning output are all unchanged.

### What ends up inside

| | |
|---|---|
| `template.xml` | the provisioning template |
| exported library files | under the folder path they came from |
| package properties | author (your Windows user name) and generator |

When `--files` is combined with a `.pnp` output, the file connector points **at the package**, so
the documents are written into it as they are exported rather than to the folder next to it.

### Reading a package back

Applying a `.pnp` opens the package, reads `template.xml` out of it, and points the template's
connector at the package — so file uploads resolve from inside it. Nothing has to be unzipped.

> **Note:** the `.pnp` support in `PnP.Core.Provisioning` is a direct port of PnP Framework's
> OpenXML connector and has no automated test coverage yet. Check the result of a package round
> trip before relying on it for anything you cannot repeat.


---

## Running as the application (app-only)

Everything above signs a **user** in. For unattended use — a build agent, a scheduled job — the
sample can authenticate as the **application** instead. Set a certificate thumbprint in
`appsettings.json` and no browser window appears:

```json
{
  "CustomSettings": {
    "ClientId": "<client id of the app registration>",
    "TenantId": "<tenant id>",
    "CertificateThumbprint": "DF5450F6FB23838465128BBFC95C86091504B16B",
    "CertificateStoreName": "My",
    "CertificateStoreLocation": "CurrentUser",
    "TemplateFolder": "Templates"
  }
}
```

Leave `CertificateThumbprint` empty and the sample signs a user in interactively as before.
`CertificateStoreName` and `CertificateStoreLocation` are optional and default to `My` and
`CurrentUser`.

### A certificate is required — a client secret will not work

Azure AD app-only against SharePoint **must** use a certificate. SharePoint rejects app-only
tokens obtained with a client secret, and PnP Core ships no client-secret provider.

```powershell
$c = New-SelfSignedCertificate -Subject "CN=PnPProvisioningAppOnly" `
    -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable `
    -KeySpec Signature -NotAfter (Get-Date).AddYears(2)
Export-Certificate -Cert $c -FilePath "$env:USERPROFILE\Desktop\PnPProvisioningAppOnly.cer"
$c.Thumbprint
```

Upload the `.cer` under **Certificates & secrets → Certificates** on the app registration.

### Application permissions

These are **Application** permissions, not Delegated, and they need **Grant admin consent**:

| API | Permission | Needed for |
|---|---|---|
| SharePoint | `Sites.FullControl.All` | applying templates, creating sites |
| SharePoint | `User.Read.All` | resolving users in security and list items |
| Microsoft Graph | `Sites.FullControl.All` | modern sites |
| Microsoft Graph | `Group.ReadWrite.All` | group connected sites |
| Microsoft Graph | `User.Read.All` | user lookup |
| Microsoft Graph | `TermStore.ReadWrite.All` | term groups, term sets, taxonomy columns |

**The Graph term store permission is the one people miss.** SharePoint has a permission with the
same name, and picking that one instead leaves every taxonomy operation failing with
`HTTP 403 accessDenied` — term groups, labels and custom properties, while term sets and terms
keep working, because those go through a different path.

### What behaves differently app-only

**There is no current user**, and two things follow from that:

- **`Owner` becomes mandatory** on any site a template creates. Under a signed-in user the owner
  defaults to that user; as an application there is nobody to default to, and site creation fails
  with *"You need to set an owner when using Application permissions to create a communication
  site"*. Set `Owner` on the site collection in hierarchy and sequence templates.
- Anything resolving "me" — a template using the current user as a value — has nothing to resolve.

Templates that apply cleanly interactively can fail unattended for exactly these reasons, so it is
worth applying a template app-only once before relying on it in automation.
---

## Configuration

`appsettings.json`:

```json
{
  "CustomSettings": {
    "ClientId": "31359c7f-bd7e-475c-86db-fdb8c937548e",
    "TenantId": "common",
    "RedirectUri": "http://localhost",
    "TemplateFolder": "Templates"
  },
  "Logging": {
    "LogLevel": { "Default": "Warning" }
  }
}
```

| Setting | What it does |
|---|---|
| `ClientId` | The Entra application to sign in with. Replace with your own registration if you have one. |
| `TenantId` | `common` asks which tenant at sign in. Set a tenant id to skip that. |
| `RedirectUri` | Where sign in returns to. **Must match the application's registration** — leave it alone unless you changed `ClientId`. |
| `TemplateFolder` | Where extracted templates are written, relative to the executable. |
| `CertificateThumbprint` | Set it to authenticate as the application instead of signing a user in. See [Running as the application](#running-as-the-application-app-only). Leave empty for interactive sign in. |
| `CertificateStoreName` | Optional, defaults to `My`. |
| `CertificateStoreLocation` | Optional, defaults to `CurrentUser`. |

Raise `Logging:LogLevel:Default` to `Information` to see what the engine is doing underneath. It is
verbose — the sample prints its own progress precisely so you do not need to.


[back to samples](../README.md)
