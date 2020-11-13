using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    internal class FieldValueCollection : IFieldValueCollection
    {
        internal FieldValueCollection(string typeAsString, string propertyName, TransientDictionary parent)
        {
            TypeAsString = typeAsString;
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

        internal object TaxonomyFieldTypeMultiToJson()
        {
            List<object> terms = new List<object>();

            foreach (var item in Values)
            {
                terms.Add(new { Label = (item as FieldTaxonomyValue).Label,
                                TermGuid = (item as FieldTaxonomyValue).TermId,
                                WssId = (item as FieldTaxonomyValue).WssId });
            }

            var updateMessage = new
            {
                __metadata = new { type = "Collection(SP.Taxonomy.TaxonomyFieldValue)" },
                results = terms.ToArray()
            };

            return updateMessage;
        }

        public void RemoveTaxonomyFieldValue(Guid termId)
        {
            foreach(var valueToRemove in Values.Cast<IFieldTaxonomyValue>().Where(p => p.TermId == termId).ToList())
            {
                Values.Remove(valueToRemove);
            }
        }

        public static object ChoiceMultiToJson(List<string> choices)
        {
            var updateMessage = new
            {
                __metadata = new { type = "Collection(Edm.String)" },
                results = choices.ToArray()
            };

            return updateMessage;
        }

    }
}
