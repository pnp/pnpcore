using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
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
    [SharePointType("SP.ListItem", Uri = "_api/web/lists/getbyid(guid'{Parent.Id}')/items({Id})", LinqGet = "_api/web/lists(guid'{Parent.Id}')/items")]
    [GraphType(OverflowProperty = "fields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class ListItem
    {
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
                                    AddMetadata(PnPConstants.MetaDataUri, $"{parentList.GetMetadata(PnPConstants.MetaDataUri)}/Items({id})");
                                    AddMetadata(PnPConstants.MetaDataType, $"SP.Data.{parentList.GetMetadata("EntityTypeName")}Item");

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
                //var parentListTitle = !string.IsNullOrEmpty(parentList.GetMetadata(PnPConstants.MetaDataRestEntityTypeName)) ? parentList.GetMetadata(PnPConstants.MetaDataRestEntityTypeName)[0..^4] : null;
                var parentListTitle = !string.IsNullOrEmpty(parentList.GetMetadata(PnPConstants.MetaDataRestEntityTypeName)) ? parentList.GetMetadata(PnPConstants.MetaDataRestEntityTypeName).Substring(0, parentList.GetMetadata(PnPConstants.MetaDataRestEntityTypeName).Length - 4)  : null;

                // If this list we're adding items to was not fetched from the server than throw an error
                if (string.IsNullOrEmpty(parentListTitle) || string.IsNullOrEmpty(parentListUri))
                {
                    throw new ClientException(ErrorType.PropertyNotLoaded,
                        PnPCoreResources.Exception_PropertyNotLoaded_List);
                }

                // drop the everything in front of _api as the batching logic will add that automatically
                var baseApiCall = parentListUri.Substring(parentListUri.IndexOf("_api"));
                // little trick here to ensure we can construct the correct list url based upon the data returned by a default load
                // Ensure the underscore "_" character is not encoded in the FolderPath to use
                var serverRelativeUrl = $"{PnPContext.Uri}/lists/{parentListTitle}".Replace("_x005f_", "_");
                
                // Define the JSON body of the update request based on the actual changes
                dynamic body = new ExpandoObject();

                body.listItemCreateInfo = new
                {
                    FolderPath = new
                    {
                        DecodedUrl = $"{serverRelativeUrl}"
                    },
                    UnderlyingObjectType = 0,
                };
                body.bNewDocumentUpdate = false;

                // Add fields to the payload
                dynamic itemValues = new List<dynamic>();
                foreach (var item in Values)
                {
                    dynamic field = new ExpandoObject();
                    field.FieldName = item.Key;
                    field.FieldValue = item.Value;
                    itemValues.Add(field);
                }
                body.formValues = itemValues;

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = true });

                // Return created api call
                return new ApiCall($"{baseApiCall}/AddValidateUpdateItemUsingPath", ApiType.SPORest, bodyContent);
            };
        }

        #region Graph/Rest interoperability overrides
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal async override Task GraphToRestMetadataAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (this.IsPropertyAvailable(p => p.Id) && Id > 0)
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
        }

        #endregion

        #region Extension methods

        #region UpdateOverwriteVersion

        public async Task UpdateOverwriteVersionAsync()
        {
            var xmlPayload = BuildXmlPayload(true);
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
            var xmlPayload = BuildXmlPayload(true);
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
            var xmlPayload = BuildXmlPayload(false);
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
            var xmlPayload = BuildXmlPayload(false);
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

        private string BuildXmlPayload(bool updateOverwriteVersion)
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

            int counter = 1;
            StringBuilder fieldValues = new StringBuilder();

            var entity = EntityManager.GetClassInfo(GetType(), this);
            IEnumerable<EntityFieldInfo> fields = entity.Fields;

            var changedProperties = this.GetChangedProperties();

            bool changeFound = false;
            foreach (PropertyDescriptor cp in changedProperties)
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
                            // Let's set its value into the update message
                            fieldValues.AppendLine(SetFieldValueXml(changedProp.Key, changedProp.Value, changedProp.Value.GetType().Name, ref counter));
                        }
                    }
                    else
                    {
                        // Let's set its value into the update message
                        fieldValues.AppendLine(SetFieldValueXml(changedField.SharePointName, this.GetValue(changedField.Name), changedField.DataType.Name, ref counter));
                    }
                }
            }

            // No changes, so bail out
            if (!changeFound)
            {
                return null;
            }

            // update field values
            xml = xml.Replace("{FieldValues}", fieldValues.ToString());

            // update counter
            xml = xml.Replace("{Counter}", counter.ToString());

            return xml;
        }

        private static string SetFieldValueXml(string fieldName, object fieldValue, string fieldType, ref int counter)
        {
            string xml = CsomHelper.ListItemSystemUpdateSetFieldValue;

            xml = xml.Replace("{Counter}", counter.ToString());
            xml = xml.Replace("{FieldName}", fieldName);
            // TODO: verify complex fieldtypes
            xml = xml.Replace("{FieldValue}", CsomHelper.XmlString(fieldValue.ToString(), false));
            xml = xml.Replace("{FieldType}", fieldType);

            counter++;
            return xml;
        }
        #endregion

        #endregion

    }
}
