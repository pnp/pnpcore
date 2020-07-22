$versionIncrement = Get-Content .\version.debug.increment -Raw
$versionIncrement = $versionIncrement -as [int]
$versionIncrement = $versionIncrement + 1

$version = Get-Content .\version.debug -Raw
$blazorVersion = Get-Content .\version.blazor -Raw

$version = $version.Replace("{incremental}", $versionIncrement).Replace("{yearmonth}", $(get-date -format yy) + $(get-date -format MM))
$blazorVersion = $blazorVersion.Replace("{incremental}", $versionIncrement).Replace("{yearmonth}", $(get-date -format yy) + $(get-date -format MM))

Write-Host "Building .Net Standard 2.1 version $version"
dotnet build ..\src\sdk\PnP.Core\PnP.Core.csproj --no-incremental /p:Version=$version

Write-Host "Building Blazor .Net Standard 2.1 version $version"
dotnet build ..\src\sdk\PnP.Core\PnP.Core.csproj --no-incremental --configuration Blazor /p:Version=$blazorVersion

Write-Host "Packinging .Net Standard 2.1 version $version"
dotnet pack ..\src\sdk\PnP.Core\PnP.Core.csproj --no-build /p:PackageVersion=$version

Write-Host "Packinging Blazor .Net Standard 2.1 version $version"
dotnet pack ..\src\sdk\PnP.Core\PnP.Core.csproj --no-build --configuration Blazor /p:PackageVersion=$blazorVersion

Write-Host "Writing $version to git"
Set-Content -Path .\version.debug.increment -Value $versionIncrement

#Push to the repo
# git add .\version.debug.increment
# git commit -m "Build increment - debug version $versionIncrement"
# git push
