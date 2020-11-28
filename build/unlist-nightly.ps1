$ApiKey = $("$env:NUGET_API_KEY")

function CleanPackage {
    param([string] $Package , [int] $VersionsToKeep)

    $json = Invoke-WebRequest -Uri "https://api.nuget.org/v3-flatcontainer/$Package/index.json" | ConvertFrom-Json

    # Count the packages to unlist
    $toUnlist = 0;
    foreach ($version in $json.versions) {
        # Only automatically unlist for preview releases
        if ($version.IndexOf("-") -gt -1) {
            $toUnlist = $toUnlist + 1;
        }
    }

    # Delete nightly versions
    $unlisted = 0;
    foreach ($version in $json.versions) {
        # Only automatically unlist for preview releases
        if (($version.IndexOf("-") -gt -1) -and ($unlisted -le ($toUnlist - $VersionsToKeep))) {
            Write-Host "Unlisting $Package, Ver $version"
            $unlisted = $unlisted + 1
            nuget delete $Package $version $ApiKey -source https://api.nuget.org/v3/index.json -NonInteractive
        }
    }
}

CleanPackage "PnP.Core" 10
CleanPackage "PnP.Core.Auth" 10
