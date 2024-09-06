#Requires -RunAsAdministrator

# Script used to release an official PnP Framework build
$versionIncrement = Get-Content "$PSScriptRoot\version.debug.increment" -Raw
$versionIncrement = $versionIncrement -as [int]
$versionIncrement++

$version = Get-Content "$PSScriptRoot\version.debug" -Raw

$version = $version.Replace("{incremental}", $versionIncrement)

# Build the release version
Write-Host "Building PnP.Core version $version..."
dotnet build $PSScriptRoot\..\src\sdk\PnP.Core\PnP.Core.csproj --configuration Release --no-incremental --force --nologo /p:Version=$version
Write-Host "Building PnP.Core.Auth version $version..."
dotnet build $PSScriptRoot\..\src\sdk\PnP.Core.Auth\PnP.Core.Auth.csproj --configuration Release --no-incremental --force --nologo /p:Version=$version
Write-Host "Building PnP.Core.Admin version $version..."
dotnet build $PSScriptRoot\..\src\sdk\PnP.Core.Admin\PnP.Core.Admin.csproj --configuration Release --no-incremental --force --nologo /p:Version=$version
Write-Host "Building PnP.Core.Transformation version $version..."
dotnet build $PSScriptRoot\..\src\sdk\PnP.Core.Transformation\PnP.Core.Transformation.csproj --configuration Release --no-incremental --force --nologo /p:Version=$version
Write-Host "Building PnP.Core.Transformation.SharePoint version $version..."
dotnet build $PSScriptRoot\..\src\sdk\PnP.Core.Transformation.SharePoint\PnP.Core.Transformation.SharePoint.csproj --configuration Release --no-incremental --force --nologo /p:Version=$version

# Sign the binaries
Write-Host "Signing the binaries..."
q:\github\SharePointPnP\CodeSigning\PnP\sign-pnpbinaries.ps1 -SignJson pnpcoreassemblies

# Package the release version
Write-Host "Packinging PnP.Core version $version..."
dotnet pack $PSScriptRoot\..\src\sdk\PnP.Core\PnP.Core.csproj --configuration Release --no-build /p:PackageVersion=$version
Write-Host "Packinging PnP.Core.Auth version $version..."
dotnet pack $PSScriptRoot\..\src\sdk\PnP.Core.Auth\PnP.Core.Auth.csproj --configuration Release --no-build /p:PackageVersion=$version
Write-Host "Packinging PnP.Core.Admin version $version..."
dotnet pack $PSScriptRoot\..\src\sdk\PnP.Core.Admin\PnP.Core.Admin.csproj --configuration Release --no-build /p:PackageVersion=$version
Write-Host "Packinging PnP.Core.Transformation version $version..."
dotnet pack $PSScriptRoot\..\src\sdk\PnP.Core.Transformation\PnP.Core.Transformation.csproj --configuration Release --no-build /p:PackageVersion=$version
Write-Host "Packinging PnP.Core.Transformation.SharePoint version $version..."
dotnet pack $PSScriptRoot\..\src\sdk\PnP.Core.Transformation.SharePoint\PnP.Core.Transformation.SharePoint.csproj --configuration Release --no-build /p:PackageVersion=$version

# Sign the nuget package is not needed as Nuget signs the package automatically

# Publish
Write-host "Verify the created NuGet packages in folder \src\sdk\PnP.Core\bin\release and \src\sdk\PnP.Core.Auth\bin\release and others. If OK enter the nuget API key to publish the package, press enter to cancel." -ForegroundColor Yellow 
$apiKey = Read-Host "NuGet API key" 

if ($apiKey.Length -gt 0)
{
    # Push the actual packages and the symbol packages
    nuget push q:\github\pnpcore\src\sdk\PnP.Core\bin\release\PnP.Core.$version.nupkg -ApiKey $apiKey -source https://api.nuget.org/v3/index.json
    nuget push q:\github\pnpcore\src\sdk\PnP.Core.Auth\bin\release\PnP.Core.Auth.$version.nupkg -ApiKey $apiKey -source https://api.nuget.org/v3/index.json
    nuget push q:\github\pnpcore\src\sdk\PnP.Core.Admin\bin\release\PnP.Core.Admin.$version.nupkg -ApiKey $apiKey -source https://api.nuget.org/v3/index.json
    nuget push q:\github\pnpcore\src\sdk\PnP.Core.Transformation\bin\release\PnP.Core.Transformation.$version.nupkg -ApiKey $apiKey -source https://api.nuget.org/v3/index.json
    nuget push q:\github\pnpcore\src\sdk\PnP.Core.Transformation.SharePoint\bin\release\PnP.Core.Transformation.SharePoint.$version.nupkg -ApiKey $apiKey -source https://api.nuget.org/v3/index.json

    # Persist last used version
    Write-Host "Writing $version to git"
    Set-Content -Path .\version.debug.increment -Value $versionIncrement -NoNewline

    # Push change to the repo
    Write-Host "!!Ensure you push in all changes!!" -ForegroundColor Yellow 
}
else 
{
    Write-Host "Publishing of the NuGet packages cancelled!"
}