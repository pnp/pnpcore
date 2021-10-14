using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant
{
    internal class SpoOperationRequest : IRequest<object>
    {

        internal SpoOperationRequest(string objectIdentity)
        {
            ObjectIdentity = objectIdentity;
        }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        public string ObjectIdentity { get; set; }

        public object Result { get; set; }

        internal int IdentityPath { get; private set; }

        internal int QueryIdPath { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            IdentityPath = idProvider.GetActionId();
            QueryIdPath = idProvider.GetActionId();

            List<ActionObjectPath> actions = new List<ActionObjectPath>();
            ActionObjectPath spoOperation = new ActionObjectPath()
            {
                Action = new QueryAction()
                {
                    Id = QueryIdPath,
                    ObjectPathId = IdentityPath.ToString(),
                    SelectQuery = new SelectQuery()
                    {
                        SelectAllProperties = false,
                        Properties = new List<Property>()
                        {
                            new Property()
                            {
                                Name = "PollingInterval"
                            },
                            new Property()
                            {
                                Name = "IsComplete"
                            },
                        }
                    }
                }
            };

            ActionObjectPath identity = new ActionObjectPath()
            {
                ObjectPath = new Identity()
                {
                    Id = IdentityPath,
                    Name = ObjectIdentity.Replace("\n", "&#xA;")
                }
            };

            actions.Add(spoOperation);
            actions.Add(identity);
            return actions;
        }

        public void ProcessResponse(string response)
        {
            SpoOperation resultItem = ResponseHelper.ProcessResponse<SpoOperation>(response, QueryIdPath);
            Result = resultItem;
        }
    }
}
