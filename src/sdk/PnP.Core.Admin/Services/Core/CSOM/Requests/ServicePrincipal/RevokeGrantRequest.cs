using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal
{
    internal class RevokeGrantRequest : IRequest<IPermissionGrant>
    {
        private int IdentityPath { get; set; }
        private CSOMResponseHelper ResponseHelper { get; } = new();
        public string ObjectId { get; set; }
        public IPermissionGrant Result { get; private set; }

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
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = objectPathPropertyPermissionGrants.Id.ToString()
                },
                ObjectPath = objectPathPropertyPermissionGrants
            };

            IdentityPath = actionPathPermissionGrants.Action.Id;

            result.Add(actionPathPermissionGrants);

            #endregion

            #region GetByObjectId

            ObjectPathMethod getByObjectIdObjectPathMethod = new()
            {
                Id = idProvider.GetActionId(),
                ParentId = objectPathPropertyPermissionGrants.Id,
                Name = "GetByObjectId",
                Parameters = new MethodParameter
                {
                    Properties = new List<Parameter>
                    {
                        new() {Type = "String", Value = ObjectId}
                    }
                }
            };

            ActionObjectPath actionPathGetByObjectId = new()
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(), ObjectPathId = getByObjectIdObjectPathMethod.Id.ToString()
                },
                ObjectPath = getByObjectIdObjectPathMethod
            };

            result.Add(actionPathGetByObjectId);

            #endregion

            #region DeleteObject

            ActionObjectPath actionPathDeleteObject = new()
            {
                Action = new MethodAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = getByObjectIdObjectPathMethod.Id.ToString(),
                    Name = "DeleteObject"
                }
            };
            
            result.Add(actionPathDeleteObject);
            
            #endregion
            
            #region Query

            ActionObjectPath actionPathQueryAllProperties = new()
            {
                Action = new QueryAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = getByObjectIdObjectPathMethod.Id.ToString(),
                    SelectQuery = new SelectQuery
                    {
                        SelectAllProperties = true, Properties = new List<Property>()
                    }
                }
            };

            result.Add(actionPathQueryAllProperties);

            #endregion

            IdentityPath = actionPathQueryAllProperties.Action.Id;

            return result;
        }

        public void ProcessResponse(string response)
        {
            PermissionGrant resultItem = ResponseHelper.ProcessResponse<PermissionGrant>(response, IdentityPath);
            Result = resultItem;
        }
    }
}
