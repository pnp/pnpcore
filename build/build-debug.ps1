$versionIncrement = Get-Content .\version.debug.increment -Raw
$versionIncrement = $versionIncrement -as [int]
$versionIncrement = $versionIncrement + 1

$version = Get-Content .\version.debug -Raw

$version = $version.Replace("{incremental}", $versionIncrement)

Write-Host "Building PnP.Core versions $version"
dotnet build ..\src\sdk\PnP.Core\PnP.Core.csproj --no-incremental /p:Version=$version

Write-Host "Building PnP.Core.Auth versions $version"
dotnet build ..\src\sdk\PnP.Core.Auth\PnP.Core.Auth.csproj --no-incremental /p:Version=$version

Write-Host "Packinging PnP.Core versions $version"
dotnet pack ..\src\sdk\PnP.Core\PnP.Core.csproj --no-build /p:PackageVersion=$version

Write-Host "Packinging PnP.Core.Auth versions $version"
dotnet pack ..\src\sdk\PnP.Core.Auth\PnP.Core.Auth.csproj --no-build /p:PackageVersion=$version

#Write-Host "Writing $version to git"
#Set-Content -Path .\version.debug.increment -Value $versionIncrement

#Push to the repo
# git add .\version.debug.increment
# git commit -m "Build increment - debug version $versionIncrement"
# git push
