using PnP.Core.QueryModel;
using PnP.Core.Services.Core.CSOM.Utils.DateHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Materializes SP.ListItem json objects returned by a CSOM List.GetItems(CamlQuery) call
    /// into the list's Items collection
    /// </summary>
    internal static class CsomListItemsHandler
    {
        private static readonly CSOMDateConverter dateConverter = new CSOMDateConverter();

        internal static async Task<List<IListItem>> ProcessItemsAsync(List list, List<JsonElement> items)
        {
            var processedItems = new List<IListItem>();

            // Mark collection as requested to avoid our linq integration to actually execute this as a query to SharePoint
            list.Items.Requested = true;

            if (items.Count == 0)
            {
                return processedItems;
            }

            // Load the fields if not yet loaded, needed to correctly type the field values
            await list.EnsurePropertiesAsync(List.LoadFieldsExpression).ConfigureAwait(false);

            Dictionary<string, IField> fieldLookupCache = new Dictionary<string, IField>();

            foreach (var item in items)
            {
                if (!item.TryGetProperty("Id", out JsonElement idElement) || idElement.ValueKind != JsonValueKind.Number)
                {
                    continue;
                }

                int listItemId = idElement.GetInt32();

                // Here we want to avoid the LINQ query and we want to rely on LINQ to Objects
                var itemToUpdate = list.Items.AsRequested().FirstOrDefault(p => p.Id == listItemId);
                if (itemToUpdate == null)
                {
                    itemToUpdate = (list.Items as ListItemCollection).CreateNewAndAdd();
                    itemToUpdate.Requested = true;
                }

                var listItem = itemToUpdate as ListItem;
                listItem.SetSystemProperty(p => p.Id, listItemId);

                // Ensure metadata handling when list items are read using this method
                await listItem.GraphToRestMetadataAsync().ConfigureAwait(false);

                var overflowDictionary = listItem.Values;

                foreach (var property in item.EnumerateObject())
                {
                    // Skip CSOM object properties and the expando field type suffix companion properties
                    if (property.Name == "_ObjectType_" || property.Name == "_ObjectIdentity_" || property.Name == "_ObjectVersion_" ||
                        property.Name == "Id" || property.Name.Contains("$"))
                    {
                        continue;
                    }

                    object fieldValue = ConvertFieldValue(property.Name, property.Value, list.Fields, fieldLookupCache);

                    if (!overflowDictionary.ContainsKey(property.Name))
                    {
                        overflowDictionary.SystemAdd(property.Name, fieldValue);
                    }
                    else
                    {
                        overflowDictionary.SystemUpdate(property.Name, fieldValue);
                    }
                }

                // Ensure the values are committed to the model so there are no pending changes
                listItem.Values.Commit();

                processedItems.Add(listItem);
            }

            return processedItems;
        }

        private static object ConvertFieldValue(string propertyName, JsonElement value, IFieldCollection fields, Dictionary<string, IField> fieldLookupCache)
        {
            // Doing the field lookup is expensive given it happens per field/row, caching improves performance when reading large sets of items.
            // Fields projected from a joined list (CAML ProjectedFields) are not part of the list schema, for those field stays null.
            if (!fieldLookupCache.TryGetValue(propertyName, out IField field))
            {
                field = fields.AsRequested().FirstOrDefault(p => p.InternalName == propertyName);
                fieldLookupCache.Add(propertyName, field);
            }

            switch (value.ValueKind)
            {
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return value.GetBoolean();
                case JsonValueKind.Number:
                    if (value.TryGetInt32(out int intValue))
                    {
                        return intValue;
                    }
                    return value.GetDouble();
                case JsonValueKind.String:
                    return ConvertStringFieldValue(value.GetString());
                case JsonValueKind.Object:
                    {
                        var typedValue = DetectSpecialFieldType(value, field);
                        if (typedValue != null)
                        {
                            typedValue.IsArray = false;
                            return typedValue.FromJson(value);
                        }

                        return value.ToObject<System.Dynamic.ExpandoObject>();
                    }
                case JsonValueKind.Array:
                    {
                        // Multi value fields (LookupMulti, UserMulti, TaxonomyFieldTypeMulti) come back as arrays
                        // of typed objects, multi choice fields as arrays of strings
                        FieldValueCollection fieldValueCollection = null;
                        List<string> stringValues = null;

                        foreach (var element in value.EnumerateArray())
                        {
                            if (element.ValueKind == JsonValueKind.Object)
                            {
                                var typedValue = DetectSpecialFieldType(element, field);
                                if (typedValue != null)
                                {
                                    fieldValueCollection ??= new FieldValueCollection(field, propertyName);
                                    typedValue.IsArray = true;
                                    typedValue.FromJson(element);

                                    if (typedValue is FieldLookupValue lookupValue)
                                    {
                                        // Only add to the collection when it points to a real value
                                        if (lookupValue.LookupId > -1)
                                        {
                                            fieldValueCollection.Values.Add(lookupValue);
                                        }
                                    }
                                    else
                                    {
                                        fieldValueCollection.Values.Add(typedValue);
                                    }
                                }
                            }
                            else
                            {
                                stringValues ??= new List<string>();
                                stringValues.Add(element.ValueKind == JsonValueKind.String ? element.GetString() : element.GetRawText());
                            }
                        }

                        if (fieldValueCollection == null && stringValues == null && field != null &&
                            (field.TypeAsString == "LookupMulti" || field.TypeAsString == "UserMulti" || field.TypeAsString == "TaxonomyFieldTypeMulti"))
                        {
                            return new FieldValueCollection(field, propertyName);
                        }

                        return (object)fieldValueCollection ?? stringValues ?? new List<string>();
                    }
                default:
                    return null;
            }
        }

        private static FieldValue DetectSpecialFieldType(JsonElement value, IField field)
        {
            // The CSOM json shapes match what the REST FromJson parsers expect, dispatch on the CSOM object type
            // so that fields projected from joined lists (no matching field in the list schema) are typed as well
            string objectType = null;
            if (value.TryGetProperty("_ObjectType_", out JsonElement objectTypeElement) && objectTypeElement.ValueKind == JsonValueKind.String)
            {
                objectType = objectTypeElement.GetString();
            }

            switch (objectType)
            {
                case "SP.FieldLookupValue": return new FieldLookupValue { Field = field };
                case "SP.FieldUserValue": return new FieldUserValue { Field = field };
                case "SP.FieldUrlValue": return new FieldUrlValue { Field = field };
                case "SP.Taxonomy.TaxonomyFieldValue": return new FieldTaxonomyValue { Field = field };
                default: return null;
            }
        }

        private static object ConvertStringFieldValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value.StartsWith("/Date(", StringComparison.Ordinal) && value.EndsWith(")/", StringComparison.Ordinal))
            {
                DateTime? date = dateConverter.ConverDate(value);
                if (date.HasValue)
                {
                    return date.Value;
                }
            }
            else if (value.StartsWith("/Guid(", StringComparison.Ordinal) && value.EndsWith(")/", StringComparison.Ordinal) &&
                     Guid.TryParse(value.Substring(6, value.Length - 8), out Guid guidValue))
            {
                return guidValue;
            }

            return value;
        }
    }
}
