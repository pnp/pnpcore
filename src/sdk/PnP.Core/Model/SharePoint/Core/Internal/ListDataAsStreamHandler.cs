using PnP.Core.QueryModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal static class ListDataAsStreamHandler
    {
        internal static readonly Regex currencyRegex = new Regex(@"([\d,.]+)", RegexOptions.Compiled);

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

            Dictionary<string, IField> fieldLookupCache = new Dictionary<string, IField>();

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
                await list.EnsurePropertiesAsync(List.LoadFieldsExpression).ConfigureAwait(false);

                // Grab the list entity information
                var entityInfo = EntityManager.Instance.GetStaticClassInfo(list.GetType());

                // Process the returned data rows
                foreach (var row in dataRows.EnumerateArray())
                {
                    if (int.TryParse(row.GetProperty("ID").GetString(), out int listItemId))
                    {
                        // Here we want to avoid the LINQ query
                        // and we want to rely on LINQ to Objects
                        var itemToUpdate = list.Items.AsRequested().FirstOrDefault(p => p.Id == listItemId);
                        if (itemToUpdate == null)
                        {
                            itemToUpdate = (list.Items as ListItemCollection).CreateNewAndAdd();
                            itemToUpdate.Requested = true;
                        }

                        itemToUpdate = itemToUpdate as ListItem;
                        itemToUpdate.SetSystemProperty(p => p.Id, listItemId);

                        // Ensure metadata handling when list items are read using this method
                        await (itemToUpdate as ListItem).GraphToRestMetadataAsync().ConfigureAwait(false);

                        var overflowDictionary = itemToUpdate.Values;

                        // Translate this row first into an object model for easier consumption
                        var rowToProcess = TransformRowData(row, list.Fields, fieldLookupCache);

                        foreach (var property in rowToProcess)
                        {
                            if (property.Name == "ID")
                            {
                                // already handled, so continue
                            }
                            // Handle the overflow fields
                            else
                            {
                                object fieldValue = null;
                                // Empty array's need to be treated as special fields
                                if (!property.Values.Any() && !property.IsArray)
                                {
                                    // regular fields
                                    if (property.Value.ValueKind == JsonValueKind.Array)
                                    {
                                        // MultiChoice property
                                        fieldValue = new List<string>();
                                        foreach (var prop in property.Value.EnumerateArray())
                                        {
                                            (fieldValue as List<string>).Add(prop.GetString());
                                        }
                                    }
                                    else
                                    {
                                        // Handle empty regular field value so that they're the same as with a regular Get call
                                        if ((property.Field.FieldTypeKind == FieldType.Text || property.Field.FieldTypeKind == FieldType.Note || property.Field.FieldTypeKind == FieldType.MultiChoice || property.Field.FieldTypeKind == FieldType.Choice) &&
                                            property.Value.ValueKind == JsonValueKind.String &&
                                            string.IsNullOrEmpty(property.Value.GetString()))
                                        {
                                            fieldValue = null;
                                        }
                                        else if ((property.Field.FieldTypeKind == FieldType.Number || property.Field.FieldTypeKind == FieldType.Currency || property.Field.FieldTypeKind == FieldType.Integer) &&
                                                 property.Value.ValueKind == JsonValueKind.Number &&
                                                 property.Value.GetDouble() == 0d)
                                        {
                                            fieldValue = 0;
                                        }
                                        else
                                        {
                                            // simple property
                                            fieldValue = GetJsonPropertyValue(property.Value, property.Type);
                                        }
                                    }
                                }
                                else
                                {
                                    // Special field
                                    if (!property.IsArray)
                                    {
                                        var listDataAsStreamPropertyValue = property.Values.First();
                                        fieldValue = listDataAsStreamPropertyValue.FieldValue.FromListDataAsStream(listDataAsStreamPropertyValue.Properties);
                                        if (fieldValue != null)
                                        {
                                            (fieldValue as FieldValue).IsArray = false;
                                        }
                                    }
                                    else
                                    {
                                        fieldValue = new FieldValueCollection(property.Field, property.Name);
                                        foreach (var xx in property.Values)
                                        {
                                            var yy = xx.FieldValue.FromListDataAsStream(xx.Properties);
                                            if (yy is FieldLookupValue yyLookup)
                                            {
                                                // Only add to collection when it points to a real value
                                                if (yyLookup.LookupId > -1)
                                                {
                                                    yyLookup.IsArray = true;
                                                    (fieldValue as FieldValueCollection).Values.Add(yyLookup);
                                                }
                                            }
                                            else
                                            {
                                                (fieldValue as FieldValueCollection).Values.Add(yy);
                                            }
                                        }
                                    }
                                }

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

                        if (row.TryGetProperty("FSObjType", out JsonElement fsObjType))
                        {
                            if (Enum.TryParse(fsObjType.GetString(), out FileSystemObjectType fsot))
                            {
                                itemToUpdate.SetSystemProperty(p => p.FileSystemObjectType, fsot);
                            }
                        }

                        // Post processing for ListItem properties
                        if (row.TryGetProperty("UniqueId", out JsonElement uniqueId))
                        {
                            itemToUpdate.SetSystemProperty(p => p.UniqueId, Guid.Parse(uniqueId.ToString()));

                            if (itemToUpdate.FileSystemObjectType == FileSystemObjectType.File)
                            {
                                var file = itemToUpdate.File;
                                file.SetSystemProperty(p => p.UniqueId, Guid.Parse(uniqueId.ToString()));
                                
                                if (!(file as IMetadataExtensible).Metadata.ContainsKey(PnPConstants.MetaDataRestId))
                                {
                                    (file as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataRestId, uniqueId.ToString());
                                }
                                
                                if (!(file as IMetadataExtensible).Metadata.ContainsKey(PnPConstants.MetaDataType))
                                {
                                    (file as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataType, "SP.File");
                                }
                                
                                (file as File).Requested = true;
                            }
                        }

                        if (row.TryGetProperty("ServerRedirectedEmbedUrl", out JsonElement serverRedirectedEmbedUrl))
                        {
                            itemToUpdate.SetSystemProperty(p => p.ServerRedirectedEmbedUri, serverRedirectedEmbedUrl.GetString());
                            itemToUpdate.SetSystemProperty(p => p.ServerRedirectedEmbedUrl, serverRedirectedEmbedUrl.GetString());
                        }

                        if (row.TryGetProperty("ContentTypeId", out JsonElement contentTypeId) && row.TryGetProperty("ContentType", out JsonElement contentTypeName))
                        {
                            var ct = itemToUpdate.ContentType;

                            ct.SetSystemProperty(p => p.Id, contentTypeId.GetString());
                            ct.SetSystemProperty(p => p.StringId, contentTypeId.GetString());
                            ct.SetSystemProperty(p => p.Name, contentTypeName.GetString());
                            (ct as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataRestId, contentTypeId.GetString());
                            (ct as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataType, "SP.ContentType");
                            (ct as ContentType).Requested = true;
                        }

                        // Ensure the values are committed to the model when an item is being added: this
                        // will ensure there's no pending changes anymore
                        itemToUpdate.Values.Commit();
                    }
                }
            }

            return result;
        }

        private static List<ListDataAsStreamProperty> TransformRowData(JsonElement row, IFieldCollection fields, Dictionary<string, IField> fieldLookupCache)
        {
            List<ListDataAsStreamProperty> properties = new List<ListDataAsStreamProperty>();

            foreach (var property in row.EnumerateObject())
            {
                // Doing the field lookup is expensive given it happens per field/row, caching drastically improves performance when reading large sets of items
                if (!fieldLookupCache.TryGetValue(property.Name, out IField field))
                {
                    field = fields.AsRequested().FirstOrDefault(p => p.InternalName == property.Name);
                    fieldLookupCache.Add(property.Name, field);
                }

                if (field != null)
                {
                    var streamProperty = new ListDataAsStreamProperty()
                    {
                        Field = field,
                        Type = field.FieldTypeKind,
                        Name = property.Name
                    };

                    // Is this a field that needs to be wrapped into a special field type?
                    var specialField = DetectSpecialFieldType(streamProperty.Name, field);
                    if (specialField != null)
                    {
                        streamProperty.IsArray = specialField.Item2;

                        if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            #region Sample json responses
                            /*
                            "PersonSingle": [
                                {
                                    "id": "15",
                                    "title": "Kevin Cook",
                                    "email": "KevinC@bertonline.onmicrosoft.com",
                                    "sip": "KevinC@bertonline.onmicrosoft.com",
                                    "picture": ""
                                }
                            ],

                            "PersonMultiple": [
                                {
                                    "id": "14",
                                    "value": "Anna Lidman",
                                    "title": "Anna Lidman",
                                    "email": "AnnaL@bertonline.onmicrosoft.com",
                                    "sip": "AnnaL@bertonline.onmicrosoft.com",
                                    "picture": ""
                                },
                                {
                                    "id": "6",
                                    "value": "Bert Jansen (Cloud)",
                                    "title": "Bert Jansen (Cloud)",
                                    "email": "bert.jansen@bertonline.onmicrosoft.com",
                                    "sip": "bert.jansen@bertonline.onmicrosoft.com",
                                    "picture": ""
                                }
                            ],

                            "MMSingle": {
                                "__type": "TaxonomyFieldValue:#Microsoft.SharePoint.Taxonomy",
                                "Label": "LBI",
                                "TermID": "ed5449ec-4a4f-4102-8f07-5a207c438571"
                            },

                            "MMMultiple": [
                                {
                                    "Label": "LBI",
                                    "TermID": "ed5449ec-4a4f-4102-8f07-5a207c438571"
                                },
                                {
                                    "Label": "MBI",
                                    "TermID": "1824510b-00e1-40ac-8294-528b1c9421e0"
                                },
                                {
                                    "Label": "HBI",
                                    "TermID": "0b709a34-a74e-4d07-b493-48041424a917"
                                }
                            ],
                             
                            "LookupSingle": [
                                {
                                    "lookupId": 71,
                                    "lookupValue": "Sample Document 01",
                                    "isSecretFieldValue": false
                                }
                            ],

                            "LookupMultiple": [
                                {
                                    "lookupId": 1,
                                    "lookupValue": "General",
                                    "isSecretFieldValue": false
                                },
                                {
                                    "lookupId": 71,
                                    "lookupValue": "Sample Document 01",
                                    "isSecretFieldValue": false
                                }
                            ],
                             
                            "Location": {
                                "DisplayName": null,
                                "LocationUri": "https://www.bingapis.com/api/v6/addresses/QWRkcmVzcy83MDA5ODMwODI3MTUyMzc1ODA5JTdjMT9h%3d%3d?setLang=en",
                                "EntityType": null,
                                "Address": {
                                    "Street": "Somewhere",
                                    "City": "XYZ",
                                    "State": "Vlaanderen",
                                    "CountryOrRegion": "Belgium",
                                    "PostalCode": "9999"
                                },
                                "Coordinates": {
                                    "Latitude": null,
                                    "Longitude": null
                                }
                            },                             
                            */
                            #endregion

                            // Add values that will become part of a FieldValueCollection later on
                            foreach (var streamPropertyElement in property.Value.EnumerateArray())
                            {
                                (var fieldValue, var isArray) = DetectSpecialFieldType(streamProperty.Name, field);
                                var listDataAsStreamPropertyValue = new ListDataAsStreamPropertyValue()
                                {
                                    FieldValue = fieldValue
                                };

                                foreach (var streamPropertyElementValue in streamPropertyElement.EnumerateObject())
                                {
                                    listDataAsStreamPropertyValue.Properties.Add(streamPropertyElementValue.Name, GetJsonPropertyValueAsString(streamPropertyElementValue.Value));
                                }

                                streamProperty.Values.Add(listDataAsStreamPropertyValue);
                            }
                        }
                        else
                        {
                            /*
                             "Url": "https:\u002f\u002fpnp.com\u002f3",
                             "Url.desc": "something3",

                             "LookupSingleField1": "",
                             "LookupSingleField1.": "",
                            */

                            var listDataAsStreamPropertyValue = new ListDataAsStreamPropertyValue()
                            {
                                FieldValue = specialField.Item1
                            };

                            if (property.Value.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var streamPropertyElementValue in property.Value.EnumerateObject())
                                {
                                    if (streamPropertyElementValue.Value.ValueKind == JsonValueKind.Object)
                                    {
                                        foreach (var streamPropertyElementValueLevel2 in streamPropertyElementValue.Value.EnumerateObject())
                                        {
                                            listDataAsStreamPropertyValue.Properties.Add(streamPropertyElementValueLevel2.Name, GetJsonPropertyValueAsString(streamPropertyElementValueLevel2.Value));
                                        }
                                    }
                                    else
                                    {
                                        listDataAsStreamPropertyValue.Properties.Add(streamPropertyElementValue.Name, GetJsonPropertyValueAsString(streamPropertyElementValue.Value));
                                    }
                                }
                            }
                            else
                            {
                                listDataAsStreamPropertyValue.Properties.Add(property.Name, GetJsonPropertyValueAsString(property.Value));
                            }

                            streamProperty.Values.Add(listDataAsStreamPropertyValue);
                        }
                    }
                    else
                    {
                        // Add as single property or simple choice collection

                        /*
                        "Title": "Item1",

                        "ChoiceMultiple": [
                            "Choice 1",
                            "Choice 3",
                            "Choice 4"
                        ],
                         */
                        streamProperty.Value = property.Value;
                    }

                    properties.Add(streamProperty);
                }
                else
                {
                    /*
                     "Url.desc": "something3",
                     "DateTime1.": "2020-12-04T11:15:15Z",
                    */

                    if (property.Name.Contains("."))
                    {
                        string[] nameParts = property.Name.Split(new char[] { '.' });

                        var propertyToUpdate = properties.FirstOrDefault(p => p.Name == nameParts[0]);
                        if (propertyToUpdate != null && propertyToUpdate.Values.Count == 1 && !string.IsNullOrEmpty(nameParts[1]))
                        {
                            var valueToUpdate = propertyToUpdate.Values.FirstOrDefault();
                            if (valueToUpdate == null)
                            {
                                valueToUpdate = new ListDataAsStreamPropertyValue();
                                propertyToUpdate.Values.Add(valueToUpdate);
                            }
                            if (!valueToUpdate.Properties.ContainsKey(nameParts[1]))
                            {
                                valueToUpdate.Properties.Add(nameParts[1], GetJsonPropertyValueAsString(property.Value));
                            }
                        }
                        else if (propertyToUpdate != null && !string.IsNullOrEmpty(nameParts[1]))
                        {
                            //"Bool1.value": "1",

                            if (!fieldLookupCache.TryGetValue(nameParts[0], out IField field2))
                            {
                                field2 = fields.AsRequested().FirstOrDefault(p => p.InternalName == nameParts[0]);
                                fieldLookupCache.Add(nameParts[0], field2);
                            }

                            // Extra properties on "regular" fields
                            if (field2 != null && field2.FieldTypeKind == FieldType.Boolean && nameParts[1] == "value")
                            {
                                propertyToUpdate.Value = property.Value;
                            }
                        }
                        else if (propertyToUpdate != null && string.IsNullOrEmpty(nameParts[1]))
                        {
                            // override the set Value
                            propertyToUpdate.Value = property.Value;
                        }
                    }

                }
            }

            return properties;
        }

        private static Tuple<FieldValue, bool> DetectSpecialFieldType(string name, IField field)
        {
            // Some system fields are of type lookup but should not be processed as lookup
            if (BuiltInFields.Contains(name))
            {
                return null;
            }

            switch (field.TypeAsString)
            {
                case "URL": return new Tuple<FieldValue, bool>(new FieldUrlValue() { Field = field }, false);
                case "UserMulti": return new Tuple<FieldValue, bool>(new FieldUserValue() { Field = field }, true);
                case "User": return new Tuple<FieldValue, bool>(new FieldUserValue() { Field = field }, false);
                case "LookupMulti": return new Tuple<FieldValue, bool>(new FieldLookupValue() { Field = field }, true);
                case "Lookup": return new Tuple<FieldValue, bool>(new FieldLookupValue() { Field = field }, false);
                case "TaxonomyFieldTypeMulti": return new Tuple<FieldValue, bool>(new FieldTaxonomyValue() { Field = field }, true);
                case "TaxonomyFieldType": return new Tuple<FieldValue, bool>(new FieldTaxonomyValue() { Field = field }, false);
                case "Location": return new Tuple<FieldValue, bool>(new FieldLocationValue() { Field = field }, false);

                default:
                    {
                        return null;
                    }
            }
        }

        private static string GetJsonPropertyValueAsString(JsonElement propertyValue)
        {
            if (propertyValue.ValueKind == JsonValueKind.True || propertyValue.ValueKind == JsonValueKind.False)
            {
                return propertyValue.GetBoolean().ToString();
            }
            else if (propertyValue.ValueKind == JsonValueKind.Number)
            {
                return propertyValue.GetInt32().ToString();
            }
            else if (propertyValue.ValueKind == JsonValueKind.Undefined)
            {
                return "Null";
            }
            else
            {
                return propertyValue.GetString();
            }
        }

        private static object GetJsonPropertyValue(JsonElement propertyValue, FieldType fieldType)
        {
            /* US formats
            "Number1": "67,687",
            "Currency1": "$67.67",

            "Number1": "67.687",

            */
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
                            return StringToBool(propertyValue.GetString());
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
                                // When there's no value then return null
                                if (propertyValue.ValueKind == JsonValueKind.String && string.IsNullOrEmpty(propertyValue.GetString()))
                                {
                                    return null;
                                }
                                else
                                {
                                    return 0;
                                }
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
                            if (int.TryParse(propertyValue.GetString(), out int intValue))
                            {
                                return intValue;
                            }
                            // Numbers and currency are provided in US format
                            else if (double.TryParse(propertyValue.GetString(), NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out double doubleValue))
                            {
                                return doubleValue;
                            }
                            else
                            {
                                // When there's no value return null
                                if (propertyValue.ValueKind == JsonValueKind.String && string.IsNullOrEmpty(propertyValue.GetString()))
                                {
                                    return null;
                                }
                                else
                                {
                                    return 0.0d;
                                }
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
                            if (DateTime.TryParse(propertyValue.GetString(), out DateTime dateTime))
                            {
                                return dateTime;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                case FieldType.Currency:
                    {
                        if (propertyValue.ValueKind != JsonValueKind.Null)
                        {
                            string currencyString = propertyValue.GetString();
                            if (!string.IsNullOrEmpty(currencyString))
                            {
                                // trim currency chars
                                var match = currencyRegex.Match(currencyString);
                                if (match.Success)
                                {
                                    // Numbers and currency are provided in US format
                                    if (double.TryParse(match.Groups[1].Value, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out double parsedCurrency))
                                    {
                                        return parsedCurrency;
                                    }
                                }
                            }
                            else
                            {
                                return null;
                            }
                        }
                        return null;
                    }
                default:
                    {
                        if (propertyValue.ValueKind == JsonValueKind.Undefined)
                        {
                            return null;
                        }
                        else
                        {
                            return propertyValue.GetString();
                        }
                    }
            }
        }

        private static bool StringToBool(string value) =>
            value.Equals("yes", StringComparison.CurrentCultureIgnoreCase) ||
            value.Equals(bool.TrueString, StringComparison.CurrentCultureIgnoreCase) ||
            value.Equals("1");

    }
}
