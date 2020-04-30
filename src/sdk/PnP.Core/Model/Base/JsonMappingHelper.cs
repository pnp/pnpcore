using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Internal class to help with the JSON mapping for Domain Model objects
    /// </summary>
    internal static class JsonMappingHelper
    {
        /// <summary>
        /// Maps a json string to the provided domain model object instance
        /// </summary>
        /// <param name="model">The Domain Model object to map json to</param>
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
                await FromJson(batchRequest.Model, batchRequest.EntityInfo, new ApiResponse(batchRequest.ApiCall, root), batchRequest.FromJsonCasting).ConfigureAwait(false);
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
            // Mark object as requested, as long as it is an IRequestable object
            if (pnpObject.GetType().ImplementsInterface(typeof(IRequestable)))
            {
                ((IRequestable)pnpObject).Requested = true;
            }

            if (apiResponse.ApiCall.Type == ApiType.SPORest)
            {
                await FromJsonRest(pnpObject, entity, apiResponse, fromJsonCasting).ConfigureAwait(false);
            }
            else if (apiResponse.ApiCall.Type == ApiType.Graph)
            {
                await FromJsonGraph(pnpObject, entity, apiResponse, fromJsonCasting).ConfigureAwait(false);
            }
        }

        internal static async Task FromJsonRest(TransientObject pnpObject, EntityInfo entity, ApiResponse apiResponse, Func<FromJson, object> fromJsonCasting = null)
        {
            // Get the type of the domain model we're populating
            var pnpObjectType = pnpObject.GetType();
            var metadataBasedObject = pnpObject as IMetadataExtensible;
            var contextAwareObject = pnpObject as IDataModelWithContext;

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

                        // Try to get the results property
                        JsonElement resultsProperty = default(JsonElement);
                        if ((property.Value.ValueKind != JsonValueKind.Array && 
                            !property.Value.TryGetProperty("results", out resultsProperty)) ||
                            property.Name == "results")
                        {
                            resultsProperty = property.Value;
                        }

                        // Expanded objects are under the results property
                        if (!resultsProperty.Equals(default(JsonElement)))
                        {
                            PropertyInfo pnpChildIdProperty = null;
                            foreach (var childJson in resultsProperty.EnumerateArray())
                            {
                                // Create a new model instance to add to the collection
                                var pnpChild = typedCollection.CreateNew();
                                var contextAwarePnPChild = pnpChild as IDataModelWithContext;

                                // Set PnPContext via a dynamic property
                                contextAwarePnPChild.PnPContext = contextAwareObject.PnPContext;

                                // Recursively map properties, call the method from the actual object as the object could have overriden it
                                await ((IDataModelGet)pnpChild).GetAsync(new ApiResponse(apiResponse.ApiCall, childJson)).ConfigureAwait(false);

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

                            if (!metadataBasedObject.Deferred.ContainsKey(entityField.Name))
                            {
                                metadataBasedObject.Deferred.Add(entityField.Name, deferredProperty.GetProperty("uri").GetString());
                            }
                        }
                    }
                    // Are we loading another model type (e.g. Site.RootWeb)?
                    else if (IsModelType(entityField.PropertyInfo.PropertyType))
                    {
                        // Get instance of the model property
                        var propertyToSetValue = entityField.PropertyInfo.GetValue(pnpObject);
                        
                        // Set PnPContext via a dynamic property
                        ((IDataModelWithContext)propertyToSetValue).PnPContext = contextAwareObject.PnPContext;

                        // Set parent
                        ((IDataModelParent)propertyToSetValue).Parent = (IDataModelParent)pnpObject;

                        await ((IDataModelGet)propertyToSetValue).GetAsync(new ApiResponse(apiResponse.ApiCall, property.Value)).ConfigureAwait(false);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(idFieldValue) && property.Name.Equals(entity.SharePointKeyField.Name))
                        {
                            idFieldValue = GetJsonPropertyValue(property).ToString();
                        }

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
                    // Let's also store the EntityTypeName value as metadata as it's usefull for constructing future rest calls
                    else if (property.Name == "EntityTypeName")
                    {
                        TrackMetaData(metadataBasedObject, property);
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

            // Store the rest ID field for follow-up rest requests
            if (!metadataBasedObject.Metadata.ContainsKey(PnPConstants.MetaDataRestId) && !string.IsNullOrEmpty(idFieldValue))
            {
                metadataBasedObject.Metadata.Add(PnPConstants.MetaDataRestId, idFieldValue);
            }
            // Store graph ID for transition to graph when needed
            if (contextAwareObject.PnPContext.GraphFirst && !metadataBasedObject.Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
            {
                // SP.Web requires a special id value
                if (entity.SharePointType.Equals("SP.Web"))
                {
                    // Call EnsurePropertiesAsync to ensure Site.Id is loaded if it was not yet the case
                    await contextAwareObject.PnPContext.Site.EnsurePropertiesAsync(p => p.Id).ConfigureAwait(false);

                    if (contextAwareObject.PnPContext.Site.IsPropertyAvailable(p => p.Id) && contextAwareObject.PnPContext.Web.IsPropertyAvailable(p => p.Id))
                    {
                        metadataBasedObject.Metadata.Add(PnPConstants.MetaDataGraphId, $"{contextAwareObject.PnPContext.Uri.DnsSafeHost},{contextAwareObject.PnPContext.Site.Id},{contextAwareObject.PnPContext.Web.Id}");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(idFieldValue))
                    {
                        metadataBasedObject.Metadata.Add(PnPConstants.MetaDataGraphId, idFieldValue);
                    }
                }
            }

        }

        /// <summary>
        /// Maps JSON to model classes
        /// </summary>
        /// <param name="pnpObject">Model to populate from JSON</param>
        /// <param name="entity">Information about the current model</param>
        /// <param name="root">JsonElement to process</param>
        /// <param name="fromJsonCasting">Delegate to be called for type conversion</param>
        /// <returns></returns>
        internal static async Task FromJsonGraph(TransientObject pnpObject, EntityInfo entity, ApiResponse apiResponse, Func<FromJson, object> fromJsonCasting = null)
        {
            // Get the type of the domain model we're populating
            var pnpObjectType = pnpObject.GetType();
            var metadataBasedObject = pnpObject as IMetadataExtensible;
            var contextAwareObject = pnpObject as IDataModelWithContext;

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
                    // Are we loading a collection (e.g. Web.Lists)?
                    if (IsModelCollection(entityField.PropertyInfo.PropertyType))
                    {
                        // Get the actual current value of the property we're setting...as that allows to detect it's type
                        var propertyToSetValue = entityField.PropertyInfo.GetValue(pnpObject);
                        // Cast object to call the needed methods on it (e.g. ListCollection)
                        var typedCollection = propertyToSetValue as IManageableCollection;

                        // expanded objects are under the results property
                        PropertyInfo pnpChildIdProperty = null;
                        foreach (var childJson in property.Value.EnumerateArray())
                        {
                            // Create a new model instance to add to the collection
                            var pnpChild = typedCollection.CreateNew();
                            var contextAwarePnPChild = pnpChild as IDataModelWithContext;

                            // Set PnPContext via a dynamic property
                            contextAwarePnPChild.PnPContext = contextAwareObject.PnPContext;

                            // Recursively map properties, call the method from the actual object as the object could have overriden it
                            await ((IDataModelGet)pnpChild).GetAsync(new ApiResponse(apiResponse.ApiCall, childJson)).ConfigureAwait(false);

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

                        await ((IDataModelGet)propertyToSetValue).GetAsync(new ApiResponse(apiResponse.ApiCall, property.Value)).ConfigureAwait(false);
                    }
                    // Are we loading a complex type
                    else if (IsComplexType(entityField.PropertyInfo.PropertyType))
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

                        // Get class info
                        var complexModelEntity = EntityManager.Instance.GetStaticClassInfo(propertyToSetValue.GetType());
                        
                        // Map returned fields
                        foreach (var childProperty in property.Value.EnumerateObject())
                        {
                            EntityFieldInfo entityChildField = (complexModelEntity.Fields as List<EntityFieldInfo>).Where(p => p.GraphName.Equals(childProperty.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                            if (entityChildField != null)
                            {
                                if (!string.IsNullOrEmpty(entityChildField.GraphJsonPath))
                                {
                                    var jsonPathFields = (complexModelEntity.Fields as List<EntityFieldInfo>).Where(p => !string.IsNullOrEmpty(p.GraphName) && p.GraphName.Equals(entityChildField.GraphName));
                                    if (jsonPathFields.Any())
                                    {
                                        foreach (var jsonPathField in jsonPathFields)
                                        {
                                            jsonPathField.PropertyInfo?.SetValue(propertyToSetValue, 
                                                GetJsonFieldValue(contextAwareObject, jsonPathField.Name,
                                                GetJsonElementFromPath(childProperty.Value, jsonPathField.GraphJsonPath), jsonPathField.DataType, jsonPathField.GraphUseCustomMapping, typedModel?.MappingHandler));
                                        }
                                    }
                                }
                                else
                                {
                                    // We only map fields that exist
                                    entityChildField.PropertyInfo?.SetValue(propertyToSetValue, GetJsonFieldValue(contextAwareObject, entityChildField.Name, childProperty.Value, entityChildField.DataType, entityChildField.GraphUseCustomMapping, typedModel?.MappingHandler));
                                }
                            }
                        }
                    }
                    else
                    {
                        // Regular field loaded

                        // Grab id field, should be present in all graph objects
                        if (string.IsNullOrEmpty(idFieldValue) && property.Name.Equals("id"))
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
                                    TrackMetaData(metadataBasedObject, overflowField);
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
                                        jsonPathField.PropertyInfo?.SetValue(pnpObject, GetJsonFieldValue(contextAwareObject, jsonPathField.Name,
                                            GetJsonElementFromPath(property.Value, jsonPathField.GraphJsonPath), jsonPathField.DataType, jsonPathField.GraphUseCustomMapping, fromJsonCasting));
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
                        TrackMetaData(metadataBasedObject, property);
                    }
                    else if (string.IsNullOrEmpty(idFieldValue) && property.Name.Equals("id"))
                    {
                        idFieldValue = property.Value.GetString();
                    }
                }
            }

            // Store metadata to enable transition to REST for follow-up requests
            if (!string.IsNullOrEmpty(entity.SharePointType))
            {
                if (!metadataBasedObject.Metadata.ContainsKey(PnPConstants.MetaDataType))
                {
                    metadataBasedObject.Metadata.Add(PnPConstants.MetaDataType, entity.SharePointType);
                }
                if (!metadataBasedObject.Metadata.ContainsKey(PnPConstants.MetaDataRestId))
                {
                    // SP.Web and SP.Site are special cases
                    if (entity.SharePointType.Equals("SP.Web"))
                    {
                        // Special case for web handling, grab web id from id string
                        // sample input: bertonline.sharepoint.com,cf1ed1cb-4a3c-43ed-bb3f-4ced4ce69ecf,1de385e4-e441-4448-8443-77680dfd845e
                        if (!string.IsNullOrEmpty(idFieldValue))
                        {
                            string id = idFieldValue.Split(",")[2];
                            metadataBasedObject.Metadata.Add(PnPConstants.MetaDataRestId, id);
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
                            string id = idFieldValue.Split(",")[1];
                            metadataBasedObject.Metadata.Add(PnPConstants.MetaDataRestId, id);
                            ((IDataModelWithKey)pnpObject).Key = Guid.Parse(id);
                        }
                    }
                    else
                    {
                        EntityFieldInfo id = null;
                        PropertyInfo idField = null;
                        id = entity.Fields.Where(p => p.IsSharePointKey).FirstOrDefault();
                        if (id != null)
                        {
                            idField = pnpObject.GetType().GetProperty(id.Name);

                            // Get key property value
                            var keyFieldValue = idField.GetValue(pnpObject);
                            metadataBasedObject.Metadata.Add(PnPConstants.MetaDataRestId, keyFieldValue.ToString());
                        }
                    }
                }
                if (!metadataBasedObject.Metadata.ContainsKey(PnPConstants.MetaDataUri))
                {
                    metadataBasedObject.Metadata.Add(PnPConstants.MetaDataUri, ApiHelper.ParseApiRequest(metadataBasedObject, $"{contextAwareObject.PnPContext.Uri.ToString().TrimEnd(new char[] { '/' })}/{entity.SharePointUri}"));
                }
                if (entity.SharePointType.Equals("SP.List") && pnpObject.HasValue("Title") && !metadataBasedObject.Metadata.ContainsKey(PnPConstants.MetaDataRestEntityTypeName))
                {
                    metadataBasedObject.Metadata.Add(PnPConstants.MetaDataRestEntityTypeName, $"{pnpObject.GetValue("Title").ToString().Replace(" ", "")}List");
                }
            }

            // Store the graph ID field for follow-up graph requests
            if (!metadataBasedObject.Metadata.ContainsKey(PnPConstants.MetaDataGraphId) && !string.IsNullOrEmpty(idFieldValue))
            {
                metadataBasedObject.Metadata.Add(PnPConstants.MetaDataGraphId, idFieldValue);
            }
        }

        internal static object GetJsonPropertyValue(JsonProperty property)
        {
            return property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.GetInt64(),
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
                if (element.TryGetProperty(part, out JsonElement nextElement))
                {
                    element = nextElement;
                }
            }

            return element;
        }

        private static EntityFieldInfo LookupEntityField(EntityInfo entity, ApiResponse apiResponse, JsonProperty property)
        {
            EntityFieldInfo entityField = null;
            if (apiResponse.ApiCall.Type == ApiType.SPORest)
            {
                entityField = entity.Fields.FirstOrDefault(p => p.SharePointName == property.Name);
            }
            else if (apiResponse.ApiCall.Type == ApiType.Graph)
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

        internal static T ToEnum<T>(JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt64(out long enumNumericValue))
            {
                if (Enum.TryParse(typeof(T), enumNumericValue.ToString(CultureInfo.InvariantCulture), out object enumValue))
                {
                    return (T)enumValue;
                }
            }
            else if (jsonElement.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(jsonElement.GetString()))
            {
                if (Enum.TryParse(typeof(T), jsonElement.GetString(), true, out object enumValue))
                {
                    return (T)enumValue;
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
                return fromJsonCasting?.Invoke(new FromJson(fieldName, jsonElement, propertyType, pnpObject.PnPContext.Logger));
            }

            switch (propertyType.FullName)
            {
                case "System.String":
                    {
                        return jsonElement.GetString();
                    }
                case "System.Boolean":
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
                case "System.Guid":
                    {
                        return jsonElement.GetGuid();
                    }
                case "System.Int16":
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
                case "System.Int32":
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
                case "System.Int64":
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
                case "System.UInt16":
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
                case "System.UInt32":
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
                case "System.UInt64":
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
                case "System.Uri":
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
                default:
                    {
                        // Do a call back for the cases where we don't have a default mapping
                        return fromJsonCasting?.Invoke(new FromJson(fieldName, jsonElement, propertyType, pnpObject.PnPContext.Logger));
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

            throw new Exception("GraphName was not set");
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

        internal static void TrackMetaData(IMetadataExtensible target, JsonProperty property)
        {
            if (!target.Metadata.ContainsKey(property.Name))
            {
                target.Metadata.Add(property.Name, JsonMappingHelper.GetJsonPropertyValue(property).ToString());
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
    }
}
