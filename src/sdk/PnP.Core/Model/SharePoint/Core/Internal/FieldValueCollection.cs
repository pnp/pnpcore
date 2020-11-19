using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PnP.Core.Model.SharePoint
{
    internal class FieldValueCollection : IFieldValueCollection
    {
        internal FieldValueCollection(IField field, string propertyName, TransientDictionary parent)
        {
            TypeAsString = field != null ? field.TypeAsString : "";
            Field = field;
            PropertyName = propertyName;
            Parent = parent;

            if (Parent != null && !string.IsNullOrEmpty(PropertyName))
            {
                Parent.MarkAsChanged(PropertyName);
            }
        }

        internal TransientDictionary Parent { get; set; }

        internal string PropertyName { get; set; }

        internal string TypeAsString { get; set; }

        internal IField Field { get; set; }

        public List<IFieldValue> Values { get; } = new List<IFieldValue>();

        internal object LookupMultiToJson()
        {
            List<int> ids = new List<int>();
            foreach (var item in Values)
            {
                ids.Add((item as FieldLookupValue).LookupId);
            }

            var updateMessage = new
            {
                results = ids.ToArray()
            };

            return updateMessage;
        }

        public void RemoveLookupValue(int lookupId)
        {
            foreach (var valueToRemove in Values.Cast<IFieldLookupValue>().Where(p => p.LookupId == lookupId).ToList())
            {
                Values.Remove(valueToRemove);
            }
        }

        internal string TaxonomyFieldTypeMultiUpdateString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in Values)
            {
                sb.Append($"{(item as FieldTaxonomyValue).WssId};#{(item as FieldTaxonomyValue).Label}|{(item as FieldTaxonomyValue).TermId};#");
            }

            return sb.ToString().TrimEnd('#');
        }

        internal IField TaxonomyFieldTypeMultiFieldToUpdate()
        {
            if (Field != null)
            {
                var fieldCollection = Field.Parent as FieldCollection;
                // Find the corresponding Note field (mm fieldname + _0) by title search
                var noteField = fieldCollection.FirstOrDefault(p => p.Title == $"{Field.Title}_0");
                if (noteField != null)
                {
                    return noteField;
                }
                else
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_ListItemUpdate_NoTaxonomyNoteField);
                }
            }
            else
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_ListItemUpdate_NoFieldsLoaded);
            }
        }

        public void RemoveTaxonomyFieldValue(Guid termId)
        {            
            foreach(var valueToRemove in Values.Cast<IFieldTaxonomyValue>().Where(p => p.TermId == termId).ToList())
            {
                Values.Remove(valueToRemove);
            }
        }

        public static object StringArrayToJson(List<string> choices)
        {
            var updateMessage = new
            {
                __metadata = new { type = "Collection(Edm.String)" },
                results = choices.ToArray()
            };

            return updateMessage;
        }

        public static object IntArrayToJson(List<int> choices)
        {
            var updateMessage = new
            {
                __metadata = new { type = "Collection(Edm.Int32)" },
                results = choices.ToArray()
            };

            return updateMessage;
        }

    }
}
