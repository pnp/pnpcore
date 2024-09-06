using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant
{
    internal sealed class ConnectSiteToHubSiteRequest : IRequest<object>
    {
        internal Uri HubSiteCollection { get; private set; }
        internal Uri HubMemberSiteCollection { get; private set; }

        internal ConnectSiteToHubSiteRequest(Uri hubSite, Uri siteToConnect)
        {
            HubSiteCollection = hubSite;
            HubMemberSiteCollection = siteToConnect;
        }

        public object Result { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            List<ActionObjectPath> result = new List<ActionObjectPath>();

            // Object Paths
            var constructor1Id = idProvider.GetActionId();
            ActionObjectPath path1 = new()
            {
                ObjectPath = new ConstructorPath()
                {
                    Id = constructor1Id,
                    TypeId = "{268004ae-ef6b-4e9b-8425-127220d84719}"
                }
            };
            result.Add(path1);

            ActionObjectPath path2 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = constructor1Id.ToString(),
                },
            };
            result.Add(path2);

            ActionObjectPath path3 = new ActionObjectPath()
            {
                Action = new MethodAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId= constructor1Id.ToString(),
                    Name = "ConnectSiteToHubSite",
                    Parameters = new()
                    {
                        new Parameter()
                        {
                            Type = "String",
                            Value = HubMemberSiteCollection.ToString()
                        },
                        new Parameter()
                        {
                            Type = "String",
                            Value = HubSiteCollection.ToString()
                        }
                    }
                }

            };            
            result.Add(path3);

            return result;
        }

        public void ProcessResponse(string response)
        {
        }
    }
}
