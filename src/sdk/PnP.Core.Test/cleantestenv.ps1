<#
.SYNOPSIS
Cleans the created test sites so they're ready for a new live run

.EXAMPLE
PS C:\> .\cleantestenv.ps1
#>

# Tenant name 
$tenantName = "" #e.g. contoso
# If you use credential manager then specify the used credential manager entry, if left blank you'll be asked for a user/pwd
$credentialManagerCredentialToUse = ""

# Get the tenant credentials.
$credentials = $null
$UPN = $null
if(![String]::IsNullOrEmpty($credentialManagerCredentialToUse) -and (Get-PnPStoredCredential -Name $credentialManagerCredentialToUse) -ne $null)
{
    $UPN = (Get-PnPStoredCredential -Name $credentialManagerCredentialToUse).UserName
    $credentials = $credentialManagerCredentialToUse
}
else
{
    # Prompts for credentials, if not found in the Windows Credential Manager.
    $UPN = Read-Host -Prompt "Please enter UPN"
    $pass = Read-host -AsSecureString "Please enter password"
    $credentials = new-object management.automation.pscredential $UPN,$pass
}

if($credentials -eq $null) 
{
    Write-Host "Error: No credentials supplied." -ForegroundColor Red
    exit 1
}

if ($tenantName -eq $null -or $tenantName.Length -le 0) 
{
    $tenantName = Read-Host -Prompt 'Input your tenant name (e.g. contoso)'
}

# Tenant admin url
$tenantUrl = "https://$tenantName.sharepoint.com" 
$targetSiteUrl = "https://$tenantName.sharepoint.com/sites/pnpcoresdktestgroup"
$targetSubSiteUrl = "https://$tenantName.sharepoint.com/sites/pnpcoresdktestgroup/subsite"
$noGroupSiteUrl = "https://$tenantName.sharepoint.com/sites/pnpcoresdktest"

$tenantContext = Connect-PnPOnline -Url $tenantUrl -Credentials $credentials -Verbose -ReturnConnection

Connect-PnPOnline -Url $targetSiteUrl -Credentials $credentials

# Remove lists
Remove-PnPList -Identity AddListItemViaBatchRest -Force
Remove-PnPList -Identity AddListItemViaRest -Force
Remove-PnPList -Identity AddListViaBatchRest -Force
Remove-PnPList -Identity AddListViaExplicitBatchRest -Force
Remove-PnPList -Identity AddListViaRest -Force
Remove-PnPList -Identity DeleteListItemViaBatchRest -Force
Remove-PnPList -Identity DeleteListItemViaRest -Force
Remove-PnPList -Identity UpdateValuesPropertyViaBatchRest -Force
Remove-PnPList -Identity UpdateValuesPropertyViaRest -Force
Remove-PnPList -Identity GetListAndListItemViaGraph -Force
Remove-PnPList -Identity GetListAndListItemViaRest -Force
Remove-PnPList -Identity GetListPropertiesAndListItemViaGraph -Force
Remove-PnPList -Identity GetListPropertiesAndListItemViaRest -Force

