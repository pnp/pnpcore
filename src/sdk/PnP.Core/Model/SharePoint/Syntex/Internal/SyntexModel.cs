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
        public ISyntexModelPublicationResult PublishModel(IList library, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            return PublishModelAsync(library, viewOption).GetAwaiter().GetResult();
        }

        public async Task<ISyntexModelPublicationResult> PublishModelAsync(IList library, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            // Ensure we have the needed data loaded
            await (library as List).EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
            await library.PnPContext.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

            return ProcessModelPublishResponse(await PublishModelApiRequestAsync(UniqueId,
                                                                      library.PnPContext.Uri.ToString(),
                                                                      library.PnPContext.Web.ServerRelativeUrl,
                                                                      library.RootFolder.ServerRelativeUrl,
                                                                      viewOption).ConfigureAwait(false));
        }

        public List<ISyntexModelPublicationResult> PublishModel(List<IList> libraries, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            return PublishModelAsync(libraries, viewOption).GetAwaiter().GetResult();
        }

        public async Task<List<ISyntexModelPublicationResult>> PublishModelAsync(List<IList> libraries, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            List<SyntexModelPublication> modelPublications = new List<SyntexModelPublication>();
            foreach (var library in libraries)
            {
                await (library as List).EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
                await library.PnPContext.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

                modelPublications.Add(new SyntexModelPublication()
                {
                    ModelUniqueId = UniqueId,
                    TargetSiteUrl = library.PnPContext.Uri.ToString(),
                    TargetWebServerRelativeUrl = library.PnPContext.Web.ServerRelativeUrl,
                    TargetLibraryServerRelativeUrl = library.RootFolder.ServerRelativeUrl,
                    ViewOption = viewOption
                });
            }
            return ProcessModelPublishResponseMultiple(await PublishModelApiRequestAsync(modelPublications).ConfigureAwait(false));
        }

        public ISyntexModelPublicationResult PublishModel(SyntexModelPublicationOptions publicationOptions)
        {
            return PublishModelAsync(publicationOptions).GetAwaiter().GetResult();
        }

        public async Task<ISyntexModelPublicationResult> PublishModelAsync(SyntexModelPublicationOptions publicationOptions)
        {
            return ProcessModelPublishResponse(await PublishModelApiRequestAsync(UniqueId,
                                                        publicationOptions.TargetSiteUrl,
                                                        publicationOptions.TargetWebServerRelativeUrl,
                                                        publicationOptions.TargetLibraryServerRelativeUrl,
                                                        publicationOptions.ViewOption).ConfigureAwait(false));
        }

        public async Task<List<ISyntexModelPublicationResult>> PublishModelAsync(List<SyntexModelPublicationOptions> publicationOptions)
        {
            List<SyntexModelPublication> modelPublications = new List<SyntexModelPublication>();
            foreach (var publication in publicationOptions)
            {
                modelPublications.Add(new SyntexModelPublication()
                {
                    ModelUniqueId = UniqueId,
                    TargetSiteUrl = publication.TargetSiteUrl,
                    TargetWebServerRelativeUrl = publication.TargetWebServerRelativeUrl,
                    TargetLibraryServerRelativeUrl = publication.TargetLibraryServerRelativeUrl,
                    ViewOption = publication.ViewOption
                });
            }
            return ProcessModelPublishResponseMultiple(await PublishModelApiRequestAsync(modelPublications).ConfigureAwait(false));
        }

        public List<ISyntexModelPublicationResult> PublishModel(List<SyntexModelPublicationOptions> publicationOptions)
        {
            return PublishModelAsync(publicationOptions).GetAwaiter().GetResult();
        }

        private static ISyntexModelPublicationResult ProcessModelPublishResponse(ApiCallResponse response)
        {
            if (!string.IsNullOrEmpty(response.Json))
            {
                List<SyntexModelPublicationResult> modelPublicationResults = ParseModelPublishResponse(response);

                var modelPublicationResult = modelPublicationResults.FirstOrDefault();
                if (modelPublicationResult is ISyntexModelPublicationResult syntexModelPublicationResult)
                {
                    return syntexModelPublicationResult;
                }
            }

            return null;
        }

        private static List<ISyntexModelPublicationResult> ProcessModelPublishResponseMultiple(ApiCallResponse response)
        {
            if (!string.IsNullOrEmpty(response.Json))
            {
                return ParseModelPublishResponse(response).Cast<ISyntexModelPublicationResult>().ToList();
            }

            return null;
        }

        private static List<SyntexModelPublicationResult> ParseModelPublishResponse(ApiCallResponse response)
        {
            var root = JsonDocument.Parse(response.Json).RootElement.GetProperty("d").GetProperty("Details").GetProperty("results");
            var modelPublicationResults = DeserializeModelPublishResult(root.ToString());
            return modelPublicationResults;
        }

        public async Task<ISyntexModelPublicationResult> UnPublishModelAsync(IList library)
        {
            // Ensure we have the needed data loaded
            await (library as List).EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
            await library.PnPContext.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

            return ProcessModelPublishResponse(await UnPublishModelApiRequestAsync(UniqueId,
                                                                      library.PnPContext.Uri.ToString(),
                                                                      library.PnPContext.Web.ServerRelativeUrl,
                                                                      library.RootFolder.ServerRelativeUrl).ConfigureAwait(false));
        }

        public ISyntexModelPublicationResult UnPublishModel(IList library)
        {
            return UnPublishModelAsync(library).GetAwaiter().GetResult();
        }

        public async Task<List<ISyntexModelPublicationResult>> UnPublishModelAsync(List<IList> libraries)
        {
            List<SyntexModelPublication> modelPublications = new List<SyntexModelPublication>();
            foreach (var library in libraries)
            {
                await (library as List).EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
                await library.PnPContext.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

                modelPublications.Add(new SyntexModelPublication()
                {
                    ModelUniqueId = UniqueId,
                    TargetSiteUrl = library.PnPContext.Uri.ToString(),
                    TargetWebServerRelativeUrl = library.PnPContext.Web.ServerRelativeUrl,
                    TargetLibraryServerRelativeUrl = library.RootFolder.ServerRelativeUrl
                });
            }
            return ProcessModelPublishResponseMultiple(await UnPublishModelApiRequestAsync(modelPublications).ConfigureAwait(false));
        }

        public List<ISyntexModelPublicationResult> UnPublishModel(List<IList> libraries)
        {
            return UnPublishModelAsync(libraries).GetAwaiter().GetResult();
        }

        public async Task<ISyntexModelPublicationResult> UnPublishModelAsync(SyntexModelUnPublicationOptions unPublicationOptions)
        {
            return ProcessModelPublishResponse(await UnPublishModelApiRequestAsync(UniqueId,
                                                        unPublicationOptions.TargetSiteUrl,
                                                        unPublicationOptions.TargetWebServerRelativeUrl,
                                                        unPublicationOptions.TargetLibraryServerRelativeUrl).ConfigureAwait(false));
        }

        public ISyntexModelPublicationResult UnPublishModel(SyntexModelUnPublicationOptions unPublicationOptions)
        {
            return UnPublishModelAsync(unPublicationOptions).GetAwaiter().GetResult();
        }

        public async Task<List<ISyntexModelPublicationResult>> UnPublishModelAsync(List<SyntexModelUnPublicationOptions> unPublicationOptions)
        {
            List<SyntexModelPublication> modelPublications = new List<SyntexModelPublication>();
            foreach (var publication in unPublicationOptions)
            {
                modelPublications.Add(new SyntexModelPublication()
                {
                    ModelUniqueId = UniqueId,
                    TargetSiteUrl = publication.TargetSiteUrl,
                    TargetWebServerRelativeUrl = publication.TargetWebServerRelativeUrl,
                    TargetLibraryServerRelativeUrl = publication.TargetLibraryServerRelativeUrl,
                });
            }
            return ProcessModelPublishResponseMultiple(await UnPublishModelApiRequestAsync(modelPublications).ConfigureAwait(false));
        }

        public List<ISyntexModelPublicationResult> UnPublishModel(List<SyntexModelUnPublicationOptions> unPublicationOptions)
        {
            return UnPublishModelAsync(unPublicationOptions).GetAwaiter().GetResult();
        }

        private async Task<ApiCallResponse> UnPublishModelApiRequestAsync(Guid uniqueId, string targetSiteUrl, string targetWebServerRelativeUrl,
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

            return await UnPublishModelApiCallAsync(unPublishInfo).ConfigureAwait(false);
        }

        private async Task<ApiCallResponse> UnPublishModelApiRequestAsync(List<SyntexModelPublication> modelUnPublications)
        {
            var unPublishInfo = new
            {
                publications = modelUnPublications
            }.AsExpando();

            return await UnPublishModelApiCallAsync(unPublishInfo).ConfigureAwait(false);
        }

        private async Task<ApiCallResponse> UnPublishModelApiCallAsync(System.Dynamic.ExpandoObject unPublishInfo)
        {
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

        private async Task<ApiCallResponse> PublishModelApiRequestAsync(Guid uniqueId, string targetSiteUrl, string targetWebServerRelativeUrl, 
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

            return await PublishModelApiCallAsync(registerInfo).ConfigureAwait(false);
        }

        private async Task<ApiCallResponse> PublishModelApiRequestAsync(List<SyntexModelPublication> modelPublications)
        {
            var registerInfo = new
            {
                __metadata = new { type = "Microsoft.Office.Server.ContentCenter.SPMachineLearningPublicationsEntityData" },
                Publications = new
                {
                    results = modelPublications
                },
                Comment = ""
            }.AsExpando();

            return await PublishModelApiCallAsync(registerInfo).ConfigureAwait(false);
        }

        private async Task<ApiCallResponse> PublishModelApiCallAsync(System.Dynamic.ExpandoObject registerInfo)
        {
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
