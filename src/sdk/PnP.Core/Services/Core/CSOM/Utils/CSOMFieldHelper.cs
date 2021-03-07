using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.Utils
{
    internal class CSOMFieldHelper
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
                            result.Add(new CSOMItemField()
                            {
                                FieldValue = changedProp.Value,
                                FieldName = changedProp.Key
                            });
                        }
                    }
                    else
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
            return result;
        }
    }
}
