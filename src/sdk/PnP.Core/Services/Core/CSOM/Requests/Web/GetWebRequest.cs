using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Requests.Web
{
    internal class GetWebRequest : IRequest<Model.SharePoint.IWeb>
    {
        public Model.SharePoint.IWeb Result { get; protected set; }
        internal int WebPropertyPath { get; set; }
        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            int identityCurrentPath = idProvider.GetActionId();
            int identityQueryId = idProvider.GetActionId();
            int webIdentityId = idProvider.GetActionId();
            int webActionId = idProvider.GetActionId();
            WebPropertyPath = idProvider.GetActionId();
            ActionObjectPath actionObjectPath = new ActionObjectPath()
            {
                Action = new IdentityQueryAction()
                {
                    Id = identityQueryId,
                    ObjectPathId = identityCurrentPath.ToString()
                },
                ObjectPath = new StaticProperty()
                {
                    TypeId = "{3747adcd-a3c3-41b9-bfab-4a64dd2f1e0a}",
                    Name = "Current",
                    Id = identityCurrentPath
                }
            };
            ActionObjectPath actionObjectPath1 = new ActionObjectPath()
            {
                Action = new IdentityQueryAction()
                {
                    Id = webActionId,
                    ObjectPathId = webIdentityId.ToString()
                }
            };
            ActionObjectPath queryObjectActionPath = new ActionObjectPath()
            {
                Action = new QueryAction.QueryAction()
                {
                    Id = WebPropertyPath,
                    ObjectPathId = webIdentityId.ToString(),
                    SelectQuery = new SelectQuery()
                    {
                        SelectAllProperties = true
                    }
                },
                ObjectPath = new Property()
                {
                    Name = "Web",
                    Id = webIdentityId,
                    ParentId = identityCurrentPath
                }
            };
            return new List<ActionObjectPath>()
            {
                actionObjectPath,
                actionObjectPath1,
                queryObjectActionPath
            };
        }

        public void ProcessResponse(string response)
        {
            Result = ResponseHelper.ProcessResponse<Model.SharePoint.IWeb>(response, WebPropertyPath);
        }
    }
}
