using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PnP.Core.Services.Core.CSOM.Utils
{
    internal sealed class CSOMFieldHelper
    {
        public ListItem Component { get; private set; }

        public CSOMFieldHelper(ListItem component)
        {
            Component = component;
        }

        internal List<CSOMItemField> GetUpdatedFieldValues(PropertyDescriptorCollection entityProperties)
        {
            List<CSOMItemField> result = new List<CSOMItemField>();
            foreach (PropertyDescriptor cp in entityProperties)
            {
                var entity = EntityManager.GetClassInfo(Component.GetType(), Component);
                var changedField = entity.Fields.FirstOrDefault(f => f.Name == cp.Name);

                // If we found a field 
                if (changedField != null)
                {
                    if (changedField.DataType.FullName == typeof(TransientDictionary).FullName)
                    {
                        // Get the changed properties in the dictionary
                        var dictionaryObject = (TransientDictionary)cp.GetValue(Component);
                        foreach (KeyValuePair<string, object> changedProp in dictionaryObject.ChangedProperties)
                        {
                            // Only include FieldValue properties when they signal they've changed
                            if (changedProp.Value is FieldValue changedPropAsFieldValue && changedPropAsFieldValue.HasChanges)
                            {
                                if (changedProp.Value is FieldLookupValue && (changedProp.Value as FieldLookupValue).LookupId == -1)
                                {
                                    result.Add(new CSOMItemField()
                                    {
                                        FieldValue = null,
                                        FieldName = changedProp.Key,
                                    });
                                }
                                else
                                {
                                    result.Add(new CSOMItemField()
                                    {
                                        FieldValue = changedProp.Value,
                                        FieldName = changedProp.Key,
                                    });
                                }
                            }
                            else if (changedProp.Value == null)
                            {
                                result.Add(new CSOMItemField()
                                {
                                    FieldValue = null,
                                    FieldName = changedProp.Key,
                                });
                            }
                            else if (changedProp.Value is FieldValueCollection)
                            {
                                var collection = changedProp.Value as FieldValueCollection;

                                // Only persist these fields if there was a change detected in the FieldValueCollection
                                if (collection.HasChanges)
                                {
                                    string typeAsString = collection.TypeAsString;
                                    if (string.IsNullOrEmpty(typeAsString))
                                    {
                                        var firstElement = collection.Values.FirstOrDefault();
                                        if (firstElement is FieldLookupValue)
                                        {
                                            typeAsString = "LookupMulti";
                                        }
                                        else if (firstElement is FieldTaxonomyValue)
                                        {
                                            typeAsString = "TaxonomyFieldTypeMulti";
                                        }
                                    }

                                    if (typeAsString == "LookupMulti" || typeAsString == "Lookup" || typeAsString == "UserMulti" || typeAsString == "TaxonomyFieldTypeMulti")
                                    {
                                        result.Add(new CSOMItemField()
                                        {
                                            FieldValue = changedProp.Value,
                                            FieldName = changedProp.Key,
                                            FieldType = typeAsString
                                        });
                                    }
                                }
                            }
                            else
                            {
                                // Let's set its value into the update message
                                result.Add(new CSOMItemField()
                                {
                                    FieldValue = changedProp.Value,
                                    FieldName = changedProp.Key,
                                    FieldType = changedProp.Value?.GetType().Name
                                });
                            }
                        }
                    }
                    else
                    {
                        if (!(JsonMappingHelper.IsModelCollection(changedField.DataType) || JsonMappingHelper.IsModelType(changedField.DataType)))
                        {
                            // Let's set its value into the update message
                            result.Add(new CSOMItemField()
                            {
                                FieldName = changedField.SharePointName,
                                FieldType = changedField.DataType.Name,
                                FieldValue = Component.GetValue(changedField.Name)
                            });
                        }
                    }
                }
            }
            return result;
        }
    }
}
