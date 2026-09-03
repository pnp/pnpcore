# Security Policy

The PnP Core SDK is an open-source, community-maintained library (`PnP.Core`,
`PnP.Core.Auth`, `PnP.Core.Admin`) and is **not** a Microsoft-supported product.
The maintainers take security seriously and appreciate responsible disclosure.

## Supported Versions

Security fixes are delivered in the latest stable release published to NuGet.
We recommend always running the most recent stable version.

| Package          | Supported            |
| ---------------- | -------------------- |
| Latest stable (1.16.x and newer) | ✅ Yes  |
| Older releases   | ❌ No (upgrade to latest) |

The SDK targets `.NET Standard 2.0`, `.NET 8.0`, `.NET 9.0`, and `.NET 10.0`.
Keep your runtime patched, as some security fixes come from the underlying
.NET runtime and dependencies (e.g. MSAL / `Microsoft.Identity.Client`).

## Reporting a Vulnerability

**Please do NOT open a public GitHub issue for security vulnerabilities.**

Instead, report privately using one of the following:

1. **Preferred — GitHub Private Vulnerability Reporting:** open a report via the
   repository's **Security → Report a vulnerability** tab 

When reporting, please include:

- A description of the vulnerability and its impact.
- The affected package(s) and version(s).
- Steps to reproduce or a proof-of-concept.
- Any suggested remediation.

### What to expect

- **Acknowledgement** of your report as soon as the maintainers can triage it.
- An assessment and, where confirmed, a coordinated fix in an upcoming release.
- Credit in the release notes if you wish (let us know).

Because this is a volunteer-maintained community project, there is no formal SLA,
but we aim to handle valid reports promptly.

## Scope

This policy covers the SDK source code and the published NuGet packages
(`PnP.Core`, `PnP.Core.Auth`, `PnP.Core.Admin`). Issues in Microsoft 365 services,
Microsoft Graph, SharePoint Online, or the .NET runtime should be reported to
Microsoft (e.g. the [Microsoft Security Response Center](https://msrc.microsoft.com/)).

## Consumer Hardening Guidance

If you build applications on top of the PnP Core SDK, we recommend:

- **Least privilege:** request the narrowest Microsoft Entra (Azure AD) permission
  scopes your app needs. Avoid `*.FullControl.All` / app-only Sites.FullControl
  unless strictly required — a compromised app inherits the SDK's granted scopes.
- **Protect credentials:** prefer certificate-based or managed-identity auth over
  client secrets / username+password flows. Store secrets in a vault, never in
  source or config committed to a repo.
- **Validate untrusted input:** sanitize/encode any user-supplied values (site URLs,
  file names, list/field values, search queries) before passing them to SDK calls.
- **Pin and update dependencies:** keep `PnP.Core*` and transitive dependencies
  (especially MSAL) up to date; subscribe to release notes.
- **Verify package integrity:** consume packages only from the official NuGet feed
  (nuget.org, authors `PnP`), and consider NuGet package signature verification.
- **Don't log tokens:** ensure your logging configuration does not capture access
  tokens, refresh tokens, secrets, or full request/response bodies.
