using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Requests.SearchConfiguration
{
    internal sealed class ExportSearchConfigurationRequest : IRequest<object>
    {
        internal SearchObjectLevel SearchObjectLevel { get; private set; }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        internal int ExportSearchConfigurationId { get; set; }

        internal ExportSearchConfigurationRequest(SearchObjectLevel searchObjectLevel)
        {
            SearchObjectLevel = searchObjectLevel;
        }

        public object Result { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            List<ActionObjectPath> result = new List<ActionObjectPath>();

            var sitePropertyId = idProvider.GetActionId();
            ActionObjectPath path6 = new ActionObjectPath()
            {
                ObjectPath = new StaticProperty()
                {
                    Name = "Current",
                    TypeId = "{3747adcd-a3c3-41b9-bfab-4a64dd2f1e0a}",
                    Id = sitePropertyId
                }
            };

            var webId = idProvider.GetActionId();

            ActionObjectPath path7 = new ActionObjectPath()
            {
                ObjectPath = new Property()
                {
                    Id = webId,
                    ParentId = path6.ObjectPath.Id,
                    Name = "Web"
                }
            };

            var constructor1Id = idProvider.GetActionId();
            ActionObjectPath path8 = new ActionObjectPath()
            {
                ObjectPath = new ConstructorPath()
                {
                    Id = constructor1Id,
                    TypeId = "{f44b2c90-ddc4-49c8-8d4d-4fb56dcc3247}"
                }
            };

            var constructor2Id = idProvider.GetActionId();
            ActionObjectPath path9 = new ActionObjectPath()
            {
                ObjectPath = new ConstructorPath()
                {
                    Id = constructor2Id,
                    TypeId = "{e6834c69-54c1-4bfc-9805-4b88406c28bb}",
                    Parameters = new MethodParameter()
                    {
                        Properties = new List<Parameter>() {
                        new Parameter()
                        {
                            Type = "Enum",
                            Value = (int)SearchObjectLevel
                        }
                    }
                    }
                }
            };

            ActionObjectPath path = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = sitePropertyId.ToString()
                },
            };
            result.Add(path);

            ActionObjectPath path2 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = webId.ToString()
                },
            };
            result.Add(path2);

            ActionObjectPath path3 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = constructor1Id.ToString()
                },
            };
            result.Add(path3);

            ActionObjectPath path4 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = constructor2Id.ToString()
                },
            };
            result.Add(path4);

            ExportSearchConfigurationId = idProvider.GetActionId();
            ActionObjectPath path5 = new ActionObjectPath()
            {
                Action = new MethodAction()
                {
                    Name = "ExportSearchConfiguration",
                    Id = ExportSearchConfigurationId,
                    ObjectPathId = constructor1Id.ToString(),
                    Parameters = new List<Parameter>() {
                        new ObjectReferenceParameter()
                        {
                            ObjectPathId = constructor2Id
                        }
                    }
                }
            };
            result.Add(path5);

            result.Add(path6);
            result.Add(path7);
            result.Add(path8);
            result.Add(path9);

            return result;
        }

        public void ProcessResponse(string response)
        {
            string resultItem = ResponseHelper.ProcessResponse<string>(response, ExportSearchConfigurationId);
            Result = resultItem;
        }
    }
}
