
function Set-AzureADPermissions {

    param(
        $appDisplayName,
        $permissionName
    )
    $GraphAppId = "00000003-0000-0000-c000-000000000000"  # Microsoft Graph
    $SPOAppId = "00000003-0000-0ff1-ce00-000000000000" # SharePoint Online

    #Retrieve the Azure AD Service Principal instance for the Microsoft Graph (00000003-0000-0000-c000-000000000000) or SharePoint Online (00000003-0000-0ff1-ce00-000000000000).
    $servicePrincipal_Graph = Get-AzureADServicePrincipal -Filter "appId eq '$GraphAppId'"
    $servicePrincipal_SPO = Get-AzureADServicePrincipal -Filter "appId eq '$SPOAppId'"
    #$permissionName = "Sites.Selected"

    $SPN = Get-AzADServicePrincipal -Filter "displayName eq '$appDisplayName'"
    Start-Sleep -Seconds 10

    # Use application permissions. Delegate permissions cannot be utilized using a Managed Identity.
    # $servicePrincipal_Graph.AppRole | Where-Object { $_.AllowedMemberType -eq "Application" -and $_.Value -eq "Sites.Selected"}

    $appRole_Graph = $servicePrincipal_Graph.AppRoles | Where-Object { $_.AllowedMemberTypes -eq "Application" -and $_.Value -eq $permissionName }
    $appRole_SPO = $servicePrincipal_SPO.AppRoles | Where-Object { $_.AllowedMemberTypes -eq "Application" -and $_.Value -eq $permissionName }

    # Grant API Permissions
    New-AzureAdServiceAppRoleAssignment -ObjectId $SPN.Id -PrincipalId $SPN.Id -ResourceId $servicePrincipal_Graph.ObjectId -Id $appRole_Graph.Id
    New-AzureAdServiceAppRoleAssignment -ObjectId $SPN.Id -PrincipalId $SPN.Id -ResourceId $servicePrincipal_SPO.ObjectId   -Id $appRole_SPO.Id

    Start-Sleep -Seconds 10
    Get-AzureADServiceAppRoleAssignedTo -ObjectId  $SPN.Id

    Write-Host "Now grant access to SPO site using the following:"
    Write-Host "App Id: $($SPN.AppId)"
    Write-Host "App Name: $($SPN.DisplayName)"

    return $SPN
}

function Set-PnPSiteAccess {
    param(
        $siteUrl,
        $appId,
        $appName,
        [ValidateSet('Read', 'Write', 'FullControl')]
        $permission

    )
    if ($permission -ne 'FullControl' ) {
        Grant-PnPAzureADAppSitePermission -AppId $appId -DisplayName $appName -Site $siteUrl -Permissions $permission
    }
    else {
        Grant-PnPAzureADAppSitePermission -AppId $appId -DisplayName $appName -Site $siteUrl -Permissions Write
        $PermissionId = Get-PnPAzureADAppSitePermission -AppIdentity $appId
        Set-PnPAzureADAppSitePermission -Site $siteurl -PermissionId $(($PermissionId).Id) -Permissions FullControl
    }
}