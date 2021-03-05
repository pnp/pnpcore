using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal class SyntexModel : ISyntexModel
    {
        #region Properties

        public IListItem ListItem { get; internal set; }

        public IFile File { get; internal set; }

        public int Id
        {
            get
            {
                if (ListItem != null)
                {
                    return ListItem.Id;
                }
                return 0;
            }
        }

        public Guid UniqueId
        {
            get
            {
                if (File != null)
                {
                    return File.UniqueId;
                }

                return Guid.Empty;
            }
        }

        public string Name
        {
            get
            {
                if (ListItem != null)
                {
                    return ListItem.Values[PageConstants.ModelMappedClassifierName].ToString();
                }
                return null;
            }
        }

        public DateTime ModelLastTrained
        {
            get
            {
                if (ListItem != null)
                {
                    if (ListItem.Values[PageConstants.ModelLastTrained] is DateTime dateTime)
                    {
                        return dateTime;
                    }

                    if (DateTime.TryParse(ListItem.Values[PageConstants.ModelLastTrained]?.ToString(), out DateTime modelLastTrained))
                    {
                        return modelLastTrained;
                    }
                }
                return DateTime.MinValue;
            }
        }

        public string Description
        {
            get
            {
                if (ListItem != null)
                {
                    return ListItem.Values[PageConstants.ModelDescription].ToString();
                }
                return null;
            }
        }

        #endregion

        #region Methods
        public ISyntexModelPublicationResult PublishModel(IList list, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            return PublishModelAsync(list, viewOption).GetAwaiter().GetResult();
        }

        public async Task<ISyntexModelPublicationResult> PublishModelAsync(IList list, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            // Ensure we have the needed data loaded
            await (list as List).EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
            await list.PnPContext.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

            return ProcessModelPublishResponse(await PublishModelApiCallAsync(UniqueId,
                                                                      list.PnPContext.Uri.ToString(),
                                                                      list.PnPContext.Web.ServerRelativeUrl,
                                                                      list.RootFolder.ServerRelativeUrl,
                                                                      viewOption).ConfigureAwait(false));
        }

        public ISyntexModelPublicationResult PublishModel(string targetLibraryServerRelativeUrl, string targetSiteUrl, string targetWebServerRelativeUrl, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            return PublishModelAsync(targetLibraryServerRelativeUrl, targetSiteUrl, targetWebServerRelativeUrl, viewOption).GetAwaiter().GetResult();
        }

        public async Task<ISyntexModelPublicationResult> PublishModelAsync(string targetLibraryServerRelativeUrl, string targetSiteUrl, string targetWebServerRelativeUrl, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            return ProcessModelPublishResponse(await PublishModelApiCallAsync(UniqueId,
                                                                      targetSiteUrl,
                                                                      targetWebServerRelativeUrl,
                                                                      targetLibraryServerRelativeUrl,
                                                                      viewOption).ConfigureAwait(false));
        }

        private static ISyntexModelPublicationResult ProcessModelPublishResponse(ApiCallResponse response)
        {
            if (!string.IsNullOrEmpty(response.Json))
            {
                var root = JsonDocument.Parse(response.Json).RootElement.GetProperty("d").GetProperty("Details").GetProperty("results");

                var modelPublicationResults = DeserializeModelPublishResult(root.ToString());

                var modelPublicationResult = modelPublicationResults.FirstOrDefault();
                if (modelPublicationResult is ISyntexModelPublicationResult syntexModelPublicationResult)
                {
                    return syntexModelPublicationResult;
                }
            }

            return null;
        }

        public async Task<ISyntexModelPublicationResult> UnPublishModelAsync(IList list)
        {
            // Ensure we have the needed data loaded
            await (list as List).EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
            await list.PnPContext.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

            return ProcessModelPublishResponse(await UnPublishModelApiCallAsync(UniqueId,
                                                                      list.PnPContext.Uri.ToString(),
                                                                      list.PnPContext.Web.ServerRelativeUrl,
                                                                      list.RootFolder.ServerRelativeUrl).ConfigureAwait(false));
        }

        public ISyntexModelPublicationResult UnPublishModel(IList list)
        {
            return UnPublishModelAsync(list).GetAwaiter().GetResult();
        }

        public async Task<ISyntexModelPublicationResult> UnPublishModelAsync(string targetLibraryServerRelativeUrl, string targetSiteUrl, string targetWebServerRelativeUrl)
        {
            return ProcessModelPublishResponse(await UnPublishModelApiCallAsync(UniqueId,
                                                                      targetSiteUrl,
                                                                      targetWebServerRelativeUrl,
                                                                      targetLibraryServerRelativeUrl).ConfigureAwait(false));
        }

        public ISyntexModelPublicationResult UnPublishModel(string targetLibraryServerRelativeUrl, string targetSiteUrl, string targetWebServerRelativeUrl)
        {
            return UnPublishModelAsync(targetLibraryServerRelativeUrl, targetSiteUrl, targetWebServerRelativeUrl).GetAwaiter().GetResult();
        }

        private async Task<ApiCallResponse> UnPublishModelApiCallAsync(Guid uniqueId, string targetSiteUrl, string targetWebServerRelativeUrl,
            string targetLibraryServerRelativeUrl)
        {
            /*
                {
                    "publications": [
                        {
                            "ModelUniqueId": "bb25a5be-aeed-436d-90e7-add975ac766e",
                            "TargetSiteUrl": "https://m365x215748.sharepoint.com/sites/AdminContentCenter",
                            "TargetWebServerRelativeUrl": "/sites/AdminContentCenter",
                            "TargetLibraryServerRelativeUrl": "/sites/AdminContentCenter/PNP_SDK_TEST_RegisterModelToList"
                        }
                    ]
                }            
            */

            var unPublishInfo = new
            {
                publications = new List<SyntexModelPublication>()
                {
                    new SyntexModelPublication()
                    {
                        ModelUniqueId = uniqueId,
                        TargetSiteUrl = targetSiteUrl,
                        TargetWebServerRelativeUrl = targetWebServerRelativeUrl,
                        TargetLibraryServerRelativeUrl = targetLibraryServerRelativeUrl,
                    }
                }
            }.AsExpando();

            string body = JsonSerializer.Serialize(unPublishInfo, new JsonSerializerOptions()
            {
                IgnoreNullValues = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            });

            var apiCall = new ApiCall("_api/machinelearning/publications/batchdelete", ApiType.SPORest, body);
            return await (ListItem as ListItem).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private async Task<ApiCallResponse> PublishModelApiCallAsync(Guid uniqueId, string targetSiteUrl, string targetWebServerRelativeUrl, 
            string targetLibraryServerRelativeUrl, MachineLearningPublicationViewOption viewOption)
        {
            /*
            {
                "__metadata": {
                    "type": "Microsoft.Office.Server.ContentCenter.SPMachineLearningPublicationsEntityData"
                },
                "Publications": {
                    "results": [
                        {
                            "ModelUniqueId": "bb25a5be-aeed-436d-90e7-add975ac766e",
                            "TargetSiteUrl": "https://m365x215748.sharepoint.com/sites/Mark8ProjectTeam",
                            "TargetWebServerRelativeUrl": "/sites/Mark8ProjectTeam",
                            "TargetLibraryServerRelativeUrl": "/sites/Mark8ProjectTeam/Shared Documents",
                            "ViewOption": "NewViewAsDefault"
                        }
                    ]
                },
                "Comment": ""
            }
            */

            var registerInfo = new
            {
                __metadata = new { type = "Microsoft.Office.Server.ContentCenter.SPMachineLearningPublicationsEntityData" },
                Publications = new
                {
                    results = new List<SyntexModelPublication>()
                    {
                        new SyntexModelPublication()
                        {
                            ModelUniqueId = uniqueId,
                            TargetSiteUrl = targetSiteUrl,
                            TargetWebServerRelativeUrl = targetWebServerRelativeUrl,
                            TargetLibraryServerRelativeUrl = targetLibraryServerRelativeUrl,
                            ViewOption = viewOption
                        }
                    }
                },
                Comment = ""
            }.AsExpando();

            string body = JsonSerializer.Serialize(registerInfo, new JsonSerializerOptions()
            {
                IgnoreNullValues = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            });

            var apiCall = new ApiCall("_api/machinelearning/publications", ApiType.SPORest, body);
            return await (ListItem as ListItem).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private static List<SyntexModelPublicationResult> DeserializeModelPublishResult(string jsonString)
        {
            /* Response:
            {
                "d": {
                    "__metadata": {
                        "id": "https://m365x215748.sharepoint.com/sites/AdminContentCenter/_api/machinelearning/publications",
                        "uri": "https://m365x215748.sharepoint.com/sites/AdminContentCenter/_api/machinelearning/publications",
                        "type": "Microsoft.Office.Server.ContentCenter.SPMachineLearningPublicationsResult"
                    },
                    "Details": {
                        "__metadata": {
                            "type": "Collection(Microsoft.Office.Server.ContentCenter.SPMachineLearningPublicationResult)"
                        },
                        "results": [
                            {
                                "ErrorMessage": null,
                                "Publication": {
                                    "__metadata": {
                                        "type": "Microsoft.Office.Server.ContentCenter.SPMachineLearningPublicationEntityData"
                                    },
                                    "ModelUniqueId": "bb25a5be-aeed-436d-90e7-add975ac766e",
                                    "TargetLibraryServerRelativeUrl": "/sites/AdminContentCenter/PNP_SDK_TEST_RegisterModelToList",
                                    "TargetSiteUrl": "https://m365x215748.sharepoint.com/sites/AdminContentCenter",
                                    "TargetWebServerRelativeUrl": "/sites/AdminContentCenter",
                                    "ViewOption": "NewViewAsDefault"
                                },
                                "StatusCode": 201
                            }
                        ]
                    },
                    "TotalFailures": 0,
                    "TotalSuccesses": 1
                }
            }
            */

            List<SyntexModelPublicationResult> results = new List<SyntexModelPublicationResult>();

            var json = JsonSerializer.Deserialize<JsonElement>(jsonString);
            if (json.ValueKind == JsonValueKind.Array)
            {
                foreach (var publicationResultJson in json.EnumerateArray())
                {
                    var publicationResult = new SyntexModelPublicationResult
                    {
                        ErrorMessage = publicationResultJson.GetProperty("ErrorMessage").GetString(),
                        StatusCode = publicationResultJson.GetProperty("StatusCode").GetInt32(),
                    };

                    if (publicationResultJson.TryGetProperty("Publication", out JsonElement publicationJson))
                    {
                        publicationResult.Publication = new SyntexModelPublication
                        {
                            ModelUniqueId = publicationJson.GetProperty("ModelUniqueId").GetGuid(),
                            TargetLibraryServerRelativeUrl = publicationJson.GetProperty("TargetLibraryServerRelativeUrl").GetString(),
                            TargetWebServerRelativeUrl = publicationJson.GetProperty("TargetWebServerRelativeUrl").GetString(),
                            TargetSiteUrl = publicationJson.GetProperty("TargetSiteUrl").ToString(),
                            ViewOption = (MachineLearningPublicationViewOption)Enum.Parse(typeof(MachineLearningPublicationViewOption), publicationJson.GetProperty("ViewOption").ToString())
                        };
                    }

                    results.Add(publicationResult);
                }
            }
            return results;
        }
        #endregion
    }
}
