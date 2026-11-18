# Page modernization: migration and CSOM retirement

`PnP.Core.Modernization` contains the page modernization engine imported from
PnP Framework. It includes wiki, web-part, blog, publishing, and Enterprise Wiki
transformators, default mapping XML/XSD resources, custom function support,
asset transfer, metadata, permissions, user/term/URL mapping, and reporting.

The library uses Core contexts and models, without PnP Framework or the managed
SharePoint Client SDK. **Only SharePoint Online is supported**; on-premises
sources and destinations are rejected. Removing managed dependencies does not
make every operation REST/Graph-only; the protocol boundary is documented below.

## What changed

The complete modernization subtree and dedicated test corpus were imported from
[`pnp/pnpframework` commit `f5b37d64d457143e4b3367ea0594fbcd18cd2f68`](https://github.com/pnp/pnpframework/tree/f5b37d64d457143e4b3367ea0594fbcd18cd2f68/src/lib/PnP.Framework/Modernization).
The library has its own assembly/package and uses the `PnP.Core.Modernization`
namespace. Default mapping resource names use the new prefix; XML schema
namespaces and mapping semantics are unchanged.

The bundled CAML builder uses `PnP.Core.Modernization.Caml`. Transformation
constructors now accept `PnPContext`, and source pages use `IListItem`.
The existing Core `IPage` writer and synchronous transformation entry points
are retained. Authentication and context lifetimes belong to the caller.

The imported extensions formerly in `Microsoft.SharePoint.Client`,
`System.Net`, and `AngleSharp.Dom` now live in `PnP.Core.Modernization`.
Call the modernization extensions explicitly when another imported namespace
defines a method with the same name:

```csharp
using PnP.Core.Model.SharePoint;
using PnP.Core.Modernization.Transform;
using ModernizationWeb = PnP.Core.Modernization.WebExtensions;

// sourceContext and targetContext are caller-owned, authenticated PnPContexts.
var pages = ModernizationWeb.GetPages(sourceContext.Web, "Example");
var transformator = new PageTransformator(
    sourceContext, targetContext, BasePageTransformator.LoadDefaultWebPartMapping());
foreach (IListItem sourcePage in pages)
{
    var information = new PageTransformationInformation(sourcePage)
    {
        TargetPageName = $"Example-{sourcePage.Id}-modern.aspx",
        Overwrite = false,
        PublishCreatedPage = false,
        SkipTelemetry = true
    };
    string destination = transformator.Transform(information);
}
```

Choose a unique destination name for each source page in real batch migrations.
Do not treat the sample as authorization to overwrite or publish existing pages.
The model overload above uses the embedded mapping. File-based constructors
retain their existing behavior: `webpartmapping.xml` must be available in the
working directory or its `Modernization` subdirectory.

For in-place publishing, use
`PnP.Core.Modernization.Publishing.PublishingPageTransformator.CreateInPlace(
sourceContext, BasePageTransformator.LoadDefaultWebPartMapping(), null)`.
The source web must already meet the existing Site Pages/feature prerequisites.
The port does not enable features or change site policy to make a migration run.

Rebuild custom function libraries against the new assembly and update any
assembly-qualified references or resource lookups using the old namespace.
Do not mix Framework transformation options or model objects with their new
Modernization equivalents; they are different managed types.

## Core models and protocol boundary

Files, fields, users, permissions, and metadata use Core interfaces. Multi-value
user, lookup, and taxonomy fields use `IFieldValueCollection.Values`; create
destination values with the target field's `NewFieldUserValue`,
`NewFieldTaxonomyValue`, and `NewFieldValueCollection` factories. Custom
function constructors must also use Core types. Clone contexts by URL with
`Clone(Uri)`, not `Clone(string)` (which selects a named configuration).

Core controls upload chunking. The legacy `fileChunkSizeInMB` parameter remains
accepted but no longer controls the actual chunk size. Term-set traversal uses
Core's Microsoft Graph term-store API and requires corresponding Graph
authentication and read permissions. LDAP-based user resolution is unsupported.

`MarkdownToSharePointObserver` now accepts a Core `IWeb`, not a CSOM
`ClientContext`, and `EnsureDestination` returns Core `IFolder`. The caller owns
the web's `PnPContext` and must keep it alive while the observer is used.
The observer uses the Core request pipeline for the SharePoint REST
`lists/ensuresitepageslibrary` operation, then Core folder and page APIs.

```csharp
using PnP.Core.Modernization.Telemetry.Observers;

// targetPnPContext is a separately authenticated PnPContext owned by the caller.
var observer = new MarkdownToSharePointObserver(
    targetPnPContext.Web, folderName: "Transformation-Reports");
transformator.RegisterObserver(observer);
// Run transformations before flushing their accumulated report.
string reportPage = await observer.FlushAsync(clearLogData: true);
```

`FlushAsync` returns the page name relative to Site Pages, propagates failures,
and clears buffered logs only after success when requested. An empty folder
name saves at the library root; nested relative folder names are retained.
The synchronous `Flush` entry point retains its existing console-error reporting.
Neither method takes ownership of or disposes the supplied Core context.

| Area | Implementation |
| --- | --- |
| Target component identity/discovery | Core `IWeb` / `IPage`; no managed-CSOM lookup |
| SharePoint report storage | Core interfaces and REST; async entry points available |
| Classic extraction | REST web-part metadata and JSON POST XML export; source export settings are not changed |
| Assets, metadata, permissions and users | Core models, field factories and REST |
| Base templates | Existing `PnP.Core.Provisioning` APIs |
| Term-set traversal | Core Microsoft Graph term-store API |
| Bare-ID term-label lookup | A narrow authenticated ProcessQuery wrapper, without managed CSOM |
| Non-empty publishing Summary Links | Core's existing CAML protocol reader; REST exposes labels but loses link targets and formatting |
| On-premises/SOAP/LDAP paths | Removed or explicitly rejected |

`Services\Core\CSOM\TaxonomyLookup.cs` is the remaining explicit protocol
fallback. A bare term ID does not supply the term-set ID required by the modern
term-store API, and Core has no public equivalent for that lookup. The wrapper
builds XML safely, authenticates with the supplied Core provider, uses Core's
configured HTTP client, validates responses, and propagates service errors.
Core may itself use ProcessQuery for some supported operations. This is managed
SDK retirement, not a promise that `client.svc` is never called.

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
the repository. On-premises execution tests are no longer supported; classic
layouts with legacy names, including 2010 layouts hosted in SharePoint Online,
remain in scope. Successful compilation alone does not establish migration
fidelity for every page or web part.

## Release and retirement boundary

Build scripts compile and pack the package, but do not add it to
their NuGet push lists. Before publication, approve live/platform coverage and
update the external signing/package-ZIP producer documented in `build\README.MD`.
The generic nightly ZIP publisher does not establish that the new DLL has been
signed or that its migration verification is complete.

Dependency removal follows the preserved migration checkpoint as a separate
phase. Its verification must use the current local build, not results from
the earlier Framework-dependent checkpoint.
