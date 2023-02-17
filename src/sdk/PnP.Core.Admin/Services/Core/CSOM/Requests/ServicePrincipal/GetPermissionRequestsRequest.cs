using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Model.SharePoint.Core.Internal;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal
{
    internal sealed class GetPermissionRequestsRequest : IRequest<List<IPermissionRequest>>
    {
        private int IdentityPath { get; set; }
        public List<IPermissionRequest> Result { get; } = new();

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
                PermissionRequest permissionRequest = JsonSerializer.Deserialize<PermissionRequest>(
                    jsonElement.GetRawText(),
                    PnPConstants.JsonSerializer_SPGuidConverter_DateTimeConverter);
                Result.Add(permissionRequest);
            }
        }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            List<ActionObjectPath> result = new();

            #region PermissionRequestsConstructorPath

            ConstructorPath permissionRequestsConstructorPath = new()
            {
                Id = idProvider.GetActionId(),
                TypeId = "{104e8f06-1e00-4675-99c6-1b9b504ed8d8}",
                Parameters = new MethodParameter {Properties = new List<Parameter>()}
            };

            ActionObjectPath permissionRequestsConstructorPathActionPath = new()
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = permissionRequestsConstructorPath.Id.ToString()
                },
                ObjectPath = permissionRequestsConstructorPath
            };

            result.Add(permissionRequestsConstructorPathActionPath);

            #endregion

            #region PermissionRequests

            Property objectPathPropertyPermissionRequests = new()
            {
                Id = idProvider.GetActionId(),
                ParentId = permissionRequestsConstructorPath.Id,
                Name = "PermissionRequests"
            };

            ActionObjectPath actionPathPermissionRequests = new()
            {
                Action = new QueryAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = objectPathPropertyPermissionRequests.Id.ToString(),
                    SelectQuery = new SelectQuery
                    {
                        SelectAllProperties = true, Properties = new List<Property>()
                    },
                    ChildItemQuery = new ChildItemQuery {SelectAllProperties = true}
                },
                ObjectPath = objectPathPropertyPermissionRequests
            };

            IdentityPath = actionPathPermissionRequests.Action.Id;

            result.Add(actionPathPermissionRequests);

            #endregion

            return result;
        }
    }
}
