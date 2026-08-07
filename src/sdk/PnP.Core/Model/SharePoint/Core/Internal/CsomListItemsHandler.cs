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
                    // Skip the CSOM object properties
                    if (property.Name == "_ObjectType_" || property.Name == "_ObjectIdentity_" || property.Name == "_ObjectVersion_" ||
                        property.Name == "Id")
                    {
                        continue;
                    }

                    // CSOM appends the value type to the property name, e.g. "MyNumber$ Double", "ID$  Int32" or
                    // "MyLookups$SP.FieldLookupValue$  Array". The part before the first $ is the internal name
                    string fieldName = property.Name;
                    string csomTypeHint = null;
                    int typeSuffixIndex = fieldName.IndexOf('$');
                    if (typeSuffixIndex > 0)
                    {
                        csomTypeHint = fieldName.Substring(typeSuffixIndex + 1).Trim();
                        fieldName = fieldName.Substring(0, typeSuffixIndex).Trim();
                    }

                    object fieldValue = ConvertFieldValue(fieldName, property.Value, list.Fields, fieldLookupCache, csomTypeHint);

                    if (!overflowDictionary.ContainsKey(fieldName))
                    {
                        overflowDictionary.SystemAdd(fieldName, fieldValue);
                    }
                    else
                    {
                        overflowDictionary.SystemUpdate(fieldName, fieldValue);
                    }
                }

                MapSystemProperties(listItem, item);

                // Ensure the values are committed to the model so there are no pending changes
                listItem.Values.Commit();

                processedItems.Add(listItem);
            }

            return processedItems;
        }

        private static object ConvertFieldValue(string propertyName, JsonElement value, IFieldCollection fields, Dictionary<string, IField> fieldLookupCache, string csomTypeHint = null)
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
                    {
                        // Keep empty special fields typed, as the REST loaders do (lookup with LookupId -1)
                        var emptyValue = DetectSpecialFieldType(propertyName, value, field);
                        if (emptyValue != null)
                        {
                            emptyValue.IsArray = false;
                            return emptyValue.FromJson(value);
                        }

                        return null;
                    }
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return value.GetBoolean();
                case JsonValueKind.Number:
                    return ConvertNumberFieldValue(value, field, csomTypeHint);
                case JsonValueKind.String:
                    return ConvertStringFieldValue(value.GetString());
                case JsonValueKind.Object:
                    {
                        var typedValue = DetectSpecialFieldType(propertyName, value, field);
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
                                var typedValue = DetectSpecialFieldType(propertyName, element, field);
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

        private static void MapSystemProperties(ListItem listItem, JsonElement item)
        {
            // Mirrors ListDataAsStreamHandler, but CSOM returns the file system object type as a number
            if (item.TryGetProperty("FileSystemObjectType", out JsonElement fileSystemObjectType) &&
                fileSystemObjectType.ValueKind == JsonValueKind.Number &&
                fileSystemObjectType.TryGetInt32(out int fileSystemObjectTypeValue) &&
                Enum.IsDefined(typeof(FileSystemObjectType), fileSystemObjectTypeValue))
            {
                listItem.SetSystemProperty(p => p.FileSystemObjectType, (FileSystemObjectType)fileSystemObjectTypeValue);
            }
            else if (item.TryGetProperty("FSObjType", out JsonElement fsObjType) && fsObjType.ValueKind == JsonValueKind.String &&
                     Enum.TryParse(fsObjType.GetString(), out FileSystemObjectType fsot))
            {
                listItem.SetSystemProperty(p => p.FileSystemObjectType, fsot);
            }

            if (item.TryGetProperty("UniqueId", out JsonElement uniqueId) && uniqueId.ValueKind == JsonValueKind.String &&
                TryParseCsomGuid(uniqueId.GetString(), out Guid uniqueIdValue))
            {
                listItem.SetSystemProperty(p => p.UniqueId, uniqueIdValue);

                // Reading a property that was never set throws, so only use it when it was mapped above
                if (listItem.IsPropertyAvailable(p => p.FileSystemObjectType) &&
                    listItem.FileSystemObjectType == FileSystemObjectType.File)
                {
                    var file = listItem.File;
                    file.SetSystemProperty(p => p.UniqueId, uniqueIdValue);

                    if (!(file as IMetadataExtensible).Metadata.ContainsKey(PnPConstants.MetaDataRestId))
                    {
                        (file as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataRestId, uniqueIdValue.ToString());
                    }

                    if (!(file as IMetadataExtensible).Metadata.ContainsKey(PnPConstants.MetaDataType))
                    {
                        (file as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataType, "SP.File");
                    }

                    (file as File).Requested = true;
                }
            }

            // CSOM returns ServerRedirectedEmbedUri as null and ServerRedirectedEmbedUrl as an empty string when there is no value
            if (item.TryGetProperty("ServerRedirectedEmbedUrl", out JsonElement serverRedirectedEmbedUrl) &&
                serverRedirectedEmbedUrl.ValueKind == JsonValueKind.String &&
                !string.IsNullOrEmpty(serverRedirectedEmbedUrl.GetString()))
            {
                listItem.SetSystemProperty(p => p.ServerRedirectedEmbedUri, serverRedirectedEmbedUrl.GetString());
                listItem.SetSystemProperty(p => p.ServerRedirectedEmbedUrl, serverRedirectedEmbedUrl.GetString());
            }
        }

        private static object ConvertNumberFieldValue(JsonElement value, IField field, string csomTypeHint)
        {
            // Type on the list schema so the .NET type matches ListDataAsStreamHandler.GetJsonPropertyValue
            if (field != null)
            {
                switch (field.FieldTypeKind)
                {
                    case FieldType.Number:
                    case FieldType.Currency:
                        return value.GetDouble();
                    case FieldType.Integer:
                    case FieldType.Counter:
                        if (value.TryGetInt32(out int typedIntValue))
                        {
                            return typedIntValue;
                        }
                        break;
                }
            }

            // Projected fields are not in the schema, use the type CSOM appended to the property name
            switch (csomTypeHint)
            {
                case "Double": return value.GetDouble();
                case "Int32":
                    if (value.TryGetInt32(out int hintedIntValue))
                    {
                        return hintedIntValue;
                    }
                    break;
            }

            if (value.TryGetInt32(out int intValue))
            {
                return intValue;
            }

            return value.GetDouble();
        }

        private static FieldValue DetectSpecialFieldType(string propertyName, JsonElement value, IField field)
        {
            // Some system fields are of type lookup but should not be processed as lookup
            if (BuiltInFields.Contains(propertyName))
            {
                return null;
            }

            // The CSOM json shapes match what the REST FromJson parsers expect, so dispatch on the CSOM
            // object type first, that also types fields projected from a joined list
            if (value.ValueKind == JsonValueKind.Object &&
                value.TryGetProperty("_ObjectType_", out JsonElement objectTypeElement) && objectTypeElement.ValueKind == JsonValueKind.String)
            {
                switch (objectTypeElement.GetString())
                {
                    case "SP.FieldLookupValue": return new FieldLookupValue { Field = field };
                    case "SP.FieldUserValue": return new FieldUserValue { Field = field };
                    case "SP.FieldUrlValue": return new FieldUrlValue { Field = field };
                    case "SP.Taxonomy.TaxonomyFieldValue": return new FieldTaxonomyValue { Field = field };
                }
            }

            // Not every value carries a CSOM object type, fall back to the schema like the REST handler
            if (field == null)
            {
                return null;
            }

            switch (field.TypeAsString)
            {
                case "URL": return new FieldUrlValue { Field = field };
                case "User":
                case "UserMulti": return new FieldUserValue { Field = field };
                case "Lookup":
                case "LookupMulti": return new FieldLookupValue { Field = field };
                case "TaxonomyFieldType":
                case "TaxonomyFieldTypeMulti": return new FieldTaxonomyValue { Field = field };
                case "Location": return new FieldLocationValue { Field = field };
                case "Thumbnail": return new FieldThumbnailValue { Field = field };
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
            else if (TryParseCsomGuid(value, out Guid guidValue))
            {
                return guidValue;
            }

            return value;
        }

        private static bool TryParseCsomGuid(string value, out Guid result)
        {
            result = Guid.Empty;

            if (string.IsNullOrEmpty(value) ||
                !value.StartsWith("/Guid(", StringComparison.Ordinal) || !value.EndsWith(")/", StringComparison.Ordinal))
            {
                return false;
            }

            return Guid.TryParse(value.Substring(6, value.Length - 8), out result);
        }
    }
}
