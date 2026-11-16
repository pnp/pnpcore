# Page modernization: migration-first port

`PnP.Core.Modernization` contains the page modernization engine imported from
PnP Framework. It includes wiki, web-part, blog, publishing, and Enterprise Wiki
transformators, default mapping XML/XSD resources, custom function support,
asset transfer, metadata, permissions, user/term/URL mapping, and reporting.

> This is a transitional migration, **not CSOM retirement**. The project still
> uses `Microsoft.SharePoint.Client.ClientContext` and retains PnP Framework
> 1.22.0 for shared CSOM extensions, authentication, and provisioning helpers.
> Moving the engine does not make its requests REST/Graph-only.

## What changed

The complete modernization subtree and dedicated test corpus were imported from
[`pnp/pnpframework` commit `f5b37d64d457143e4b3367ea0594fbcd18cd2f68`](https://github.com/pnp/pnpframework/tree/f5b37d64d457143e4b3367ea0594fbcd18cd2f68/src/lib/PnP.Framework/Modernization).
The library has its own assembly/package and uses the `PnP.Core.Modernization`
namespace. Default mapping resource names use the new prefix; XML schema
namespaces and mapping semantics are unchanged.

The bundled CAML builder now uses `PnP.Core.Modernization.Caml`, avoiding
duplicate `CamlBuilder` types from the transitional Framework dependency.

CSOM contexts, synchronous entry points, transformation options, source-version
branches, and server operations are retained. The existing Core `IPage` writer
is reused. No new REST/Graph replacements or CSOM request wrappers were added.

The imported extensions formerly in `Microsoft.SharePoint.Client`,
`System.Net`, and `AngleSharp.Dom` now live in `PnP.Core.Modernization`.
This avoids declaring the same extension types in both assemblies. Because the
transitional Framework dependency still contains its original extensions,
call the new extensions explicitly when both namespaces are imported:

```csharp
using Microsoft.SharePoint.Client;
using PnP.Core.Modernization.Transform;
using ModernizationWeb = PnP.Core.Modernization.WebExtensions;

// sourceContext and targetContext are separately authenticated ClientContexts.
var pages = ModernizationWeb.GetPages(sourceContext.Web, "Example");
var transformator = new PageTransformator(sourceContext, targetContext);
foreach (ListItem sourcePage in pages)
{
    var information = new PageTransformationInformation(sourcePage)
    {
        TargetPageName = "Example-modern.aspx",
        Overwrite = false,
        PublishCreatedPage = false,
        SkipTelemetry = true
    };
    string destination = transformator.Transform(information);
}
```

Choose a unique destination name for each source page in real batch migrations.
Do not treat the sample as authorization to overwrite or publish existing pages.

For in-place publishing, use
`PnP.Core.Modernization.Publishing.PublishingPageTransformator.CreateInPlace(sourceContext)`.
The source web must already meet the existing Site Pages/feature prerequisites.
The port does not enable features or change site policy to make a migration run.

Rebuild custom function libraries against the new assembly and update any
assembly-qualified references or resource lookups using the old namespace.
Do not mix Framework transformation options or model objects with their new
Modernization equivalents; they are different managed types.

## Build and verification

From the repository root:

```powershell
dotnet build src\sdk\PnP.Core.Modernization\PnP.Core.Modernization.csproj --configuration Release
dotnet test src\sdk\PnP.Core.Modernization.Test\PnP.Core.Modernization.Test.csproj --filter TestCategory=Offline
dotnet pack src\sdk\PnP.Core.Modernization\PnP.Core.Modernization.csproj --configuration Release --no-build
```

The library targets `netstandard2.0`, `net8.0`, `net9.0`, and `net10.0`; its
MSTest project runs on `net10.0`. Default test settings select only `Offline`
tests. Legacy `Integration` and `Manual` tests remain in the project but are not
silently treated as passing offline tests.

See the test project's README for opt-in, certificate-authenticated `Live`
smoke checks. Live environment configuration and credentials must remain outside
the repository. Existing on-premises and manual scenarios still need their own
fixtures and verification; successful compilation does not establish parity
for every source platform.

## Release and retirement boundary

Build scripts compile and pack the transitional package, but do not add it to
their NuGet push lists. Before publication, approve live/platform coverage and
update the external signing/package-ZIP producer documented in `build\README.MD`.
The generic nightly ZIP publisher does not establish that the new DLL has been
signed or that its migration verification is complete.

CSOM dependency removal starts only after the migration checkpoint is accepted.
Its separate work includes `PnPContext`-based contracts, removal of Framework
helpers, and operation-by-operation REST/Graph substitutions. A custom
`ProcessQuery` wrapper removes an SDK dependency, not the underlying protocol.
