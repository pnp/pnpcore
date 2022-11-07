param (
    [Parameter(Mandatory = $true)]
    [string]
    $SiteUrl,
    [Parameter(Mandatory = $true)]
    [string]
    $TenantId,
    [Parameter(Mandatory = $true)]
    [string]
    $CertificatePassword,
    [string]$AzureADAppName = "Demo-AzFunction-ManagedIdentity",
    [string]$CertificateOutDir = ".\Certificates"
)
Import-Module "./ConfigureHelper.ps1"

if (-not ( Test-Path -Path $CertificateOutDir -PathType Container )) {
    md $CertificateOutDir
}
$appDisplayName = "$AzureADAppName-LocalDev"

$app = Register-PnPAzureADApp -ApplicationName $appDisplayName -Tenant $TenantId -OutPath $CertificateOutDir `
    -CertificatePassword (ConvertTo-SecureString -String $CertificatePassword -AsPlainText -Force) `
    -GraphApplicationPermissions "Sites.Selected" -SharePointApplicationPermissions "Sites.Selected" `
    -Store CurrentUser -Interactive

Start-Sleep -Seconds 10
$SPN = Get-AzADServicePrincipal -Filter "displayName eq '$appDisplayName'"
Set-PnPSiteAccess -siteUrl $siteUrl -appId $($SPN.AppId) -appName $($SPN.DisplayName) -permission Write

$localSettings = Get-Content "./../local.settings.json" | ConvertFrom-JSON
$localSettings.Values.TenantId= $TenantId
$localSettings.Values.ClientId= $app.'AzureAppId/ClientId'
$localSettings.Values.CertificateThumbPrint= $app.'Certificate Thumbprint'
$localSettings.Values.WEBSITE_LOAD_CERTIFICATES= $app.'Certificate Thumbprint'
$localSettings.Values.SiteName= $siteName
$localSettings.Values.TenantName= (Get-AzureADTenantDetail).DisplayName

($localSettings | ConvertTo-JSON) | Out-File "./../local.settings.json"  -Encoding utf8
