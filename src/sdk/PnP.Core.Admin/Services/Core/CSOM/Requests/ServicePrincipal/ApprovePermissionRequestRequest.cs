using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal
{
    internal sealed class ApprovePermissionRequest : IRequest<IPermissionGrant>
    {
        public string RequestId { get; set; }
        private CSOMResponseHelper ResponseHelper { get; set; } = new();
        private int IdentityPath { get; set; }
        public IPermissionGrant Result { get; private set; }

        public void ProcessResponse(string response)
        {
            PermissionGrant resultItem = ResponseHelper.ProcessResponse<PermissionGrant>(response, IdentityPath);
            Result = resultItem;
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
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = objectPathPropertyPermissionRequests.Id.ToString()
                },
                ObjectPath = objectPathPropertyPermissionRequests
            };

            result.Add(actionPathPermissionRequests);

            #endregion

            #region GetById

            ObjectPathMethod getByIdObjectPathMethod = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = objectPathPropertyPermissionRequests.Id,
                Name = "GetById",
                Parameters = new MethodParameter
                {
                    Properties = new List<Parameter> {new() {Type = "Guid", Value = RequestId}}
                }
            };

            ActionObjectPath actionPathGetById = new()
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(), ObjectPathId = getByIdObjectPathMethod.Id.ToString()
                },
                ObjectPath = getByIdObjectPathMethod
            };

            result.Add(actionPathGetById);

            #endregion

            #region Approve

            ObjectPathMethod approveObjectPathMethod = new ObjectPathMethod
            {
                Id = idProvider.GetActionId(),
                ParentId = getByIdObjectPathMethod.Id,
                Name = "Approve",
                Parameters = new MethodParameter {Properties = new List<Parameter>()}
            };

            ActionObjectPath actionPathApprove = new()
            {
                Action = new BaseAction
                {
                    ObjectPathId = approveObjectPathMethod.Id.ToString(), Id = idProvider.GetActionId()
                },
                ObjectPath = approveObjectPathMethod
            };

            result.Add(actionPathApprove);

            #endregion

            #region Query

            ActionObjectPath actionPathQueryAllProperties = new()
            {
                Action = new QueryAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = approveObjectPathMethod.Id.ToString(),
                    SelectQuery = new SelectQuery
                    {
                        SelectAllProperties = true, Properties = new List<Property>()
                    }
                }
            };

            result.Add(actionPathQueryAllProperties);

            IdentityPath = actionPathQueryAllProperties.Action.Id;

            #endregion

            return result;
        }
    }
}
