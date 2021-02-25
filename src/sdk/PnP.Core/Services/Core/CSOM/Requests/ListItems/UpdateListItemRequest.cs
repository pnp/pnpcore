using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Requests.ListItems
{
    internal class UpdateListItemRequest : IRequest<ListItem>
    {
        public ListItem Result { get; private set; }
        internal string ListId { get; set; }
        internal string SiteId { get; set; }
        internal string WebId { get; set; }
        internal int ItemId { get; set; }
        internal List<CSOMItemField> FieldsToUpdate { get; set; } = new List<CSOMItemField>();
        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();
        internal int IdentityPath { get; private set; }
        internal virtual string UpdateMethodName { get; set; } = "Update";

        internal UpdateListItemRequest(string siteId, string webId, string listId, int itemId)
        {
            SiteId = siteId;
            WebId = webId;
            ListId = listId;
            ItemId = itemId;
        }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            int updateActionId = idProvider.GetActionId();
            IdentityPath = idProvider.GetActionId();
            List<Parameter> parameters = new List<Parameter>();
            foreach(CSOMItemField field in FieldsToUpdate)
            {
                parameters.Add(new Parameter()
                {
                    Type = "String",
                    Value = field.FieldName
                });
                parameters.Add(new Parameter()
                {
                    Type = field.FieldType,
                    Value = field.SerializeFieldValue()
                });
            }
            return new List<ActionObjectPath>()
            {
                new ActionObjectPath()
                {
                    Action = new MethodAction()
                    {
                        Id = updateActionId,
                        Name = "SetFieldValue",
                        ObjectPathId = IdentityPath.ToString(),
                        Parameters = parameters
                    },
                    ObjectPath = new Identity()
                    {
                        Id = IdentityPath,
                        Name = $"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{SiteId}:web:{WebId}:list:{ListId}:item:{ItemId},1"
                    }
                },
                new ActionObjectPath()
                {
                    Action = new MethodAction()
                    {
                        Id = updateActionId,
                        ObjectPathId = IdentityPath.ToString(),
                        Name = UpdateMethodName,
                        Parameters = new List<Parameter>()
                    }
                }
            };
        }

        public void ProcessResponse(string response)
        {
            ListItem resultItem = ResponseHelper.ProcessResponse<ListItem>(response, IdentityPath);
            Result = resultItem;
        }
    }
}
