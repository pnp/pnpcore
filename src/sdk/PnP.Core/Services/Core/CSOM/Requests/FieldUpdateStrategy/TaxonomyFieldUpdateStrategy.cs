using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System;
using System.Collections.Generic;
using System.Text;

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
            ObjectPathMethod getFieldByInternalNameMethod = new ObjectPathMethod();
            getFieldByInternalNameMethod.Id = IdProvider.GetActionId();
            getFieldByInternalNameMethod.ParentId = FieldsProperty.Id;
            getFieldByInternalNameMethod.Name = "GetByInternalNameOrTitle";
            getFieldByInternalNameMethod.Parameters = new MethodParameter()

            {
                Properties = new List<Parameter>()
                    {
                        new Parameter()
                        {
                            Type = "String",
                            Value = fld.FieldName
                        }
                    }
            };

            MethodAction setFieldValueByValue = new MethodAction();
            setFieldValueByValue.ObjectPathId = getFieldByInternalNameMethod.Id.ToString();
            setFieldValueByValue.Id = IdProvider.GetActionId();
            setFieldValueByValue.Name = "SetFieldValueByValue";
            setFieldValueByValue.Parameters = fld.GetRequestParameters(identity.Id);
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
