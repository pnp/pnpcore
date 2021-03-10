# Build scripts and tools

## Instructions to release a new major version

Releasing a new major PnP Core version takes these steps:

- Update the major version number in the `version.release` file by 1 (e.g. `1.{minorrelease}.0` will become `2.{minorrelease}.0`)
- Reset the minor release version counter by setting it to -1 in the `version.release.increment` file
- Update the `Version` tag in PnP.Core.csproj and PnP.Core.Auth.csproj to match the new version
- Update the nightly version number in the `version.debug` file to match the major and minor versions of the new release (e.g. `1.3.{incremental}-nightly` will become `2.0.{incremental}-nightly`)
- Reset the nightly release version counter by setting it to 0 in the `version.debug.increment` file
- Run the `release-official.ps1` script and follow the steps
- Update readme.md if needed
- Update the changelog to reflect the released version
- Create a tag for the created version
- Ensure everything is checked into dev and merged into master

## Instructions to release a new minor version

Releasing a new minor PnP Core version takes these steps:

- Update the nightly version number in the `version.debug` file to match the minor version of the new minor release (e.g. `1.0.{incremental}-nightly` will become `1.1.{incremental}-nightly`)
- Reset the nightly release version counter by setting it to 0 in the `version.debug.increment` file
- Update the `Version` tag in PnP.Core.csproj and PnP.Core.Auth.csproj to match the new version
- Run the `release-official.ps1` script and follow the steps
- Update readme.md if needed
- Update the changelog to reflect the released version
- Create a tag for the created version
- Ensure everything is checked into dev and merged into master