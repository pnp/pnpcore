# PnP Core SDK - Azure Function with Managed Idendtity Sample

This solution demonstrates how to build an Azure Function that uses System Managed Identity to connect to SharePoint Online Site.
The API Permissions are set to `Sites.Selected`, and access is granted to specific SPO sites only to ensure only minimum requzired permissions are assigned

## Quickstart

> TBC

1. Create new Function App
2. Enable Managed Identity
3. Execute [](./Tools/SetupV2.ps1) using
    - tenant id
    - name of your app
    - sp site url
    With Write/FullControl for SPO (if write, part of the test will fail)

4. App Configuration:
    - SiteName
    - TenantName
