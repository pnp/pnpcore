using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal
{
    internal sealed class ApprovePermissionRequest : IRequest<bool>
    {
        public bool Result => throw new System.NotImplementedException();

        public void ProcessResponse(string response)
        {
            throw new System.NotImplementedException();
        }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            List<ActionObjectPath> result = new List<ActionObjectPath>();

            #region PermissionRequestsConstructorPath

            ConstructorPath permissionRequestsConstructorPath = new ConstructorPath
            {
                Id = idProvider.GetActionId(),
                TypeId = "{104e8f06-1e00-4675-99c6-1b9b504ed8d8}",
                Parameters = new MethodParameter { Properties = new List<Parameter>() }
            };

            ActionObjectPath permissionRequestsConstructorPathActionPath = new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = permissionRequestsConstructorPath.Id.ToString()
                },
                ObjectPath = permissionRequestsConstructorPath
            };

            #endregion

            return result;
        }
    }
}