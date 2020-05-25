$versionIncrement = Get-Content .\version.debug.increment -Raw
$versionIncrement = $versionIncrement -as [int]
$versionIncrement = $versionIncrement + 1

$version = Get-Content .\version.debug -Raw

$version = $version.Replace("{incremental}", $versionIncrement).Replace("{yearmonth}", $(get-date -format yy) + $(get-date -format MM))

Write-Host "Building version $version"
dotnet build ..\src\sdk\PnP.Core\PnP.Core.csproj --no-incremental /p:Version=$version

Write-Host "Packinging $version"
dotnet pack ..\src\sdk\PnP.Core\PnP.Core.csproj --no-build /p:PackageVersion=$version

Write-Host "Writing $version to git"
Set-Content -Path .\version.debug.increment -Value $versionIncrement
