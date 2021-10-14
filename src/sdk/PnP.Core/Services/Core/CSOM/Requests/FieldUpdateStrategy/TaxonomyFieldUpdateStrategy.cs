using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Requests.FieldUpdateStrategy
{
    internal class TaxonomyFieldUpdateStrategy : IFieldUpdateStrategy
    {
        internal IIdProvider IdProvider { get; private set; }

        internal Identity ListIdentity { get; set; }

        internal Property FieldsProperty { get; set; }

        internal TaxonomyFieldUpdateStrategy(IIdProvider idProvider, Identity listIdentity, Property fieldsProperty)
        {
            IdProvider = idProvider;
            ListIdentity = listIdentity;
            FieldsProperty = fieldsProperty;
        }

        List<ActionObjectPath> IFieldUpdateStrategy.GetFieldUpdateAction(CSOMItemField fld, Identity identity)
        {
            //Get field
            ObjectPathMethod getFieldByInternalNameMethod = new ObjectPathMethod
            {
                Id = IdProvider.GetActionId(),
                ParentId = FieldsProperty.Id,
                Name = "GetByInternalNameOrTitle",
                Parameters = new MethodParameter()

                {
                    Properties = new List<Parameter>()
                    {
                        new Parameter()
                        {
                            Type = "String",
                            Value = fld.FieldName
                        }
                    }
                }
            };

            MethodAction setFieldValueByValue = new MethodAction
            {
                ObjectPathId = getFieldByInternalNameMethod.Id.ToString(),
                Id = IdProvider.GetActionId(),
                Name = "SetFieldValueByValue",
                Parameters = fld.GetRequestParameters(identity.Id)
            };

            return new List<ActionObjectPath>(){
                new ActionObjectPath()
                {
                    Action = setFieldValueByValue,
                    ObjectPath = getFieldByInternalNameMethod
                }
            };
        }
    }
}
