#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

$versionIncrement = Get-Content ./build/version.debug.increment -Raw
$versionIncrement = $versionIncrement -as [int]
$versionIncrement = $versionIncrement + 1

$version = Get-Content ./build/version.debug -Raw
$versionUnreleased = Get-Content ./build/version.unreleased.debug -Raw

$version = $version.Replace("{incremental}", $versionIncrement)

Write-Host "Building PnP.Core version $version"
dotnet build ./src/sdk/PnP.Core/PnP.Core.csproj --configuration Release --no-incremental /p:Version=$version

Write-Host "Building PnP.Core.Auth version $version"
dotnet build ./src/sdk/PnP.Core.Auth/PnP.Core.Auth.csproj --configuration Release --no-incremental /p:Version=$version

Write-Host "Building PnP.Core.Admin version $version"
dotnet build ./src/sdk/PnP.Core.Admin/PnP.Core.Admin.csproj --configuration Release --no-incremental /p:Version=$versionUnreleased

Write-Host "Packinging PnP.Core version $version"
dotnet pack ./src/sdk/PnP.Core/PnP.Core.csproj --configuration Release --no-build /p:PackageVersion=$version

Write-Host "Packinging PnP.Core.Auth version $version"
dotnet pack ./src/sdk/PnP.Core.Auth/PnP.Core.Auth.csproj --configuration Release --no-build /p:PackageVersion=$version

Write-Host "Packinging PnP.Core.Admin version $version"
dotnet pack ./src/sdk/PnP.Core.Admin/PnP.Core.Admin.csproj --configuration Release --no-build /p:PackageVersion=$versionUnreleased

Write-Host "Publishing to nuget"
$nupkg = $("./src/sdk/PnP.Core/bin/Release/PnP.Core.$version.nupkg")
$authNupkg = $("./src/sdk/PnP.Core.Auth/bin/Release/PnP.Core.Auth.$version.nupkg")
$adminNupkg = $("./src/sdk/PnP.Core.Admin/bin/Release/PnP.Core.Admin.$versionUnreleased.nupkg")
$apiKey = $("$env:NUGET_API_KEY")

#Write-Host "API Key starts with:" $apiKey.Substring(0,10)

dotnet nuget push $nupkg --api-key $apiKey --source https://api.nuget.org/v3/index.json
dotnet nuget push $authNupkg --api-key $apiKey --source https://api.nuget.org/v3/index.json
dotnet nuget push $adminNupkg --api-key $apiKey --source https://api.nuget.org/v3/index.json

Write-Host "Writing $version to git"
Set-Content -Path ./build/version.debug.increment -Value $versionIncrement
