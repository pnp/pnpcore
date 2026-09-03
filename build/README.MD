# Build scripts and tools

## Instructions to release a new minor version

Releasing a new minor PnP Core version takes these steps:

- checkout `dev` branch (be sure you have all the changes pulled)
- Update the nightly version number in the `version.debug` file to match the minor version of the new minor release (e.g. `1.0.{incremental}-nightly` will become `1.1.{incremental}-nightly`)
- Reset the nightly release version counter by setting it to 0 in the `version.debug.increment` file
- Update the `Version` tag in PnP.Core.csproj, PnP.Core.Auth.csproj, PnP.Core.Admin.csproj to match the new version
- git commit and push
- In PnP PowerShell repo, run the [PnP Core build](https://github.com/pnp/powershell/actions/workflows/pnpcorebuild.yml) with `version.release` input Version file. This will build, sign and package the project and commit it to the PnP Core repository as a zip file.
- In PnP Core repo, run the [Nightly nuget release](https://github.com/pnp/pnpcore/actions/workflows/nightlynuget_dev.yml) GitHub workflow which will unzip the NuGet packages and publish it to NuGet.org
- git pull the latest changes
- Update Readme.md if needed
- Update the changelog to reflect the released version and add the version number to the latest unreleased section
- git commit and push the changes to `dev` branch with commit message like 'Version 1.16.0'
- Perform a new release using GiHub UI, create a new tag (like 1.16.0) for the created version and copy over the changelog
- Ensure everything is checked into dev and merged into master to perform a snapshot release of the new minor version

---

// TODO guide to release new major version is outdated and should be revised when performing new major release

## Instructions to release a new major version

Releasing a new major PnP Core version takes these steps:

- Update the major version number in the `version.release` file by 1 (e.g. `1.{minorrelease}.0` will become `2.{minorrelease}.0`)
- Reset the minor release version counter by setting it to -1 in the `version.release.increment` file
- Update the `Version` tag in PnP.Core.csproj, PnP.Core.Auth.csproj, PnP.Core.Admin.csproj to match the new version
- Update the nightly version number in the `version.debug` file to match the major and minor versions of the new release (e.g. `1.3.{incremental}-nightly` will become `2.0.{incremental}-nightly`)
- Reset the nightly release version counter by setting it to 0 in the `version.debug.increment` file
- Run the `release-official.ps1` script and follow the steps
- Update readme.md if needed
- Update the changelog to reflect the released version
- Create a tag for the created version
- Ensure everything is checked into dev and merged into master