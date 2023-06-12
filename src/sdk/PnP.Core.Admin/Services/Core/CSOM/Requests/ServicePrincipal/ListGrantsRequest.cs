using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal
{
    internal class ListGrantsRequest : IRequest<List<IPermissionGrant>>
    {
        private int IdentityPath { get; set; }
        public List<IPermissionGrant> Result { get; } = new();

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            List<ActionObjectPath> result = new();

            #region PermissionRequestsConstructorPath

            ConstructorPath permissionGrantsConstructorPath = new()
            {
                Id = idProvider.GetActionId(),
                TypeId = "{104e8f06-1e00-4675-99c6-1b9b504ed8d8}",
                Parameters = new MethodParameter {Properties = new List<Parameter>()}
            };

            ActionObjectPath permissionRequestsConstructorPathActionPath = new()
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(), ObjectPathId = permissionGrantsConstructorPath.Id.ToString()
                },
                ObjectPath = permissionGrantsConstructorPath
            };

            result.Add(permissionRequestsConstructorPathActionPath);

            #endregion

            #region PermissionGrants

            Property objectPathPropertyPermissionGrants = new()
            {
                Id = idProvider.GetActionId(),
                ParentId = permissionGrantsConstructorPath.Id,
                Name = "PermissionGrants"
            };

            ActionObjectPath actionPathPermissionGrants = new()
            {
                Action = new QueryAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = objectPathPropertyPermissionGrants.Id.ToString(),
                    SelectQuery = new SelectQuery
                    {
                        SelectAllProperties = true, Properties = new List<Property>()
                    },
                    ChildItemQuery = new ChildItemQuery {SelectAllProperties = true}
                },
                ObjectPath = objectPathPropertyPermissionGrants
            };

            IdentityPath = actionPathPermissionGrants.Action.Id;

            result.Add(actionPathPermissionGrants);

            #endregion

            IdentityPath = actionPathPermissionGrants.Action.Id;

            return result;
        }

        public void ProcessResponse(string response)
        {
            List<JsonElement> results = JsonSerializer.Deserialize<List<JsonElement>>(response,
                PnPConstants.JsonSerializer_SPGuidConverter_DateTimeConverter);

            if (results == null)
            {
                return;
            }

            int idIndex = results.FindIndex(r => CSOMResponseHelper.CompareIdElement(r, IdentityPath));

            if (idIndex < 0)
            {
                return;
            }

            JsonElement result = results[idIndex + 1];
            result.TryGetProperty("_Child_Items_", out JsonElement childItemsProperty);

            List<JsonElement> childItems = JsonSerializer.Deserialize<List<JsonElement>>(
                childItemsProperty.GetRawText(), PnPConstants.JsonSerializer_SPGuidConverter_DateTimeConverter);

            if (childItems == null)
            {
                return;
            }

            foreach (JsonElement jsonElement in childItems)
            {
                PermissionGrant permissionRequest = JsonSerializer.Deserialize<PermissionGrant>(
                    jsonElement.GetRawText(),
                    PnPConstants.JsonSerializer_SPGuidConverter_DateTimeConverter);
                Result.Add(permissionRequest);
            }
        }
    }
}
