using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ListItem class, write your custom code here
    /// </summary>
    [SharePointType("SP.ListItem", Uri = "_api/web/lists/getbyid(guid'{Parent.Id}')/items({Id})", LinqGet = "_api/web/lists(guid'{Parent.Id}')/items")]
    [GraphType(OverflowProperty = "fields")] // , LinqGet = "sites/{Web.GraphId}/lists/{Parent.GraphId}/items")]
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
                    input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");
                }

                return null;
            };

            PostMappingHandler = (string json) =>
            {
                // Extra processing of returned json
            };

            AddApiCallHandler = (keyValuePairs) =>
            {
                var parentList = Parent.Parent as List;
                // sample parent list uri: https://bertonline.sharepoint.com/sites/modern/_api/Web/Lists(guid'b2d52a36-52f1-48a4-b499-629063c6a38c')
                var parentListUri = parentList.GetMetadata(PnPConstants.MetaDataUri);
                // sample parent list entity type name: DemolistList
                var parentListTitle = !string.IsNullOrEmpty(parentList.GetMetadata(PnPConstants.MetaDataRestEntityTypeName)) ? parentList.GetMetadata(PnPConstants.MetaDataRestEntityTypeName)[0..^4] : null;

                // If this list we're adding items to was not fetched from the server than throw an error
                if (string.IsNullOrEmpty(parentListTitle) || string.IsNullOrEmpty(parentListUri))
                {
                    throw new ClientException(ErrorType.PropertyNotLoaded, $"Please first fetch this list from SharePoint before doing operations on it");
                }

                // drop the everything in front of _api as the batching logic will add that automatically
                var baseApiCall = parentListUri.Substring(parentListUri.IndexOf("_api"));
                // little trick here to ensure we can construct the correct list url based upon the data returned by a default load
                var serverRelativeUrl = $"{PnPContext.Uri}/lists/{parentListTitle}";

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
                foreach(var item in Values)
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

    }
}
