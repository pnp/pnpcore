using PnP.Core.Model;
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

            if (apiResponse.ApiCall.Type == ApiType.SPORest)
            {
                await FromJsonRest(pnpObject, entity, apiResponse, fromJsonCasting).ConfigureAwait(false);
            }
            else if (apiResponse.ApiCall.Type == ApiType.Graph || apiResponse.ApiCall.Type == ApiType.GraphBeta)
            {
                await FromJsonGraph(pnpObject, entity, apiResponse, fromJsonCasting).ConfigureAwait(false);
            }

            // Mark object as requested, as long as it is an IRequestable object
            if (pnpObject.GetType().ImplementsInterface(typeof(IRequestable)))
            {
                ((IRequestable)pnpObject).Requested = true;
            }
        }

        internal static async Task FromJsonRest(TransientObject pnpObject, EntityInfo entity, ApiResponse apiResponse, Func<FromJson, object> fromJsonCasting = null)
        {
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
                    if (!useOverflowField && IsModelCollection(entityField.PropertyInfo.PropertyType))
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

                                // Set PnPContext via a dynamic property
                                contextAwarePnPChild.PnPContext = contextAwareObject.PnPContext;

                                // Recursively map properties, call the method from the actual object as the object could have overriden it
                                await ((IDataModelGet)pnpChild).GetAsync(new ApiResponse(apiResponse.ApiCall, childJson, apiResponse.BatchRequestId)).ConfigureAwait(false);

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
                                        string key = (keyField as EntityFieldInfo).Name;
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

                        await ((IDataModelGet)propertyToSetValue).GetAsync(new ApiResponse(apiResponse.ApiCall, property.Value, apiResponse.BatchRequestId)).ConfigureAwait(false);
                    }
                    else if (IsComplexType(entityField.PropertyInfo.PropertyType))
                    {
                        MapJsonToComplexTypePropertyRecursive(pnpObject, contextAwareObject, property, entityField);
                    }
                    else if (IsComplexTypeList(entityField.PropertyInfo.PropertyType))
                    {
                        MapJsonToComplexTypeItemListRecursive(pnpObject, contextAwareObject, apiResponse, property, entityField);
                    }
                    else // Simple property mapping
                    {
                        // Keep the id value aside when seeing it for later usage
                        if (string.IsNullOrEmpty(idFieldValue) && property.Name.Equals(entity.SharePointKeyField.Name))
                        {
                            idFieldValue = GetJsonPropertyValue(property).ToString();
                        }

                        // Set the object property value taken from the JSON payload
                        entityField.PropertyInfo?.SetValue(pnpObject, GetJsonFieldValue(contextAwareObject, entityField.Name,
                            GetJsonElementFromPath(property.Value, entityField.SharePointJsonPath), entityField.DataType, entityField.SharePointUseCustomMapping, fromJsonCasting));
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
                        if (metadataBasedObject is Model.SharePoint.List)
                        {
                            var listItemCollection = (metadataBasedObject as Model.SharePoint.List).Items;
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
                            // Add to overflow dictionary
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
                    }
                }
            }

            // Try populate the PnP Object metadata from the mapped info
            await TryPopuplatePnPObjectMetadataFromRest(contextAwareObject, metadataBasedObject, entity, idFieldValue).ConfigureAwait(false);

            // Store __next link for paging in case we detected a $top in the issued query
            if (apiResponse.ApiCall.Request.Contains("$top", StringComparison.InvariantCultureIgnoreCase))
            {
                var parent = (pnpObject as IDataModelParent).Parent;
                if (parent != null && parent is IManageableCollection && parent is IMetadataExtensible && parent.GetType().ImplementsInterface(typeof(ISupportPaging<>)))
                {
                    TrackAndUpdateMetaData(parent as IMetadataExtensible, "__next", BuildNextPageRestUrl(apiResponse.ApiCall.Request));
                }
            }
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
            if (pnpContext.GraphFirst && entity.SupportsGraphAndRest && !Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
            {
                // SP.Web requires a special id value
                if (entity.SharePointType.Equals("SP.Web"))
                {
                    if (!Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                    {
                        // Ensure site and web id's are loaded, use batching to load them both in one go in case that would be needed
                        if (!pnpContext.Site.IsPropertyAvailable(p => p.Id) || !pnpContext.Web.IsPropertyAvailable(p => p.Id))
                        {
                            var idBatch = pnpContext.NewBatch();
                            if (!pnpContext.Site.IsPropertyAvailable(p => p.Id))
                            {
                                await pnpContext.Site.GetBatchAsync(idBatch, p => p.Id).ConfigureAwait(false);
                            }
                            if (!pnpContext.Web.IsPropertyAvailable(p => p.Id))
                            {
                                await pnpContext.Web.GetBatchAsync(idBatch, p => p.Id).ConfigureAwait(false);
                            }
                            await pnpContext.ExecuteAsync(idBatch).ConfigureAwait(false);
                        }

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
        internal static async Task FromJsonGraph(TransientObject pnpObject, EntityInfo entity, ApiResponse apiResponse, Func<FromJson, object> fromJsonCasting = null)
        {
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
                    if (!string.IsNullOrEmpty(apiResponse.ApiCall.ReceivingProperty) && property.NameEquals("value"))
                    {
                        entityField = entity.Fields.FirstOrDefault(p => !string.IsNullOrEmpty(p.GraphName) && p.GraphName.Equals(apiResponse.ApiCall.ReceivingProperty, StringComparison.InvariantCultureIgnoreCase));
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
                                await ((IDataModelGet)pnpChild).GetAsync(new ApiResponse(apiResponse.ApiCall, childJson, apiResponse.BatchRequestId)).ConfigureAwait(false);

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
                                        string key = (keyField as EntityFieldInfo).Name;
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
                        }
                        // Are we loading a another model type (e.g. Site.RootWeb)?
                        else if (IsModelType(entityField.PropertyInfo.PropertyType))
                        {
                            // Get instance of the model property
                            var propertyToSetValue = entityField.PropertyInfo.GetValue(pnpObject);

                            // Set the batch request id property
                            SetBatchRequestId(propertyToSetValue as TransientObject, apiResponse.BatchRequestId);

                            await ((IDataModelGet)propertyToSetValue).GetAsync(new ApiResponse(apiResponse.ApiCall, property.Value, apiResponse.BatchRequestId)).ConfigureAwait(false);
                        }
                        // Are we loading a complex type
                        else if (IsComplexType(entityField.PropertyInfo.PropertyType))
                        {
                            MapJsonToComplexTypePropertyRecursive(pnpObject, contextAwareObject, property, entityField);
                        }
                        // Are we loading a list of complex types
                        else if (IsComplexTypeList(entityField.PropertyInfo.PropertyType))
                        {
                            MapJsonToComplexTypeItemListRecursive(pnpObject, contextAwareObject, apiResponse, property, entityField);
                        }
                        else
                        {
                            // Regular field loaded

                            // Grab id field, should be present in all graph objects
                            if (string.IsNullOrEmpty(idFieldValue) && property.Name.Equals(entity.GraphId))
                            {
                                idFieldValue = property.Value.GetString();
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

                                            // Don't assume that the requested json path was also loaded. When using the LoadProperties model there can be 
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

                    // Call EnsurePropertiesAsync to ensure Site.Id is loaded if it was not yet the case
                    await contextAwareObject.PnPContext.Site.EnsurePropertiesAsync(p => p.Id).ConfigureAwait(false);
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

        private static void MapJsonToComplexTypePropertyRecursive(TransientObject pnpObject, IDataModelWithContext contextAwareObject, JsonProperty property, EntityFieldInfo entityField)
        {
            // Do we still need to instantiate this object
            if (!pnpObject.HasValue(entityField.Name))
            {
                // entityField.PropertyInfo.PropertyType.Namespace = PnP.Core.Model.Teams
                // entityField.PropertyInfo.PropertyType.Name = ITeamDiscoverySettings
                entityField.PropertyInfo.SetValue(pnpObject, Activator.CreateInstance(Type.GetType($"{entityField.PropertyInfo.PropertyType.Namespace}.{entityField.PropertyInfo.PropertyType.Name.Substring(1)}")));
            }

            // Get instance of the model property
            var propertyToSetValue = entityField.PropertyInfo.GetValue(pnpObject);
            var typedModel = propertyToSetValue as IDataModelMappingHandler;
            var metadataExtensible = propertyToSetValue as IMetadataExtensible;
            var expandoComplexType = propertyToSetValue as IExpandoComplexType;

            // Set the batch request id property
            SetBatchRequestId(propertyToSetValue as TransientObject, pnpObject.BatchRequestId);

            // Get class info
            var complexModelEntity = EntityManager.Instance.GetStaticClassInfo(propertyToSetValue.GetType());

            // Map returned fields
            foreach (var childProperty in property.Value.EnumerateObject())
            {
                EntityFieldInfo entityChildField = (complexModelEntity.Fields as List<EntityFieldInfo>)
                                            .FirstOrDefault(p => p.Name.Equals(childProperty.Name, StringComparison.InvariantCultureIgnoreCase)
                                            || (!string.IsNullOrEmpty(p.GraphName) && p.GraphName.Equals(childProperty.Name, StringComparison.InvariantCultureIgnoreCase))
                                            || (!string.IsNullOrEmpty(p.SharePointName) && p.SharePointName.Equals(childProperty.Name, StringComparison.InvariantCultureIgnoreCase)));
                if (entityChildField != null)
                {
                    // if the complex type contains another complex type and there's a value provided then let's recursively call this method again
                    if (IsComplexType(entityChildField.PropertyInfo.PropertyType) && childProperty.Value.ValueKind != JsonValueKind.Null)
                    {
                        MapJsonToComplexTypePropertyRecursive(propertyToSetValue as TransientObject, propertyToSetValue as IDataModelWithContext, childProperty, entityChildField);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(entityChildField.GraphJsonPath))
                        {
                            var jsonPathFields = (complexModelEntity.Fields as List<EntityFieldInfo>).Where(p => !string.IsNullOrEmpty(p.GraphName) && p.GraphName.Equals(entityChildField.GraphName));
                            if (jsonPathFields.Any())
                            {
                                foreach (var jsonPathField in jsonPathFields)
                                {
                                    var jsonElement = GetJsonElementFromPath(childProperty.Value, jsonPathField.GraphJsonPath);
                                    
                                    // Don't assume that the requested json path was also loaded. When using the LoadProperties model there can be 
                                    // a json object returned that does have all properties loaded (e.g. a TeamsApp object with only id and distributionMethod loaded)
                                    if (!jsonElement.Equals(property.Value))
                                    {
                                        jsonPathField.PropertyInfo?.SetValue(propertyToSetValue, GetJsonFieldValue(contextAwareObject, jsonPathField.Name,
                                        jsonElement, jsonPathField.DataType, jsonPathField.GraphUseCustomMapping, typedModel?.MappingHandler));
                                    }
                                }
                            }
                        }
                        // TODO: Test this case when some domain models require it
                        //else if (!string.IsNullOrEmpty(entityChildField.SharePointJsonPath))
                        //{
                        //    var jsonPathFields = (complexModelEntity.Fields as List<EntityFieldInfo>).Where(p => !string.IsNullOrEmpty(p.SharePointName) && p.SharePointName.Equals(entityChildField.SharePointName));
                        //    if (jsonPathFields.Any())
                        //    {
                        //        foreach (var jsonPathField in jsonPathFields)
                        //        {
                        //            jsonPathField.PropertyInfo?.SetValue(propertyToSetValue,
                        //                GetJsonFieldValue(contextAwareObject, jsonPathField.Name,
                        //                GetJsonElementFromPath(childProperty.Value, jsonPathField.SharePointJsonPath), jsonPathField.DataType, jsonPathField.SharePointUseCustomMapping, typedModel?.MappingHandler));
                        //        }
                        //    }
                        //}
                        else
                        {
                            // We only map fields that exist
                            entityChildField.PropertyInfo?.SetValue(propertyToSetValue, GetJsonFieldValue(contextAwareObject, entityChildField.Name, childProperty.Value, entityChildField.DataType, entityChildField.GraphUseCustomMapping, typedModel?.MappingHandler));
                        }
                    }
                }
                else if (expandoComplexType != null)
                {
                    if (childProperty.Name == "__metadata")
                    {
                        // If the current property is metadata extensible
                        if (metadataExtensible != null)
                        {
                            TrackSharePointMetaData(metadataExtensible, childProperty);
                        }
                    }
                    else
                    {
                        // If a strongly typed property is not found in the current object, and the current object support extensible properties (is expando complex type)
                        expandoComplexType[childProperty.Name] = GetJsonPropertyValue(childProperty);
                    }
                }
            }
        }

        private static void MapJsonToComplexTypeItemListRecursive(TransientObject pnpObject, IDataModelWithContext contextAwareObject, ApiResponse apiResponse, JsonProperty property, EntityFieldInfo entityField)
        {
            JsonElement jsonArray = apiResponse.ApiCall.Type == ApiType.SPORest
                    ? TryGetRestComplexTypeArray(property)
                    : property.Value;

            // If property is not a valid array, skip the mapping process
            if (jsonArray.ValueKind != JsonValueKind.Array)
                return;

            // Get the actual current value of the property we're setting...as that allows to detect it's type
            var propertyToSetValue = entityField.PropertyInfo.GetValue(pnpObject);

            // Always clear the list on load
            (propertyToSetValue as System.Collections.IList).Clear();

            // Load each child as a complex type class in the list
            foreach (var childJson in jsonArray.EnumerateArray())
            {
                // create the list item 
                var typeToCreate = Type.GetType($"{entityField.PropertyInfo.PropertyType.GenericTypeArguments[0].Namespace}.{entityField.PropertyInfo.PropertyType.GenericTypeArguments[0].Name.Substring(1)}");
                var complexTypeInstance = Activator.CreateInstance(typeToCreate);

                // Set the batch request id property
                SetBatchRequestId(complexTypeInstance as TransientObject, apiResponse.BatchRequestId);

                (propertyToSetValue as System.Collections.IList).Add(complexTypeInstance);

                // Get the complex model metadata
                var complexModelEntity = EntityManager.Instance.GetStaticClassInfo(typeToCreate);

                // Map returned fields
                foreach (var childProperty in childJson.EnumerateObject())
                {
                    EntityFieldInfo entityChildField = (complexModelEntity.Fields as List<EntityFieldInfo>)
                                            .FirstOrDefault(p => p.Name.Equals(childProperty.Name, StringComparison.InvariantCultureIgnoreCase)
                                            || (!string.IsNullOrEmpty(p.GraphName) && p.GraphName.Equals(childProperty.Name, StringComparison.InvariantCultureIgnoreCase))
                                            || (!string.IsNullOrEmpty(p.SharePointName) && p.SharePointName.Equals(childProperty.Name, StringComparison.InvariantCultureIgnoreCase)));
                    if (entityChildField != null)
                    {
                        // if the complex type contains another complex type and there's a value provided then let's recursively call this method again
                        if (IsComplexType(entityChildField.PropertyInfo.PropertyType) && childProperty.Value.ValueKind != JsonValueKind.Null)
                        {
                            MapJsonToComplexTypePropertyRecursive(complexTypeInstance as TransientObject, complexTypeInstance as IDataModelWithContext, childProperty, entityChildField);
                        }
                        else
                        {
                            var typedModel = complexTypeInstance as IDataModelMappingHandler;

                            if (!string.IsNullOrEmpty(entityChildField.GraphJsonPath))
                            {
                                var jsonPathFields = (complexModelEntity.Fields as List<EntityFieldInfo>).Where(p => !string.IsNullOrEmpty(p.GraphName) && p.GraphName.Equals(entityChildField.GraphName));
                                if (jsonPathFields.Any())
                                {
                                    foreach (var jsonPathField in jsonPathFields)
                                    {
                                        var jsonElement = GetJsonElementFromPath(childProperty.Value, jsonPathField.GraphJsonPath);

                                        // Don't assume that the requested json path was also loaded. When using the LoadProperties model there can be 
                                        // a json object returned that does have all properties loaded (e.g. a TeamsApp object with only id and distributionMethod loaded)
                                        if (!jsonElement.Equals(property.Value))
                                        {
                                            jsonPathField.PropertyInfo?.SetValue(complexTypeInstance, GetJsonFieldValue(contextAwareObject, jsonPathField.Name,
                                            jsonElement, jsonPathField.DataType, jsonPathField.GraphUseCustomMapping, typedModel?.MappingHandler));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // We only map fields that exist
                                entityChildField.PropertyInfo?.SetValue(complexTypeInstance, GetJsonFieldValue(contextAwareObject, entityChildField.Name, childProperty.Value, entityChildField.DataType, entityChildField.GraphUseCustomMapping, typedModel?.MappingHandler));
                            }
                        }
                    }
                }
            }
        }

        private static JsonElement TryGetRestComplexTypeArray(JsonProperty property)
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                return property.Value;
            }
            else
            {
                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    if (property.Value.TryGetProperty("results", out JsonElement resultsProperty))
                    {
                        // If found "results" property
                        if (resultsProperty.ValueKind == JsonValueKind.Array)
                        {
                            // And is an array, it is the array to iterate through
                            return resultsProperty;
                        }
                    }

                    // Otherwise, the mapped property should have default value
                    return default;
                }

                // The JSON property type is not expected
                throw new ClientException(ErrorType.UnexpectedMappingType, $"The property {property.Name} is expected to be an array but is of type {property.Value.ValueKind} instead.");
            }
        }

        internal static object GetJsonPropertyValue(JsonProperty property)
        {
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
                                throw new IndexOutOfRangeException("The expected JSON array is empty.");
                            }

                            if (index > arrayLength-1)
                            {
                                throw new IndexOutOfRangeException("The requested index is out of the JSON array boundaries.");
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
                else if (element.TryGetProperty(part, out JsonElement nextElement))
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

        internal static T ToEnum<T>(JsonElement jsonElement) where T : struct
        {
            if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt64(out long enumNumericValue))
            {
                if (Enum.TryParse(enumNumericValue.ToString(CultureInfo.InvariantCulture), out T enumValue))
                {
                    return enumValue;
                }
            }
            else if (jsonElement.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(jsonElement.GetString()))
            {
                if (Enum.TryParse(jsonElement.GetString(), true, out T enumValue))
                {
                    return enumValue;
                }
            }
            return default;
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

            throw new ClientException(ErrorType.ModelMetadataIncorrect, "GraphName was not set");
        }

        internal static bool IsModelType(Type propertyType)
        {
            return propertyType.ImplementsInterface(typeof(IDataModel<>));
        }

        internal static bool IsComplexType(Type propertyType)
        {
            return propertyType.ImplementsInterface(typeof(IComplexType));
        }

        internal static bool IsModelCollection(Type propertyType)
        {
            return propertyType.ImplementsInterface(typeof(IDataModelCollection<>));
        }

        internal static bool IsComplexTypeList(Type propertyType)
        {
            return (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(List<>)) && propertyType.GetGenericArguments()[0].ImplementsInterface(typeof(IComplexType)));
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
