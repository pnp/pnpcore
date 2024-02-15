using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PnP.Core.Services
{
    /// <summary>
    /// Internal class to help with the JSON mapping for Domain Model objects
    /// </summary>
    internal static class JsonMappingHelper
    {
        internal static readonly Regex arrayMatchingRegex = new Regex(@"\[(?<index>[0-9]+)\]", RegexOptions.Compiled);

        /// <summary>
        /// Maps a json string to the provided domain model object instance
        /// </summary>
        /// <param name="batchRequest">The batch request to map json from</param>
        /// <returns></returns>
        internal static async Task MapJsonToModel(BatchRequest batchRequest)
        {
            if (string.IsNullOrEmpty(batchRequest.ResponseJson))
            {
                // We have nothing to process, so return
                return;
            }

            // Parse the received json content
            var json = JsonSerializer.Deserialize<JsonElement>(batchRequest.ResponseJson, PnPConstants.JsonSerializer_AllowTrailingCommasTrue);
            // for SharePoint REST calls the root property is d, for recursive calls this is not the case
            if (!json.TryGetProperty("d", out JsonElement root))
            {
                root = json;
            }

            // TODO: The below method's signature is quite complex and reuses the same object
            // multiple times. Can we try to simplify it?

            // Map the returned JSON to the respective entities
            await FromJson(batchRequest.Model, batchRequest.EntityInfo, new ApiResponse(batchRequest.ApiCall, root, batchRequest.Id), batchRequest.FromJsonCasting).ConfigureAwait(false);
            batchRequest.PostMappingJson?.Invoke(batchRequest.ResponseJson);
        }

        /// <summary>
        /// Maps JSON to model classes
        /// </summary>
        /// <param name="pnpObject">Model to populate from JSON</param>
        /// <param name="entity">Information about the current model</param>
        /// <param name="apiResponse">The REST response to process</param>
        /// <param name="fromJsonCasting">Delegate to be called for type conversion</param>
        /// <returns></returns>
        internal static async Task FromJson(TransientObject pnpObject, EntityInfo entity, ApiResponse apiResponse, Func<FromJson, object> fromJsonCasting = null)
        {
            // Mark object as not requested during load time
            if (pnpObject.GetType().ImplementsInterface(typeof(IRequestable)))
            {
                ((IRequestable)pnpObject).Requested = false;
            }

            bool requested = false;
            if (apiResponse.ApiCall.Type == ApiType.SPORest)
            {
                requested = await FromJsonRest(pnpObject, entity, apiResponse, fromJsonCasting).ConfigureAwait(false);
            }
            else if (apiResponse.ApiCall.Type == ApiType.Graph || apiResponse.ApiCall.Type == ApiType.GraphBeta)
            {
                requested = await FromJsonGraph(pnpObject, entity, apiResponse, fromJsonCasting).ConfigureAwait(false);
            }

            // Mark object as requested, as long as it is an IRequestable object
            if (pnpObject.GetType().ImplementsInterface(typeof(IRequestable)) && requested)
            {
                ((IRequestable)pnpObject).Requested = true;
            }
        }

        private static async Task<bool> FromJsonRest(TransientObject pnpObject, EntityInfo entity, ApiResponse apiResponse, Func<FromJson, object> fromJsonCasting = null)
        {
            bool requested = false;

            // Get the type of the domain model we're populating
            var pnpObjectType = pnpObject.GetType();
            var metadataBasedObject = pnpObject as IMetadataExtensible;
            var contextAwareObject = pnpObject as IDataModelWithContext;

            // Set the batch request id property
            SetBatchRequestId(pnpObject, apiResponse.BatchRequestId);

            // Check if we somehow require pushing values into a generic dictionary
            TransientDictionary dictionaryPropertyToAddValueTo = null;
            bool useOverflowField = false;
            if (entity.UseOverflowField)
            {
                // Get the dictionary property that will hold the overflow data
                dictionaryPropertyToAddValueTo = pnpObjectType.GetProperty(ExpandoBaseDataModel<IExpandoDataModel>.OverflowFieldName).GetValue(pnpObject) as TransientDictionary;

                useOverflowField = true;
            }

            // variable to capture the id value
            string idFieldValue = null;

            if (apiResponse.JsonElement.ValueKind == JsonValueKind.Object)
            {
                // Enumerate the received properties and try to map them to the model
                //foreach (var property in apiResponse.JsonElement.EnumerateObject())
                foreach(var orderedProperty in OrderedProperties(apiResponse.JsonElement))
                {
                    var property = orderedProperty.Value;

                    // Find the model field linked to this field
                    EntityFieldInfo entityField = null;

                    // Do we need to re-parent this json mapping to a non expandable collection in the current model?
                    if (!string.IsNullOrEmpty(apiResponse.ApiCall.ReceivingProperty) && property.NameEquals("value"))
                    {
                        entityField = entity.Fields.FirstOrDefault(p => !string.IsNullOrEmpty(p.Name) && p.Name.Equals(apiResponse.ApiCall.ReceivingProperty, StringComparison.InvariantCultureIgnoreCase)) ??
                                      entity.Fields.FirstOrDefault(p => !string.IsNullOrEmpty(p.SharePointName) && p.SharePointName.Equals(apiResponse.ApiCall.ReceivingProperty, StringComparison.InvariantCultureIgnoreCase));
                    }
                    else
                    {
                        entityField = LookupEntityField(entity, apiResponse, property);
                    }

                    // Entity field should be populated for the actual fields we've requested
                    if (entityField != null)
                    {
                        // Are we loading a collection (e.g. Web.Lists)?
                        if (IsModelCollection(entityField.PropertyInfo.PropertyType))
                        {
                            // Get the actual current value of the property we're setting...as that allows to detect it's type
                            // Doing the get also initializes the model collection, so keep this code block 
                            var propertyToSetValue = entityField.PropertyInfo.GetValue(pnpObject);
                            // Cast object to call the needed methods on it (e.g. ListCollection)
                            var typedCollection = propertyToSetValue as IManageableCollection;

                            // Try to get the results property, start with a default value
                            JsonElement resultsProperty = default;

                            // If the property is named "results" and is of type Array, it means it is a collection of items
                            if (property.Value.ValueKind == JsonValueKind.Array)
                            {
                                // and we use it directly
                                resultsProperty = property.Value;
                            }
                            else
                            {
                                // Some collections are returned as null when they're empty (e.g. Mentions on IComment)
                                if (property.Value.ValueKind == JsonValueKind.Null)
                                {
                                    continue;
                                }

                                // otherwise we try to get the child property called "results", if any
                                property.Value.TryGetProperty("value", out resultsProperty);
                            }

                            // Expanded objects are under the results property, if any (i.e. it is not the default one)
                            if (!resultsProperty.Equals(default(JsonElement)))
                            {
                                PropertyInfo pnpChildIdProperty = null;
                                foreach (var childJson in resultsProperty.EnumerateArray())
                                {
                                    // Create a new model instance to add to the collection
                                    var pnpChild = typedCollection.CreateNew();

                                    // Set the batch request id property
                                    SetBatchRequestId(pnpChild as TransientObject, apiResponse.BatchRequestId);

                                    // Recursively map properties, call the method from the actual object as the object could have overriden it
                                    await ((IDataModelProcess)pnpChild).ProcessResponseAsync(new ApiResponse(apiResponse.ApiCall, childJson, apiResponse.BatchRequestId)).ConfigureAwait(false);

                                    // Check if we've marked a field to be the key field for the entities in this collection
                                    if (pnpChildIdProperty == null)
                                    {
                                        // Grab the child entity information, important to call this from the actual child entity
                                        var pnpChildEntityInfo = EntityManager.Instance.GetStaticClassInfo(pnpChild.GetType());
                                        // Was there a key defined on the child entity
                                        var keyField = pnpChildEntityInfo.SharePointKeyField;
                                        if (keyField != null)
                                        {
                                            // There was a key, so that property should exist
                                            string key = keyField.Name;
                                            pnpChildIdProperty = pnpChild.GetType().GetProperty(key);
                                        }
                                    }

                                    // We do have a key property, use that to avoid loading duplicates
                                    if (pnpChildIdProperty != null)
                                    {
                                        var pnpChildIdPropertyValue = ParseStringValueToTyped(pnpChildIdProperty.GetValue(pnpChild).ToString(), pnpChildIdProperty.PropertyType);

                                        // Only add to collection when not yet added
                                        // or replace the already existing item, if any
                                        typedCollection.AddOrUpdate(pnpChild,
                                            i => ((IDataModelWithKey)i).Key.Equals(pnpChildIdPropertyValue));
                                    }
                                    else
                                    {
                                        // No key checking...so let's just add
                                        typedCollection.Add(pnpChild);
                                    }
                                }

                                // Set the collection as requested, if it is supported by
                                // the target type, because we've got the items
                                // from the JSON response, or an empty result set if the
                                // collection is empty
                                if (typedCollection.GetType().ImplementsInterface(typeof(IRequestableCollection)))
                                {
                                    ((IRequestableCollection)typedCollection).Requested = true;
                                }

                                requested = true;
                            }
                        }
                        // Are we loading another model type (e.g. Site.RootWeb)?
                        else if (IsModelType(entityField.PropertyInfo.PropertyType))
                        {
                            // Get instance of the model property
                            var propertyToSetValue = entityField.PropertyInfo.GetValue(pnpObject);

                            // Set the batch request id property
                            SetBatchRequestId(propertyToSetValue as TransientObject, apiResponse.BatchRequestId);

                            // Set PnPContext via a dynamic property
                            ((IDataModelWithContext)propertyToSetValue).PnPContext = contextAwareObject.PnPContext;

                            // Set parent
                            ((IDataModelParent)propertyToSetValue).Parent = (IDataModelParent)pnpObject;

                            await ((IDataModelProcess)propertyToSetValue).ProcessResponseAsync(new ApiResponse(apiResponse.ApiCall, property.Value, apiResponse.BatchRequestId)).ConfigureAwait(false);
                        }
                        else // Simple property mapping
                        {
                            // Keep the id value aside when seeing it for later usage
                            if (string.IsNullOrEmpty(idFieldValue) && property.Name.Equals(entity.SharePointKeyField.SharePointName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                idFieldValue = GetJsonPropertyValue(property).ToString();
                                
                                // Store the rest ID field for follow-up rest requests
                                if (!(pnpObject as IMetadataExtensible).Metadata.ContainsKey(PnPConstants.MetaDataRestId) && !string.IsNullOrEmpty(idFieldValue))
                                {
                                    (pnpObject as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataRestId, idFieldValue);
                                }
                            }

                            if (!string.IsNullOrEmpty(entityField.SharePointJsonPath))
                            {
                                var jsonPathFields = entity.Fields.Where(p => !string.IsNullOrEmpty(p.SharePointName) && p.SharePointName.Equals(entityField.SharePointName));
                                if (jsonPathFields.Any())
                                {
                                    foreach (var jsonPathField in jsonPathFields)
                                    {
                                        var jsonElement = GetJsonElementFromPath(property.Value, jsonPathField.SharePointJsonPath);

                                        // Don't assume that the requested json path was also loaded. When using the QueryProperties model there can be 
                                        // a json object returned that does have all properties loaded 
                                        if (!jsonElement.Equals(property.Value))
                                        {
                                            jsonPathField.PropertyInfo?.SetValue(pnpObject, GetJsonFieldValue(contextAwareObject, jsonPathField.Name, apiResponse,
                                                    jsonElement, jsonPathField.DataType, jsonPathField.SharePointUseCustomMapping, fromJsonCasting));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Set the object property value taken from the JSON payload
                                entityField.PropertyInfo?.SetValue(pnpObject, GetJsonFieldValue(contextAwareObject, entityField.Name, apiResponse,
                                    property.Value, entityField.DataType, entityField.SharePointUseCustomMapping, fromJsonCasting));
                            }
                            requested = true;
                        }
                    }
                    else
                    {
                        // Let's keep track of the object metadata, useful when creating new requests
                        if (property.Name == PnPConstants.SharePointRestMetadata)
                        {
                            TrackSharePointMetaData(metadataBasedObject, property);
                        }
                        // Let's also store: 
                        // - the EntityTypeName value as metadata as it's useful for constructing future rest calls
                        // - the __next link, used in (list item) paging
                        else if (property.Name == PnPConstants.MetaDataRestEntityTypeName)
                        {
                            TrackMetaData(metadataBasedObject, property, null);
                        }
                        else if (property.Name == PnPConstants.SharePointRestListItemNextLink)
                        {
                            // Only applies to paging on listitems
                            if (metadataBasedObject is List)
                            {
                                var listItemCollection = (metadataBasedObject as List).Items;
                                if (listItemCollection != null)
                                {
                                    TrackAndUpdateMetaData(listItemCollection as IMetadataExtensible, property);
                                }
                            }
                            else if (metadataBasedObject is ListItem)
                            {
                                var commentCollection = (metadataBasedObject as ListItem).Comments;
                                if (commentCollection != null)
                                {
                                    TrackAndUpdateMetaData(commentCollection as IMetadataExtensible, property);
                                }
                            }
                        }
                        else if (useOverflowField)
                        {
                            if (string.IsNullOrEmpty(idFieldValue) && property.Name.Equals(entity.SharePointKeyField.Name))
                            {
                                idFieldValue = GetJsonPropertyValue(property).ToString();
                            }

                            // Add to overflow, skip __deferred properties
                            if (!(property.Value.ValueKind == JsonValueKind.Object && property.Value.TryGetProperty("__deferred", out JsonElement _)))
                            {

                                // Verify if we can map the received json to a supported field type
                                (object fieldValue, string fieldName) = await ProcessSpecialRestFieldTypeAsync(pnpObject, property.Name, property.Value).ConfigureAwait(false);
                                if (fieldName != null)
                                {
                                    AddToDictionary(dictionaryPropertyToAddValueTo, fieldName, fieldValue, pnpObject);
                                }
                                else
                                {
                                    if (property.Value.ValueKind == JsonValueKind.Object || (property.Value.ValueKind == JsonValueKind.Array && property.Name == "value"))
                                    {
                                        // Handling of complex type via calling out to custom handler, no value is returned as the custom
                                        // handler can update multiple fields based upon what json result came back
                                        fromJsonCasting?.Invoke(new FromJson(property.Name, property.Value, Type.GetType("System.Object"), contextAwareObject.PnPContext.Logger, apiResponse));
                                    }
                                    else
                                    {
                                        // Default mapping to dictionary can handle simple types, more complex types require custom logic
                                        AddToDictionary(dictionaryPropertyToAddValueTo, property.Name, GetJsonPropertyValue(property), pnpObject);
                                    }
                                }
                                requested = true;
                            }
                        }
                    }
                }
            }

            // Ensure all changes are reset (as setting Title on the ListItem will trigger a change)
            if (useOverflowField)
            {
                dictionaryPropertyToAddValueTo.Commit();
            }

            // Try populate the PnP Object metadata from the mapped info
            await TryPopuplatePnPObjectMetadataFromRest(contextAwareObject, metadataBasedObject, entity, idFieldValue).ConfigureAwait(false);

            return requested;
        }

        private static SortedList<int, JsonProperty> OrderedProperties(JsonElement jsonElement)
        {
            SortedList<int, JsonProperty> result =  new SortedList<int, JsonProperty>();

            int order = 0;
            foreach (var property in jsonElement.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Array && property.Value.ValueKind != JsonValueKind.Object)
                {
                    result.Add(order, property);
                    order++;
                }
            }

            foreach (var property in jsonElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array || property.Value.ValueKind == JsonValueKind.Object)
                {
                    result.Add(order, property);
                    order++;
                }
            }

            return result;
        }


        private async static Task<Tuple<object, string>> ProcessSpecialRestFieldTypeAsync(TransientObject pnpObject, string propertyName, JsonElement json)
        {
            // This special processing only applies to list items, property bags are excluded
            if (pnpObject.GetType() != typeof(ListItem) && pnpObject.GetType() != typeof(ListItemVersion))
            {
                return new Tuple<object, string>(null, null);
            }

            var field = await GetListItemFieldAsync(pnpObject, propertyName).ConfigureAwait(false);

            // For Lookup, LookupMulti, User and UserMulti fields we use the "Id" field
            if (field == null && propertyName.EndsWith("Id") && propertyName.Length > 2)
            {
                propertyName = propertyName.Substring(0, propertyName.Length - 2);
                field = await GetListItemFieldAsync(pnpObject, propertyName).ConfigureAwait(false);
            }

            if (field != null)
            {
                if (BuiltInFields.Contains(field.InternalName))
                {
                    return new Tuple<object, string>(null, null);
                }

                switch (field.TypeAsString)
                {
                    case "URL":
                        {
                            var fieldValue = new FieldUrlValue() { Field = field };

                            if (json.ValueKind == JsonValueKind.Null)
                            {
                                fieldValue.FromJson(new JsonElement());
                            }
                            else
                            {
                                fieldValue.FromJson(json);
                            }

                            fieldValue.IsArray = false;
                            return new Tuple<object, string>(fieldValue, propertyName);
                        };
                    case "TaxonomyFieldType":
                        {
                            var fieldValue = new FieldTaxonomyValue() { Field = field };

                            if (json.ValueKind == JsonValueKind.Null)
                            {
                                fieldValue.FromJson(new JsonElement());
                            }
                            else
                            {
                                fieldValue.FromJson(json);
                            }

                            fieldValue.IsArray = false;
                            return new Tuple<object, string>(fieldValue, propertyName);
                        }
                    case "TaxonomyFieldTypeMulti":
                        {
                            var values = new FieldValueCollection(field, propertyName);
                            if (json.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var term in json.EnumerateArray())
                                {
                                    var fieldValue = new FieldTaxonomyValue() { Field = field };
                                    fieldValue.FromJson(term);
                                    fieldValue.IsArray = true;
                                    values.Values.Add(fieldValue);
                                }
                            }
                            return new Tuple<object, string>(values, propertyName);
                        }
                    case "MultiChoice":
                        {
                            var values = new List<string>();
                            if (json.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var choice in json.EnumerateArray())
                                {
                                    values.Add(choice.GetString());
                                }
                            }
                            return new Tuple<object, string>(values, propertyName);
                        }
                    case "Lookup":
                        {
                            var fieldValue = new FieldLookupValue() { Field = field };
                            if (json.ValueKind == JsonValueKind.Null || json.ValueKind == JsonValueKind.Undefined)
                            {
                                fieldValue.FromJson(new JsonElement());
                            }
                            else
                            {
                                fieldValue.FromJson(json);
                            }
                            fieldValue.IsArray = false;
                            return new Tuple<object, string>(fieldValue, propertyName);
                        }
                    case "LookupMulti":
                        {
                            var values = new FieldValueCollection(field, propertyName);
                            if (json.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var lookupId in json.EnumerateArray())
                                {
                                    FieldLookupValue fieldValue = new FieldLookupValue() { Field = field };
                                    fieldValue.FromJson(lookupId);
                                    fieldValue.IsArray = true;
                                    values.Values.Add(fieldValue);
                                }
                            }
                            return new Tuple<object, string>(values, propertyName);
                        }
                    case "User":
                        {
                            var fieldValue = new FieldUserValue() { Field = field };
                            if (json.ValueKind != JsonValueKind.Null && json.ValueKind != JsonValueKind.Undefined)
                            {
                                fieldValue.FromJson(json);
                            }
                            else
                            {
                                fieldValue.FromJson(new JsonElement());
                            }
                            fieldValue.IsArray = false;
                            return new Tuple<object, string>(fieldValue, propertyName);
                        }
                    case "UserMulti":
                        {
                            var values = new FieldValueCollection(field, propertyName);
                            if (json.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var lookupId in json.EnumerateArray())
                                {
                                    FieldUserValue fieldValue = new FieldUserValue() { Field = field };
                                    fieldValue.FromJson(lookupId);
                                    fieldValue.IsArray = true;
                                    values.Values.Add(fieldValue);
                                }
                            }
                            return new Tuple<object, string>(values, propertyName);
                        }
                    case "Location":
                        {
                            var fieldValue = new FieldLocationValue() { Field = field };
                            if (json.ValueKind != JsonValueKind.Null && json.ValueKind != JsonValueKind.Undefined)
                            {
                                var parsedFieldContent = JsonSerializer.Deserialize<JsonElement>(json.GetString());
                                fieldValue.FromJson(parsedFieldContent);
                            }
                            else
                            {
                                fieldValue.FromJson(new JsonElement());
                            }
                            fieldValue.IsArray = false;
                            return new Tuple<object, string>(fieldValue, propertyName);
                        }
                    default:
                        {
                            return new Tuple<object, string>(null, null);
                        }
                }
            }

            return new Tuple<object, string>(null, null);
        }

        private async static Task<IField> GetListItemFieldAsync(TransientObject pnpObject, string fieldName)
        {
            List parentList = null;

            if ((pnpObject as IDataModelParent).Parent.Parent is List)
            {
                parentList = (pnpObject as IDataModelParent).Parent.Parent as List;
            }
            else if ((pnpObject as IDataModelParent).Parent.Parent is ListItem)
            {
                // We're dealing with a ListItemVersion
                if ((pnpObject as IDataModelParent).Parent.Parent.Parent != null && (pnpObject as IDataModelParent).Parent.Parent.Parent.Parent != null)
                {
                    parentList = (pnpObject as IDataModelParent).Parent.Parent.Parent.Parent as List;
                }
            }
            else if (((pnpObject as IDataModelParent).Parent is File) || (pnpObject as IDataModelParent).Parent is Folder)
            {
                if ((pnpObject as ListItem).IsPropertyAvailable(p => p.ParentList))
                {
                    parentList = (List)(pnpObject as IListItem).ParentList;
                }
            }
            
            if (parentList != null)
            {
                if (!parentList.ArePropertiesAvailable(List.LoadFieldsExpression) && !parentList.Metadata.ContainsKey(PnPConstants.MetaDataRestId))
                {
                    //todo
                }
                else
                {
                    // Ensure the needed list fields data is loaded
                    await parentList.EnsurePropertiesAsync(List.LoadFieldsExpression).ConfigureAwait(false);

                    fieldName = PrepareFieldForLookup(fieldName);

                    return parentList.Fields.AsRequested().FirstOrDefault(p => p.InternalName == fieldName);
                }
            }

            return null;
        }

        private static async Task TryPopuplatePnPObjectMetadataFromRest(IDataModelWithContext contextAwareObject, IMetadataExtensible targetMetadataObject, EntityInfo entity, string idFieldValue)
        {
            // Store the rest ID field for follow-up rest requests
            if (!targetMetadataObject.Metadata.ContainsKey(PnPConstants.MetaDataRestId) && !string.IsNullOrEmpty(idFieldValue))
            {
                targetMetadataObject.Metadata.Add(PnPConstants.MetaDataRestId, idFieldValue);
            }

            // Store the SharePointType 
            if (!targetMetadataObject.Metadata.ContainsKey(PnPConstants.MetaDataType) && !string.IsNullOrEmpty(entity.SharePointType))
            {
                targetMetadataObject.Metadata.Add(PnPConstants.MetaDataType, entity.SharePointType);
            }

            // Store graph ID for transition to graph when needed. No point in doing this when we've disabled graph first behaviour or when the entity does not support rest + graph
            if (entity.SupportsGraphAndRest && !targetMetadataObject.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
            {

                // SP.Web requires a special id value
                if (entity.SharePointType.Equals("SP.Web"))
                {
                    if (!targetMetadataObject.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                    {
                        if (contextAwareObject.PnPContext.Site.IsPropertyAvailable(p => p.Id) && contextAwareObject.PnPContext.Web.IsPropertyAvailable(p => p.Id))
                        {
                            // Check again here due to the recursive nature of this code
                            if (!targetMetadataObject.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                            {
                                targetMetadataObject.Metadata.Add(PnPConstants.MetaDataGraphId,
                                    $"{contextAwareObject.PnPContext.Uri.DnsSafeHost},{contextAwareObject.PnPContext.Site.Id},{contextAwareObject.PnPContext.Web.Id}");
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(idFieldValue) && !targetMetadataObject.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                    {
                        targetMetadataObject.Metadata.Add(PnPConstants.MetaDataGraphId, idFieldValue);
                    }
                }
            }

            // Additional metadata population to enable the transition from Rest to Graph
            await targetMetadataObject.SetRestToGraphMetadataAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Maps JSON to model classes
        /// </summary>
        /// <param name="pnpObject">Model to populate from JSON</param>
        /// <param name="entity">Information about the current model</param>
        /// <param name="apiResponse">Response of the API call</param>
        /// <param name="fromJsonCasting">Delegate to be called for type conversion</param>
        /// <returns></returns>
        private static async Task<bool> FromJsonGraph(TransientObject pnpObject, EntityInfo entity, ApiResponse apiResponse, Func<FromJson, object> fromJsonCasting = null)
        {
            bool requested = false;

            // Get the type of the domain model we're populating
            var pnpObjectType = pnpObject.GetType();
            var metadataBasedObject = pnpObject as IMetadataExtensible;
            var contextAwareObject = pnpObject as IDataModelWithContext;

            // Set the batch request id property
            SetBatchRequestId(pnpObject, apiResponse.BatchRequestId);

            // Check if we somehow require pushing values into a generic dictionary
            TransientDictionary dictionaryPropertyToAddValueTo = null;
            bool useOverflowField = false;
            if (entity.UseOverflowField)
            {
                // Get the dictionary property that will hold the overflow data
                dictionaryPropertyToAddValueTo = pnpObjectType.GetProperty(ExpandoBaseDataModel<IExpandoDataModel>.OverflowFieldName).GetValue(pnpObject) as TransientDictionary;
                useOverflowField = true;
            }

            // variable to capture the id value
            string idFieldValue = null;

            // collect metadata
            Dictionary<string, string> metadata = new Dictionary<string, string>();

            if (apiResponse.JsonElement.ValueKind != JsonValueKind.Null)
            {
                // Enumerate the received properties and try to map them to the model
                foreach (var property in apiResponse.JsonElement.EnumerateObject())
                {
                    // Find the model field linked to this field
                    EntityFieldInfo entityField = null;

                    // Do we need to re-parent this json mapping to a non expandable collection in the current model?
                    bool modelReparented = false;
                    if (!string.IsNullOrEmpty(apiResponse.ApiCall.ReceivingProperty) && (property.NameEquals("value") || apiResponse.ApiCall.ReceivingProperty == "primaryChannel"))
                    {
                        entityField = entity.Fields.FirstOrDefault(p => !string.IsNullOrEmpty(p.Name) && p.Name.Equals(apiResponse.ApiCall.ReceivingProperty, StringComparison.InvariantCultureIgnoreCase));
                        if (entityField == null)
                        {
                            entityField = entity.Fields.FirstOrDefault(p => !string.IsNullOrEmpty(p.GraphName) && p.GraphName.Equals(apiResponse.ApiCall.ReceivingProperty, StringComparison.InvariantCultureIgnoreCase));
                        }
                        modelReparented = true;
                    }
                    else
                    {
                        entityField = LookupEntityField(entity, apiResponse, property);
                    }

                    // Entity field should be populate for the actual fields we've requested
                    if (entityField != null)
                    {
                        // Are we loading a collection (e.g. Team.Channels)?
                        if (IsModelCollection(entityField.PropertyInfo.PropertyType))
                        {
                            // Get the actual current value of the property we're setting...as that allows to detect it's type
                            var propertyToSetValue = entityField.PropertyInfo.GetValue(pnpObject);
                            // Cast object to call the needed methods on it (e.g. TeamChannelCollection)
                            var typedCollection = propertyToSetValue as IManageableCollection;
                            // Cast object to handle metadata on the collection
                            var typedMetaDataCollection = propertyToSetValue as IMetadataExtensible;

                            // copy over collected metadata to collection
                            if (metadata.Count > 0)
                            {
                                foreach (var data in metadata)
                                {
                                    if (typedMetaDataCollection.Metadata.ContainsKey(data.Key))
                                    {
                                        typedMetaDataCollection.Metadata[data.Key] = data.Value;
                                    }
                                    else
                                    {
                                        typedMetaDataCollection.Metadata.Add(data.Key, data.Value);
                                    }
                                }
                            }

                            // expanded objects are under the results property
                            PropertyInfo pnpChildIdProperty = null;
                            foreach (var childJson in property.Value.EnumerateArray())
                            {
                                // Create a new model instance to add to the collection
                                var pnpChild = typedCollection.CreateNew();

                                // Set the batch request id property
                                SetBatchRequestId(pnpChild as TransientObject, apiResponse.BatchRequestId);

                                // Recursively map properties, call the method from the actual object as the object could have overriden it
                                ApiResponse modelResponse;
                                if (modelReparented)
                                {
                                    // Clone the original API call so we can blank out the ReceivingProperty to avoid getting into an endless loop
                                    var newApiCall = new ApiCall(apiResponse.ApiCall.Request, apiResponse.ApiCall.Type, apiResponse.ApiCall.JsonBody)
                                    {
                                        Type = apiResponse.ApiCall.Type,
                                    };
                                    modelResponse = new ApiResponse(newApiCall, childJson, apiResponse.BatchRequestId);
                                }
                                else
                                {
                                    modelResponse = new ApiResponse(apiResponse.ApiCall, childJson, apiResponse.BatchRequestId);
                                }

                                await ((IDataModelProcess)pnpChild).ProcessResponseAsync(/*new ApiResponse(apiResponse.ApiCall, childJson, apiResponse.BatchRequestId)*/ modelResponse).ConfigureAwait(false);

                                // Check if we've marked a field to be the key field for the entities in this collection
                                if (pnpChildIdProperty == null)
                                {
                                    // Grab the child entity information, important to call this from the actual child entity
                                    var pnpChildEntityInfo = EntityManager.Instance.GetStaticClassInfo(pnpChild.GetType());
                                    // Was there a key defined on the child entity
                                    EntityFieldInfo keyField = pnpChildEntityInfo.GraphKeyField;
                                    if (keyField != null)
                                    {
                                        // There was a key, so that property should exist
                                        string key = keyField.Name;
                                        pnpChildIdProperty = pnpChild.GetType().GetProperty(key);
                                    }
                                }

                                // We do have a key property, use that to avoid loading duplicates
                                if (pnpChildIdProperty != null)
                                {
                                    var pnpChildIdPropertyValue = ParseStringValueToTyped(pnpChildIdProperty.GetValue(pnpChild).ToString(), pnpChildIdProperty.PropertyType);

                                    // Only add to collection when not yet added
                                    // or replace the already existing item, if any
                                    typedCollection.AddOrUpdate(pnpChild,
                                        i => ((IDataModelWithKey)i).Key.Equals(pnpChildIdPropertyValue));
                                }
                                else
                                {
                                    // No key checking...so let's just add
                                    typedCollection.Add(pnpChild);
                                }
                            }

                            // Set the collection as requested, if it is supported by
                            // the target type, because we've got the items
                            // from the JSON response, or an empty result set if the
                            // collection is empty
                            if (typedCollection.GetType().ImplementsInterface(typeof(IRequestableCollection)))
                            {
                                ((IRequestableCollection)typedCollection).Requested = true;
                            }

                            requested = true;
                        }
                        // Are we loading a another model type (e.g. Site.RootWeb)?
                        else if (IsModelType(entityField.PropertyInfo.PropertyType))
                        {
                            // Get instance of the model property
                            var propertyToSetValue = entityField.PropertyInfo.GetValue(pnpObject);

                            // Set the batch request id property
                            SetBatchRequestId(propertyToSetValue as TransientObject, apiResponse.BatchRequestId);

                            ApiResponse modelResponse;
                            if (modelReparented)
                            {
                                // Clone the original API call so we can blank out the ReceivingProperty to avoid getting into an endless loop
                                var newApiCall = new ApiCall(apiResponse.ApiCall.Request, apiResponse.ApiCall.Type, apiResponse.ApiCall.JsonBody)
                                {
                                    Type = apiResponse.ApiCall.Type,
                                };
                                modelResponse = new ApiResponse(newApiCall, apiResponse.JsonElement, apiResponse.BatchRequestId);
                            }
                            else
                            {
                                modelResponse = new ApiResponse(apiResponse.ApiCall, property.Value, apiResponse.BatchRequestId);
                            }

                            await ((IDataModelProcess)propertyToSetValue).ProcessResponseAsync(modelResponse).ConfigureAwait(false);

                            if (modelReparented)
                            {
                                // The full model was processed, no point in continueing as that would 
                                break;
                            }
                        }
                        else
                        {
                            // Regular field loaded

                            // Grab id field, should be present in all graph objects
                            if (string.IsNullOrEmpty(idFieldValue) && property.Name.Equals(entity.GraphId))
                            {

                                idFieldValue = GetJsonPropertyValue(property).ToString();
                            }

                            if (useOverflowField && entityField.Name.Equals(ExpandoBaseDataModel<IExpandoDataModel>.OverflowFieldName))
                            {
                                // overflow field
                                foreach (var overflowField in property.Value.EnumerateObject())
                                {
                                    // Let's keep track of the object metadata, useful when creating new requests
                                    if (overflowField.Name.StartsWith("@odata.", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        TrackMetaData(metadataBasedObject, overflowField, metadata);
                                    }
                                    else
                                    {
                                        // Add to overflow dictionary
                                        if (overflowField.Value.ValueKind == JsonValueKind.Object)
                                        {
                                            // Handling of complex type via calling out to custom handler, no value is returned as the custom
                                            // handler can update multiple fields based upon what json result came back
                                            fromJsonCasting?.Invoke(new FromJson(overflowField.Name, overflowField.Value, Type.GetType("System.Object"), contextAwareObject.PnPContext.Logger, apiResponse));
                                        }
                                        else
                                        {
                                            // Default mapping to dictionary can handle simple types, more complex types require custom logic
                                            AddToDictionary(dictionaryPropertyToAddValueTo, overflowField.Name, GetJsonPropertyValue(overflowField), pnpObject);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // do we have multiple json paths defined for this property?
                                if (!string.IsNullOrEmpty(entityField.GraphJsonPath))
                                {
                                    var jsonPathFields = entity.Fields.Where(p => !string.IsNullOrEmpty(p.GraphName) && p.GraphName.Equals(entityField.GraphName));
                                    if (jsonPathFields.Any())
                                    {
                                        foreach (var jsonPathField in jsonPathFields)
                                        {
                                            var jsonElement = GetJsonElementFromPath(property.Value, jsonPathField.GraphJsonPath);

                                            // Don't assume that the requested json path was also loaded. When using the QueryProperties model there can be 
                                            // a json object returned that does have all properties loaded (e.g. a TeamsApp object with only id and distributionMethod loaded)
                                            if (!jsonElement.Equals(property.Value))
                                            {
                                                jsonPathField.PropertyInfo?.SetValue(pnpObject, GetJsonFieldValue(contextAwareObject, jsonPathField.Name, apiResponse,
                                                    jsonElement, jsonPathField.DataType, jsonPathField.GraphUseCustomMapping, fromJsonCasting));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // regular field mapping
                                    entityField.PropertyInfo?.SetValue(pnpObject, GetJsonFieldValue(contextAwareObject, entityField.Name, apiResponse, property.Value, entityField.DataType, entityField.GraphUseCustomMapping, fromJsonCasting));
                                }
                            }

                            requested = true;
                        }
                    }
                    else
                    {
                        // Let's keep track of the object metadata, useful when creating new requests
                        if (property.Name.StartsWith("@odata.", StringComparison.InvariantCultureIgnoreCase))
                        {
                            TrackMetaData(metadataBasedObject, property, metadata);
                        }
                        else if (string.IsNullOrEmpty(idFieldValue) && property.Name.Equals(entity.GraphId))
                        {
                            idFieldValue = property.Value.GetString();
                        }
                    }
                }
            }
            else
            {
                // Seems nothing was returned...so nothing to load here
            }

            // Store metadata to enable transition to REST for follow-up requests
            await TryPopulatePnPObjectMetadataFromGraph(pnpObject, contextAwareObject, metadataBasedObject, entity, idFieldValue).ConfigureAwait(false);

            // Store the graph ID field for follow-up graph requests
            if (!metadataBasedObject.Metadata.ContainsKey(PnPConstants.MetaDataGraphId) && !string.IsNullOrEmpty(idFieldValue))
            {
                metadataBasedObject.Metadata.Add(PnPConstants.MetaDataGraphId, idFieldValue);
            }

            return requested;
        }

        private static async Task TryPopulatePnPObjectMetadataFromGraph(TransientObject pnpObject, IDataModelWithContext contextAwareObject, IMetadataExtensible targetMetadataObject, EntityInfo entity, string idFieldValue)
        {
            // If the SharePoint type of the entity is not known, metadata cannot be populated
            if (string.IsNullOrEmpty(entity.SharePointType))
            {
                return;
            }

            Dictionary<string, string> metadata = targetMetadataObject.Metadata;
            if (!metadata.ContainsKey(PnPConstants.MetaDataType))
            {
                metadata.Add(PnPConstants.MetaDataType, entity.SharePointType);
            }

            // Set the rest key metadata, the web and site model instances follow a specific path
            if (entity.SupportsGraphAndRest && !metadata.ContainsKey(PnPConstants.MetaDataRestId))
            {
                // SP.Web and SP.Site are special cases
                if (entity.SharePointType.Equals("SP.Web"))
                {
                    // Special case for web handling, grab web id from id string
                    // sample input: bertonline.sharepoint.com,cf1ed1cb-4a3c-43ed-bb3f-4ced4ce69ecf,1de385e4-e441-4448-8443-77680dfd845e
                    if (!string.IsNullOrEmpty(idFieldValue))
                    {
                        string id = idFieldValue.Split(new char[] { ',' })[2];
                        metadata.Add(PnPConstants.MetaDataRestId, id);
                        ((IDataModelWithKey)pnpObject).Key = Guid.Parse(id);
                    }
                }
                else if (entity.SharePointType.Equals("SP.Site"))
                {
                    // Special case for web handling, grab web id from id string
                    // sample input: bertonline.sharepoint.com,cf1ed1cb-4a3c-43ed-bb3f-4ced4ce69ecf,1de385e4-e441-4448-8443-77680dfd845e
                    if (!string.IsNullOrEmpty(idFieldValue))
                    {
                        string id = idFieldValue.Split(new char[] { ',' })[1];
                        metadata.Add(PnPConstants.MetaDataRestId, id);
                        ((IDataModelWithKey)pnpObject).Key = Guid.Parse(id);
                    }
                }
                else
                {
                    EntityFieldInfo id = null;
                    PropertyInfo idField = null;
                    id = entity.Fields.FirstOrDefault(p => p.IsSharePointKey);
                    if (id != null)
                    {
                        idField = pnpObject.GetType().GetProperty(id.Name);

                        // Get key property value
                        var keyFieldValue = idField.GetValue(pnpObject);
                        metadata.Add(PnPConstants.MetaDataRestId, keyFieldValue.ToString());
                    }
                }
            }
            if (!metadata.ContainsKey(PnPConstants.MetaDataUri))
            {
                var parsedApiCall = await ApiHelper.ParseApiRequestAsync(targetMetadataObject, $"{contextAwareObject.PnPContext.Uri.AbsoluteUri.ToString().TrimEnd(new char[] { '/' })}/{entity.SharePointUri}").ConfigureAwait(false);
                metadata.Add(PnPConstants.MetaDataUri, parsedApiCall);
            }

            // Additional metadata population to enable the transition from Graph to Rest
            await targetMetadataObject.SetGraphToRestMetadataAsync().ConfigureAwait(false);
        }

        internal static object GetJsonPropertyValue(JsonProperty property)
        {
            /*
            "Number1": 67687,
            "DateTime1": "2020-12-03T08:00:00Z",
            "Currency1": 67.67
            */
            if (property.Value.ValueKind == JsonValueKind.Number)
            {
                if (property.Value.TryGetInt32(out int int32Value))
                {
                    return int32Value;
                }
                else if (property.Value.TryGetInt64(out long int64Value))
                {
                    return int64Value;
                }
                else
                {
                    return property.Value.GetDouble();
                }
            }

            if (property.Value.ValueKind == JsonValueKind.String)
            {
                // SharePoint REST/Graph date formats, assume length of 10 or more before trying to parse to avoid getting strings like "1.1" being parsed as date (issue 519)
                // This is fix will still parse text fields with date formatted content to datetime, would be better if this code path understood the type of the field it's
                // parsing data for, but that's a costly thing to implement
                if (property.Value.GetString() != null && property.Value.GetString().Length >= 10 && DateTime.TryParse(property.Value.GetString(), out DateTime foundDate2))
                {
                    return foundDate2;
                }
            }

            return property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => null,
            };
        }

        internal static JsonElement GetJsonElementFromPath(JsonElement root, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return root;
            }

            var jsonPathTree = path.Split(new char[] { '.' });

            JsonElement element = root;
            foreach (var part in jsonPathTree)
            {
                // Won't support multi-dimensions arrays
                Match indexMatch = arrayMatchingRegex.Match(part);
                if (indexMatch.Success)
                {
                    if (int.TryParse(indexMatch.Groups["index"].Value, out int index))
                    {
                        string[] subParts = part.Split(new char[] { '[' });
                        string currentPropertyName = subParts[0];
                        if (element.TryGetProperty(currentPropertyName, out JsonElement nextElement))
                        {
                            element = nextElement;
                        }

                        if (element.ValueKind == JsonValueKind.Array)
                        {
                            int arrayLength = element.GetArrayLength();
                            if (arrayLength == 0)
                            {
                                throw new IndexOutOfRangeException(PnPCoreResources.Exception_Json_EmptyArray);
                            }

                            if (index > arrayLength - 1)
                            {
                                throw new IndexOutOfRangeException(PnPCoreResources.Exception_Json_ArrayOutOfBoundaries);
                            }

                            int currentIndex = 0;
                            foreach (var arrayItem in element.EnumerateArray())
                            {
                                if (currentIndex == index)
                                {
                                    element = arrayItem;
                                    break;
                                }
                                currentIndex++;
                            }
                        }
                    }
                }
                else if (element.ValueKind != JsonValueKind.Null &&
                    element.TryGetProperty(part, out JsonElement nextElement))
                {
                    element = nextElement;
                }
            }

            return element;
        }

        private static void SetBatchRequestId(TransientObject pnpObject, Guid batchRequestId)
        {
            pnpObject.BatchRequestId = batchRequestId;
        }

        private static EntityFieldInfo LookupEntityField(EntityInfo entity, ApiResponse apiResponse, JsonProperty property)
        {
            EntityFieldInfo entityField = null;
            if (apiResponse.ApiCall.Type == ApiType.SPORest)
            {
                // Changed to case insensitive because when loading data via DataStream, the ID field comes back not as "Id", but as "ID"
                entityField = entity.Fields.FirstOrDefault(p => !string.IsNullOrEmpty(p.SharePointName) && p.SharePointName.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));

                // Checking on ValueKind as it's possible there's a regular field (e.g. column named Comments in a list item) that can have the same name
                // as an expandable collection (e.g. Comments collection on IListItem)
                if (entityField != null && IsModelCollection(entityField.DataType) && property.Value.ValueKind != JsonValueKind.Array && apiResponse.ApiCall.Type == ApiType.SPORest)
                {
                    return null;
                }
            }
            else if (apiResponse.ApiCall.Type == ApiType.Graph || apiResponse.ApiCall.Type == ApiType.GraphBeta)
            {
                // Use case insensitive match for Graph as we get json in camel case while we use pascal case for the model
                entityField = entity.Fields.FirstOrDefault(p => !string.IsNullOrEmpty(p.GraphName) && p.GraphName.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));
            }

            return entityField;
        }

        private static string PrepareFieldForLookup(string fieldName)
        {
            // Handle the OData__ case: SPO REST will replace the _ in field names that start with an _ by OData__
            // This will pose compatability issues as other data retrieval methods don't do this, hence
            // we're normalizing the fieldnames again 

            if (fieldName.StartsWith("OData__"))
            {
                fieldName = fieldName.Replace("OData__", "_");
            }

            if (fieldName.Contains("_x005f_"))
            {
                fieldName = fieldName.Replace("_x005f_", "_");
            }

            return fieldName;
        }

        private static void AddToDictionary(TransientDictionary dictionary, string key, object value, TransientObject pnpObject)
        {
            // Handle the OData__ case: SPO REST will replace the _ in field names that start with an _ by OData__
            // This will pose compatability issues as other data retrieval methods don't do this, hence
            // we're normalizing the fieldnames again 

            if (key.StartsWith("OData__"))
            {
                key = key.Replace("OData__", "_");
            }

            // Only normalize in case of propertyvalues, not in case of ListItem properties
            if (pnpObject is PropertyValues)
            {
                if (key.Contains("_x005f_"))
                {
                    key = key.Replace("_x005f_", "_");
                }

                if (key.Contains("_x0020_"))
                {
                    key = key.Replace("_x0020_", " ");
                }

                if (key.Contains("_x002d_"))
                {
                    key = key.Replace("_x002d_", "-");
                }

                if (key.Contains("_x002e_"))
                {
                    key = key.Replace("_x002e_", ".");
                }

                if (key.Contains("_x002f_"))
                {
                    key = key.Replace("_x002f_", "/");
                }

                if (key.Contains("_x003a_"))
                {
                    key = key.Replace("_x003a_", ":");
                }

                if (key.Contains("_x003c_"))
                {
                    key = key.Replace("_x003c_", "<");
                }

                if (key.Contains("_x003e_"))
                {
                    key = key.Replace("_x003e_", ">");
                }

                if (key.Contains("_x007c_"))
                {
                    key = key.Replace("_x007c_", "|");
                }

                if (key.Contains("_x005b_"))
                {
                    key = key.Replace("_x005b_", "[");
                }

                if (key.Contains("_x005d_"))
                {
                    key = key.Replace("_x005d_", "]");
                }
            }

            if (!dictionary.ContainsKey(key))
            {
                dictionary.SystemAdd(key, value);
            }
        }

        internal static object ParseStringValueToTyped(string input, Type propertyType)
        {
            switch (propertyType.FullName)
            {
                case "System.String":
                    {
                        return input;
                    }
                case "System.Boolean":
                    {
                        return Boolean.Parse(input);
                    }
                case "System.Guid":
                    {
                        return Guid.Parse(input);
                    }
                case "System.Int16":
                    {
                        return short.Parse(input, CultureInfo.InvariantCulture);
                    }
                case "System.Int32":
                    {
                        return int.Parse(input, CultureInfo.InvariantCulture);
                    }
                case "System.Int64":
                    {
                        return long.Parse(input, CultureInfo.InvariantCulture);
                    }
                case "System.UInt16":
                    {
                        return ushort.Parse(input, CultureInfo.InvariantCulture);
                    }
                case "System.UInt32":
                    {
                        return uint.Parse(input, CultureInfo.InvariantCulture);
                    }
                case "System.UInt64":
                    {
                        return ulong.Parse(input, CultureInfo.InvariantCulture);
                    }
                case "System.Uri":
                    {
                        if (!string.IsNullOrEmpty(input))
                        {
                            if (Uri.TryCreate(input, UriKind.RelativeOrAbsolute, out Uri result))
                            {
                                return result;
                            }
                        }
                        return null;
                    }
                default:
                    {
                        return input;
                    }
            }
        }

        internal static object GetJsonFieldValue(IDataModelWithContext pnpObject, string fieldName, ApiResponse apiResponse, JsonElement jsonElement, Type propertyType, bool useCustomMapping, Func<FromJson, object> fromJsonCasting)
        {
            // If a field mandates a custom mapping then the field handling is fully handled via the custom mapping handler
            if (useCustomMapping)
            {
                return fromJsonCasting?.Invoke(new FromJson(fieldName, jsonElement, propertyType, pnpObject?.PnPContext.Logger, apiResponse));
            }

            if (propertyType.IsEnum)
            {
                if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt64(out long enumNumericValue))
                {
                    return Enum.Parse(propertyType, enumNumericValue.ToString());
                }
                else if (jsonElement.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(jsonElement.GetString()))
                {
                    return Enum.Parse(propertyType, jsonElement.GetString(), true);
                }
                else
                {
                    // Return the default value
                    return Enum.Parse(propertyType, "0");
                }
            }
            else
            {

                switch (propertyType.Name)
                {
                    case "String":
                        {
                            return jsonElement.GetString();
                        }
                    case "Object":
                        {
                            return jsonElement.ToString();
                        }
                    case "String[]":
                        {
                            if (jsonElement.ValueKind != JsonValueKind.Array)
                            {
                                return Array.Empty<string>();
                            }
                            else
                            {
                                return jsonElement.EnumerateArray().Select(item => item.GetString()).ToArray();
                            }
                        }
                    case "Dictionary`2":
                        {
                            var dictionary = new Dictionary<string, object>();
                            foreach (var element in jsonElement.EnumerateArray())
                            {
                                var value = element.GetProperty("Value");
                                var key = element.GetProperty("Key").GetString();

                                switch (value.ValueKind)
                                {
                                    case JsonValueKind.String:
                                        dictionary.Add(key, value.GetString());
                                        break;
                                    case JsonValueKind.Number:
                                        dictionary.Add(key, value.GetInt32());
                                        break;
                                    case JsonValueKind.False:
                                    case JsonValueKind.True:
                                        dictionary.Add(key, value.GetBoolean());
                                        break;

                                }
                            }

                            return dictionary;
                        }
                    case "List`1":
                        {
                            if (jsonElement.ValueKind != JsonValueKind.Array)
                            {
                                return null;
                            }
                            else
                            {
                                // Generic list does have a type, we support the types that can be delivered by the parser
                                switch (propertyType.GenericTypeArguments[0].Name)
                                {
                                    case "String":
                                        {
                                            return jsonElement.EnumerateArray().Select(item => item.GetString()).ToList();
                                        }
                                    case "Int32":
                                        {
                                            return jsonElement.EnumerateArray().Select(item => item.GetInt32()).ToList();
                                        }
                                    case "Int16":
                                        {
                                            return jsonElement.EnumerateArray().Select(item => item.GetInt16()).ToList();
                                        }
                                    case "Int64":
                                        {
                                            return jsonElement.EnumerateArray().Select(item => item.GetInt64()).ToList();
                                        }
                                    case "UInt32":
                                        {
                                            return jsonElement.EnumerateArray().Select(item => item.GetUInt32()).ToList();
                                        }
                                    case "UInt16":
                                        {
                                            return jsonElement.EnumerateArray().Select(item => item.GetUInt16()).ToList();
                                        }
                                    case "UInt64":
                                        {
                                            return jsonElement.EnumerateArray().Select(item => item.GetUInt64()).ToList();
                                        }
                                    case "Double":
                                        {
                                            return jsonElement.EnumerateArray().Select(item => item.GetDouble()).ToList();
                                        }
                                    case "Guid":
                                        {
                                            return jsonElement.EnumerateArray().Select(item => item.GetGuid()).ToList();
                                        }
                                    case "Boolean":
                                        {
                                            return jsonElement.EnumerateArray().Select(item => item.GetBoolean()).ToList();
                                        }
                                    case "DateTime":
                                        {
                                            return jsonElement.EnumerateArray().Select(item => item.GetDateTime()).ToList();
                                        }
                                    case "DateTimeOffset":
                                        {
                                            return jsonElement.EnumerateArray().Select(item => item.GetDateTimeOffset()).ToList();
                                        }
                                    default:
                                        {
                                            return jsonElement.EnumerateArray().Select(item => item.GetString()).ToList();
                                        }
                                }
                            }
                        }
                    case "Boolean":
                        {
                            if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
                            {
                                return jsonElement.GetBoolean();
                            }
                            else if (jsonElement.ValueKind == JsonValueKind.String)
                            {
                                if (bool.TryParse(jsonElement.GetString(), out bool parsedBool))
                                {
                                    return parsedBool;
                                }
                            }
                            else if (jsonElement.ValueKind == JsonValueKind.Number)
                            {
                                var number = jsonElement.GetInt32();

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
                    case "Guid":
                        {
                            // Overriding parsing instead of using .GetGuid() because
                            // some formats (with squirrely braces) throw an exception (e.g. - "{00000000-0000-0000-0000-000000000000}")
                            var guidString = jsonElement.ToString();
                            if (!string.IsNullOrEmpty(guidString))
                            {
                                return Guid.Parse(guidString);
                            }
                            else
                            {
                                return Guid.Empty;
                            }
                        }
                    case "Int16":
                        {
                            if (jsonElement.ValueKind != JsonValueKind.Number)
                            {
                                // Override parsing in case it's not a number, assume string
                                if (short.TryParse(jsonElement.GetString(), out short shortValue))
                                {
                                    return shortValue;
                                }
                                else
                                {
                                    return 0;
                                }
                            }
                            else
                            {
                                return jsonElement.GetInt16();
                            }
                        }
                    case "Int32":
                        {
                            if (jsonElement.ValueKind != JsonValueKind.Number)
                            {
                                // Override parsing in case it's not a number, assume string
                                if (int.TryParse(jsonElement.GetString(), out int intValue))
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
                                return jsonElement.GetInt32();
                            }
                        }
                    case "Int64":
                        {
                            if (jsonElement.ValueKind != JsonValueKind.Number)
                            {
                                // Override parsing in case it's not a number, assume string
                                if (long.TryParse(jsonElement.GetString(), out long longValue))
                                {
                                    return longValue;
                                }
                                else
                                {
                                    return 0;
                                }
                            }
                            else
                            {
                                return jsonElement.GetInt64();
                            }
                        }
                    case "UInt16":
                        {
                            if (jsonElement.ValueKind != JsonValueKind.Number)
                            {
                                // Override parsing in case it's not a number, assume string
                                if (short.TryParse(jsonElement.GetString(), out short shortValue))
                                {
                                    return shortValue;
                                }
                                else
                                {
                                    return 0;
                                }
                            }
                            else
                            {
                                return jsonElement.GetUInt16();
                            }
                        }
                    case "UInt32":
                        {
                            if (jsonElement.ValueKind != JsonValueKind.Number)
                            {
                                // Override parsing in case it's not a number, assume string
                                if (int.TryParse(jsonElement.GetString(), out int intValue))
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
                                return jsonElement.GetUInt32();
                            }
                        }
                    case "UInt64":
                        {
                            if (jsonElement.ValueKind != JsonValueKind.Number)
                            {
                                // Override parsing in case it's not a number, assume string
                                if (long.TryParse(jsonElement.GetString(), out long longValue))
                                {
                                    return longValue;
                                }
                                else
                                {
                                    return 0;
                                }
                            }
                            else
                            {
                                return jsonElement.GetUInt64();
                            }
                        }
                    case "Double":
                        {
                            if (jsonElement.ValueKind != JsonValueKind.Number)
                            {
                                if (double.TryParse(jsonElement.GetString(), out double doubleValue))
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
                                return jsonElement.GetDouble();
                            }
                        }
                    case "Uri":
                        {
                            var s = jsonElement.GetString();
                            if (!string.IsNullOrEmpty(s))
                            {
                                if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out Uri result))
                                {
                                    return result;
                                }
                            }
                            return null;
                        }
                    case "DateTime":
                        {
                            if (jsonElement.ValueKind != JsonValueKind.Null)
                            {
                                return jsonElement.GetDateTime();
                            }
                            else
                            {
                                return null;
                            }
                        }
                    case "DateTimeOffset":
                        {
                            if (jsonElement.ValueKind != JsonValueKind.Null)
                            {
                                return jsonElement.GetDateTimeOffset();
                            }
                            else
                            {
                                return null;
                            }
                        }
                    default:
                        {
                            // Do a call back for the cases where we don't have a default mapping
                            return fromJsonCasting?.Invoke(new FromJson(fieldName, jsonElement, propertyType, pnpObject?.PnPContext.Logger, apiResponse));
                        }
                }
            }
        }

        internal static string GetRestField(EntityFieldInfo field)
        {
            if (!string.IsNullOrEmpty(field.SharePointName))
            {
                return field.SharePointName;
            }
            else
            {
                return field.Name;
            }
        }

        internal static string GetGraphField(EntityFieldInfo field)
        {
            if (!string.IsNullOrEmpty(field.GraphName))
            {
                return field.GraphName;
            }

            throw new ClientException(ErrorType.ModelMetadataIncorrect,
                PnPCoreResources.Exception_ModelMetadataIncorrect_MissingGraphName);
        }

        internal static bool IsModelType(Type propertyType)
        {
            return propertyType.ImplementsInterface(typeof(IDataModel<>));
        }

        internal static bool IsTypeWithoutGet(Type propertyType)
        {
            return propertyType.ImplementsInterface(typeof(IDataModel<>)) && !propertyType.ImplementsInterface(typeof(IDataModelGet<>));
        }

        internal static bool IsModelCollection(Type propertyType)
        {
            return propertyType.ImplementsInterface(typeof(IDataModelCollection<>));
        }

        internal static void TrackAndUpdateMetaData(IMetadataExtensible target, JsonProperty property)
        {
            TrackAndUpdateMetaData(target, property.Name, GetJsonPropertyValue(property).ToString());
        }

        internal static void TrackAndUpdateMetaData(IMetadataExtensible target, string propertyName, string propertyValue)
        {
            if (!target.Metadata.ContainsKey(propertyName))
            {
                target.Metadata.Add(propertyName, propertyValue);
            }
            else
            {
                target.Metadata[propertyName] = propertyValue;
            }
        }

        internal static void TrackMetaData(IMetadataExtensible target, JsonProperty property, Dictionary<string, string> metadata)
        {
            if (!target.Metadata.ContainsKey(property.Name))
            {
                target.Metadata.Add(property.Name, GetJsonPropertyValue(property).ToString());
            }

            if (metadata != null && !metadata.ContainsKey(property.Name))
            {
                metadata.Add(property.Name, GetJsonPropertyValue(property).ToString());
            }
        }

        internal static void TrackSharePointMetaData(IMetadataExtensible target, JsonProperty property)
        {
            AddSharePointMetaDataProperty(target, property.Value, PnPConstants.MetaDataUri);
            AddSharePointMetaDataProperty(target, property.Value, PnPConstants.MetaDataId);
            AddSharePointMetaDataProperty(target, property.Value, PnPConstants.MetaDataType);
        }

        internal static void AddSharePointMetaDataProperty(IMetadataExtensible target, JsonElement jsonElement, string propertyName)
        {
            if (jsonElement.TryGetProperty(propertyName, out JsonElement foundProperty))
            {
                if (!target.Metadata.ContainsKey(propertyName))
                {
                    target.Metadata.Add(propertyName, foundProperty.GetString());
                }
            }
        }
    }
}
