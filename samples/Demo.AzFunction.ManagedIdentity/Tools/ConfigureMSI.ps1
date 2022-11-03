[CmdletBinding()]
param(
    [string] $tenantId,
    [string] $siteUrl ,
    [string] $appDisplayName=''
)

Import-Module "./ConfigureHelper.ps1"

# Connect-AzureAD -TenantId $tenantId
$SPN= Set-AzureADPermissions  -appDisplayName $appDisplayName -permissionName "Sites.Selected"
$appId=$SPN.AppId
$appName = $SPN.DisplayName

# Connect-PnPOnline -Url $siteUrl -Interactive
Set-PnPSiteAccess -siteUrl $siteUrl -appId $appId -appName $appName -permission Write

# Add to local.settings.json
[uri]$URL = (Get-PnPConnection).Url
$siteName = $URL.Segments[2]

$localSettings = Get-Content "./../local.settings.json" | ConvertFrom-JSON

$localSettings.Values.SiteName= $siteName
$localSettings.Values.TenantName = (Get-AzureADTenantDetail).DisplayName

# Save with encoding UTF-8 without BOM
#Otherwise "failed to parse local.settings.json invalid symbol near line 1 column 0"
($localSettings | ConvertTo-JSON) | Out-File "./../local.settings.json" -Encoding utf8