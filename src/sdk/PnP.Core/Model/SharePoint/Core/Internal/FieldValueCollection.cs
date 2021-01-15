using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal class FieldValueCollection : IFieldValueCollection
    {
        private bool hasChangedDueToDeleteOrAdd;

        internal FieldValueCollection(IField field, string propertyName)
        {
            TypeAsString = field != null ? field.TypeAsString : "";
            Field = field;
            PropertyName = propertyName;

            // Track events on the observable collection
            Values.CollectionChanged += Values_CollectionChanged;
        }

        internal string PropertyName { get; set; }

        internal string TypeAsString { get; set; }

        internal IField Field { get; set; }

        public ObservableCollection<IFieldValue> Values { get; } = new ObservableCollection<IFieldValue>();

        private void Values_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                hasChangedDueToDeleteOrAdd = true;

                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (var item in e.NewItems)
                    {
                        (item as FieldValue).MarkAsChanged();
                    }
                }
            }
        }

        internal List<IFieldValue> GetChangedValues()
        {
            // Check if there was a changed value or an added value (the changeflag for added item is set during add)
            // or a change introduced by a delete or a collection reset If so return the current values as these need to be written back
            if (hasChangedDueToDeleteOrAdd || Values.Any(p => (p as FieldValue).HasChanges))
            {
                return Values.ToList();
            }
            else
            {
                return null;
            }
        }

        internal void Commit()
        {
            hasChangedDueToDeleteOrAdd = false;
            foreach(var fieldValue in Values)
            {
                (fieldValue as FieldValue).Commit();
            }
        }

        internal object UserMultiToValidateUpdateItemJson()
        {
            var users = new List<object>();
            foreach (var item in Values)
            {
                if (item is FieldUserValue fieldUserValue)
                {
                    if (fieldUserValue.Principal == null)
                    {
                        throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_MissingSharePointPrincipal);
                    }

                    users.Add(new { Key = fieldUserValue.Principal.LoginName });
                }
            }

            return JsonSerializer.Serialize(users.ToArray());
        }

        internal object TaxonomyMultiToValidateUpdateItemJson()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in Values)
            {
                sb.Append($"{(item as FieldTaxonomyValue).ToValidateUpdateItemJson()}");
            }

            return sb.ToString();
        }

        internal object LookupMultiToValidateUpdateItemJson()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in Values)
            {
                sb.Append($"{(item as FieldLookupValue).ToValidateUpdateItemJson()};#");
            }

            return sb.ToString();
        }


        public void RemoveLookupValue(int lookupId)
        {
            foreach (var valueToRemove in Values.Cast<IFieldLookupValue>().Where(p => p.LookupId == lookupId).ToList())
            {
                Values.Remove(valueToRemove);
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
