# PnP Core Modernization

This is the migration-first port of the PnP Framework page modernization engine.
The source and test fixtures were imported from
[`pnp/pnpframework` at `f5b37d64d457143e4b3367ea0594fbcd18cd2f68`](https://github.com/pnp/pnpframework/tree/f5b37d64d457143e4b3367ea0594fbcd18cd2f68/src/lib/PnP.Framework/Modernization).

The production library uses **PnP Core SDK**, with no direct or transitive
PnP Framework or managed SharePoint Client SDK dependency. Only SharePoint Online
sources and destinations are supported; on-premises URLs and LDAP-based user
resolution are explicitly rejected.

Transformation constructors take `PnPContext`. Source pages, files, fields,
users, permissions, and metadata use the Core interfaces (`IListItem`, `IFile`,
`IField`, and related types). The caller creates and owns the contexts and their
authentication providers. Cross-site contexts are cloned with `Clone(Uri)`.
Do not pass a URL to Core's `Clone(string)`, which selects a named configuration.

Multi-value user, lookup, and taxonomy fields use `IFieldValueCollection.Values`,
not CSOM arrays. Create destination values with the target field's
`NewFieldUserValue`, `NewFieldTaxonomyValue`, and `NewFieldValueCollection`
factories. User mappings resolve the source user's ID to a login before remapping.
Asset upload uses Core's automatic chunking; the legacy `fileChunkSizeInMB`
argument remains accepted but Core controls the actual chunk size.
Downloads resolve the asset's owning web before requesting its binary content,
and reject a length mismatch before overwriting a destination asset.
The term-set reader uses Core's Microsoft Graph term-store API, so cross-term-set
mapping requires the authentication provider to support Microsoft Graph and
the corresponding term-store read permissions.

## Protocol boundaries

The intended boundary is that modernization does not construct or send CSOM
requests directly. Calling public PnP Core APIs that internally use CSOM is
supported. The taxonomy adapter below is a temporary exception whose removal
is deferred to a future PR.

Removing the managed SDK does not remove all SharePoint protocols:

- Classic web-part definitions, zone metadata, and permitted XML exports use
  authenticated SharePoint REST through Core's request pipeline. Export is a
  JSON `POST` to `GetLimitedWebPartManager(scope=1)/ExportWebPart`; source export
  settings are never changed.
- Localized resources, theme shades, file-to-web resolution, and effective
  list/item permissions use small SharePoint REST adapters.
- Non-default page metadata and publishing images use Core's
  `RenderListDataAsStream`, followed by raw REST readback for ordinary rich text.
  Non-empty publishing Summary Links require Core's existing
  `LoadItemsByCamlQueryViaCsom` protocol implementation: REST rendering/edit values
  omit the links' URLs, groups and formatting. The fallback reads one source
  item and does not change it or introduce a managed SDK dependency.
- Taxonomy values left uninitialized by Core's CAML reader are hydrated from
  REST using public Core field factories, preserving populated terms without
  modifying the source item.
- Enterprise Wiki rating readback also uses Core's CAML reader when REST omits
  the native hidden rating fields. Empty user sentinels are not voters, and
  incomplete or inconsistent rating histories still fail validation.
- `Services\Core\CSOM\TaxonomyLookup.cs` is the remaining direct ProcessQuery
  adapter: resolving a term label from a bare term ID has no public Core
  equivalent, and the modern term-store API requires the term-set ID.
  It builds XML with `XElement`, authenticates using the supplied Core provider,
  uses Core's configured HTTP client, validates the response, and surfaces
  SharePoint errors as `CsomServiceException`. It has no managed CSOM dependency.
- Core itself uses ProcessQuery internally for operations such as page/report
  saving, `SystemUpdate`, `UpdateOverwriteVersion`, and content-type field-link
  changes. These Core-backed calls, including the Summary Links reader above,
  comply with the intended boundary. This library does not use reflection or
  access Core internals.

**Deferred follow-up: direct taxonomy CSOM removal**

The remaining call chain is `PublishingBuiltIn.DefaultTaxonomyFieldValue` ->
`CacheManager.GetTermFromId` -> `TaxonomyLookup.GetLabel` ->
`ProcessQueryClient.ExecuteAsync`. Modernization constructs the CSOM XML and
sends it to `client.svc/ProcessQuery`; using Core's HTTP client does not make
this a Core-owned operation.

Core's current `TermStore.GetTermById` requires both a term-set ID and a term
ID, while this helper receives only the term ID. A future PR should supply the
term-set context or introduce a suitable public Core lookup API, preserving
the existing term-label resolution and caching behavior. Once that path uses
Core, remove `Services\Core\CSOM\TaxonomyLookup.cs` and
`Services\Core\CSOM\ProcessQueryClient.cs` and add regression coverage.
This follow-up does not require replacing the other Core-backed CSOM calls.

`MarkdownToSharePointObserver` now takes a Core `IWeb` (for example,
`targetPnPContext.Web`) instead of `ClientContext`. Its `FlushAsync` method
returns the saved report's library-relative page name and propagates failures.
The caller retains ownership of the Core context.

The modernization namespaces are now `PnP.Core.Modernization`. XML mapping
schemas and transformation options retain their existing semantics. Rebuild
custom function libraries against the new assembly.

Modernization extensions live in `PnP.Core.Modernization`, for example
`PnP.Core.Modernization.WebExtensions.GetPages(context.Web)`. Custom function
constructors and arguments must also use Core contexts and models.

See `docs\using-the-sdk\modernization-intro.md` in the repository for migration
guidance. The SDK build scripts pack this project but deliberately do not
publish it while migration verification and release approval are pending.

The upstream MIT license is included in `LICENSE`; CAML attribution is retained
in `Caml\credits.txt`.
