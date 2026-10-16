<#
.SYNOPSIS
Creates the needed sites (communication site with sub site + group connected team site with sub site and Team) for doing "live" testing of the PnP Microsoft 365 Library.

This script creates:
- A communication site named pnpcoresdktest with a sub site named subsite
- A modern team site which uses the group name pnpcoresdktestgroup and has a sub site named subsite. The group connected to this site also does have a Team connected (teamified site)
- a test document named test.docx

Note: You do need to have an app catalog setup before running this script. Check if Get-PnPTenantAppCatalogUrl returns an app catalog url if you are in doubt

.PARAMETER TenantName
The name of the tenant (e.g. contoso). Required.

.PARAMETER CredentialManagerCredentialToUse
The name of the credential manager entry to use for authentication. If omitted, you will be prompted for credentials.

.PARAMETER ClientId
The client ID (app ID) of the Entra app registration to use for sign-in. Required.

.PARAMETER AppsAlreadyInCatalog
Indicates that the client side app packages have already been added to the tenant app catalog. When specified, the script will retrieve the existing apps instead of uploading them. Defaults to $false (packages will be uploaded).

.EXAMPLE
PS C:\> .\setuptestenv-refactor.ps1 -TenantName contoso -ClientId 00000000-0000-0000-0000-000000000000

.EXAMPLE
PS C:\> .\setuptestenv-refactor.ps1 -TenantName contoso -ClientId 00000000-0000-0000-0000-000000000000 -CredentialManagerCredentialToUse MyCredEntry

.EXAMPLE
PS C:\> .\setuptestenv-refactor.ps1 -TenantName contoso -ClientId 00000000-0000-0000-0000-000000000000 -AppsAlreadyInCatalog
#>

[CmdletBinding()]
param (
  [Parameter(Mandatory = $true, HelpMessage = "The name of the tenant (e.g. contoso).")]
  [string]$TenantName,

  [Parameter(Mandatory = $false, HelpMessage = "The name of the credential manager entry to use for authentication. If omitted, you will be prompted for credentials.")]
  [string]$CredentialManagerCredentialToUse,

  [Parameter(Mandatory = $true, HelpMessage = "The client ID (app ID) of the Entra app registration to use for sign-in.")]
  [string]$ClientId,

  [Parameter(Mandatory = $false, HelpMessage = "Indicate that the client side app packages have already been added to the tenant app catalog. When set, existing apps are retrieved instead of uploaded.")]
  [switch]$AppsAlreadyInCatalog = $false
)

$ErrorActionPreference = 'Stop'

# Resolve credentials
$credentials = $null
$UPN = $null

if (![String]::IsNullOrEmpty($CredentialManagerCredentialToUse) -and (Get-PnPStoredCredential -Name $CredentialManagerCredentialToUse) -ne $null) {
  Write-Host "Using credentials from Credential Manager entry: $CredentialManagerCredentialToUse"

  $UPN = (Get-PnPStoredCredential -Name $CredentialManagerCredentialToUse).UserName
  $credentials = $CredentialManagerCredentialToUse
   
  if ($credentials -eq $null) {
    Write-Error "Error: No credentials supplied." -ForegroundColor Red
    exit 1
  }
}

# Tenant admin url
$tenantUrl = "https://$TenantName.sharepoint.com"

