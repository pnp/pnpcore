using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Requests.Web
{
    internal class UpdatePropertyBagRequest : IRequest<object>
    {
        public object Result { get; set; }

        internal string ObjectId { get; set; } = "";

        internal string SiteId { get; set; }

        internal string WebId { get; set; }

        internal string PropertyName { get; set; } = "AllProperties";

        internal List<CSOMItemField> FieldsToUpdate { get; } = new List<CSOMItemField>();

        internal int IdentityPath { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            int updateActionId = idProvider.GetActionId();
            IdentityPath = idProvider.GetActionId();
            int propertiesId = idProvider.GetActionId();

            List<Parameter> parameters = new List<Parameter>();
            foreach (CSOMItemField field in FieldsToUpdate)
            {
                parameters.AddRange(field.GetRequestParameters());
            }

            return new List<ActionObjectPath>()
            {
                new ActionObjectPath()
                {
                    Action = new MethodAction()
                    {
                        Id = updateActionId,
                        Name = "SetFieldValue",
                        ObjectPathId = propertiesId.ToString(),
                        Parameters = parameters
                    },
                    ObjectPath = new Property()
                    {
                        Id = propertiesId,
                        ParentId = IdentityPath,
                        Name = PropertyName
                    }
                },
                new ActionObjectPath()
                {
                    Action = new MethodAction()
                    {
                        Id = updateActionId,
                        ObjectPathId = IdentityPath.ToString(),
                        Name = "Update",
                        Parameters = new List<Parameter>()
                    },
                    ObjectPath = new Identity()
                    {
                        Id = IdentityPath,
                        Name = $"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{SiteId}:web:{WebId}{ObjectId}"
                    }
                }
            };
        }

        public void ProcessResponse(string response)
        {

        }
    }
}
