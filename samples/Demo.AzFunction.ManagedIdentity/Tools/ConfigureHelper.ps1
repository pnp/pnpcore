
function Set-AzureADPermissions {
    param(
        [Parameter(Mandatory = $true)]
        [string]
        $appDisplayName,
        [Parameter(Mandatory = $true)]
        [string]
        $permissionName
    )

    $GraphAppId = "00000003-0000-0000-c000-000000000000"  # Microsoft Graph
    $SPOAppId = "00000003-0000-0ff1-ce00-000000000000" # SharePoint Online

    #Retrieve the Azure AD Service Principal instance for the Microsoft Graph (00000003-0000-0000-c000-000000000000) or SharePoint Online (00000003-0000-0ff1-ce00-000000000000).
    $servicePrincipal_Graph = Get-AzureADServicePrincipal -Filter "appId eq '$GraphAppId'"
    $servicePrincipal_SPO = Get-AzureADServicePrincipal -Filter "appId eq '$SPOAppId'"
    #$permissionName = "Sites.Selected"

    $SPN = Get-AzureADServicePrincipal -Filter "displayName eq '$appDisplayName'"
    Write-Host "App $appDisplayName created with client id: $($SPN.AppId)"

    # Use application permissions. Delegate permissions cannot be utilized using a Managed Identity.
    # $servicePrincipal_Graph.AppRole | Where-Object { $_.AllowedMemberType -eq "Application" -and $_.Value -eq "Sites.Selected"}

    $appRole_GraphId = ($servicePrincipal_Graph.AppRoles | Where-Object { $_.AllowedMemberTypes -eq "Application" -and $_.Value -eq $permissionName }).Id
    $appRole_SPOId = ($servicePrincipal_SPO.AppRoles | Where-Object { $_.AllowedMemberTypes -eq "Application" -and $_.Value -eq $permissionName }).Id

    # Grant API Permissions
    New-AzureAdServiceAppRoleAssignment -ObjectId $($SPN.ObjectId) -PrincipalId $($SPN.ObjectId) -ResourceId $($servicePrincipal_Graph.ObjectId) -Id $appRole_GraphId
    New-AzureAdServiceAppRoleAssignment -ObjectId $($SPN.ObjectId) -PrincipalId $($SPN.ObjectId) -ResourceId $($servicePrincipal_SPO.ObjectId)   -Id $appRole_SPOId

    Start-Sleep -Seconds 10
    Get-AzureADServiceAppRoleAssignedTo -ObjectId  $SPN.Id

    return $SPN
}

function Set-PnPSiteAccess {
    param(
        [Parameter(Mandatory = $true)]
        [string]
        $siteUrl,
        [Parameter(Mandatory = $true)]
        [string]
        $appId,
        [Parameter(Mandatory = $true)]
        [string]
        $appName,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Read', 'Write', 'FullControl')]
        [string]$Permissions
    )

    if ($permission -ne 'FullControl' ) {
        Grant-PnPAzureADAppSitePermission -AppId $appId -DisplayName $appName -Site $siteUrl -Permissions $Permissions
    }
    else {
        Grant-PnPAzureADAppSitePermission -AppId $appId -DisplayName $appName -Site $siteUrl -Permissions Write
        $PermissionId = Get-PnPAzureADAppSitePermission -AppIdentity $appId
        Set-PnPAzureADAppSitePermission -Site $siteurl -PermissionId $(($PermissionId).Id) -Permissions FullControl
    }
}

function Add-AppRegistration{
    param(
        [Parameter(Mandatory = $true)]
        [string]
        $SiteUrl,
        [Parameter(Mandatory = $true)]
        [string]
        $TenantId,
        [string]$AzureADAppName = "Demo-AzFunction-ManagedIdentity",
        [Parameter(Mandatory = $true)]
        [ValidateSet('Read', 'Write', 'FullControl')]
        [string]$Permissions,

        [Parameter(Mandatory = $true)]
        [string]
        $CertificatePwd,
        [string]$CertificateOutDir = ".\Certificates"
    )
    if (-not ( Test-Path -Path $CertificateOutDir -PathType Container )) {
        $f= mkdir $CertificateOutDir
    }
    # Set the name for Appregistration to {app}-LocalDev
    $appDisplayName = "$AzureADAppName-LocalDev"

    $app= Register-PnPAzureADApp -ApplicationName $appDisplayName -Tenant $TenantId -OutPath $CertificateOutDir `
        -CertificatePassword (ConvertTo-SecureString -String $CertificatePwd -AsPlainText -Force) `
        -GraphApplicationPermissions "Sites.Selected" -SharePointApplicationPermissions "Sites.Selected" `
        -Store CurrentUser -Interactive

    Write-Host "App $appDisplayName created with client id: $($app.'AzureAppId/ClientId')"
    Write-Host "Granting permissions to the SPO site..."
    # Grant API permissions to SPO site [Read | Write | FullControl]
    Set-PnPSiteAccess -siteUrl $siteUrl -appId $($app.'AzureAppId/ClientId') -appName $appDisplayName -permission $Permissions

    return $app
}

function Set-ApiPermissionsMSI{
param(
    [Parameter(Mandatory = $true)]
    [string]
    $SiteUrl,
    [Parameter(Mandatory = $true)]
    [string]
    $TenantId,
    [string] $AzureADAppName = "Demo-AzFunction-ManagedIdentity",
    [Parameter(Mandatory = $true)]
    [ValidateSet('Read', 'Write', 'FullControl')]
    [string]$Permissions
)

    # Find the Enterprise Application related to the az Function's Managed Identity
    # Grant Graph and SharePoint API Permission: Sites.Selected
    $SPN= Set-AzureADPermissions  -appDisplayName $AzureADAppName -permissionName "Sites.Selected"

    Write-Host "Granting $($SPN.DisplayName) permissions to the SPO site..."
    # Grant API permissions to SPO site [Read | Write | FullControl]
    Set-PnPSiteAccess -siteUrl $siteUrl -appId $($SPN.AppId) -appName $($SPN.DisplayName) -permission $Permissions
}