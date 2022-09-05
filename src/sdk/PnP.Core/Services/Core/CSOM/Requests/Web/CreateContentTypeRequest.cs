using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Requests.Web
{
    internal sealed class CreateContentTypeRequest : IRequest<object>
    {
        internal ContentTypeCreationInfo ContentTypeCreationInfo { get; set; }

        public CreateContentTypeRequest(ContentTypeCreationInfo creationInfo)
        {
            ContentTypeCreationInfo = creationInfo;
        }

        public object Result { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            StaticProperty siteProperty = new StaticProperty()
            {
                Name = "Current",
                TypeId = "{3747adcd-a3c3-41b9-bfab-4a64dd2f1e0a}",
                Id = idProvider.GetActionId()
            };

            Property web = new Property()
            {
                Id = idProvider.GetActionId(),
                ParentId = siteProperty.Id,
                Name = "Web"
            };

            Property contentTypes = new Property()
            {
                Id = idProvider.GetActionId(),
                Name = "ContentTypes",
                ParentId = web.Id
            };

            ObjectPathMethod addCtMethod = new ObjectPathMethod()
            {
                Id = idProvider.GetActionId(),
                ParentId = contentTypes.Id,
                Name = "Add",
                Parameters = new MethodParameter()
                {
                    TypeId = "{168f3091-4554-4f14-8866-b20d48e45b54}",
                    Properties = new List<Parameter>()
                    {
                        new ContentTypeCreationParameter()
                        {
                            Value = ContentTypeCreationInfo
                        }
                    }
                }
            };

            List<ActionObjectPath> result = new List<ActionObjectPath>();

            ActionObjectPath path = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = addCtMethod.Id.ToString()
                },
                ObjectPath = addCtMethod,
            };
            result.Add(path);

            ActionObjectPath identityQuery = new ActionObjectPath()
            {
                Action = new IdentityQueryAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = addCtMethod.Id.ToString()
                },
                ObjectPath = contentTypes
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
