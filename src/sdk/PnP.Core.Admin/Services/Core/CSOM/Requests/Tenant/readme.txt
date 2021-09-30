XML payload for Tenant.RemoveDeletedSite

<Request AddExpandoFieldTypeSuffix="true" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0" ApplicationName="PnPCoreSDK"
    xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009">
    <Actions>
        <ObjectPath Id="16" ObjectPathId="15" />
        <Query Id="17" ObjectPathId="15">
            <Query SelectAllProperties="false">
                <Properties>
                    <Property Name="PollingInterval" ScalarProperty="true" />
                    <Property Name="IsComplete" ScalarProperty="true" />
                </Properties>
            </Query>
        </Query>
    </Actions>
    <ObjectPaths>
        <Method Id="15" ParentId="1" Name="RemoveDeletedSite">
            <Parameters>
                <Parameter Type="String">https://bertonline.sharepoint.com/sites/something</Parameter>
            </Parameters>
        </Method>
        <Constructor Id="1" TypeId="{268004ae-ef6b-4e9b-8425-127220d84719}" />
    </ObjectPaths>
</Request>

JSON Response:
[
{
"SchemaVersion":"15.0.0.0","LibraryVersion":"16.0.20502.12004","ErrorInfo":null,"TraceCorrelationId":"dfef799f-f09f-2000-5afd-be06bb02fea3"
},2,{
"IsNull":false
},4,{
"IsNull":false
},5,{
"_ObjectType_":"Microsoft.Online.SharePoint.TenantAdministration.SpoOperation","_ObjectIdentity_":"dfef799f-f09f-2000-5afd-be06bb02fea3|908bed80-a04a-4433-b4a0-883d9847d110:7f31a598-4881-464e-bb24-c7a56cb72338\nSpoOperation\nRestoreDeletedSite\n637356871521261694\nhttps%3a%2f%2fcontoso.sharepoint.com%2fsites%2fContoso569\nf10824a7-d766-4be2-bc43-cf43f020082e","PollingInterval":15000,"IsComplete":true
}
]


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