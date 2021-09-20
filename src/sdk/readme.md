# PnP Core SDK source

The PnP Core SDK consists out of one solution named `PnP.Core.sln` which has two projects:

- `PnP.Core`: this is the actual SDK code base project
- `PnP.Core.Admin`: this is a library that extends the PnP.Core library with admin related tasks
- `PnP.Core.Auth`: this is a library of Authentication Providers for the SDK code base project, it relies on MSAL for the Authentication logic
- `PnP.Core.Test`: project containing the SDK tests
- `PnP.Core.Admin.Test`: project containing the PnP.Core.Admin tests
- `PnP.Core.Auth.Test`: project containing the PnP.Core.Auth tests

If you want to contribute to this SDK check-out our [contributor guidance](../../docs/index.md).
