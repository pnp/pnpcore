using Microsoft.Extensions.Logging;
using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ListItem class, write your custom code here
    /// </summary>
    [SharePointType("SP.ListItem", Target = typeof(List), Uri = "_api/web/lists/getbyid(guid'{Parent.Id}')/items({Id})", LinqGet = "_api/web/lists(guid'{Parent.Id}')/items")]
    [SharePointType("SP.ListItem", Target = typeof(File), Uri = "_api/web/getFileById('{Parent.Id}')/listitemallfields")]
    [GraphType(OverflowProperty = "fields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class ListItem : ExpandoBaseDataModel<IListItem>, IListItem
    {
        internal const string FolderPath = "folderPath";
        internal const string UnderlyingObjectType = "underlyingObjectType";

        #region Construction
        public ListItem()
        {
            MappingHandler = (FromJson input) =>
            {
                // The AddValidateUpdateItemUsingPath call returns the id of the added list item
                if (input.FieldName == "AddValidateUpdateItemUsingPath")
                {
                    if (input.JsonElement.TryGetProperty("results", out JsonElement resultsProperty))
                    {
                        foreach (var field in resultsProperty.EnumerateArray())
                        {
                            var fieldName = field.GetProperty("FieldName").GetString();
                            var fieldValue = field.GetProperty("FieldValue").GetString();

                            if (fieldName == "Id")
                            {
                                if (int.TryParse(fieldValue, out int id))
                                {
                                    Id = id;

                                    // Flag the parent collection as requested, most requests return a "full" json structure and then the standard json parsing 
                                    // is used, which sets the collection as requested. Since in this case we get back a special structure we use custom
                                    // parsing and hence we need to flag the collection as requested ourselves
                                    (Parent as ListItemCollection).Requested = true;

                                    // populate the uri and type metadata fields to enable actions upon 
                                    // this item without having to read it again from the server
                                    var parentList = Parent.Parent as List;
                                    AddMetadata(PnPConstants.MetaDataRestId, $"{id}");
                                    MetadataSetup();

                                    // Ensure the values are committed to the model when an item is being added
                                    Values.Commit();

                                    // We're currently only interested in the Id property
                                    continue;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Handle the mapping from json to the domain model for the cases which are not generically handled           
                    input.Log.LogDebug(PnPCoreResources.Log_Debug_JsonCannotMapField, input.FieldName);
                }

                return null;
            };

            PostMappingHandler = (string json) =>
            {
                // Extra processing of returned json
            };
            AddApiCallHandler = async (keyValuePairs) => {
                var parentList = Parent.Parent as List;
                // sample parent list uri: https://bertonline.sharepoint.com/sites/modern/_api/Web/Lists(guid'b2d52a36-52f1-48a4-b499-629063c6a38c')
                var parentListUri = parentList.GetMetadata(PnPConstants.MetaDataUri);

                // Fall back to loading the rootfolder propery if we can't determine the list name
                await parentList.EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
                var serverRelativeUrl = parentList.RootFolder.ServerRelativeUrl;

                // drop the everything in front of _api as the batching logic will add that automatically
                var baseApiCall = parentListUri.Substring(parentListUri.IndexOf("_api"));

                // Define the JSON body of the update request based on the actual changes
                dynamic body = new ExpandoObject();

                var decodedUrlFolderPath = serverRelativeUrl;
                if (keyValuePairs.ContainsKey(FolderPath))
                {
                    if (keyValuePairs[FolderPath] != null)
                    {
                        var folderPath = keyValuePairs[FolderPath].ToString();
                        if (!string.IsNullOrEmpty(folderPath))
                        {
                            if (folderPath.ToLower().StartsWith(serverRelativeUrl))
                            {
                                decodedUrlFolderPath = folderPath;
                            }
                            else
                            {
                                decodedUrlFolderPath = $"{serverRelativeUrl}/{folderPath.TrimStart('/')}";
                            }
                        }
                    }
                }

                var underlyingObjectType = (int)FileSystemObjectType.File;

                if (keyValuePairs.ContainsKey(UnderlyingObjectType))
                {
                    if (keyValuePairs[UnderlyingObjectType] != null)
                    {
                        underlyingObjectType = (int)((FileSystemObjectType)keyValuePairs[UnderlyingObjectType]);
                    }
                }

                body.listItemCreateInfo = new
                {
                    FolderPath = new
                    {
                        DecodedUrl = decodedUrlFolderPath
                    },
                    UnderlyingObjectType = underlyingObjectType
                };

                body.bNewDocumentUpdate = false;

                if (Values.Any())
                {
                    // Verify the needed locale settings are loaded
                    await EnsureRegionalSettingsAsync(PnPContext).ConfigureAwait(false);
                }

                // Add fields to the payload
                dynamic itemValues = new List<dynamic>();
                foreach (var item in Values)
                {
                    dynamic field = new ExpandoObject();
                    field.FieldName = item.Key;

                    BuildValidateUpdateItemPayload(PnPContext, item, field);

                    itemValues.Add(field);
                }
                body.formValues = itemValues;

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = true });

                // Return created api call
                return new ApiCall($"{baseApiCall}/AddValidateUpdateItemUsingPath", ApiType.SPORest, bodyContent);
            };
        }
        #endregion

        #region Properties
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string Title { get => (string)Values["Title"]; set => Values["Title"] = value; }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }
        #endregion

        #region Methods

        #region Folder
        public async Task<bool> IsFolderAsync()
        {
            if (!Values.ContainsKey("ContentTypeId"))
            {
                await LoadKeyListItemProperties().ConfigureAwait(false);
            }

            return Values["ContentTypeId"].ToString().StartsWith("0x0120", StringComparison.InvariantCultureIgnoreCase);
        }

        public bool IsFolder()
        {
            return IsFolderAsync().GetAwaiter().GetResult();
        }

        public async Task<IFolder> GetFolderAsync()
        {
            if (!Values.ContainsKey("FileDirRef"))
            {
                await LoadKeyListItemProperties().ConfigureAwait(false);
            }

            return await PnPContext.Web.GetFolderByServerRelativeUrlAsync(Values["FileDirRef"].ToString()).ConfigureAwait(false);
        }

        public IFolder GetFolder()
        {
            return GetFolderAsync().GetAwaiter().GetResult();
        }

        private async Task LoadKeyListItemProperties()
        {
            ApiCall apiCall = new ApiCall($"_api/web/lists/getbyid(guid'{{Parent.Id}}')/items({Id})?$select=ContentTypeId,FileDirRef", ApiType.SPORest);
            await RequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
        }
        #endregion

        #region Item updates

        internal override async Task BaseUpdate(Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();

            var api = await BuildUpdateApiCallAsync(PnPContext).ConfigureAwait(false);

            // Add the request to the batch and execute the batch
            var batch = PnPContext.BatchClient.EnsureBatch();
            batch.Add(this, entityInfo, HttpMethod.Post, api, default, null, null, "Update");
            await PnPContext.BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
        }


        internal override async Task BaseBatchUpdateAsync(Batch batch, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();

            var api = await BuildUpdateApiCallAsync(PnPContext).ConfigureAwait(false);

            // Add the request to the batch
            batch.Add(this, entityInfo, HttpMethod.Post, api, default, null, null, "UpdateBatch");
        }

        private async Task<ApiCall> BuildUpdateApiCallAsync(PnPContext context)
        {
            // Verify the needed locale settings are loaded
            await EnsureRegionalSettingsAsync(context).ConfigureAwait(false);

            // Define the JSON body of the update request based on the actual changes
            dynamic updateMessage = new ExpandoObject();

            // increment version
            updateMessage.bNewDocumentUpdate = false;

            dynamic itemValues = new List<dynamic>();

            foreach (KeyValuePair<string, object> changedProp in Values.ChangedProperties)
            {
                dynamic field = new ExpandoObject();
                field.FieldName = changedProp.Key;

                BuildValidateUpdateItemPayload(context, changedProp, field);

                itemValues.Add(field);
            }

            updateMessage.formValues = itemValues;

            // Get the corresponding JSON text content
            var jsonUpdateMessage = JsonSerializer.Serialize(updateMessage,
                typeof(ExpandoObject),
                new JsonSerializerOptions { WriteIndented = true });

            var itemUri = GetMetadata(PnPConstants.MetaDataUri);

            // If this list we're adding items to was not fetched from the server than throw an error
            if (string.IsNullOrEmpty(itemUri))
            {
                throw new ClientException(ErrorType.PropertyNotLoaded, PnPCoreResources.Exception_PropertyNotLoaded_List);
            }

            // Prepare the variable to contain the target URL for the update operation
            var updateUrl = await ApiHelper.ParseApiCallAsync(this, $"{itemUri}/ValidateUpdateListItem").ConfigureAwait(false);
            var api = new ApiCall(updateUrl, ApiType.SPORest, jsonUpdateMessage)
            {
                Commit = true
            };
            return api;
        }

        private static async Task EnsureRegionalSettingsAsync(PnPContext context)
        {
            bool loadRegionalSettings = false;
            if (context.Web.IsPropertyAvailable(p => p.RegionalSettings))
            {
                if (!context.Web.RegionalSettings.IsPropertyAvailable(p => p.TimeZone) ||
                    !context.Web.RegionalSettings.IsPropertyAvailable(p => p.DecimalSeparator) ||
                    !context.Web.RegionalSettings.IsPropertyAvailable(p => p.DateSeparator))
                {
                    loadRegionalSettings = true;
                }
            }
            else
            {
                loadRegionalSettings = true;
            }

            if (loadRegionalSettings)
            {
                await context.Web.RegionalSettings.EnsurePropertiesAsync(RegionalSettings.LocaleSettingsExpression).ConfigureAwait(false);
            }
        }

        private static void BuildValidateUpdateItemPayload(PnPContext context, KeyValuePair<string, object> changedProp, dynamic field)
        {
            // Only include FieldValue properties when they signal they've changed
            if (changedProp.Value is FieldValue fieldItemValue && fieldItemValue.HasChanges)
            {
                field.FieldValue = fieldItemValue.ToValidateUpdateItemJson();
            }
            else if (changedProp.Value is FieldValueCollection fieldValueCollection)
            {
                // Only process if there were changes in the field value collection
                if (fieldValueCollection.HasChanges)
                {
                    if (fieldValueCollection.Field.TypeAsString == "UserMulti")
                    {
                        field.FieldValue = fieldValueCollection.UserMultiToValidateUpdateItemJson();
                    }
                    else if (fieldValueCollection.Field.TypeAsString == "TaxonomyFieldTypeMulti")
                    {
                        field.FieldValue = fieldValueCollection.TaxonomyMultiToValidateUpdateItemJson();
                    }
                    else if (fieldValueCollection.Field.TypeAsString == "LookupMulti")
                    {
                        field.FieldValue = fieldValueCollection.LookupMultiToValidateUpdateItemJson();
                    }
                }
                else
                {
                    field.FieldValue = null;
                }
            }
            else if (changedProp.Value is List<string> stringList)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var choice in stringList)
                {
                    sb.Append($"{choice};#");
                }
                field.FieldValue = sb.ToString();
            }
            else if (changedProp.Value is DateTime dateValue)
            {
                field.FieldValue = DateTimeToSharePointString(context, dateValue);
            }
            else if (changedProp.Value != null && (changedProp.Value is int))
            {
                field.FieldValue = changedProp.Value.ToString();
            }
            else if (changedProp.Value != null && (changedProp.Value is double doubleValue))
            {
                field.FieldValue = DoubleToSharePointString(context, doubleValue);
            }
            else if (changedProp.Value != null && (changedProp.Value is bool boolValue))
            {
                field.FieldValue = $"{(boolValue ? "1" : "0")}";
            }
            else
            {
                field.FieldValue = changedProp.Value;
            }
        }

        private static string DateTimeToSharePointString(PnPContext context, DateTime input)
        {
            // Convert incoming date to UTC
            DateTime inputInUTC = input.ToUniversalTime();

            // Convert to the time zone used by the SharePoint site, take in account the daylight savings delta
            bool isDaylight = TimeZoneInfo.Local.IsDaylightSavingTime(input);
            TimeSpan utcDelta = new TimeSpan(0, context.Web.RegionalSettings.TimeZone.Bias + (isDaylight ? context.Web.RegionalSettings.TimeZone.DaylightBias : context.Web.RegionalSettings.TimeZone.StandardBias), 0);

            // Apply the delta from UTC to get the date used by the site and apply formatting 
            return (inputInUTC - utcDelta).ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
        }

        private static string DoubleToSharePointString(PnPContext context, double input)
        {
            NumberFormatInfo nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = context.Web.RegionalSettings.DecimalSeparator
            };

            return input.ToString(nfi);
        }

        #endregion

        #region UpdateOverwriteVersion

        public async Task UpdateOverwriteVersionAsync()
        {
            var xmlPayload = await BuildXmlPayloadAsync(true).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(xmlPayload))
            {
                var apiCall = new ApiCall(xmlPayload)
                {
                    Commit = true
                };
                await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                PnPContext.Logger.LogInformation(PnPCoreResources.Log_Information_NoChangesSkipSystemUpdate);
            }
        }

        public void UpdateOverwriteVersion()
        {
            UpdateOverwriteVersionAsync().GetAwaiter().GetResult();
        }

        public async Task UpdateOverwriteVersionBatchAsync()
        {
            await UpdateOverwriteVersionBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void UpdateOverwriteVersionBatch()
        {
            UpdateOverwriteVersionBatchAsync().GetAwaiter().GetResult();
        }

        public async Task UpdateOverwriteVersionBatchAsync(Batch batch)
        {
            var xmlPayload = await BuildXmlPayloadAsync(true).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(xmlPayload))
            {
                var apiCall = new ApiCall(xmlPayload)
                {
                    Commit = true
                };
                await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                PnPContext.Logger.LogInformation(PnPCoreResources.Log_Information_NoChangesSkipSystemUpdate);
            }
        }

        public void UpdateOverwriteVersionBatch(Batch batch)
        {
            UpdateOverwriteVersionBatchAsync(batch).GetAwaiter().GetResult();
        }

        #endregion

        #region SystemUpdate

        public async Task SystemUpdateAsync()
        {
            var xmlPayload = await BuildXmlPayloadAsync(false).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(xmlPayload))
            {
                var apiCall = new ApiCall(xmlPayload)
                {
                    Commit = true
                };
                await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                PnPContext.Logger.LogInformation(PnPCoreResources.Log_Information_NoChangesSkipSystemUpdate);
            }
        }

        public void SystemUpdate()
        {
            SystemUpdateAsync().GetAwaiter().GetResult();
        }

        public async Task SystemUpdateBatchAsync()
        {
            await SystemUpdateBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void SystemUpdateBatch()
        {
            SystemUpdateBatchAsync().GetAwaiter().GetResult();
        }

        public async Task SystemUpdateBatchAsync(Batch batch)
        {
            var xmlPayload = await BuildXmlPayloadAsync(false).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(xmlPayload))
            {
                var apiCall = new ApiCall(xmlPayload)
                {
                    Commit = true
                };
                await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                PnPContext.Logger.LogInformation(PnPCoreResources.Log_Information_NoChangesSkipSystemUpdate);
            }
        }

        public void SystemUpdateBatch(Batch batch)
        {
            SystemUpdateBatchAsync(batch).GetAwaiter().GetResult();
        }

        private async Task<string> BuildXmlPayloadAsync(bool updateOverwriteVersion)
        {
            string xml;

            if (updateOverwriteVersion)
            {
                xml = CsomHelper.ListItemUpdateOverwriteVersion;
            }
            else
            {
                xml = CsomHelper.ListItemSystemUpdate;
            }

            if ((this as IDataModelParent).Parent is IFile file)
            {
                // When it's a file then we need to resolve the {Parent.Id} token manually as otherwise this 
                // will point to the File id while we need to list Id here
                await file.EnsurePropertiesAsync(p => p.ListId).ConfigureAwait(false);
                xml = xml.Replace("{Parent.Id}", file.ListId.ToString());
            }

            int counter = 1;
            StringBuilder fieldValues = new StringBuilder();

            int taxFieldObjectId = 100;
            int taxFieldIdentityObjectId = 200;
            StringBuilder taxonomyMultiValueObjectPaths = new StringBuilder();
            StringBuilder taxonomyMultiValueIdentities = new StringBuilder();

            var entity = EntityManager.GetClassInfo(GetType(), this);
            IEnumerable<EntityFieldInfo> fields = entity.Fields;

            bool changeFound = false;
            foreach (PropertyDescriptor cp in ChangedProperties)
            {
                changeFound = true;
                // Look for the corresponding property in the type
                var changedField = fields.FirstOrDefault(f => f.Name == cp.Name);

                // If we found a field 
                if (changedField != null)
                {
                    if (changedField.DataType.FullName == typeof(TransientDictionary).FullName)
                    {
                        // Get the changed properties in the dictionary
                        var dictionaryObject = (TransientDictionary)cp.GetValue(this);
                        foreach (KeyValuePair<string, object> changedProp in dictionaryObject.ChangedProperties)
                        {
                            // Only include FieldValue properties when they signal they've changed
                            if (changedProp.Value is FieldValue changedPropAsFieldValue && changedPropAsFieldValue.HasChanges)
                            {
                                if (changedProp.Value is FieldLookupValue && (changedProp.Value as FieldLookupValue).LookupId == -1)
                                {
                                    fieldValues.Append(SetFieldValueAsNullXml(changedProp.Key, ref counter));
                                }
                                else
                                {
                                    fieldValues.Append(SetSpecialFieldValueXml(changedProp.Key, changedProp.Value as FieldValue, ref counter));
                                }
                            }
                            else if (changedProp.Value == null)
                            {
                                fieldValues.Append(SetFieldValueAsNullXml(changedProp.Key, ref counter));
                            }
                            else if (changedProp.Value is FieldValueCollection)
                            {
                                var collection = changedProp.Value as FieldValueCollection;
                                
                                // Only persist these fields if there was a change detected in the FieldValueCollection
                                if (collection.HasChanges)
                                {
                                    string typeAsString = collection.TypeAsString;
                                    if (string.IsNullOrEmpty(typeAsString))
                                    {
                                        var firstElement = collection.Values.FirstOrDefault();
                                        if (firstElement is FieldLookupValue)
                                        {
                                            typeAsString = "LookupMulti";
                                        }
                                        else if (firstElement is FieldTaxonomyValue)
                                        {
                                            typeAsString = "TaxonomyFieldTypeMulti";
                                        }
                                    }

                                    if (typeAsString == "LookupMulti" || typeAsString == "Lookup" || typeAsString == "UserMulti")
                                    {
                                        fieldValues.Append(SetArraySpecialFieldValueXml(changedProp.Key, changedProp.Value as FieldValueCollection, ref counter));
                                    }
                                    else if (typeAsString == "TaxonomyFieldTypeMulti")
                                    {
                                        fieldValues.Append(SetManagedMetadataMultiValueXml(changedProp.Key, changedProp.Value as FieldValueCollection,
                                            taxonomyMultiValueObjectPaths, taxonomyMultiValueIdentities, ref counter, ref taxFieldObjectId, ref taxFieldIdentityObjectId));
                                    }
                                }
                            }
                            else if (changedProp.Value is List<string>)
                            {
                                // multi value choice field
                                fieldValues.Append(SetArrayFieldValueXml(changedProp.Key, changedProp.Value as List<string>, ref counter));
                            }
                            else
                            {
                                // Let's set its value into the update message
                                fieldValues.Append(SetFieldValueXml(changedProp.Key, changedProp.Value, changedProp.Value?.GetType().Name, ref counter));
                            }
                        }
                    }
                    else
                    {
                        // Let's set its value into the update message
                        fieldValues.Append(SetFieldValueXml(changedField.SharePointName, GetValue(changedField.Name), changedField.DataType.Name, ref counter));
                    }
                }
            }

            // No changes, so bail out
            if (!changeFound)
            {
                return null;
            }

            // update field values
            xml = xml.Replace(CsomHelper.FieldValues, fieldValues.ToString());

            // update counter
            xml = xml.Replace(CsomHelper.Counter, counter.ToString());

            // replace the default taxonomy multi value field update placeholders
            xml = xml.Replace(CsomHelper.TaxonomyMultiValueIdentities, taxonomyMultiValueIdentities.ToString()); ;
            xml = xml.Replace(CsomHelper.TaxonomyMultiValueObjectPaths, taxonomyMultiValueObjectPaths.ToString());

            return xml;
        }

        private static string SetFieldValueAsNullXml(string fieldName, ref int counter)
        {
            string xml = CsomHelper.ListItemSystemUpdateSetFieldValueToNull;
            xml = xml.Replace(CsomHelper.Counter, counter.ToString());
            xml = xml.Replace(CsomHelper.FieldName, fieldName);

            counter++;
            return xml;
        }

        private static string SetManagedMetadataMultiValueXml(string fieldName, FieldValueCollection fieldValueCollection,
            StringBuilder taxonomyMultiValueObjectPaths, StringBuilder taxonomyMultiValueIdentities,
            ref int counter, ref int taxFieldObjectId, ref int taxFieldIdentityObjectId)
        {
            #region Sample XML snippets
            /*
            <Request AddExpandoFieldTypeSuffix="true" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0" ApplicationName=".NET Library"
                xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009">
                <Actions>
                    <ObjectPath Id="24" ObjectPathId="23" />
                    <Method Name="PopulateFromLabelGuidPairs" Id="25" ObjectPathId="23">
                        <Parameters>
                            <Parameter Type="String">MBI|1824510b-00e1-40ac-8294-528b1c9421e0;LBI|ed5449ec-4a4f-4102-8f07-5a207c438571</Parameter>
                        </Parameters>
                    </Method>
                    <Method Name="SetFieldValue" Id="26" ObjectPathId="13" Version="50">
                        <Parameters>
                            <Parameter Type="String">MMMultiple</Parameter>
                            <Parameter ObjectPathId="23" />
                        </Parameters>
                    </Method>
                    <Method Name="UpdateOverwriteVersion" Id="27" ObjectPathId="13" Version="50" />
                </Actions>
                <ObjectPaths>
                    <Constructor Id="23" TypeId="{c3dfae10-f3bf-4894-9012-bb60665b6d91}">
                        <Parameters>
                            <Parameter Type="Null" />
                            <Parameter ObjectPathId="19" />
                        </Parameters>
                    </Constructor>
                    <Identity Id="13" Name="86e78d9f-e06b-2000-915f-2825d672e6ac|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:b56adf79-ff6a-4964-a63a-ff1fa23be9f8:web:8c8e101c-1b0d-4253-85e7-c30039bf46e2:list:7d644d41-d86e-4594-99de-479594a68fd9:item:1,1" />
                    <Identity Id="19" Name="86e78d9f-90e2-2000-915f-217ff0ac791d|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:b56adf79-ff6a-4964-a63a-ff1fa23be9f8:web:8c8e101c-1b0d-4253-85e7-c30039bf46e2:list:7d644d41-d86e-4594-99de-479594a68fd9:field:9295d18b-0742-47f8-b3af-ee73c8b2692e" />
                </ObjectPaths>
            </Request>
            */
            #endregion

            // <Actions> content
            string xml = CsomHelper.ListItemTaxonomyMultiValueFieldAction;
            xml = xml.Replace(CsomHelper.Counter, counter.ToString());
            counter++;
            xml = xml.Replace(CsomHelper.Counter2, counter.ToString());
            counter++;
            xml = xml.Replace(CsomHelper.Counter3, counter.ToString());
            xml = xml.Replace(CsomHelper.TaxFieldObjectId, taxFieldObjectId.ToString());
            xml = xml.Replace(CsomHelper.FieldName, fieldName);

            StringBuilder sb = new StringBuilder();
            foreach (var fieldValue in fieldValueCollection.Values)
            {
                sb.Append($"{(fieldValue as FieldTaxonomyValue).Label}|{(fieldValue as FieldTaxonomyValue).TermId};");
            }
            xml = xml.Replace(CsomHelper.FieldValue, sb.ToString().TrimEnd(';'));

            // <ObjectPath> constructors
            taxonomyMultiValueObjectPaths.Append(CsomHelper.ListItemTaxonomyMultiValueFieldObjectPath
                .Replace(CsomHelper.TaxFieldObjectId, taxFieldObjectId.ToString())
                .Replace(CsomHelper.TaxFieldIdentityObjectId, taxFieldIdentityObjectId.ToString())
                );

            // <Identity> nodes
            taxonomyMultiValueIdentities.Append(CsomHelper.ListItemTaxonomyMultiValueFieldIdentity
                .Replace(CsomHelper.TaxFieldIdentityObjectId, taxFieldIdentityObjectId.ToString())
                .Replace(CsomHelper.TaxonomyFieldId, fieldValueCollection.Field.Id.ToString())
                );

            // update counters and return
            counter++;
            taxFieldObjectId++;
            taxFieldIdentityObjectId++;
            return xml;
        }

        private static string SetSpecialFieldValueXml(string fieldName, FieldValue fieldValue, ref int counter)
        {
            #region Sample XML snippets
            /* Sample XML snippet for Url field
            <Method Name="SetFieldValue" Id="17" ObjectPathId="13" Version="8">
                <Parameters>
                    <Parameter Type="String">Url</Parameter>
                    <Parameter TypeId="{fa8b44af-7b43-43f2-904a-bd319497011e}">
                        <Property Name="Description" Type="String">fdmslkfqmsl</Property>
                        <Property Name="Url" Type="String">https://bla1</Property>
                    </Parameter>
                </Parameters>
            </Method>

            Sample snippet for a user field
            <Method Name="SetFieldValue" Id="17" ObjectPathId="13" Version="11">
                <Parameters>
                    <Parameter Type="String">PersonSingle</Parameter>
                    <Parameter TypeId="{c956ab54-16bd-4c18-89d2-996f57282a6f}">
                        <Property Name="Email" Type="Null" />
                        <Property Name="LookupId" Type="Int32">6</Property>
                        <Property Name="LookupValue" Type="Null" />
                    </Parameter>
                </Parameters>
            </Method>

            Taxonomy field
            <Method Name="SetFieldValue" Id="17" ObjectPathId="13" Version="40">
                <Parameters>
                    <Parameter Type="String">MMSingle</Parameter>
                    <Parameter TypeId="{19e70ed0-4177-456b-8156-015e4d163ff8}">
                        <Property Name="Label" Type="String">MBI</Property>
                        <Property Name="TermGuid" Type="String">1824510b-00e1-40ac-8294-528b1c9421e0</Property>
                        <Property Name="WssId" Type="Int32">-1</Property>
                    </Parameter>
                </Parameters>
            </Method>
            */
            #endregion

            string xml = CsomHelper.ListItemSpecialField;

            xml = xml.Replace(CsomHelper.Counter, counter.ToString());
            xml = xml.Replace(CsomHelper.ObjectId, $"{{{fieldValue.CsomType}}}");
            xml = xml.Replace(CsomHelper.FieldName, fieldName);
            xml = xml.Replace(CsomHelper.FieldValues, fieldValue.ToCsomXml());
            xml = xml.Replace(CsomHelper.FieldType, "String");

            counter++;
            return xml;
        }

        private static string SetArraySpecialFieldValueXml(string fieldName, FieldValueCollection fieldValueCollection, ref int counter)
        {
            #region Sample XML snippets
            /* 
            Sample snippet for an array of User fields    
            <Method Name="SetFieldValue" Id="18" ObjectPathId="13" Version="28">
                <Parameters>
                    <Parameter Type="String">PersonMultiple</Parameter>
                    <Parameter Type="Array">
                        <Object TypeId="{c956ab54-16bd-4c18-89d2-996f57282a6f}">
                            <Property Name="Email" Type="Null" />
                            <Property Name="LookupId" Type="Int32">15</Property>
                            <Property Name="LookupValue" Type="Null" />
                        </Object>
                        <Object TypeId="{c956ab54-16bd-4c18-89d2-996f57282a6f}">
                            <Property Name="Email" Type="Null" />
                            <Property Name="LookupId" Type="Int32">6</Property>
                            <Property Name="LookupValue" Type="Null" />
                        </Object>
                    </Parameter>
                </Parameters>
            </Method>
            */
            #endregion

            string xml = CsomHelper.ListItemSpecialArrayField;

            xml = xml.Replace(CsomHelper.Counter, counter.ToString());
            xml = xml.Replace(CsomHelper.FieldName, fieldName);

            StringBuilder sb = new StringBuilder();
            foreach (var fieldValue in fieldValueCollection.Values)
            {
                sb.Append(CsomHelper.ListItemSpecialArrayObject
                    .Replace(CsomHelper.ObjectId, $"{{{(fieldValue as FieldValue).CsomType}}}")
                    .Replace(CsomHelper.FieldValue, (fieldValue as FieldValue).ToCsomXml())
                    );
            }
            xml = xml.Replace(CsomHelper.ArrayValues, sb.ToString());

            counter++;
            return xml;


        }

        private static string SetArrayFieldValueXml(string fieldName, IEnumerable fieldValue, ref int counter)
        {
            #region Sample XML snippets
            /*
            <Method Name="SetFieldValue" Id="18" ObjectPathId="13" Version="26">
                <Parameters>
                    <Parameter Type="String">ChoiceMultiple</Parameter>
                    <Parameter Type="Array">
                        <Object Type="String">Choice 2</Object>
                        <Object Type="String">Choice 3</Object>
                    </Parameter>
                </Parameters>
            </Method>              
             */
            #endregion

            string xml = CsomHelper.ListItemSystemUpdateSetArrayFieldValue;
            xml = xml.Replace(CsomHelper.Counter, counter.ToString());
            xml = xml.Replace(CsomHelper.FieldName, fieldName);

            StringBuilder sb = new StringBuilder();
            foreach (var item in fieldValue)
            {
                sb.Append(CsomHelper.ListItemArrayFieldProperty
                    .Replace(CsomHelper.FieldType, item.GetType().Name))
                    .Replace(CsomHelper.FieldValue, CsomHelper.XmlString(item.ToString())
                    );
            }
            xml = xml.Replace(CsomHelper.ArrayValues, sb.ToString());

            counter++;
            return xml;
        }

        private static string SetFieldValueXml(string fieldName, object fieldValue, string fieldType, ref int counter)
        {
            #region Sample XML snippets
            /*
            <Method Name="SetFieldValue" Id="17" ObjectPathId="13" Version="26">
                <Parameters>
                    <Parameter Type="String">ChoiceSingle</Parameter>
                    <Parameter Type="String">Choice 2</Parameter>
                </Parameters>
            </Method>
             */
            #endregion

            string xml = CsomHelper.ListItemSystemUpdateSetFieldValue;

            xml = xml.Replace(CsomHelper.Counter, counter.ToString());
            xml = xml.Replace(CsomHelper.FieldName, fieldName);
            xml = xml.Replace(CsomHelper.FieldValue, fieldValue == null ? "" : CsomHelper.XmlString(TypeSpecificHandling(fieldValue, fieldType), false));
            xml = xml.Replace(CsomHelper.FieldType, fieldType ?? "Null");

            counter++;
            return xml;
        }

        private static string TypeSpecificHandling(object value, string fieldType)
        {
            if (!string.IsNullOrEmpty(fieldType))
            {
                if (fieldType.Equals("Boolean"))
                {
                    return value.ToString().ToLowerInvariant();
                }
                else if (fieldType.Equals("DateTime"))
                {
                    if (value is DateTime time)
                    {
                        return time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz");
                    }
                }
                else if (fieldType.Equals("Double"))
                {
                    if (value is double doubleValue)
                    {
                        return doubleValue.ToString("G", CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    return value.ToString();
                }
            }

            return value.ToString();
        }
        #endregion

        #region Comment handling

        public bool AreCommentsDisabled()
        {
            return AreCommentsDisabledAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> AreCommentsDisabledAsync()
        {
            var apiCall = new ApiCall("_api/web/lists/getbyid(guid'{Parent.Id}')/items({Id})/CommentsDisabled", ApiType.SPORest);

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                var json = JsonDocument.Parse(response.Json).RootElement.GetProperty("d");

                if (json.TryGetProperty("CommentsDisabled", out JsonElement commentsDisabled))
                {
                    return commentsDisabled.GetBoolean();
                }
            }

            return false;
        }

        public void SetCommentsDisabled(bool commentsDisabled)
        {
            SetCommentsDisabledAsync(commentsDisabled).GetAwaiter().GetResult();
        }

        public async Task SetCommentsDisabledAsync(bool commentsDisabled)
        {
            // Build the API call to ensure this graph user as a SharePoint User in this site collection
            var parameters = new
            {
                value = commentsDisabled
            };

            string body = JsonSerializer.Serialize(parameters);

            var apiCall = new ApiCall("_api/web/lists/getbyid(guid'{Parent.Id}')/items({Id})/SetCommentsDisabled", ApiType.SPORest, body);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        #endregion

        #region Compliance Tag handling

        public void SetComplianceTag(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            SetComplianceTagAsync(complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock).GetAwaiter().GetResult();
        }

        public async Task SetComplianceTagAsync(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            ApiCall apiCall = SetComplianceTagApiCall(complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void SetComplianceTagBatch(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            SetComplianceTagBatchAsync(PnPContext.CurrentBatch, complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock).GetAwaiter().GetResult();
        }

        public async Task SetComplianceTagBatchAsync(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            await SetComplianceTagBatchAsync(PnPContext.CurrentBatch, complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock).ConfigureAwait(false);
        }

        public void SetComplianceTagBatch(Batch batch, string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            SetComplianceTagBatchAsync(batch, complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock).GetAwaiter().GetResult();
        }

        public async Task SetComplianceTagBatchAsync(Batch batch, string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            ApiCall apiCall = SetComplianceTagApiCall(complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private static ApiCall SetComplianceTagApiCall(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            var parameters = new
            {
                complianceTag,
                isTagPolicyHold,
                isTagPolicyRecord,
                isEventBasedTag,
                isTagSuperLock
            };
            string body = JsonSerializer.Serialize(parameters);
            var apiCall = new ApiCall("_api/web/lists/getbyid(guid'{Parent.Id}')/items({Id})/SetComplianceTag", ApiType.SPORest, body);
            return apiCall;
        }
        #endregion

        #region Recycle
        public Guid Recycle()
        {
            return RecycleAsync().GetAwaiter().GetResult();
        }

        public async Task<Guid> RecycleAsync()
        {
            ApiCall apiCall = BuildRecycleApiCall();

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                var document = JsonSerializer.Deserialize<JsonElement>(response.Json);
                if (document.TryGetProperty("d", out JsonElement root))
                {
                    if (root.TryGetProperty("Recycle", out JsonElement recycleBinItemId))
                    {
                        // return the recyclebin item id
                        return recycleBinItemId.GetGuid();
                    }
                }
            }

            return Guid.Empty;
        }

        public void RecycleBatch()
        {
            RecycleBatchAsync().GetAwaiter().GetResult();
        }

        public async Task RecycleBatchAsync()
        {
            await RecycleBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void RecycleBatch(Batch batch)
        {
            RecycleBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task RecycleBatchAsync(Batch batch)
        {
            ApiCall apiCall = BuildRecycleApiCall();
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private static ApiCall BuildRecycleApiCall()
        {
            return new ApiCall("_api/Web/Lists(guid'{Parent.Id}')/items({Id})/recycle", ApiType.SPORest)
            {
                RemoveFromModel = true
            };
        }
        #endregion

        #region Graph/Rest interoperability overrides
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal async override Task GraphToRestMetadataAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (IsPropertyAvailable(p => p.Id) && Id > 0)
            {
                if (!Metadata.ContainsKey(PnPConstants.MetaDataRestId))
                {
                    Metadata.Add(PnPConstants.MetaDataRestId, Id.ToString());
                }

                if (!Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                {
                    Metadata.Add(PnPConstants.MetaDataGraphId, Id.ToString());
                }
            }

            MetadataSetup();
        }

        internal void MetadataSetup()
        {
            if (Parent != null && Parent.Parent != null && Parent.Parent is IList)
            {
                if (!Metadata.ContainsKey(PnPConstants.MetaDataUri))
                {
                    AddMetadata(PnPConstants.MetaDataUri, $"{(Parent.Parent as List).GetMetadata(PnPConstants.MetaDataUri)}/Items({Id})");
                }
                if (!Metadata.ContainsKey(PnPConstants.MetaDataType))
                {
                    if ((Parent.Parent as List).IsPropertyAvailable(p => p.ListItemEntityTypeFullName))
                    {
                        AddMetadata(PnPConstants.MetaDataType, (Parent.Parent as List).ListItemEntityTypeFullName);
                    }
                    else if (!string.IsNullOrEmpty((Parent.Parent as List).GetMetadata(PnPConstants.MetaDataRestEntityTypeName)))
                    {
                        AddMetadata(PnPConstants.MetaDataType, $"SP.Data.{(Parent.Parent as List).GetMetadata(PnPConstants.MetaDataRestEntityTypeName)}Item");
                    }
                    else
                    {
                        AddMetadata(PnPConstants.MetaDataType, $"SP.Data.ListItem");
                    }
                }
            }
        }
        #endregion

        #region Special field handling
        public IFieldUrlValue NewFieldUrlValue(IField fieldToUpdate, string url, string description = null)
        {
            if (fieldToUpdate == null)
            {
                throw new ArgumentNullException(nameof(fieldToUpdate));
            }

            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            return new FieldUrlValue()
            {
                Url = url,
                Description = description ?? url,
                Field = fieldToUpdate
            };
        }

        public IFieldLookupValue NewFieldLookupValue(IField fieldToUpdate, int lookupId)
        {
            if (fieldToUpdate == null)
            {
                throw new ArgumentNullException(nameof(fieldToUpdate));
            }

            if (lookupId < -1)
            {
                throw new ArgumentNullException(nameof(lookupId));
            }

            return new FieldLookupValue()
            {
                LookupId = lookupId,
                Field = fieldToUpdate
            };
        }

        public IFieldUserValue NewFieldUserValue(IField fieldToUpdate, int userId)
        {
            if (fieldToUpdate == null)
            {
                throw new ArgumentNullException(nameof(fieldToUpdate));
            }

            if (userId < -1)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return new FieldUserValue()
            {
                LookupId = userId,
                Field = fieldToUpdate
            };
        }

        public IFieldUserValue NewFieldUserValue(IField fieldToUpdate, ISharePointPrincipal principal)
        {
            if (fieldToUpdate == null)
            {
                throw new ArgumentNullException(nameof(fieldToUpdate));
            }

            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            return new FieldUserValue()
            {
                Principal = principal,
                Field = fieldToUpdate
            };
        }

        public IFieldTaxonomyValue NewFieldTaxonomyValue(IField fieldToUpdate, Guid termId, string label, int wssId = -1)
        {
            if (fieldToUpdate == null)
            {
                throw new ArgumentNullException(nameof(fieldToUpdate));
            }

            if (termId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(termId));
            }

            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }

            return new FieldTaxonomyValue()
            {
                TermId = termId,
                Label = label,
                WssId = wssId,
                Field = fieldToUpdate
            };
        }

        public IFieldValueCollection NewFieldValueCollection(IField fieldToUpdate)
        {
            return new FieldValueCollection(fieldToUpdate, fieldToUpdate.InternalName);
        }
        #endregion

        #endregion
    }
}
