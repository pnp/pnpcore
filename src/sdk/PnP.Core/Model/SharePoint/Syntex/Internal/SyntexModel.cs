using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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
                    return ListItem.Values[PageConstants.ModelDescription]?.ToString();
                }
                return null;
            }
        }

        #endregion

        #region Methods

        #region Get published models

        public async Task<List<ISyntexModelPublication>> GetModelPublicationsAsync()
        {
            ApiCall apiCall = BuildGetModelPublicationsApiCall();
            var publications = await (ListItem as ListItem).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            List<ISyntexModelPublication> results = new List<ISyntexModelPublication>();

            if (!string.IsNullOrEmpty(publications.Json))
            {
                ProcessGetModelPublicationsResponse(publications.Json, results);
            }

            return results;
        }

        public List<ISyntexModelPublication> GetModelPublications()
        {
            return GetModelPublicationsAsync().GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<ISyntexModelPublication>> GetModelPublicationsBatchAsync(Batch batch)
        {
            ApiCall apiCall = BuildGetModelPublicationsApiCall();

            // Since we're doing a raw batch request the processing of the batch response needs be implemented
            apiCall.RawEnumerableResult = new List<ISyntexModelPublication>();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                ProcessGetModelPublicationsResponse(json, (List<ISyntexModelPublication>)apiCall.RawEnumerableResult);
            };

            // Add the request to the batch
            var batchRequest = await (ListItem as ListItem).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);

            // Return the batch result as Enumerable
            return new BatchEnumerableBatchResult<ISyntexModelPublication>(batch, batchRequest.Id, (IReadOnlyList<ISyntexModelPublication>)apiCall.RawEnumerableResult);
        }

        public IEnumerableBatchResult<ISyntexModelPublication> GetModelPublicationsBatch(Batch batch)
        {
            return GetModelPublicationsBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<ISyntexModelPublication>> GetModelPublicationsBatchAsync()
        {
            return await GetModelPublicationsBatchAsync((ListItem as ListItem).PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<ISyntexModelPublication> GetModelPublicationsBatch()
        {
            return GetModelPublicationsBatchAsync().GetAwaiter().GetResult();
        }

        private ApiCall BuildGetModelPublicationsApiCall()
        {
            return new ApiCall($"_api/machinelearning/publications/getbymodeluniqueid('{UniqueId}')", ApiType.SPORest);
        }

        private static void ProcessGetModelPublicationsResponse(string json, List<ISyntexModelPublication> results)
        {
            var root = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("d").GetProperty("results");
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var publicationResultJson in root.EnumerateArray())
                {
                    var modelPublication = new SyntexModelPublication
                    {
                        ModelUniqueId = publicationResultJson.GetProperty("ModelUniqueId").GetGuid(),
                        TargetLibraryServerRelativeUrl = publicationResultJson.GetProperty("TargetLibraryServerRelativeUrl").GetString(),
                        TargetWebServerRelativeUrl = publicationResultJson.GetProperty("TargetWebServerRelativeUrl").GetString(),
                        TargetSiteUrl = publicationResultJson.GetProperty("TargetSiteUrl").ToString(),
                        ViewOption = (MachineLearningPublicationViewOption)Enum.Parse(typeof(MachineLearningPublicationViewOption), publicationResultJson.GetProperty("ViewOption").ToString())
                    };

                    results.Add(modelPublication);
                }
            }
        }
        #endregion

        #region Model Publication

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
                                                                      library.PnPContext.Uri.AbsoluteUri.ToString(),
                                                                      library.PnPContext.Web.ServerRelativeUrl,
                                                                      library.RootFolder.ServerRelativeUrl,
                                                                      viewOption).ConfigureAwait(false));
        }

        public async Task<IBatchSingleResult<ISyntexModelPublicationResult>> PublishModelBatchAsync(Batch batch, IList library, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            // Ensure we have the needed data loaded
            await (library as List).EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
            await library.PnPContext.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

            return await PublishModelBatchImplementationAsync(batch, UniqueId,
                                                                      library.PnPContext.Uri.AbsoluteUri.ToString(),
                                                                      library.PnPContext.Web.ServerRelativeUrl,
                                                                      library.RootFolder.ServerRelativeUrl,
                                                                      viewOption).ConfigureAwait(false);
        }

        private async Task<IBatchSingleResult<ISyntexModelPublicationResult>> PublishModelBatchImplementationAsync(Batch batch, Guid uniqueId, string targetSiteUrl, string targetWebServerRelativeUrl,
                                                        string targetLibraryServerRelativeUrl, MachineLearningPublicationViewOption viewOption)
        {
            System.Dynamic.ExpandoObject registerInfo = PublishModelApiRequestBody(uniqueId,
                                                                                  targetSiteUrl,
                                                                                  targetWebServerRelativeUrl,
                                                                                  targetLibraryServerRelativeUrl,
                                                                                  viewOption);

            string body = JsonSerializer.Serialize(registerInfo, PnPConstants.JsonSerializer_IgnoreNullValues_StringEnumConvertor);

            var apiCall = new ApiCall("_api/machinelearning/publications", ApiType.SPORest, body)
            {
                RawSingleResult = new SyntexModelPublicationResult(),
                RawResultsHandler = (json, apiCall) =>
                {
                    List<SyntexModelPublicationResult> modelPublicationResults = ParseModelPublishResponse(json);
                    var firstModelPublicationResult = modelPublicationResults.First();
                    (apiCall.RawSingleResult as SyntexModelPublicationResult).ErrorMessage = firstModelPublicationResult.ErrorMessage;
                    (apiCall.RawSingleResult as SyntexModelPublicationResult).Publication = firstModelPublicationResult.Publication;
                    (apiCall.RawSingleResult as SyntexModelPublicationResult).StatusCode = firstModelPublicationResult.StatusCode;
                }
            };
            var batchRequest = await (ListItem as ListItem).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);

            return new BatchSingleResult<ISyntexModelPublicationResult>(batch, batchRequest.Id, apiCall.RawSingleResult as ISyntexModelPublicationResult);
        }

        public IBatchSingleResult<ISyntexModelPublicationResult> PublishModelBatch(Batch batch, IList library, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            return PublishModelBatchAsync(batch, library, viewOption).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<ISyntexModelPublicationResult>> PublishModelBatchAsync(IList library, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            return await PublishModelBatchAsync((ListItem as ListItem).PnPContext.CurrentBatch, library, viewOption).ConfigureAwait(false);
        }

        public IBatchSingleResult<ISyntexModelPublicationResult> PublishModelBatch(IList library, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            return PublishModelBatchAsync(library, viewOption).GetAwaiter().GetResult();
        }

        public async Task<List<ISyntexModelPublicationResult>> PublishModelAsync(List<IList> libraries, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            List<SyntexModelPublication> modelPublications = await BuildModelPublicationListAsync(libraries, viewOption).ConfigureAwait(false);
            return ProcessModelPublishResponseMultiple(await PublishModelApiRequestAsync(modelPublications).ConfigureAwait(false));
        }

        private async Task<List<SyntexModelPublication>> BuildModelPublicationListAsync(List<IList> libraries, MachineLearningPublicationViewOption viewOption)
        {
            List<SyntexModelPublication> modelPublications = new List<SyntexModelPublication>();
            foreach (var library in libraries)
            {
                await (library as List).EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
                await library.PnPContext.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

                modelPublications.Add(new SyntexModelPublication()
                {
                    ModelUniqueId = UniqueId,
                    TargetSiteUrl = library.PnPContext.Uri.AbsoluteUri.ToString(),
                    TargetWebServerRelativeUrl = library.PnPContext.Web.ServerRelativeUrl,
                    TargetLibraryServerRelativeUrl = library.RootFolder.ServerRelativeUrl,
                    ViewOption = viewOption
                });
            }

            return modelPublications;
        }

        public List<ISyntexModelPublicationResult> PublishModel(List<IList> libraries, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            return PublishModelAsync(libraries, viewOption).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<ISyntexModelPublicationResult>> PublishModelBatchAsync(Batch batch, List<IList> libraries, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            List<SyntexModelPublication> modelPublications = await BuildModelPublicationListAsync(libraries, viewOption).ConfigureAwait(false);
            return await PublishModelBatchImplementationAsync(batch, modelPublications).ConfigureAwait(false);
        }

        private async Task<IEnumerableBatchResult<ISyntexModelPublicationResult>> PublishModelBatchImplementationAsync(Batch batch, List<SyntexModelPublication> modelPublications)
        {
            System.Dynamic.ExpandoObject registerInfo = PublishModelApiRequestBody(modelPublications);
            string body = JsonSerializer.Serialize(registerInfo, PnPConstants.JsonSerializer_IgnoreNullValues_StringEnumConvertor);

            var apiCall = new ApiCall("_api/machinelearning/publications", ApiType.SPORest, body)
            {
                RawEnumerableResult = new List<ISyntexModelPublicationResult>(),
                RawResultsHandler = (json, apiCall) =>
                {
                    (apiCall.RawEnumerableResult as List<ISyntexModelPublicationResult>).AddRange(ParseModelPublishResponse(json).Cast<ISyntexModelPublicationResult>());
                }
            };

            var batchRequest = await (ListItem as ListItem).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            return new BatchEnumerableBatchResult<ISyntexModelPublicationResult>(batch, batchRequest.Id, (IReadOnlyList<ISyntexModelPublicationResult>)apiCall.RawEnumerableResult);
        }

        public IEnumerableBatchResult<ISyntexModelPublicationResult> PublishModelBatch(Batch batch, List<IList> libraries, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            return PublishModelBatchAsync(batch, libraries, viewOption).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<ISyntexModelPublicationResult>> PublishModelBatchAsync(List<IList> libraries, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            return await PublishModelBatchAsync((ListItem as ListItem).PnPContext.CurrentBatch, libraries, viewOption).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<ISyntexModelPublicationResult> PublishModelBatch(List<IList> libraries, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault)
        {
            return PublishModelBatchAsync(libraries, viewOption).GetAwaiter().GetResult();
        }

        public async Task<ISyntexModelPublicationResult> PublishModelAsync(SyntexModelPublishOptions publicationOptions)
        {
            return ProcessModelPublishResponse(await PublishModelApiRequestAsync(UniqueId,
                                                        publicationOptions.TargetSiteUrl,
                                                        publicationOptions.TargetWebServerRelativeUrl,
                                                        publicationOptions.TargetLibraryServerRelativeUrl,
                                                        publicationOptions.ViewOption).ConfigureAwait(false));
        }

        public ISyntexModelPublicationResult PublishModel(SyntexModelPublishOptions publicationOptions)
        {
            return PublishModelAsync(publicationOptions).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<ISyntexModelPublicationResult>> PublishModelBatchAsync(Batch batch, SyntexModelPublishOptions publicationOptions)
        {
            return await PublishModelBatchImplementationAsync(batch, UniqueId,
                                                                      publicationOptions.TargetSiteUrl,
                                                                      publicationOptions.TargetWebServerRelativeUrl,
                                                                      publicationOptions.TargetLibraryServerRelativeUrl,
                                                                      publicationOptions.ViewOption).ConfigureAwait(false);
        }

        public IBatchSingleResult<ISyntexModelPublicationResult> PublishModelBatch(Batch batch, SyntexModelPublishOptions publicationOptions)
        {
            return PublishModelBatchAsync(batch, publicationOptions).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<ISyntexModelPublicationResult>> PublishModelBatchAsync(SyntexModelPublishOptions publicationOptions)
        {
            return await PublishModelBatchAsync((ListItem as ListItem).PnPContext.CurrentBatch, publicationOptions).ConfigureAwait(false);
        }

        public IBatchSingleResult<ISyntexModelPublicationResult> PublishModelBatch(SyntexModelPublishOptions publicationOptions)
        {
            return PublishModelBatchAsync(publicationOptions).GetAwaiter().GetResult();
        }

        public async Task<List<ISyntexModelPublicationResult>> PublishModelAsync(List<SyntexModelPublishOptions> publicationOptions)
        {
            List<SyntexModelPublication> modelPublications = BuildModelPublicationList(publicationOptions);
            return ProcessModelPublishResponseMultiple(await PublishModelApiRequestAsync(modelPublications).ConfigureAwait(false));
        }

        private List<SyntexModelPublication> BuildModelPublicationList(List<SyntexModelPublishOptions> publicationOptions)
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

            return modelPublications;
        }

        public List<ISyntexModelPublicationResult> PublishModel(List<SyntexModelPublishOptions> publicationOptions)
        {
            return PublishModelAsync(publicationOptions).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<ISyntexModelPublicationResult>> PublishModelBatchAsync(Batch batch, List<SyntexModelPublishOptions> publicationOptions)
        {
            List<SyntexModelPublication> modelPublications = BuildModelPublicationList(publicationOptions);
            return await PublishModelBatchImplementationAsync(batch, modelPublications).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<ISyntexModelPublicationResult> PublishModelBatch(Batch batch, List<SyntexModelPublishOptions> publicationOptions)
        {
            return PublishModelBatchAsync(batch, publicationOptions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Publish this model to a list of libraries
        /// </summary>
        /// <param name="publicationOptions">Information defining the model publications</param>
        /// <returns>Information about the model publications</returns>
        public async Task<IEnumerableBatchResult<ISyntexModelPublicationResult>> PublishModelBatchAsync(List<SyntexModelPublishOptions> publicationOptions)
        {
            return await PublishModelBatchAsync((ListItem as ListItem).PnPContext.CurrentBatch, publicationOptions).ConfigureAwait(false);
        }

        /// <summary>
        /// Publish this model to a list of libraries
        /// </summary>
        /// <param name="publicationOptions">Information defining the model publications</param>
        /// <returns>Information about the model publications</returns>
        public IEnumerableBatchResult<ISyntexModelPublicationResult> PublishModelBatch(List<SyntexModelPublishOptions> publicationOptions)
        {
            return PublishModelBatchAsync(publicationOptions).GetAwaiter().GetResult();
        }

        private static ISyntexModelPublicationResult ProcessModelPublishResponse(ApiCallResponse response)
        {
            if (!string.IsNullOrEmpty(response.Json))
            {
                List<SyntexModelPublicationResult> modelPublicationResults = ParseModelPublishResponse(response.Json);

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
                return ParseModelPublishResponse(response.Json).Cast<ISyntexModelPublicationResult>().ToList();
            }

            return null;
        }

        private static List<SyntexModelPublicationResult> ParseModelPublishResponse(string json)
        {
            var root = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("d").GetProperty("Details").GetProperty("results");
            var modelPublicationResults = DeserializeModelPublishResult(root.ToString());
            return modelPublicationResults;
        }

        private async Task<ApiCallResponse> PublishModelApiRequestAsync(Guid uniqueId, string targetSiteUrl, string targetWebServerRelativeUrl,
                                                        string targetLibraryServerRelativeUrl, MachineLearningPublicationViewOption viewOption)
        {
            System.Dynamic.ExpandoObject registerInfo = PublishModelApiRequestBody(uniqueId, targetSiteUrl, targetWebServerRelativeUrl, targetLibraryServerRelativeUrl, viewOption);

            return await PublishModelApiCallAsync(registerInfo).ConfigureAwait(false);
        }

        private static System.Dynamic.ExpandoObject PublishModelApiRequestBody(Guid uniqueId, string targetSiteUrl, string targetWebServerRelativeUrl, string targetLibraryServerRelativeUrl, MachineLearningPublicationViewOption viewOption)
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

            return new
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
        }

        private async Task<ApiCallResponse> PublishModelApiRequestAsync(List<SyntexModelPublication> modelPublications)
        {
            System.Dynamic.ExpandoObject registerInfo = PublishModelApiRequestBody(modelPublications);

            return await PublishModelApiCallAsync(registerInfo).ConfigureAwait(false);
        }

        private static System.Dynamic.ExpandoObject PublishModelApiRequestBody(List<SyntexModelPublication> modelPublications)
        {
            return new
            {
                __metadata = new { type = "Microsoft.Office.Server.ContentCenter.SPMachineLearningPublicationsEntityData" },
                Publications = new
                {
                    results = modelPublications
                },
                Comment = ""
            }.AsExpando();
        }

        private async Task<ApiCallResponse> PublishModelApiCallAsync(System.Dynamic.ExpandoObject registerInfo)
        {
            string body = JsonSerializer.Serialize(registerInfo, PnPConstants.JsonSerializer_IgnoreNullValues_StringEnumConvertor);

            var apiCall = new ApiCall("_api/machinelearning/publications", ApiType.SPORest, body);
            return await (ListItem as ListItem).RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        #endregion

        #region Model unpublication

        public async Task<ISyntexModelPublicationResult> UnPublishModelAsync(IList library)
        {
            // Ensure we have the needed data loaded
            await (library as List).EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
            await library.PnPContext.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

            return ProcessModelPublishResponse(await UnPublishModelApiRequestAsync(UniqueId,
                                                                      library.PnPContext.Uri.AbsoluteUri.ToString(),
                                                                      library.PnPContext.Web.ServerRelativeUrl,
                                                                      library.RootFolder.ServerRelativeUrl).ConfigureAwait(false));
        }

        public ISyntexModelPublicationResult UnPublishModel(IList library)
        {
            return UnPublishModelAsync(library).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<ISyntexModelPublicationResult>> UnPublishModelBatchAsync(Batch batch, IList library)
        {
            // Ensure we have the needed data loaded
            await (library as List).EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
            await library.PnPContext.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

            System.Dynamic.ExpandoObject unPublishInfo = UnPublishModelApiRequestBody(UniqueId,
                                                                                    library.PnPContext.Uri.AbsoluteUri.ToString(),
                                                                                    library.PnPContext.Web.ServerRelativeUrl,
                                                                                    library.RootFolder.ServerRelativeUrl);

            string body = JsonSerializer.Serialize(unPublishInfo, PnPConstants.JsonSerializer_IgnoreNullValues_StringEnumConvertor);

            var apiCall = new ApiCall("_api/machinelearning/publications/batchdelete", ApiType.SPORest, body)
            {
                RawSingleResult = new SyntexModelPublicationResult(),
                RawResultsHandler = (json, apiCall) =>
                {
                    List<SyntexModelPublicationResult> modelPublicationResults = ParseModelPublishResponse(json);
                    var firstModelPublicationResult = modelPublicationResults.First();
                    (apiCall.RawSingleResult as SyntexModelPublicationResult).ErrorMessage = firstModelPublicationResult.ErrorMessage;
                    (apiCall.RawSingleResult as SyntexModelPublicationResult).Publication = firstModelPublicationResult.Publication;
                    (apiCall.RawSingleResult as SyntexModelPublicationResult).StatusCode = firstModelPublicationResult.StatusCode;
                }
            };

            var batchRequest = await (ListItem as ListItem).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            return new BatchSingleResult<ISyntexModelPublicationResult>(batch, batchRequest.Id, apiCall.RawSingleResult as ISyntexModelPublicationResult);
        }

        public IBatchSingleResult<ISyntexModelPublicationResult> UnPublishModelBatch(Batch batch, IList library)
        {
            return UnPublishModelBatchAsync(batch, library).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<ISyntexModelPublicationResult>> UnPublishModelBatchAsync(IList library)
        {
            return await UnPublishModelBatchAsync((ListItem as ListItem).PnPContext.CurrentBatch, library).ConfigureAwait(false);
        }

        public IBatchSingleResult<ISyntexModelPublicationResult> UnPublishModelBatch(IList library)
        {
            return UnPublishModelBatchAsync(library).GetAwaiter().GetResult();
        }

        public async Task<List<ISyntexModelPublicationResult>> UnPublishModelAsync(List<IList> libraries)
        {
            List<SyntexModelPublication> modelUnPublications = await BuildModelUnPublicationListAsync(libraries).ConfigureAwait(false);
            return ProcessModelPublishResponseMultiple(await UnPublishModelApiRequestAsync(modelUnPublications).ConfigureAwait(false));
        }

        private async Task<List<SyntexModelPublication>> BuildModelUnPublicationListAsync(List<IList> libraries)
        {
            List<SyntexModelPublication> modelPublications = new List<SyntexModelPublication>();
            foreach (var library in libraries)
            {
                await (library as List).EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
                await library.PnPContext.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

                modelPublications.Add(new SyntexModelPublication()
                {
                    ModelUniqueId = UniqueId,
                    TargetSiteUrl = library.PnPContext.Uri.AbsoluteUri.ToString(),
                    TargetWebServerRelativeUrl = library.PnPContext.Web.ServerRelativeUrl,
                    TargetLibraryServerRelativeUrl = library.RootFolder.ServerRelativeUrl
                });
            }

            return modelPublications;
        }

        public List<ISyntexModelPublicationResult> UnPublishModel(List<IList> libraries)
        {
            return UnPublishModelAsync(libraries).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<ISyntexModelPublicationResult>> UnPublishModelBatchAsync(Batch batch, List<IList> libraries)
        {
            List<SyntexModelPublication> modelUnPublications = await BuildModelUnPublicationListAsync(libraries).ConfigureAwait(false);
            var unPublishInfo = new
            {
                publications = modelUnPublications
            }.AsExpando();

            string body = JsonSerializer.Serialize(unPublishInfo, PnPConstants.JsonSerializer_IgnoreNullValues_StringEnumConvertor);

            var apiCall = new ApiCall("_api/machinelearning/publications/batchdelete", ApiType.SPORest, body)
            {
                RawEnumerableResult = new List<ISyntexModelPublicationResult>(),
                RawResultsHandler = (json, apiCall) =>
                {
                    (apiCall.RawEnumerableResult as List<ISyntexModelPublicationResult>).AddRange(ParseModelPublishResponse(json).Cast<ISyntexModelPublicationResult>());
                }
            };

            var batchRequest = await (ListItem as ListItem).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            return new BatchEnumerableBatchResult<ISyntexModelPublicationResult>(batch, batchRequest.Id, (IReadOnlyList<ISyntexModelPublicationResult>)apiCall.RawEnumerableResult);
        }

        public IEnumerableBatchResult<ISyntexModelPublicationResult> UnPublishModelBatch(Batch batch, List<IList> libraries)
        {
            return UnPublishModelBatchAsync(batch, libraries).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<ISyntexModelPublicationResult>> UnPublishModelBatchAsync(List<IList> libraries)
        {
            return await UnPublishModelBatchAsync((ListItem as ListItem).PnPContext.CurrentBatch, libraries).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<ISyntexModelPublicationResult> UnPublishModelBatch(List<IList> libraries)
        {
            return UnPublishModelBatchAsync(libraries).GetAwaiter().GetResult();
        }

        public async Task<ISyntexModelPublicationResult> UnPublishModelAsync(SyntexModelUnPublishOptions unPublicationOptions)
        {
            return ProcessModelPublishResponse(await UnPublishModelApiRequestAsync(UniqueId,
                                                        unPublicationOptions.TargetSiteUrl,
                                                        unPublicationOptions.TargetWebServerRelativeUrl,
                                                        unPublicationOptions.TargetLibraryServerRelativeUrl).ConfigureAwait(false));
        }

        public ISyntexModelPublicationResult UnPublishModel(SyntexModelUnPublishOptions unPublicationOptions)
        {
            return UnPublishModelAsync(unPublicationOptions).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<ISyntexModelPublicationResult>> UnPublishModelBatchAsync(Batch batch, SyntexModelUnPublishOptions unPublicationOptions)
        {
            System.Dynamic.ExpandoObject unPublishInfo = UnPublishModelApiRequestBody(UniqueId, unPublicationOptions.TargetSiteUrl,
                                                        unPublicationOptions.TargetWebServerRelativeUrl,
                                                        unPublicationOptions.TargetLibraryServerRelativeUrl);

            string body = JsonSerializer.Serialize(unPublishInfo, PnPConstants.JsonSerializer_IgnoreNullValues_StringEnumConvertor);

            var apiCall = new ApiCall("_api/machinelearning/publications/batchdelete", ApiType.SPORest, body)
            {
                RawSingleResult = new SyntexModelPublicationResult(),
                RawResultsHandler = (json, apiCall) =>
                {
                    List<SyntexModelPublicationResult> modelPublicationResults = ParseModelPublishResponse(json);
                    var firstModelPublicationResult = modelPublicationResults.First();
                    (apiCall.RawSingleResult as SyntexModelPublicationResult).ErrorMessage = firstModelPublicationResult.ErrorMessage;
                    (apiCall.RawSingleResult as SyntexModelPublicationResult).Publication = firstModelPublicationResult.Publication;
                    (apiCall.RawSingleResult as SyntexModelPublicationResult).StatusCode = firstModelPublicationResult.StatusCode;
                }
            };

            var batchRequest = await (ListItem as ListItem).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            return new BatchSingleResult<ISyntexModelPublicationResult>(batch, batchRequest.Id, apiCall.RawSingleResult as ISyntexModelPublicationResult);
        }

        public IBatchSingleResult<ISyntexModelPublicationResult> UnPublishModelBatch(Batch batch, SyntexModelUnPublishOptions unPublicationOptions)
        {
            return UnPublishModelBatchAsync(batch, unPublicationOptions).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<ISyntexModelPublicationResult>> UnPublishModelBatchAsync(SyntexModelUnPublishOptions unPublicationOptions)
        {
            return await UnPublishModelBatchAsync((ListItem as ListItem).PnPContext.CurrentBatch, unPublicationOptions).ConfigureAwait(false);
        }

        public IBatchSingleResult<ISyntexModelPublicationResult> UnPublishModelBatch(SyntexModelUnPublishOptions unPublicationOptions)
        {
            return UnPublishModelBatchAsync(unPublicationOptions).GetAwaiter().GetResult();
        }

        public async Task<List<ISyntexModelPublicationResult>> UnPublishModelAsync(List<SyntexModelUnPublishOptions> unPublicationOptions)
        {
            List<SyntexModelPublication> modelUnPublications = BuildModelUnPublishList(unPublicationOptions);
            return ProcessModelPublishResponseMultiple(await UnPublishModelApiRequestAsync(modelUnPublications).ConfigureAwait(false));
        }

        private List<SyntexModelPublication> BuildModelUnPublishList(List<SyntexModelUnPublishOptions> unPublicationOptions)
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

            return modelPublications;
        }

        public List<ISyntexModelPublicationResult> UnPublishModel(List<SyntexModelUnPublishOptions> unPublicationOptions)
        {
            return UnPublishModelAsync(unPublicationOptions).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<ISyntexModelPublicationResult>> UnPublishModelBatchAsync(Batch batch, List<SyntexModelUnPublishOptions> unPublicationOptions)
        {
            List<SyntexModelPublication> modelUnPublications = BuildModelUnPublishList(unPublicationOptions);
            
            var unPublishInfo = new
            {
                publications = modelUnPublications
            }.AsExpando();

            string body = JsonSerializer.Serialize(unPublishInfo, PnPConstants.JsonSerializer_IgnoreNullValues_StringEnumConvertor);

            var apiCall = new ApiCall("_api/machinelearning/publications/batchdelete", ApiType.SPORest, body)
            {
                RawEnumerableResult = new List<ISyntexModelPublicationResult>(),
                RawResultsHandler = (json, apiCall) =>
                {
                    (apiCall.RawEnumerableResult as List<ISyntexModelPublicationResult>).AddRange(ParseModelPublishResponse(json).Cast<ISyntexModelPublicationResult>());
                }
            };

            var batchRequest = await (ListItem as ListItem).RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            return new BatchEnumerableBatchResult<ISyntexModelPublicationResult>(batch, batchRequest.Id, (IReadOnlyList<ISyntexModelPublicationResult>)apiCall.RawEnumerableResult);
        }

        public IEnumerableBatchResult<ISyntexModelPublicationResult> UnPublishModelBatch(Batch batch, List<SyntexModelUnPublishOptions> unPublicationOptions)
        {
            return UnPublishModelBatchAsync(batch, unPublicationOptions).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<ISyntexModelPublicationResult>> UnPublishModelBatchAsync(List<SyntexModelUnPublishOptions> unPublicationOptions)
        {
            return await UnPublishModelBatchAsync((ListItem as ListItem).PnPContext.CurrentBatch, unPublicationOptions).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<ISyntexModelPublicationResult> UnPublishModelBatch(List<SyntexModelUnPublishOptions> unPublicationOptions)
        {
            return UnPublishModelBatchAsync(unPublicationOptions).GetAwaiter().GetResult();
        }

        private async Task<ApiCallResponse> UnPublishModelApiRequestAsync(Guid uniqueId, string targetSiteUrl, string targetWebServerRelativeUrl,
            string targetLibraryServerRelativeUrl)
        {
            System.Dynamic.ExpandoObject unPublishInfo = UnPublishModelApiRequestBody(uniqueId, targetSiteUrl, targetWebServerRelativeUrl, targetLibraryServerRelativeUrl);

            return await UnPublishModelApiCallAsync(unPublishInfo).ConfigureAwait(false);
        }

        private static System.Dynamic.ExpandoObject UnPublishModelApiRequestBody(Guid uniqueId, string targetSiteUrl, string targetWebServerRelativeUrl, string targetLibraryServerRelativeUrl)
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

            return new
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
            string body = JsonSerializer.Serialize(unPublishInfo, PnPConstants.JsonSerializer_IgnoreNullValues_StringEnumConvertor);

            var apiCall = new ApiCall("_api/machinelearning/publications/batchdelete", ApiType.SPORest, body);
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

        #endregion
    }
}
