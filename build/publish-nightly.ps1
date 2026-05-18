# Script used in the github nightly release workflow to publish a pre-built NuGet package
$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

$packageZip = "./build/package/PnP.Core-packages.zip"
$extractPath = "./build/package/extracted"

Write-Host "Extracting $packageZip"
Expand-Archive -Path $packageZip -DestinationPath $extractPath -Force

$nupkgs = Get-ChildItem -Path $extractPath -Filter "*.nupkg" -Recurse

if ($nupkgs.Count -eq 0) {
    Write-Error "No .nupkg files found in $extractPath"
    exit 1
}

$apiKey = $env:NUGET_API_KEY

foreach ($nupkg in $nupkgs) {
    Write-Host "Publishing $($nupkg.Name) to NuGet"
    dotnet nuget push $nupkg.FullName --api-key $apiKey --source https://api.nuget.org/v3/index.json
}