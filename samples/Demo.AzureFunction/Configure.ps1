param (
	[Parameter(Mandatory=$true)]
	[string]
	$SiteUrl,
	[Parameter(Mandatory=$true)]
	[string]
	$Tenant,
	[Parameter(Mandatory=$true)]
	[string]
	$CertificatePassword,
	[string]$AzureADAppName = "PnP.Core.SDK.AzureFunctionSample",
	[string]$CertificateOutDir = ".\Certificates"
)

if (-not ( Test-Path -Path $CertificateOutDir -PathType Container )) {
	md $CertificateOutDir
}

$app = Register-PnPAzureADApp -ApplicationName $AzureADAppName -Tenant $Tenant -OutPath $CertificateOutDir `
	-CertificatePassword (ConvertTo-SecureString -String $CertificatePassword -AsPlainText -Force) `
	-Scopes "MSGraph.Group.ReadWrite.All","MSGraph.User.ReadWrite.All","SPO.Sites.FullControl.All","SPO.TermStore.ReadWrite.All","SPO.User.ReadWrite.All" `
	-Store CurrentUser -DeviceLogin

$localSettings = Get-Content local.settings.sample.json | ConvertFrom-JSON
$localSettings.Values.SiteUrl = $SiteUrl
$localSettings.Values.TenantId = Get-PnPTenantId
$localSettings.Values.ClientId = $app.'AzureAppId/ClientId'
$localSettings.Values.CertificateThumbPrint = $app.'Certificate Thumbprint' 
$localSettings.Values.WEBSITE_LOAD_CERTIFICATES = $app.'Certificate Thumbprint'

($localSettings | ConvertTo-JSON) > local.settings.json