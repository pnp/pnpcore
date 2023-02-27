using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal
{
    /// <summary>
    /// Based upon the Microsoft.Online.SharePoint.TenantAdministration.Internal.SPOWebAppServicePrincipal CSOM Request
    /// </summary>
    internal sealed class DenyPermissionRequest : IRequest<object>
    {
        public string RequestId { get; set; }
        public object Result { get; } = new();

        public void ProcessResponse(string response)
        {
            // if BatchClient.ProcessCsomBatchResponse didn't throw an exception, everything is fine
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

            ActionObjectPath action = new()
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(), ObjectPathId = getByIdObjectPathMethod.Id.ToString()
                }
            };

            result.Add(action);

            #endregion

            #region Deny

            ActionObjectPath actionPathGetById = new()
            {
                Action = new MethodAction
                {
                    Name = "Deny",
                    ObjectPathId = getByIdObjectPathMethod.Id.ToString(),
                    Id = idProvider.GetActionId()
                },
                ObjectPath = getByIdObjectPathMethod
            };

            result.Add(actionPathGetById);

            #endregion

            return result;
        }
    }
}
