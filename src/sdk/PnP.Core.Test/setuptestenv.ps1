<#
.SYNOPSIS
Creates the needed sites (communitation site with sub site + group connected team site with sub site and Team) for doing "live" testing of the PnP Microsoft 365 Library.

This script creates:
- A communication site named pnpcoresdktest with a sub site named subsite
- A modern team site which uses the group name pnpcoresdktestgroup and has a sub site anmed subsite. The group connected to this site also does have a Team connected (teamified site)
    - a test document named test.docx

.EXAMPLE
PS C:\> .\setuptestenv.ps1
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

$tenantContext = Connect-PnPOnline -Url $tenantUrl -Credentials $credentials -Verbose -ReturnConnection

# Add test Client Side app package
$app = Add-PnPApp -Path .\TestAssets\pnpcoresdk-test-app.sppkg


# Create test site without a group 
$pnpTestSite = New-PnPSite -Type CommunicationSite -Title "PnP Microsoft 365 library test" -Url  "https://$tenantName.sharepoint.com/sites/pnpcoresdktest"  -Wait -Connection $tenantContext
# Connect to created site
Connect-PnPOnline -Url $pnpTestSite -Credentials $credentials
# Add sub site 
New-PnPWeb -Title "Sub site" -Url "subsite" -Locale 1033 -Template "STS#3"

# Create test site with group
$pnpTestSiteWithGroup = New-PnPSite -Type TeamSite -Title "PnP Microsoft 365 library test with group" -Alias pnpcoresdktestgroup -IsPublic -Wait -Connection $tenantContext
# Connect to the newly created site
Connect-PnPOnline -Url $pnpTestSiteWithGroup -Credentials $credentials
# Teamify the site
Add-PnPTeamsTeam
# Create test document in default documents Library
Add-PnPFile -Path .\TestAssets\test.docx -Folder "Shared Documents"  
# Add sub site 
New-PnPWeb -Title "Sub site" -Url "subsite" -Locale 1033 -Template "STS#3"
# TODO: When ALM support is implemented, remove this from here and move to TestAssets helper
# Install the client side app to the site
Install-PnPApp -Identity $app.Id 

Write-Host "All sites are created, next step is updating your test configuration file with the created urls"

Disconnect-PnPOnline
