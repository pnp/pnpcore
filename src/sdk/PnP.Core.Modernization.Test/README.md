# Modernization migration tests

The corpus originated at Framework commit
`f5b37d64d457143e4b3367ea0594fbcd18cd2f68`. Online cases and fixtures are retained;
on-premises execution cases are replaced by explicit unsupported-platform
checks. Legacy layout names do not make an Online fixture on-premises.

| Category | Execution |
| --- | --- |
| `Offline` | Pure transformation, Core-interface mocks, and HTTP-backed Core contexts; selected by default and in CI. |
| `Integration` | Imported Online scenarios using `TestCommon`/`App.config` for fixture URLs and Core certificate authentication; require manual fixture review. |
| `Manual` | Imported HTML file-output exercise with a machine-specific path; never selected by default. |
| `Live` | Opt-in smoke checks of the local library's public transformation and Core-native reporting entry points. |

`offline.runsettings` protects an unfiltered `dotnet test` invocation from
running legacy tenant-writing scenarios. An explicit `--filter` overrides that
selection. Some imported integration tests are exploratory or conclude with
`Assert.Inconclusive`; retaining them is not a claim that they passed.

## Scoped live checks

`MigrationLiveTests` authenticates with a certificate in `CurrentUser\My`.
It does not export the key or use the installed PnP PowerShell module. Its output
records the actual loaded Modernization/Core/Core Auth assembly paths. These
are direct local-library checks, not `ConvertTo-PnPPage` command coverage.

Use approved, existing fixtures without images or asset-transfer requirements.
The wiki scenario writes a GUID-named draft to a separate destination's existing
Site Pages library. The publishing scenario writes a GUID-named draft in the
source web's existing Site Pages library. Both verify the modern-page marker,
canvas, title, draft state, and unchanged source content/modification time.
Each deletes only its exact run-owned destination in `finally` and verifies
cleanup. The tests do not activate features, create libraries, publish pages,
swap source names, or change permissions. Do not select arbitrary production
pages: their embedded parts can have additional migration requirements.

`CoreReportCreatesModernPageAndCleansUpOwnedFolder` uses the configured wiki
destination's existing Site Pages library. It writes one report in a new,
GUID-named folder, verifies the modern-page marker, title and Markdown control,
then deletes only that exact run-owned folder and verifies cleanup. The report
path and certificate authentication use Core APIs without a Framework bridge.

Offline HTTP fixtures use the public Core factory and reject unexpected
requests; no Core friend assembly or test-only changes to another SDK project
are required.

Configure these environment variables locally:

- `PNP_MODERNIZATION_LIVE=1`
- `PNP_MODERNIZATION_CLIENT_ID`
- `PNP_MODERNIZATION_TENANT_ID`
- `PNP_MODERNIZATION_CERTIFICATE_THUMBPRINT`
- `PNP_MODERNIZATION_WIKI_SOURCE`: source web URL
- `PNP_MODERNIZATION_WIKI_PAGE`: existing source page's server-relative URL
- `PNP_MODERNIZATION_WIKI_TARGET`: separate destination web URL
- `PNP_MODERNIZATION_PUBLISHING_SOURCE`: publishing source web URL
- `PNP_MODERNIZATION_PUBLISHING_PAGE`: existing publishing page's server-relative URL

```powershell
dotnet test src\sdk\PnP.Core.Modernization.Test\PnP.Core.Modernization.Test.csproj --filter TestCategory=Live
```

Missing opt-in/configuration is reported as inconclusive, not success. Use a
specific test-name filter when configuring only one scenario. Keep tenant
details, certificate identifiers, test results, and credentials out of commits.
Do not copy the legacy sample authentication/tracing configuration into CI.
