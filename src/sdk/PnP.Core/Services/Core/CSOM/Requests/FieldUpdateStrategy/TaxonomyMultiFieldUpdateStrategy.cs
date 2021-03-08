using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System;
using System.Collections.Generic;
using System.Text;

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
            MethodAction setFieldValueByValue = new MethodAction();
            setFieldValueByValue.ObjectPathId = getByInternalNameOrTitleId.ToString();
            setFieldValueByValue.Id = IdProvider.GetActionId();
            setFieldValueByValue.Name = "SetFieldValueByValueCollection";
            setFieldValueByValue.Parameters = new List<Parameter>()
                {
                    new ObjectReferenceParameter()
                    {
                        ObjectPathId=identity.Id
                    }
                };

            //Get field
            ObjectPathMethod getFieldByInternalNameMethod = new ObjectPathMethod();
            getFieldByInternalNameMethod.Id = getByInternalNameOrTitleId;
            getFieldByInternalNameMethod.ParentId = FieldsProperty.Id;
            getFieldByInternalNameMethod.Name = "GetByInternalNameOrTitle";
            getFieldByInternalNameMethod.Parameters = new MethodParameter()
            {
                Properties = new List<Parameter>() {
                        new Parameter()
                        {
                            Type = "String",
                            Value = fld.FieldName
                        }
                    }
            };

            //Create taxonomy collection
            ConstructorPath taxonomyCollectionConstructor = new ConstructorPath();
            taxonomyCollectionConstructor.Id = taxonomyParameterConstructorId;
            taxonomyCollectionConstructor.TypeId = "{c3dfae10-f3bf-4894-9012-bb60665b6d91}";
            taxonomyCollectionConstructor.Parameters = new MethodParameter()
            {
                Properties = fld.GetRequestParameters(identity.Id)
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
