using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{

    [SharePointType(NavigationConstants.NodeMetadataType, Target = typeof(Web), Uri = getNodeUri)]
    internal sealed class NavigationNode : BaseDataModel<INavigationNode>, INavigationNode
    {
        private const string baseUri = NavigationConstants.NavigationUri;
        private const string getNodeUri = baseUri + "/GetNodeById({Id})";
        
        internal const string NavigationNodeOptionsAdditionalInformationKey = NavigationConstants.NavigationNodeOptions;
        internal const string NavigationTypeKey = NavigationConstants.NavigationType;

        #region Construction
        public NavigationNode()
        {

            // Handler to construct the Add request for this list
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (additionalInformation) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var navigationNodeOptions = (NavigationNodeOptions)additionalInformation[NavigationNodeOptionsAdditionalInformationKey];

                string apiUrl = BuildNavigationApiUrl(additionalInformation, navigationNodeOptions);

                // Build body
                ExpandoObject navigationNodeCreationInformation;
                if (navigationNodeOptions.AudienceIds != null && navigationNodeOptions.AudienceIds.Count > 0)
                {
                    navigationNodeCreationInformation = new
                    {
                        __metadata = new { type = NavigationConstants.NodeMetadataType },
                        navigationNodeOptions.Title,
                        navigationNodeOptions.Url,
                        AudienceIds = new
                        {
                            __metadata = new { type = "Collection(Edm.Guid)" },
                            results = navigationNodeOptions.AudienceIds.ToArray()
                        },
                    }.AsExpando();
                }
                else
                {
                    navigationNodeCreationInformation = new
                    {
                        __metadata = new { type = NavigationConstants.NodeMetadataType },
                        navigationNodeOptions.Title,
                        navigationNodeOptions.Url,
                    }.AsExpando();
                }

                string body = JsonSerializer.Serialize(navigationNodeCreationInformation, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);

                return new ApiCall(apiUrl, ApiType.SPORest, body);
            };
        }

        private static string BuildNavigationApiUrl(Dictionary<string, object> additionalInformation, NavigationNodeOptions navigationNodeOptions)
        {
            var apiUrl = string.Empty;
            if (navigationNodeOptions != null && navigationNodeOptions.ParentNode != null)
            {
                apiUrl += $"{getNodeUri.Replace("{Id}", navigationNodeOptions.ParentNode.Id.ToString())}/Children";
            }
            else
            {
                apiUrl = baseUri;
                var navigationType = (NavigationType)additionalInformation[NavigationTypeKey];
                if (navigationType == NavigationType.QuickLaunch)
                {
                    apiUrl += NavigationConstants.QuickLaunchUri;
                }
                else if (navigationType == NavigationType.TopNavigationBar)
                {
                    apiUrl += NavigationConstants.TopNavigationBarUri;
                }
            }

            return apiUrl;
        }
        #endregion

        #region Override update payload generation
        internal override async Task BaseUpdate(Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            var api = BuildUpdateApiCall();

            await RawRequestAsync(api, new HttpMethod("PATCH"), "Update").ConfigureAwait(false);
        }

        internal override async Task BaseBatchUpdateAsync(Batch batch, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            var api = BuildUpdateApiCall();

            // Add the request to the batch
            await RawRequestBatchAsync(batch, api, new HttpMethod("PATCH"), "UpdateBatch").ConfigureAwait(false);
        }

        private ApiCall BuildUpdateApiCall()
        {
            ExpandoObject body;

            if (AudienceIds != null && AudienceIds.Count > 0)
            {
                body = new
                {
                    __metadata = new { type = NavigationConstants.NodeMetadataType },                    
                    Title,
                    Url,
                    IsVisible,
                    AudienceIds = new
                    {
                        __metadata = new { type = "Collection(Edm.Guid)" },
                        results = AudienceIds.ToArray()
                    },
                }.AsExpando();
            }
            else
            {
                body = new
                {
                    __metadata = new { type = NavigationConstants.NodeMetadataType },
                    Title,
                    Url,
                    IsVisible,
                    AudienceIds = new
                    {
                        __metadata = new { type = "Collection(Edm.Guid)" },
                        results = new List<Guid>().ToArray()
                    },
                }.AsExpando();
            }

            string bodyString = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);

            return new ApiCall($"{getNodeUri}", ApiType.SPORest, bodyString);
        }
        #endregion

        #region Properties
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public bool IsExternal { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsDocLib { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsVisible { get => GetValue<bool>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public int CurrentLCID { get => GetValue<int>(); set => SetValue(value); }

        public ListTemplateType ListTemplateType { get => GetValue<ListTemplateType>(); set => SetValue(value); }

        public List<Guid> AudienceIds { get => GetValue<List<Guid>>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Convert.ToInt32(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }

        #endregion

        #region Methods
        public List<INavigationNode> GetChildNodes(params Expression<Func<INavigationNode, object>>[] selectors)
        {
            return GetChildNodesAsync(selectors).GetAwaiter().GetResult();
        }

        public async Task<List<INavigationNode>> GetChildNodesAsync(params Expression<Func<INavigationNode, object>>[] selectors)
        {
            var apiCall = new ApiCall($"{getNodeUri.Replace("{Id}", Id.ToString())}/Children", ApiType.SPORest);
           
            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(response.Json))
            {
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

                if (json.TryGetProperty("value", out JsonElement getChildNodes))
                {
                    var childNodesList = new List<INavigationNode>();
                    var childNodes = JsonSerializer.Deserialize<IEnumerable<NavigationNode>>(getChildNodes.GetRawText(), PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue);
                    foreach (var childNode in childNodes)
                    {
                        childNode.PnPContext = PnPContext;
                        childNode.AddMetadata(PnPConstants.MetaDataRestId, $"{childNode.Id}");
                        childNodesList.Add(childNode);
                    }
                    return childNodesList;
                }

            }
            return new List<INavigationNode>();
        }
        #endregion
    }
}
