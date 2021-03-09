using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests.FieldUpdateStrategy;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Services.Core.CSOM.Requests.ListItems
{
    internal class UpdateListItemRequest : IRequest<ListItem>
    {
        public ListItem Result { get; private set; }

        internal string ListId { get; set; }

        internal string SiteId { get; set; }

        internal string WebId { get; set; }

        internal int ItemId { get; set; }

        internal List<CSOMItemField> FieldsToUpdate { get; } = new List<CSOMItemField>();

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
            IdentityPath = idProvider.GetActionId();

            Identity listIdentity = GetListIdentity(idProvider);
            Property fieldsProperty = GetFieldsProperty(idProvider, listIdentity);

            Dictionary<string, IFieldUpdateStrategy> fieldUpdateStrategies = ConstructCustomUpdateStrategies(idProvider, listIdentity, fieldsProperty);
            IFieldUpdateStrategy defaultStrategy = new SetItemFieldUpdateStrategy(idProvider);

            Identity listItemIdentity = new Identity()
            {
                Id = IdentityPath,
                Name = $"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{SiteId}:web:{WebId}:list:{ListId}:item:{ItemId},1"
            };

            List<ActionObjectPath> result = new List<ActionObjectPath>();
            foreach (CSOMItemField fld in FieldsToUpdate)
            {
                if(fld.FieldType == null)
                {
                    result.AddRange(defaultStrategy.GetFieldUpdateAction(fld, listItemIdentity));
                }
                else if (fieldUpdateStrategies.ContainsKey(fld.FieldType))
                {
                    result.AddRange(fieldUpdateStrategies[fld.FieldType].GetFieldUpdateAction(fld, listItemIdentity));
                }
                else
                {
                    result.AddRange(defaultStrategy.GetFieldUpdateAction(fld, listItemIdentity));
                }
            }

            if(FieldsToUpdate.Any(fld=>fld.FieldType == "TaxonomyFieldType" || fld.FieldType == "TaxonomyFieldTypeMulti"))
            {
                result.Add(new ActionObjectPath()
                {
                    ObjectPath = listIdentity
                });

                result.Add(new ActionObjectPath()
                {
                    ObjectPath = fieldsProperty
                });
            }

            result.Add(new ActionObjectPath()
            {
                Action = new MethodAction()
                {
                    Id = idProvider.GetActionId(),
                    Name = UpdateMethodName,
                    ObjectPathId = listItemIdentity.Id.ToString()
                },
                ObjectPath = listItemIdentity
            });

            return result;
        }

        private static Dictionary<string, IFieldUpdateStrategy> ConstructCustomUpdateStrategies(IIdProvider idProvider, Identity listIdentity, Property fieldsProperty)
        {
            Dictionary<string, IFieldUpdateStrategy> fieldUpdateStrategies = new Dictionary<string, IFieldUpdateStrategy>
            {
                { "FieldTaxonomyValue", new TaxonomyFieldUpdateStrategy(idProvider, listIdentity, fieldsProperty) },
                { "TaxonomyFieldTypeMulti", new TaxonomyMultiFieldUpdateStrategy(idProvider, listIdentity, fieldsProperty) }
            };
            return fieldUpdateStrategies;
        }

        private static Property GetFieldsProperty(IIdProvider idProvider, Identity listIdentity)
        {

            //Get fields
            Property fieldsProperty = new Property
            {
                Id = idProvider.GetActionId(),
                Name = "Fields",
                ParentId = listIdentity.Id
            };
            return fieldsProperty;
        }

        private Identity GetListIdentity(IIdProvider idProvider)
        {
            Identity listIdentity = new Identity
            {
                Id = idProvider.GetActionId(),
                Name = $"121a659f-e03e-2000-4281-1212829d67dd|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{SiteId}:web:{WebId}:list:{ListId}"
            };
            return listIdentity;
        }

        public void ProcessResponse(string response)
        {
            ListItem resultItem = ResponseHelper.ProcessResponse<ListItem>(response, IdentityPath);
            Result = resultItem;
        }
    }
}
