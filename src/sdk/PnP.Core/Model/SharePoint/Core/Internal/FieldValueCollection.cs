using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of 'special' field values
    /// </summary>
    public sealed class FieldValueCollection : IFieldValueCollection
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

        /// <summary>
        /// Default constructor
        /// </summary>
        public FieldValueCollection()
        {
            TypeAsString = "";
            
            // Track events on the observable collection
            Values.CollectionChanged += Values_CollectionChanged;
        }

        /// <summary>
        /// Default constructor taking in a collection of <see cref="IFieldValue"/> objects
        /// </summary>
        /// <param name="fieldValues">Collection of <see cref="IFieldValue"/> objects</param>
        public FieldValueCollection(IEnumerable<IFieldValue> fieldValues) : this()
        {
            foreach (var value in fieldValues)
            {
                Values.Add(value);
            }
        }        

        internal string PropertyName { get; set; }

        internal string TypeAsString { get; set; }

        internal IField Field { get; set; }

        /// <summary>
        /// Values in the collection
        /// </summary>
        public ObservableCollection<IFieldValue> Values { get; } = new ObservableCollection<IFieldValue>();

        internal bool HasChanges => GetChangedValues() != null;

        private void Values_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e?.Action != null && (e.Action == NotifyCollectionChangedAction.Remove ||
                                      e.Action == NotifyCollectionChangedAction.Reset ||
                                      e.Action == NotifyCollectionChangedAction.Add))
            {
                hasChangedDueToDeleteOrAdd = true;

                if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (var item in e.NewItems.OfType<FieldValue>())
                    {
                        item.MarkAsChanged();
                    }
                }
            }
        }

        private List<IFieldValue> GetChangedValues()
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
            foreach (var fieldValue in Values)
            {
                if (fieldValue != null && fieldValue is FieldValue)
                {
                    (fieldValue as FieldValue).Commit();
                }
            }
        }

        internal object UserMultiToValidateUpdateItemJson()
        {
            var users = new List<object>();
            foreach (var item in Values)
            {
                if (item is FieldUserValue fieldUserValue)
                {
                    if (!fieldUserValue.HasValue("Principal") || fieldUserValue.Principal == null)
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
                sb.Append($"{(item as FieldLookupValue).ToValidateUpdateItemJson()};#;#");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Removes <see cref="IFieldLookupValue"/> from the collection if found
        /// </summary>
        /// <param name="lookupId">Id of the <see cref="IFieldLookupValue"/> to remove</param>
        public void RemoveLookupValue(int lookupId)
        {
            foreach (var valueToRemove in Values.Cast<IFieldLookupValue>().Where(p => p.LookupId == lookupId).ToList())
            {
                Values.Remove(valueToRemove);
            }
        }

        /// <summary>
        /// Removes <see cref="IFieldTaxonomyValue"/> from the collection if found
        /// </summary>
        /// <param name="termId">Id of the <see cref="IFieldTaxonomyValue"/> to remove</param>
        public void RemoveTaxonomyFieldValue(Guid termId)
        {
            foreach (var valueToRemove in Values.Cast<IFieldTaxonomyValue>().Where(p => p.TermId == termId).ToList())
            {
                Values.Remove(valueToRemove);
            }
        }

        internal static object StringArrayToJson(List<string> choices)
        {
            var updateMessage = new
            {
                __metadata = new { type = "Collection(Edm.String)" },
                results = choices.ToArray()
            };

            return updateMessage;
        }

        internal static object IntArrayToJson(List<int> choices)
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
