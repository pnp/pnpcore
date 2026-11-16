# PnP Core Modernization

This is the migration-first port of the PnP Framework page modernization engine.
The source and test fixtures were imported from
[`pnp/pnpframework` at `f5b37d64d457143e4b3367ea0594fbcd18cd2f68`](https://github.com/pnp/pnpframework/tree/f5b37d64d457143e4b3367ea0594fbcd18cd2f68/src/lib/PnP.Framework/Modernization).

**This is a transitional port, not a CSOM-free implementation.** It retains
`ClientContext`, existing SharePoint calls, and a pinned PnP Framework dependency
for shared extensions, authentication, and provisioning helpers. CSOM retirement
is a separate step after migration parity is verified.

The modernization namespaces are now `PnP.Core.Modernization`. XML mapping
schemas and transformation options retain their existing semantics. Rebuild
custom function libraries against the new assembly.

Imported CSOM/HTTP/AngleSharp extensions also live in `PnP.Core.Modernization`
instead of third-party namespaces. When both Framework and Modernization
extensions are in scope, use explicit static calls such as
`PnP.Core.Modernization.WebExtensions.GetPages(context.Web)`.

See `docs\using-the-sdk\modernization-intro.md` in the repository for migration
guidance. The SDK build scripts pack this project but deliberately do not
publish it while migration verification and release approval are pending.

The upstream MIT license is included in `LICENSE`; CAML attribution is retained
in `Caml\credits.txt`.
