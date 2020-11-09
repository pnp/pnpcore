using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal static class ListDataAsStreamHandler
    {

        internal static async Task<Dictionary<string, object>> Deserialize(string json, List list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json));
            }

            var result = new Dictionary<string, object>();

            var document = JsonSerializer.Deserialize<JsonElement>(json);

            // Process "non-rows" data
            foreach (var property in document.EnumerateObject())
            {
                // The rows are handled seperately
                if (property.Name != "Row")
                {
                    FieldType fieldType = FieldType.Text;
                    if (property.Value.ValueKind == JsonValueKind.Number)
                    {
                        fieldType = FieldType.Integer;
                    }
                    else if (property.Value.ValueKind == JsonValueKind.False || property.Value.ValueKind == JsonValueKind.True)
                    {
                        fieldType = FieldType.Boolean;
                    }

                    var fieldValue = GetJsonPropertyValue(property.Value, fieldType);

                    if (!result.ContainsKey(property.Name))
                    {
                        result.Add(property.Name, fieldValue);
                    }
                }
            }

            // Process rows
            if (document.TryGetProperty("Row", out JsonElement dataRows))
            {
                // Mark collection as requested to avoid our linq integration to actually execute this as a query to SharePoint
                list.Items.Requested = true;

                // No data returned, stop processing
                if (dataRows.GetArrayLength() == 0)
                {
                    return result;
                }

                // Load the fields if not yet loaded
                await list.EnsurePropertiesAsync(List.GetListDataAsStreamExpression).ConfigureAwait(false);

                // Grab the list entity information
                var entityInfo = EntityManager.Instance.GetStaticClassInfo(list.GetType());

                // Process the returned data rows
                foreach (var row in dataRows.EnumerateArray())
                {
                    if (int.TryParse(row.GetProperty("ID").GetString(), out int listItemId))
                    {
                        var itemToUpdate = list.Items.FirstOrDefault(p => p.Id == listItemId);
                        if (itemToUpdate == null)
                        {
                            itemToUpdate = (list.Items as ListItemCollection).CreateNewAndAdd();
                        }

                        itemToUpdate = itemToUpdate as ListItem;
                        itemToUpdate.SetSystemProperty(p => p.Id, listItemId);

                        // Ensure metadata handling when list items are read using this method
                        await (itemToUpdate as ListItem).GraphToRestMetadataAsync().ConfigureAwait(false);

                        var overflowDictionary = itemToUpdate.Values;

                        foreach (var property in row.EnumerateObject())
                        {
                            //var entityField = entityInfo.Fields.FirstOrDefault(p => p.Name == property.Name);
                            var field = list.Fields.FirstOrDefault(p => p.InternalName == property.Name);
                            if (field != null)
                            {
                                // Handle the regular fields, Title is handled as an overflow field
                                if (property.Name == "ID")
                                {
                                    // already handled, so continue
                                }
                                else if (property.Name == "_CommentsFlags")
                                {
                                    string commentsFlags = row.GetProperty("_CommentsFlags").GetString();
                                    // TODO: translate to model
                                }
                                // Handle the overflow fields
                                else
                                {
                                    var fieldValue = GetJsonPropertyValue(property.Value, field.FieldTypeKind);

                                    // all properties are returned as string
                                    if (!overflowDictionary.ContainsKey(property.Name))
                                    {
                                        overflowDictionary.SystemAdd(property.Name, fieldValue);
                                    }
                                    else
                                    {
                                        overflowDictionary.SystemUpdate(property.Name, fieldValue);
                                    }
                                }
                            }
                            else
                            {
                                // TODO: figure out how to present the other returned information
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static object GetJsonPropertyValue(JsonElement propertyValue, FieldType fieldType)
        {
            switch (fieldType)
            {
                case FieldType.Boolean:
                    {
                        if (propertyValue.ValueKind == JsonValueKind.True || propertyValue.ValueKind == JsonValueKind.False)
                        {
                            return propertyValue.GetBoolean();
                        }
                        else if (propertyValue.ValueKind == JsonValueKind.String)
                        {
                            if (bool.TryParse(propertyValue.GetString(), out bool parsedBool))
                            {
                                return parsedBool;
                            }
                        }
                        else if (propertyValue.ValueKind == JsonValueKind.Number)
                        {
                            var number = propertyValue.GetInt32();

                            if (number == 1)
                            {
                                return true;
                            }
                            else if (number == 0)
                            {
                                return false;
                            }
                        }

                        // last result, return default bool value
                        return false;
                    }
                case FieldType.Integer:
                    {
                        if (propertyValue.ValueKind != JsonValueKind.Number)
                        {
                            // Override parsing in case it's not a number, assume string
                            if (int.TryParse(propertyValue.GetString(), out int intValue))
                            {
                                return intValue;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                        else
                        {
                            return propertyValue.GetInt32();
                        }
                    }
                case FieldType.Number:
                    {
                        if (propertyValue.ValueKind != JsonValueKind.Number)
                        {
                            if (double.TryParse(propertyValue.GetString(), out double doubleValue))
                            {
                                return doubleValue;
                            }
                            else
                            {
                                return 0.0d;
                            }
                        }
                        else
                        {
                            return propertyValue.GetDouble();
                        }
                    }
                case FieldType.DateTime:
                    {
                        if (propertyValue.ValueKind != JsonValueKind.Null)
                        {
                            return propertyValue.GetDateTime();
                        }
                        else
                        {
                            return null;
                        }
                    }
                default:
                    {
                        return propertyValue.GetString();
                    }
            }
        }

    }
}
