using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;

namespace PnP.Core.Test.Services.Core.CSOM.Requests
{
    internal class GetTitleErrorRequest : IRequest<object>
    {
        public GetTitleErrorRequest()
        {
        }

        public object Result { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            StaticProperty siteProperty = new StaticProperty()
            {
                Name = "Current",
                TypeId = "",
                Id = idProvider.GetActionId()
            };

            Property web = new Property()
            {
                Id = idProvider.GetActionId(),
                ParentId = siteProperty.Id,
                Name = "Web"
            };

            List<ActionObjectPath> result = new List<ActionObjectPath>();

            ActionObjectPath path = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = siteProperty.Id.ToString()
                },
            };
            result.Add(path);

            ActionObjectPath path2 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = web.Id.ToString()
                },
            };
            result.Add(path2);

            ActionObjectPath identityQuery = new ActionObjectPath()
            {
                Action = new QueryAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = web.Id.ToString(),
                    SelectQuery = new SelectQuery()
                    {
                        SelectAllProperties = false,
                        Properties = new List<Property>()
                        {
                            new Property()
                            {
                                Name = "Bla"
                            }
                        }
                    }
                },
            };
            result.Add(identityQuery);


            ActionObjectPath webIdentity = new ActionObjectPath()
            {
                ObjectPath = web
            };
            result.Add(webIdentity);

            ActionObjectPath siteIdentity = new ActionObjectPath()
            {
                ObjectPath = siteProperty
            };
            result.Add(siteIdentity);

            return result;
        }

        public void ProcessResponse(string response)
        {
        }
    }
}
