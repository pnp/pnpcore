using PnP.Core.Model.SharePoint;
using PnP.Core.Model.SharePoint.Core.Internal;
using PnP.Core.Services.Core.CSOM.QueryAction;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Services.Core.CSOM.Utils.Model
{
    internal sealed class CSOMItemField
    {
        private string fieldType;

        public string FieldName { get; set; }

        public string FieldType
        {
            get
            {
                if (string.IsNullOrEmpty(fieldType))
                {
                    fieldType = GetFieldType();
                }
                return fieldType;
            }
            set
            {
                fieldType = value;
            }
        }

        private string GetFieldType()
        {
            if (FieldValue is FieldValueCollection)
            {
                FieldValueCollection collection = FieldValue as FieldValueCollection;
                string typeAsString = collection.TypeAsString;
                if (string.IsNullOrEmpty(typeAsString))
                {
                    var firstElement = collection.Values.FirstOrDefault();
                    if (firstElement is FieldLookupValue)
                    {
                        return "LookupMulti";
                    }
                    else if (firstElement is FieldTaxonomyValue)
                    {
                        return "TaxonomyFieldTypeMulti";
                    }
                }
                return typeAsString;
            }
            return FieldValue?.GetType().Name;
        }

        public object FieldValue { get; set; }

        public List<Parameter> GetRequestParameters(int referenceObjectPathId = 0)
        {
            List<Parameter> parameters = new List<Parameter>();
            if (referenceObjectPathId == 0)
            {
                parameters.Add(new Parameter()
                {
                    Type = "String",
                    Value = FieldName
                });
            }
            else
            {
                parameters.Add(new ObjectReferenceParameter()
                {
                    ObjectPathId = referenceObjectPathId
                });
            }
            parameters.Add(GetFieldValueParameter());

            return parameters;
        }

        internal Parameter GetFieldValueParameter()
        {
            string fieldType = FieldType ?? (FieldValue?.GetType().Name);
            if (FieldValue is IFieldValueCollection)
            {
                IFieldValueCollection collection = FieldValue as IFieldValueCollection;

                if (collection.Values.FirstOrDefault() is FieldTaxonomyValue)
                {
                    return new TaxonomyMultiParameter(collection);
                }

                return new ArrayParameter(collection)
                {
                    Type = fieldType,
                    Name = FieldName,
                    Value = FieldValue
                };
            }
            else if (FieldValue is ICSOMField)
            {
                return new CSOMFieldParameter(FieldValue as ICSOMField)
                {
                    Type = fieldType,
                    Name = FieldName,
                    Value = FieldValue
                };
            }
            else
            {
                return new Parameter()
                {
                    Type = fieldType,
                    Name = FieldName,
                    Value = FieldValue
                };
            }
        }
    }
}
