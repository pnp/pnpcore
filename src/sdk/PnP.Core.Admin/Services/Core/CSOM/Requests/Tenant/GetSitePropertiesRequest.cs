using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant
{
    internal class GetSitePropertiesRequest : IRequest<object>
    {
        internal GetSitePropertiesRequest(Uri siteUrl, bool detailed)
        {
            SiteUrl = siteUrl;
            Detailed = detailed;
        }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        public object Result { get; set; }

        internal Uri SiteUrl { get; set; }

        internal bool Detailed { get; set; }

        internal int IdentityPath { get; private set; }

        internal int QueryIdPath { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            IdentityPath = idProvider.GetActionId();
            QueryIdPath = idProvider.GetActionId();
            int getSitePropertiesByUrl = idProvider.GetActionId();

            List<ActionObjectPath> actions = new List<ActionObjectPath>();

            ActionObjectPath spoOperation = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = IdentityPath.ToString()
                },                
            };

            ActionObjectPath spoOperation2 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = getSitePropertiesByUrl.ToString()
                },
            };

            ActionObjectPath spoOperation3 = new ActionObjectPath()
            {
                Action = new QueryAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = getSitePropertiesByUrl.ToString(),
                    SelectQuery = new SelectQuery()
                    {
                        SelectAllProperties = true,
                    }
                },
            };

            ActionObjectPath createSiteAction = new ActionObjectPath()
            {
                ObjectPath = new ObjectPathMethod()
                {
                    Id = getSitePropertiesByUrl,
                    ParentId = IdentityPath,
                    Name = "GetSitePropertiesByUrl",
                    Parameters = new MethodParameter()
                    {
                        Properties = new List<Parameter>()
                        {
                            new Parameter()
                            {
                                Type = "String",
                                Value = SiteUrl.AbsoluteUri
                            },
                            new Parameter()
                            {
                                Type = "Boolean",
                                Value = Detailed
                            }
                        }
                    }
                }
            };

            ActionObjectPath spoGetSitePropertiesByUrlCollection = new ActionObjectPath()
            {
                ObjectPath = new ConstructorPath
                {
                    Id = IdentityPath,
                    TypeId = PnPAdminConstants.CsomTenantObject
                }
            };

            actions.Add(spoOperation);
            actions.Add(spoOperation2);
            actions.Add(spoOperation3);
            actions.Add(createSiteAction);
            actions.Add(spoGetSitePropertiesByUrlCollection);

            return actions;
        }

        public void ProcessResponse(string response)
        {
            //throw new NotImplementedException();
        }
    }
}
