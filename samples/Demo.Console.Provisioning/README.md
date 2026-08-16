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
  0  Exit
```

Site urls are typed in when they are needed rather than configured, precisely so that extracting from
one site and applying to another does not mean editing a settings file in between.

---

## Extracting from the command line

```
dotnet run -- extract <site-url> <output.xml> [options]
```

A template is **structure only** by default — columns, content types, lists, security,
navigation, branding, but none of the content sitting in them. Content is the expensive part
of an extract, so you opt into it:

| Option | |
|---|---|
| `--items` | Include the items of **every** list on the site |
| `--items=A,B` | Include the items of these lists only |
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

Raise `Logging:LogLevel:Default` to `Information` to see what the engine is doing underneath. It is
verbose — the sample prints its own progress precisely so you do not need to.


[back to samples](../README.md)
