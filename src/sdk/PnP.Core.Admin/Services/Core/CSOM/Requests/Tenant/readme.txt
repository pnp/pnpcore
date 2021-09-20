
XML payload for Tenant.GetTenantInstances():

<Request AddExpandoFieldTypeSuffix="true" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0"
    ApplicationName=".NET Library" xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009">
    <Actions>
        <ObjectPath Id="2" ObjectPathId="1" />
        <ObjectPath Id="4" ObjectPathId="3" />
        <Query Id="5" ObjectPathId="3">
            <Query SelectAllProperties="true">
                <Properties />
            </Query>
            <ChildItemQuery SelectAllProperties="true">
                <Properties />
            </ChildItemQuery>
        </Query>
    </Actions>
    <ObjectPaths>
        <Constructor Id="1" TypeId="{268004ae-ef6b-4e9b-8425-127220d84719}" />
        <Method Id="3" ParentId="1" Name="GetTenantInstances" />
    </ObjectPaths>
</Request>

JSON response: 

[
    {
        "SchemaVersion": "15.0.0.0",
        "LibraryVersion": "16.0.21625.12000",
        "ErrorInfo": null,
        "TraceCorrelationId": "5428ee9f-20ba-3000-12db-df43f108915c"
    },
    2,
    {
        "IsNull": false
    },
    4,
    {
        "IsNull": false
    },
    5,
    {
        "_ObjectType_": "SP.ClientObjectList",
        "_Child_Items_": [
            {
                "_ObjectType_": "Microsoft.Online.SharePoint.TenantAdministration.SPOTenantInstance",
                "DataLocation": null,
                "IsDefaultDataLocation": false,
                "MySiteHostUrl": "https:\u002f\u002fbertonline-my.sharepoint.com\u002f",
                "PortalUrl": "https:\u002f\u002fbertonline.sharepoint.com\u002f",
                "RootSiteUrl": "https:\u002f\u002fbertonline.sharepoint.com\u002f",
                "TenantAdminUrl": "https:\u002f\u002fbertonline-admin.sharepoint.com\u002f"
            }
        ]
    }
]