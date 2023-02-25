using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.ServicePrincipal
{
    internal class SetServicePrincipalRequest : IRequest<IServicePrincipalProperties>
    {
        public IServicePrincipalProperties Result { get; private set; }
        public bool Enabled { get; set; }
        private CSOMResponseHelper ResponseHelper { get; set; } = new();
        private int IdentityPath { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            List<ActionObjectPath> result = new();

            #region ConstructorPath

            var constructorObjectPathId = idProvider.GetActionId();

            ConstructorPath permissionRequestsConstructorPath = new()
            {
                Id = constructorObjectPathId,
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

            #region AccountEnabledProprty

            ActionObjectPath accountEnabledActionObjectPath = new()
            {
                Action = new SetPropertyAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = constructorObjectPathId.ToString(),
                    Name = "AccountEnabled",
                    SetParameter = new Parameter {Type = "Boolean", Value = this.Enabled}
                }
            };

            result.Add(accountEnabledActionObjectPath);

            #endregion

            #region UpdateRegion

            ActionObjectPath updateActionObjectPath = new()
            {
                Action = new MethodAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = constructorObjectPathId.ToString(),
                    Name = "Update",
                }
            };

            result.Add(updateActionObjectPath);

            #endregion

            #region QueryAll

            ActionObjectPath actionPathQueryAllProperties = new()
            {
                Action = new QueryAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = constructorObjectPathId.ToString(),
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

        public void ProcessResponse(string response)
        {
            IServicePrincipalProperties resultItem = ResponseHelper.ProcessResponse<ServicePrincipalProperties>(response, IdentityPath);
            Result = resultItem;
        }
    }
}
