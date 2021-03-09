using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Requests.FieldUpdateStrategy
{
    internal class TaxonomyMultiFieldUpdateStrategy : IFieldUpdateStrategy
    {
        internal IIdProvider IdProvider { get; private set; }

        internal Identity ListIdentity { get; set; }

        internal Property FieldsProperty { get; set; }

        internal TaxonomyMultiFieldUpdateStrategy(IIdProvider idProvider, Identity listIdentity, Property fieldsProperty)
        {
            IdProvider = idProvider;
            ListIdentity = listIdentity;
            FieldsProperty = fieldsProperty;
        }

        List<ActionObjectPath> IFieldUpdateStrategy.GetFieldUpdateAction(CSOMItemField fld, Identity identity)
        {
            int getByInternalNameOrTitleId = IdProvider.GetActionId();
            int taxonomyParameterConstructorId = IdProvider.GetActionId();
            //Set field value
            MethodAction setFieldValueByValue = new MethodAction
            {
                ObjectPathId = getByInternalNameOrTitleId.ToString(),
                Id = IdProvider.GetActionId(),
                Name = "SetFieldValueByValueCollection",
                Parameters = new List<Parameter>()
                {
                    new ObjectReferenceParameter()
                    {
                        ObjectPathId=identity.Id
                    },
                    new ObjectReferenceParameter()
                    {
                        ObjectPathId=taxonomyParameterConstructorId
                    }
                }
            };

            //Get field
            ObjectPathMethod getFieldByInternalNameMethod = new ObjectPathMethod
            {
                Id = getByInternalNameOrTitleId,
                ParentId = FieldsProperty.Id,
                Name = "GetByInternalNameOrTitle",
                Parameters = new MethodParameter()
                {
                    Properties = new List<Parameter>() {
                        new Parameter()
                        {
                            Type = "String",
                            Value = fld.FieldName
                        }
                    }
                }
            };

            //Create taxonomy collection
            List<Parameter> parameters = fld.GetRequestParameters(/*identity.Id*/ getByInternalNameOrTitleId);
            parameters.Reverse();

            ConstructorPath taxonomyCollectionConstructor = new ConstructorPath
            {
                Id = taxonomyParameterConstructorId,
                TypeId = "{c3dfae10-f3bf-4894-9012-bb60665b6d91}",
                Parameters = new MethodParameter()
                {
                    Properties = parameters
                }
            };

            ActionObjectPath setTaxonomyActionObject = new ActionObjectPath()
            {
                Action = setFieldValueByValue,
                ObjectPath = getFieldByInternalNameMethod
            };

            ActionObjectPath constructTaxonomyCollection = new ActionObjectPath()
            {
                ObjectPath = taxonomyCollectionConstructor
            };

            return new List<ActionObjectPath>()
            {
                setTaxonomyActionObject,
                constructTaxonomyCollection
            };
        }
    }
}
