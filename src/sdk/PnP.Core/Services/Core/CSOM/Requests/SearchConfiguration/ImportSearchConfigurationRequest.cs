using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Requests.SearchConfiguration
{
    internal sealed class ImportSearchConfigurationRequest : IRequest<object>
    {
        internal SearchObjectLevel SearchObjectLevel { get; private set; }

        internal string Configuration { get; private set; }

        //internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        internal int ImportSearchConfigurationId { get; set; }

        internal ImportSearchConfigurationRequest(SearchObjectLevel searchObjectLevel, string configuration)
        {
            SearchObjectLevel = searchObjectLevel;
            Configuration = configuration;
        }

        public object Result { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            List<ActionObjectPath> result = new List<ActionObjectPath>();

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
                    ObjectPathId = constructor1Id.ToString()
                },
            };
            result.Add(path);

            ActionObjectPath path2 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = constructor2Id.ToString()
                },
            };
            result.Add(path2);

            ImportSearchConfigurationId = idProvider.GetActionId();
            ActionObjectPath path5 = new ActionObjectPath()
            {
                Action = new MethodAction()
                {
                    Name = "ImportSearchConfiguration",
                    Id = ImportSearchConfigurationId,
                    ObjectPathId = constructor1Id.ToString(),
                    Parameters = new List<Parameter>() {
                        new ObjectReferenceParameter()
                        {
                            ObjectPathId = constructor2Id
                        },
                        new Parameter()
                        {
                            Type = "String",
                            Value = Configuration
                        }
                    }
                }
            };
            result.Add(path5);

            ActionObjectPath path6 = new ActionObjectPath()
            {                
                Action = new QueryAction.QueryAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = constructor1Id.ToString(),
                    SelectQuery = new SelectQuery()
                    {
                        SelectAllProperties = true
                    }
                }
            };
            result.Add(path6);
            
            result.Add(path8);
            result.Add(path9);

            return result;
        }

        public void ProcessResponse(string response)
        {
            //string resultItem = ResponseHelper.ProcessResponse<string>(response, ImportSearchConfigurationId);
            //Result = resultItem;
        }
    }
}
