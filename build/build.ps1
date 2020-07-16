#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

$versionIncrement = Get-Content ./build/version.debug.increment -Raw
$versionIncrement = $versionIncrement -as [int]
$versionIncrement = $versionIncrement + 1

$version = Get-Content ./build/version.debug -Raw

$version = $version.Replace("{incremental}", $versionIncrement).Replace("{yearmonth}", $(get-date -format yy) + $(get-date -format MM))

Write-Host "Building version $version"
dotnet build ./src/sdk/PnP.Core/PnP.Core.csproj --no-incremental /p:Version=$version

Write-Host "Packinging $version"
dotnet pack ./src/sdk/PnP.Core/PnP.Core.csproj --no-build /p:PackageVersion=$version

Write-Host "Publishing to nuget"
$nupkg = $("./src/sdk/PnP.Core/bin/Debug/PnP.Core.$version.nupkg")
$apiKey = $("$env:NUGET_API_KEY")

Write-Host "API Key starts with:" $apiKey.Substring(0,10)

dotnet nuget push $nupkg --api-key $apiKey --source https://api.nuget.org/v3/index.json

Write-Host "Writing $version to git"
Set-Content -Path ./build/version.debug.increment -Value $versionIncrement