if (![String]::IsNullOrEmpty($CredentialManagerCredentialToUse)) {
    Write-Host "Connecting to tenant admin site with credentials from Credential Manager entry: $CredentialManagerCredentialToUse"

    $tenantContext = Connect-PnPOnline -Url $tenantUrl -Credentials $credentials -ClientId $ClientId -ReturnConnection
  }
  else {
    Write-Host "Connecting to tenant admin site with interactive login"

    $tenantContext = Connect-PnPOnline -Url $tenantUrl -Interactive -ClientId $ClientId -ReturnConnection
  }

  # Add test Client Side app package
  Write-Host "Adding client side app packages to tenant app catalog (if not already present)"
  if (-not $AppsAlreadyInCatalog) {
    $app = Add-PnPApp -Path .\TestAssets\pnpcoresdk-test-app.sppkg -Publish -Connection $tenantContext
    $app2 = Add-PnPApp -Path .\TestAssets\viva-async-ace.sppkg -Publish -Connection $tenantContext
  }
  else {
    # Retrieve existing apps from the tenant app catalog
    $app = Get-PnPApp -Identity "pnpcoresdk-test-app-client-side-solution" -Connection $tenantContext
    $app2 = Get-PnPApp -Identity "viva-async-ace-client-side-solution" -Connection $tenantContext
  }

  # Create test site without a group
  Write-Host "Checking if communication site already exists: https://$TenantName.sharepoint.com/sites/pnpcoresdktest"
  $existingCommSite = Get-PnPTenantSite -Url "https://$TenantName.sharepoint.com/sites/pnpcoresdktest" -Connection $tenantContext -ErrorAction SilentlyContinue
  if ($null -eq $existingCommSite) {
    Write-Host "Creating communication site: https://$TenantName.sharepoint.com/sites/pnpcoresdktest"
    $pnpTestSite = New-PnPSite -Type CommunicationSite -Title "PnP Microsoft 365 library test" -Url "https://$TenantName.sharepoint.com/sites/pnpcoresdktest" -Connection $tenantContext
    Start-Sleep -Seconds 30
  }
  else {
    Write-Host "Communication site already exists, skipping creation"
    $pnpTestSite = $existingCommSite.Url
  }
  
  # Connect to created site
  if (![String]::IsNullOrEmpty($CredentialManagerCredentialToUse)) {
    Connect-PnPOnline -Url $pnpTestSite -Credentials $credentials -ClientId $ClientId
  }
  else {
    Connect-PnPOnline -Url $pnpTestSite -Interactive -ClientId $ClientId
  }
  
  # Add sub site
  Write-Host "Checking if sub site already exists: https://$TenantName.sharepoint.com/sites/pnpcoresdktest/subsite"
  $existingCommSubSite = Get-PnPSubWeb -Identity "subsite" -ErrorAction SilentlyContinue
  if ($null -eq $existingCommSubSite) {
    Write-Host "Adding sub site to communication site: https://$TenantName.sharepoint.com/sites/pnpcoresdktest/subsite"
    New-PnPWeb -Title "Sub site" -Url "subsite" -Locale 1033 -Template "STS#3"
  }
  else {
    Write-Host "Sub site already exists, skipping creation"
  }

  # Create test site with group
  Write-Host "Checking if team site with group already exists: https://$TenantName.sharepoint.com/sites/pnpcoresdktestgroup"
  $existingGroupSite = Get-PnPTenantSite -Url "https://$TenantName.sharepoint.com/sites/pnpcoresdktestgroup" -Connection $tenantContext -ErrorAction SilentlyContinue
  if ($null -eq $existingGroupSite) {
    Write-Host "Creating team site with group: https://$TenantName.sharepoint.com/sites/pnpcoresdktestgroup"
    $pnpTestSiteWithGroup = New-PnPSite -Type TeamSite -Title "PnP Microsoft 365 library test with group" -Alias pnpcoresdktestgroup -IsPublic -Connection $tenantContext
    Start-Sleep -Seconds 30
  }
  else {
    Write-Host "Team site with group already exists, skipping creation"
    $pnpTestSiteWithGroup = $existingGroupSite.Url
  }
  
  # Connect to the newly created site
  if (![String]::IsNullOrEmpty($CredentialManagerCredentialToUse))   {
    Connect-PnPOnline -Url $pnpTestSiteWithGroup -Credentials $credentials -ClientId $ClientId
  }
  else {
    Connect-PnPOnline -Url $pnpTestSiteWithGroup -Interactive -ClientId $ClientId
  }

  # Install the client side apps to the site
  Write-Host "Installing client side apps to team site with group"
  $installedApp = Get-PnPApp -Identity $app.Id -ErrorAction SilentlyContinue
  if ($null -eq $installedApp -or $null -eq $installedApp.InstalledVersion) {
    Install-PnPApp -Identity $app
  }
  else {
    Write-Host "App '$($app.Title)' is already installed, skipping"
  }
  $installedApp2 = Get-PnPApp -Identity $app2.Id -ErrorAction SilentlyContinue
  if ($null -eq $installedApp2 -or $null -eq $installedApp2.InstalledVersion) {
    Install-PnPApp -Identity $app2
  }
  else {
    Write-Host "App '$($app2.Title)' is already installed, skipping"
  }

  # Teamify the site
  Write-Host "Checking if team site with group is already teamified"
  $teamsTeam = Get-PnPMicrosoft365GroupTeam -ErrorAction SilentlyContinue
  if ($null -eq $teamsTeam) {
    Write-Host "Teamifying the team site with group"
    Add-PnPTeamsTeam
  }
  else {
    Write-Host "Team site with group is already teamified, skipping"
  }

  # Create test document in default documents Library
  Write-Host "Checking if test document already exists in Shared Documents"
  $existingFile = Get-PnPFile -Url "Shared Documents/test.docx" -ErrorAction SilentlyContinue
  if ($null -eq $existingFile) {
    Write-Host "Adding test document to team site with group"
    Add-PnPFile -Path .\TestAssets\test.docx -Folder "Shared Documents"
  }
  else {
    Write-Host "Test document already exists, skipping upload"
  }

  # Add sub site
  Write-Host "Checking if sub site already exists: https://$TenantName.sharepoint.com/sites/pnpcoresdktestgroup/subsite"
  $existingGroupSubSite = Get-PnPSubWeb -Identity "subsite" -ErrorAction SilentlyContinue
  if ($null -eq $existingGroupSubSite) {
    Write-Host "Adding sub site to team site with group: https://$TenantName.sharepoint.com/sites/pnpcoresdktestgroup/subsite"
    New-PnPWeb -Title "Sub site" -Url "subsite" -Locale 1033 -Template "STS#3"
  }
  else {
    Write-Host "Sub site already exists, skipping creation"
  }

  # Create the classic team site
  Write-Host "Checking if classic team site already exists: https://$TenantName.sharepoint.com/sites/sts0"
  $existingClassicSite = Get-PnPTenantSite -Url "https://$TenantName.sharepoint.com/sites/sts0" -Connection $tenantContext -ErrorAction SilentlyContinue
  if ($null -eq $existingClassicSite) {
    Write-Host "Creating classic team site: https://$TenantName.sharepoint.com/sites/sts0"
    if ([String]::IsNullOrEmpty($UPN)) {
      $currentUser = (Get-PnPWeb -Connection $tenantContext -Includes CurrentUser).CurrentUser
      $UPN = $currentUser.Email

      if ([String]::IsNullOrEmpty($UPN)) {
        # Fall back to LoginName and strip the claims prefix
        # LoginName comes back as i:0#.f|membership|my-upn@contoso.onmicrosoft.com so strip the prefix
        $UPN = $currentUser.LoginName -replace "i:0#\.f\|membership\|", ""
      }
    }
    New-PnPTenantSite -Title "Classic Team site" -Url "https://$TenantName.sharepoint.com/sites/sts0" -Lcid 1033 -Template "STS#0" -Owner $UPN -TimeZone 4
  }
  else {
    Write-Host "Classic team site already exists, skipping creation"
  }

  # TODO: When ALM support is implemented, remove this from here and move to TestAssets helper
  # Install the client side app to the communication site
  if (![String]::IsNullOrEmpty($CredentialManagerCredentialToUse)) {
    Connect-PnPOnline -Url $pnpTestSite -Credentials $credentials -ClientId $ClientId
  }
  else {
    Connect-PnPOnline -Url $pnpTestSite -Interactive -ClientId $ClientId
  }
  Write-Host "Checking if client side app is already installed on communication site"
  $installedAppComm = Get-PnPApp -Identity $app.Id -ErrorAction SilentlyContinue
  if ($null -eq $installedAppComm -or $null -eq $installedAppComm.InstalledVersion) {
    Write-Host "Installing client side app to communication site"
    Install-PnPApp -Identity $app.Id
  }
  else {
    Write-Host "App '$($app.Title)' is already installed on communication site, skipping"
  }

Write-Host "All sites are created, next step is updating your test configuration file with the created urls"
Disconnect-PnPOnline
