using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

            // Json parsing options
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            };

            // Parse the received json content
            using (JsonDocument document = JsonDocument.Parse(batchRequest.ResponseJson, options))
            {
                // for SharePoint REST calls the root property is d, for recursive calls this is not the case
                if (!document.RootElement.TryGetProperty("d", out JsonElement root))
                {
                    root = document.RootElement;
                }

                // TODO: The below method's signature is quite complex and reuses the same object
                // multiple times. Can we try to simplify it?

                // Map the returned JSON to the respective entities
                await FromJson(batchRequest.Model, batchRequest.EntityInfo, new ApiResponse(batchRequest.ApiCall, root, batchRequest.Id), batchRequest.FromJsonCasting).ConfigureAwait(false);
                batchRequest.PostMappingJson?.Invoke(batchRequest.ResponseJson);
            }
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

                // hack - skip using overflow for IListItem
                //if (!pnpObjectType.ImplementsInterface(typeof(IListItem)))
                //{
                //    useOverflowField = true;
                //}      

                useOverflowField = true;
            }

            // variable to capture the id value
            string idFieldValue = null;

            // collect metadata
            Dictionary<string, string> metadata = new Dictionary<string, string>();

            // Enumerate the received properties and try to map them to the model
            foreach (var property in apiResponse.JsonElement.EnumerateObject())
            {
                // Find the model field linked to this field
                EntityFieldInfo entityField = LookupEntityField(entity, apiResponse, property);

                // Do we need to re-parent this json mapping to a non expandable collection in the current model?
                if (!string.IsNullOrEmpty(apiResponse.ApiCall.ReceivingProperty) && property.NameEquals("results"))
                {
                    entityField = entity.Fields.FirstOrDefault(p => !string.IsNullOrEmpty(p.SharePointName) && p.SharePointName.Equals(apiResponse.ApiCall.ReceivingProperty, StringComparison.InvariantCultureIgnoreCase));
                }

                // Entity field should be populated for the actual fields we've requested
                if (entityField != null)
                {
                    // Are we loading a collection (e.g. Web.Lists)?
                    if ((!useOverflowField && IsModelCollection(entityField.PropertyInfo.PropertyType)) || pnpObjectType.ImplementsInterface(typeof(IListItem)) && IsModelCollection(entityField.PropertyInfo.PropertyType))
                    {
                        // Get the actual current value of the property we're setting...as that allows to detect it's type
                        var propertyToSetValue = entityField.PropertyInfo.GetValue(pnpObject);
                        // Cast object to call the needed methods on it (e.g. ListCollection)
                        var typedCollection = propertyToSetValue as IManageableCollection;

                        // Try to get the results property, start with a default value
                        JsonElement resultsProperty = default;

                        // If the property is named "results" and is of type Array, it means it is a collection of items
                        if (property.Name == "results" && property.Value.ValueKind == JsonValueKind.Array)
                        {
                            // and we use it directly
                            resultsProperty = property.Value;
                        }
                        else
                        {
                            // otherwise we try to get the child property called "results", if any
                            property.Value.TryGetProperty("results", out resultsProperty);
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

                                var contextAwarePnPChild = pnpChild as IDataModelWithContext;

                                // TODO: In CreateNew (line 163) we already configure the PnPContext
                                // Do we really need code in line 174?

                                // Set PnPContext via a dynamic property
                                contextAwarePnPChild.PnPContext = contextAwareObject.PnPContext;

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
                        else if (property.Value.TryGetProperty("__deferred", out JsonElement deferredProperty))
                        {
                            // Let's keep track of these "pointers" to load additional data, no actual usage at this point yet though

                            // __deferred property
                            //"__deferred": {
                            //    "uri": "https://bertonline.sharepoint.com/sites/modern/_api/site/RootWeb/WorkflowAssociations"
                            //}

                            if (!metadataBasedObject.Metadata.ContainsKey(entityField.Name))
                            {
                                metadataBasedObject.Metadata.Add(entityField.Name, deferredProperty.GetProperty("uri").GetString());
                            }
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
                        if (string.IsNullOrEmpty(idFieldValue) && property.Name.Equals(entity.SharePointKeyField.Name))
                        {
                            idFieldValue = GetJsonPropertyValue(property).ToString();
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
                                        jsonPathField.PropertyInfo?.SetValue(pnpObject, GetJsonFieldValue(contextAwareObject, jsonPathField.Name,
                                            jsonElement, jsonPathField.DataType, jsonPathField.SharePointUseCustomMapping, fromJsonCasting));
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Set the object property value taken from the JSON payload
                            entityField.PropertyInfo?.SetValue(pnpObject, GetJsonFieldValue(contextAwareObject, entityField.Name,
                                property.Value, entityField.DataType, entityField.SharePointUseCustomMapping, fromJsonCasting));
                        }
                        requested = true;
                    }
                }
                else
                {
                    // Let's keep track of the object metadata, useful when creating new requests
                    if (property.Name == "__metadata")
                    {
                        TrackSharePointMetaData(metadataBasedObject, property);
                    }
                    // Let's also store: 
                    // - the EntityTypeName value as metadata as it's usefull for constructing future rest calls
                    // - the __next link, used in (list item) paging
                    else if (property.Name == "EntityTypeName")
                    {
                        TrackMetaData(metadataBasedObject, property, metadata);
                    }
                    else if (property.Name == "__next")
                    {
                        // Only applies to paging on listitems
                        if (metadataBasedObject is List)
                        {
                            var listItemCollection = (metadataBasedObject as List).Items;
                            if (listItemCollection != null && listItemCollection.Requested)
                            {
                                TrackAndUpdateMetaData(listItemCollection as IMetadataExtensible, property);
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
                        if (!(property.Value.ValueKind == JsonValueKind.Object && property.Value.TryGetProperty("__deferred", out JsonElement deferredProperty)))
                        {

                            // Verify if we can map the received json to a supported field type
                            (object fieldValue, string fieldName) = ProcessSpecialRestFieldType(pnpObject, property.Name, property.Value);
                            if (fieldName != null)
                            {                                
                                AddToDictionary(dictionaryPropertyToAddValueTo, fieldName, fieldValue);
                            }
                            else
                            {
                                if (property.Value.ValueKind == JsonValueKind.Object)
                                {
                                    // Handling of complex type via calling out to custom handler, no value is returned as the custom
                                    // handler can update multiple fields based upon what json result came back
                                    fromJsonCasting?.Invoke(new FromJson(property.Name, property.Value, Type.GetType("System.Object"), contextAwareObject.PnPContext.Logger));
                                }
                                else
                                {
                                    // Default mapping to dictionary can handle simple types, more complex types require custom logic
                                    AddToDictionary(dictionaryPropertyToAddValueTo, property.Name, GetJsonPropertyValue(property));
                                }
                            }
                            requested = true;
                        }
                    }
                }
            }

            // Ensure all changes are reset (as setting Title on the ListItem will trigger a change)
            if (useOverflowField)
            {
                dictionaryPropertyToAddValueTo.RemoveTitleFieldChange();
            }

            // Try populate the PnP Object metadata from the mapped info
            await TryPopuplatePnPObjectMetadataFromRest(contextAwareObject, metadataBasedObject, entity, idFieldValue).ConfigureAwait(false);

            // Store __next link for paging in case we detected a $top in the issued query
            if (apiResponse.ApiCall.Request.Contains("$top", StringComparison.InvariantCultureIgnoreCase))
            {
                var parent = (pnpObject as IDataModelParent).Parent;
                if (parent != null && parent is IManageableCollection && parent is IMetadataExtensible && parent.GetType().ImplementsInterface(typeof(ISupportPaging)))
                {
                    TrackAndUpdateMetaData(parent as IMetadataExtensible, "__next", BuildNextPageRestUrl(apiResponse.ApiCall.Request));
                }
            }

            return requested;
        }

        private static Tuple<object, string> ProcessSpecialRestFieldType(TransientObject pnpObject, string propertyName, JsonElement json)
        {
            // This special processing only applies to list items, property bags are excluded
            if (pnpObject.GetType() != typeof(ListItem))
            {
                return new Tuple<object, string>(null, null);
            }

            if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty(PnPConstants.SharePointRestMetadata, out JsonElement metadata) && metadata.TryGetProperty(PnPConstants.MetaDataType, out JsonElement type))
            {

                switch (type.GetString())
                {
                    case "SP.FieldUrlValue": 
                        {                            
                            var fieldValue = new FieldUrlValue() {  Field = GetListItemField(pnpObject, propertyName) };
                            fieldValue.FromJson(json);
                            fieldValue.IsArray = false;
                            return new Tuple<object, string>(fieldValue, propertyName);
                        };
                    case "SP.Taxonomy.TaxonomyFieldValue":
                        {
                            #region Sample json
                            /*
                              "MMSingle": {
                                "__metadata": {
                                  "type": "SP.Taxonomy.TaxonomyFieldValue"
                                },
                                "Label": "1",
                                "TermGuid": "ed5449ec-4a4f-4102-8f07-5a207c438571",
                                "WssId": 1
                              },
                            */
                            #endregion

                            var fieldValue = new FieldTaxonomyValue() { Field = GetListItemField(pnpObject, propertyName) };
                            fieldValue.FromJson(json);
                            fieldValue.IsArray = false;
                            return new Tuple<object, string>(fieldValue, propertyName);
                        }
                    case "Collection(SP.Taxonomy.TaxonomyFieldValue)":
                        {
                            #region Sample json
                            /*
                                "MMMultiple": {
                                "__metadata": {
                                    "type": "Collection(SP.Taxonomy.TaxonomyFieldValue)"
                                },
                                "results": [
                                    {
                                    "Label": "LBI",
                                    "TermGuid": "ed5449ec-4a4f-4102-8f07-5a207c438571",
                                    "WssId": 1
                                    },
                                    {
                                    "Label": "MBI",
                                    "TermGuid": "1824510b-00e1-40ac-8294-528b1c9421e0",
                                    "WssId": 2
                                    },
                                    {
                                    "Label": "HBI",
                                    "TermGuid": "0b709a34-a74e-4d07-b493-48041424a917",
                                    "WssId": 3
                                    }
                                ]
                                },
                            */
                            #endregion

                            if (json.TryGetProperty("results", out JsonElement results))
                            {
                                var field = GetListItemField(pnpObject, propertyName);
                                var values = new FieldValueCollection(field, propertyName);
                                if (results.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var term in results.EnumerateArray())
                                    {
                                        var fieldValue = new FieldTaxonomyValue() { Field = field };
                                        fieldValue.FromJson(term);
                                        fieldValue.IsArray = true;
                                        values.Values.Add(fieldValue);
                                    }
                                }
                                return new Tuple<object, string>(values, propertyName);
                            }

                            return new Tuple<object, string>(null, null);
                        }
                    case "Collection(Edm.Int32)":
                        {
                            #region Sample json
                            /*
                              "LookupMultipleId": {
                                "__metadata": {
                                  "type": "Collection(Edm.Int32)"
                                },
                                "results": [
                                  1,
                                  71
                                ]
                              },

                              "PersonMultipleId": {
                                "__metadata": {
                                  "type": "Collection(Edm.Int32)"
                                },
                                "results": [
                                  14,
                                  6
                                ]
                              },
                             */
                            #endregion

                            if (json.TryGetProperty("results", out JsonElement results))
                            {
                                string actualPropertyName = propertyName.Substring(0, propertyName.Length - 2);
                                var field = GetListItemField(pnpObject, actualPropertyName);
                                var values = new FieldValueCollection(field, actualPropertyName);
                                if (results.ValueKind == JsonValueKind.Array)
                                {                                    
                                    foreach (var lookupId in results.EnumerateArray())
                                    {
                                        FieldLookupValue fieldValue;
                                        if (field != null)
                                        {
                                            if (field.TypeAsString == "UserMulti")
                                            {
                                                fieldValue = new FieldUserValue() { Field = field };
                                            }
                                            else
                                            {
                                                fieldValue = new FieldLookupValue() { Field = field };
                                            }
                                        }
                                        else
                                        {
                                            // No field found...assume lookup value
                                            fieldValue = new FieldLookupValue() { Field = field };
                                        }

                                        fieldValue.FromJson(lookupId);
                                        fieldValue.IsArray = true;
                                        values.Values.Add(fieldValue);
                                    }
                                }
                                return new Tuple<object, string>(values, actualPropertyName);
                            }

                            return new Tuple<object, string>(null, null);
                        }
                    case "Collection(Edm.String)":
                        {
                            #region Sample json
                            /*
                              "ChoiceMultiple": {
                                "__metadata": {
                                  "type": "Collection(Edm.String)"
                                },
                                "results": [
                                  "Choice 1",
                                  "Choice 3",
                                  "Choice 4"
                                ]
                              }
                             */
                            #endregion

                            // PersonMulitple fields are also listed under the same type, but have StringId appended to their name
                            if (!(propertyName.EndsWith("StringId") && propertyName.Length > 8) && json.TryGetProperty("results", out JsonElement results))
                            {
                                var values = new List<string>();
                                if (results.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var choice in results.EnumerateArray())
                                    {
                                        values.Add(choice.GetString());
                                    }
                                }
                                return new Tuple<object, string>(values, propertyName);
                            }

                            return new Tuple<object, string>(null, null);
                        }
                    default:
                        {
                            return new Tuple<object, string>(null, null);
                        }
                }
            }
            else
            {
                if (propertyName.EndsWith("StringId") && propertyName.Length > 8)
                {
                    #region Sample json
                    /*
                     "LookupSingleField1Id": 1
                     "PersonSingleId": 6,
                     "PersonSingleStringId": "6",
                     "UserSingleField1Id": null,
                     "UserSingleField1StringId": null,
                     "UserMultiField1Id": null,
                     "UserMultiField1StringId": null,
                     */
                    #endregion

                    string actualPropertyName = propertyName.Substring(0, propertyName.Length - 8);
                    var field = GetListItemField(pnpObject, actualPropertyName);
                    if (json.ValueKind == JsonValueKind.Null && field != null && field.TypeAsString == "UserMulti")
                    {
                        // Add empty value collection
                        var values = new FieldValueCollection(field, actualPropertyName);
                        return new Tuple<object, string>(values, actualPropertyName);
                    }
                    else
                    {
                        var fieldValue = new FieldUserValue() { Field = GetListItemField(pnpObject, actualPropertyName) };
                        fieldValue.FromJson(json);
                        fieldValue.IsArray = false;
                        return new Tuple<object, string>(fieldValue, actualPropertyName);
                    }
                }
                else if (propertyName.EndsWith("Id") && propertyName.Length > 2)
                {
                    string actualPropertyName = propertyName.Substring(0, propertyName.Length - 2);
                    var field = GetListItemField(pnpObject, actualPropertyName);
                    if (field != null && field.TypeAsString == "Lookup")
                    {
                        if (json.ValueKind == JsonValueKind.Null)
                        {
                            return new Tuple<object, string>(null, actualPropertyName);
                        }
                        else
                        {
                            var fieldValue = new FieldLookupValue() { Field = field };
                            fieldValue.FromJson(json);
                            fieldValue.IsArray = false;
                            return new Tuple<object, string>(fieldValue, actualPropertyName);
                        }
                    }
                }
                else if (json.ValueKind == JsonValueKind.String && (json.GetString().StartsWith("{") && json.GetString().Contains("LocationUri") && json.GetString().EndsWith("}")))
                {
                    #region Sample json
                    /*
                      "Location": "{\"LocationSource\":\"Bing\",\"LocationUri\":\"https://www.bingapis.com/api/v6/addresses/QWRkcmVzcy83MDA5ODM%3d%3d?setLang=en\",\"UniqueId\":\"https://www.bingapis.com/api/v6/addresses/QWRkcmVzcy83MDA5ODMwODI3MTUyMzc1%3d%3d?setLang=en\",\"Address\":{\"Street\":\"ffffffstraat 1\",\"City\":\"cccc\",\"State\":\"Vlaanderen\",\"CountryOrRegion\":\"Belgium\",\"PostalCode\":\"9999\"},\"Coordinates\":{}}",                     
                    */
                    #endregion

                    // investigate if this can be a location field
                    var parsedFieldContent = JsonDocument.Parse(json.GetString()).RootElement;
                    var fieldValue = new FieldLocationValue() { Field = GetListItemField(pnpObject, propertyName) };
                    fieldValue.FromJson(parsedFieldContent);
                    fieldValue.IsArray = false;
                    return new Tuple<object, string>(fieldValue, propertyName);
                }

            }

            return new Tuple<object, string>(null, null);
        }

        private static IField GetListItemField(TransientObject pnpObject, string fieldName)
        {
            if ((pnpObject as IDataModelParent).Parent.Parent is List parentList)
            {
                if (parentList.ArePropertiesAvailable(List.LoadFieldsExpression))
                {
                    return parentList.Fields.AsRequested().FirstOrDefault(p => p.InternalName == fieldName);
                }
            }
            return null;
        }

        internal static async Task TryPopuplatePnPObjectMetadataFromRest(IDataModelWithContext contextAwareObject, IMetadataExtensible targetMetadataObject, EntityInfo entity, string idFieldValue)
        {
            // Store the rest ID field for follow-up rest requests
            Dictionary<string, string> Metadata = targetMetadataObject.Metadata;
            PnPContext pnpContext = contextAwareObject.PnPContext;

            if (!Metadata.ContainsKey(PnPConstants.MetaDataRestId) && !string.IsNullOrEmpty(idFieldValue))
            {
                Metadata.Add(PnPConstants.MetaDataRestId, idFieldValue);
            }
            // Store graph ID for transition to graph when needed. No point in doing this when we've disabled graph first behaviour or when the entity does not support rest + graph
            if (entity.SupportsGraphAndRest && !Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
            {

                // SP.Web requires a special id value
                if (entity.SharePointType.Equals("SP.Web"))
                {
                    if (!Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                    {
                        if (pnpContext.Site.IsPropertyAvailable(p => p.Id) && pnpContext.Web.IsPropertyAvailable(p => p.Id))
                        {
                            // Check again here due to the recursive nature of this code
                            if (!Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                            {
                                Metadata.Add(PnPConstants.MetaDataGraphId, $"{pnpContext.Uri.DnsSafeHost},{pnpContext.Site.Id},{pnpContext.Web.Id}");
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
                    EntityFieldInfo entityField = LookupEntityField(entity, apiResponse, property);

                    // Do we need to re-parent this json mapping to a non expandable collection in the current model?
                    bool modelReparented = false;
                    if (!string.IsNullOrEmpty(apiResponse.ApiCall.ReceivingProperty) && (property.NameEquals("value") || apiResponse.ApiCall.ReceivingProperty == "primaryChannel"))
                    {
                        entityField = entity.Fields.FirstOrDefault(p => !string.IsNullOrEmpty(p.GraphName) && p.GraphName.Equals(apiResponse.ApiCall.ReceivingProperty, StringComparison.InvariantCultureIgnoreCase));
                        modelReparented = true;
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

                                var contextAwarePnPChild = pnpChild as IDataModelWithContext;

                                // Set PnPContext via a dynamic property
                                contextAwarePnPChild.PnPContext = contextAwareObject.PnPContext;

                                // Recursively map properties, call the method from the actual object as the object could have overriden it
                                await ((IDataModelProcess)pnpChild).ProcessResponseAsync(new ApiResponse(apiResponse.ApiCall, childJson, apiResponse.BatchRequestId)).ConfigureAwait(false);

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
                                var newApiCall = new ApiCall(apiResponse.ApiCall.Request, apiResponse.ApiCall.JsonBody)
                                {
                                    Type  = apiResponse.ApiCall.Type,                                    
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
                                            fromJsonCasting?.Invoke(new FromJson(overflowField.Name, overflowField.Value, Type.GetType("System.Object"), contextAwareObject.PnPContext.Logger));
                                        }
                                        else
                                        {
                                            // Default mapping to dictionary can handle simple types, more complex types require custom logic
                                            AddToDictionary(dictionaryPropertyToAddValueTo, overflowField.Name, GetJsonPropertyValue(overflowField));
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
                                                jsonPathField.PropertyInfo?.SetValue(pnpObject, GetJsonFieldValue(contextAwareObject, jsonPathField.Name,
                                                    jsonElement, jsonPathField.DataType, jsonPathField.GraphUseCustomMapping, fromJsonCasting));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // regular field mapping
                                    entityField.PropertyInfo?.SetValue(pnpObject, GetJsonFieldValue(contextAwareObject, entityField.Name, property.Value, entityField.DataType, entityField.GraphUseCustomMapping, fromJsonCasting));
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
                var parsedApiCall = await ApiHelper.ParseApiRequestAsync(targetMetadataObject, $"{contextAwareObject.PnPContext.Uri.ToString().TrimEnd(new char[] { '/' })}/{entity.SharePointUri}").ConfigureAwait(false);
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
                // SharePoint REST data format
                if (DateTime.TryParse(property.Value.GetString(), out DateTime foundDate2))
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
                Match indexMatch = new Regex(@"\[(?<index>[0-9]+)\]").Match(part);
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
                entityField = entity.Fields.FirstOrDefault(p => p.SharePointName == property.Name);
            }
            else if (apiResponse.ApiCall.Type == ApiType.Graph || apiResponse.ApiCall.Type == ApiType.GraphBeta)
            {
                // Use case insentive match for Graph as we get json in camel case while we use pascal case for the model
                entityField = entity.Fields.FirstOrDefault(p => !string.IsNullOrEmpty(p.GraphName) && p.GraphName.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));
            }

            return entityField;
        }

        private static void AddToDictionary(TransientDictionary dictionary, string key, object value)
        {
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

        internal static object GetJsonFieldValue(IDataModelWithContext pnpObject, string fieldName, JsonElement jsonElement, Type propertyType, bool useCustomMapping, Func<FromJson, object> fromJsonCasting)
        {
            // If a field mandates a custom mapping then the field handling is fully handled via the custom mapping handler
            if (useCustomMapping)
            {
                return fromJsonCasting?.Invoke(new FromJson(fieldName, jsonElement, propertyType, pnpObject?.PnPContext.Logger));
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
                    // Used on TermStore model (and maybe more in future)
                    case "List`1":
                        {
                            // When the call was made via SharePoint REST the list is wrapped into a collection object. E.g.
                            //
                            //"SupportedUILanguageIds": {
                            //    "__metadata": {
                            //        "type": "Collection(Edm.Int32)"
                            //    },
                            //"results": [
                            //    1033,
                            //    1043,
                            //    1031,
                            //    1036
                            //    ]
                            //}
                            //
                            // If so see if there's a results property and use that

                            if (jsonElement.TryGetProperty("results", out JsonElement results))
                            {
                                jsonElement = results;
                            }

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
                            return jsonElement.GetGuid();
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
                            return fromJsonCasting?.Invoke(new FromJson(fieldName, jsonElement, propertyType, pnpObject?.PnPContext.Logger));
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

            if (!metadata.ContainsKey(property.Name))
            {
                metadata.Add(property.Name, GetJsonPropertyValue(property).ToString());
            }
        }

        internal static void TrackSharePointMetaData(IMetadataExtensible target, JsonProperty property)
        {
            AddSharePointMetaDataProperty(target, property.Value, PnPConstants.MetaDataUri);
            AddSharePointMetaDataProperty(target, property.Value, PnPConstants.MetaDataId);
            AddSharePointMetaDataProperty(target, property.Value, PnPConstants.MetaDataType);
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

        internal static string BuildNextPageRestUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
                if (queryString["$top"] != null && queryString["$skip"] != null)
                {
                    // Build a url for a second, third, fourth etc page
                    if (int.TryParse(queryString["$top"], out int top) &&
                        int.TryParse(queryString["$skip"], out int skip))
                    {
                        int nextPage = (skip / top) + 1;
                        queryString["$skip"] = (nextPage * top).ToString(CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (queryString["$top"] != null)
                {
                    // Build the url for the first page after the initial load
                    queryString.Add("$skip", queryString["$top"]);
                }

                return $"{uri.Scheme}://{uri.DnsSafeHost}{uri.AbsolutePath}?{queryString}";
            }

            return null;
        }
    }
}
