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
Remove-PnPList -Identity AddListViaRestAsyncPropertiesTest -Force
Remove-PnPList -Identity AddListItemViaBatchRest -Force
Remove-PnPList -Identity AddListItemViaRest -Force
Remove-PnPList -Identity AddListItemViaRestExceptionTest -Force
Remove-PnPList -Identity AddListItemViaRestNonAsync -Force
Remove-PnPList -Identity AddListItemViaSpecificBatchNonAsyncTest -Force
Remove-PnPList -Identity AddListViaBatchAsyncRest -Force
Remove-PnPList -Identity AddListViaExplicitBatchAsyncRest -Force
Remove-PnPList -Identity AddListViaRestAsync -Force
Remove-PnPList -Identity AddListViaBatchRest -Force
Remove-PnPList -Identity AddListViaExplicitBatchRest -Force
Remove-PnPList -Identity AddListViaRest -Force
Remove-PnPList -Identity CamlListItemGetPagedAsyncPaging -Force
Remove-PnPList -Identity DeleteListItemViaBatchRest -Force
Remove-PnPList -Identity DeleteListItemViaRest -Force
Remove-PnPList -Identity UpdateValuesPropertyViaBatchRest -Force
Remove-PnPList -Identity UpdateValuesPropertyViaRest -Force
Remove-PnPList -Identity GetListAndListItemViaGraph -Force
Remove-PnPList -Identity GetListAndListItemViaRest -Force
Remove-PnPList -Identity GetListPropertiesAndListItemViaGraph -Force
Remove-PnPList -Identity GetListPropertiesAndListItemViaRest -Force
Remove-PnPList -Identity ContentTypesOnListAddTest -Force
Remove-PnPList -Identity ContentTypesOnListAddAvailableTest -Force
Remove-PnPList -Identity ContentTypesOnListDeleteTest -Force
Remove-PnPList -Identity TestQueryListsDeleteConsistency -Force
Remove-PnPList -Identity TestQueryListItemsAddConsistency -Force
Remove-PnPList -Identity TestQueryListItemsDeleteConsistency -Force
Remove-PnPList -Identity TestQueryListItemsUpdateConsistency -Force
Remove-PnPList -Identity TestQueryListsConsistency -Force
Remove-PnPList -Identity TestQueryListsDeleteConsistency -Force
Remove-PnPList -Identity TestQueryListsUpdateConsistency -Force
Remove-PnPList -Identity RESTListItemPaging -Force
Remove-PnPList -Identity RESTListItemGetPagedAsyncPaging -Force
Remove-PnPList -Identity GetItemsByCAMLQuery -Force
Remove-PnPList -Identity GetListIRMSettingsBatchTest -Force
Remove-PnPList -Identity ListLinqGetMethods -Force
Remove-PnPList -Identity SystemUpdate -Force
Remove-PnPList -Identity InteractivePostRequest -Force
Remove-PnPList -Identity "Fail list" -Force
Remove-PnPList -Identity "TestQueryListsUpdateConsistency - Updated" -Force
Remove-PnPList -Identity "TestQueryListsUpdateConsistency - Updated" -Force
Remove-PnPList -Identity "TestQueryListsUpdateConsistency - Updated" -Force
Remove-PnPList -Identity "TestQueryListsUpdateConsistency - Updated" -Force
Remove-PnPList -Identity "TestQueryListsUpdateConsistency - Updated" -Force
Remove-PnPList -Identity UpdateOverwriteVersionBatch -Force

$lists = Get-PnPList
foreach($list in $lists)
{
    if ($list.Title.StartsWith("PNP_SDK_TEST_"))
    {
        Remove-PnPList -Identity $list.Id -Force
    }
}

# Don't Uninstall the app as it's used in pages test scenarios
# $app = Get-PnPApp pnpcoresdk-test-app-client-side-solution
# Uninstall-PnPApp -Identity $app.Id

Disable-PnPFeature -Identity 3bae86a2-776d-499d-9db8-fa4cdc7884f8 -Scope Site
Disable-PnPFeature -Identity fa6a1bcc-fb4b-446b-8460-f4de5f7411d5 -Scope Web
Disable-PnPFeature -Identity 24611c05-ee19-45da-955f-6602264abaf8 -Scope Site # Multilingual

Disconnect-PnPOnline