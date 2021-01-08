using Microsoft.Extensions.Logging;
using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
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
    internal partial class ListItem : ListItemBase<IListItem>, IListItem
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (keyValuePairs) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var parentList = Parent.Parent as List;
                // sample parent list uri: https://bertonline.sharepoint.com/sites/modern/_api/Web/Lists(guid'b2d52a36-52f1-48a4-b499-629063c6a38c')
                var parentListUri = parentList.GetMetadata(PnPConstants.MetaDataUri);
                // sample parent list entity type name: DemolistList
                var parentListTitle = !string.IsNullOrEmpty(parentList.GetMetadata(PnPConstants.MetaDataRestEntityTypeName)) ? parentList.GetMetadata(PnPConstants.MetaDataRestEntityTypeName).Substring(0, parentList.GetMetadata(PnPConstants.MetaDataRestEntityTypeName).Length - 4) : null;

                // If this list we're adding items to was not fetched from the server than throw an error
                string serverRelativeUrl = null;
                if (string.IsNullOrEmpty(parentListTitle) || string.IsNullOrEmpty(parentListUri))
                {
                    // Fall back to loading the rootfolder propery if we can't determine the list name
                    await parentList.EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
                    serverRelativeUrl = parentList.RootFolder.ServerRelativeUrl;
                }
                else
                {
                    // little trick here to ensure we can construct the correct list url based upon the data returned by a default load
                    // Ensure the underscore "_" character is not encoded in the FolderPath to use
                    serverRelativeUrl = $"{PnPContext.Uri}/lists/{parentListTitle}".Replace("_x005f_", "_");
                }

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

        #region Methods

        #region Item updates

        internal override async Task BaseUpdate(Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();

            var api = await BuildUpdateApiCallAsync(PnPContext).ConfigureAwait(false);

            // Add the request to the batch and execute the batch
            var batch = PnPContext.BatchClient.EnsureBatch();
            batch.Add(this, entityInfo, HttpMethod.Post, api, default, null, null);
            await PnPContext.BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
        }

        internal override async Task BaseBatchUpdateAsync(Batch batch, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();

            var api = await BuildUpdateApiCallAsync(PnPContext).ConfigureAwait(false);

            // Add the request to the batch
            batch.Add(this, entityInfo, HttpMethod.Post, api, default, null, null);
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
            var api = new ApiCall(updateUrl, ApiType.SPORest, jsonUpdateMessage);
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
            if (changedProp.Value is FieldValue fieldItemValue)
            {
                field.FieldValue = fieldItemValue.ToValidateUpdateItemJson();
            }
            else if (changedProp.Value is FieldValueCollection fieldValueCollection)
            {
                if (fieldValueCollection.Values.Any())
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

            return new FieldUrlValue(fieldToUpdate.InternalName, Values)
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

            return new FieldLookupValue(fieldToUpdate.InternalName, Values)
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

            return new FieldUserValue(fieldToUpdate.InternalName, Values)
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

            return new FieldUserValue(fieldToUpdate.InternalName, Values)
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

            return new FieldTaxonomyValue(fieldToUpdate.InternalName, Values)
            {
                TermId = termId,
                Label = label,
                WssId = wssId,
                Field = fieldToUpdate
            };
        }

        public IFieldValueCollection NewFieldValueCollection(IField fieldToUpdate, TransientDictionary parent)
        {
            return new FieldValueCollection(fieldToUpdate, fieldToUpdate.InternalName, parent);
        }
        #endregion

        #endregion
    }
}
