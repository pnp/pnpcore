using CamlBuilder;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PnP.Core.Transformation.Extensions;
using PnP.Core.Transformation.SharePoint.KQL;
using PnP.Core.Transformation.SharePoint.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PnP.Core.Transformation.SharePoint.Utilities
{
    #region Highlighted content properties model

    public enum ContentRollupLayout
    {
        Card = 1,
        List = 2,
        Carousel = 3,
        FilmStrip = 4,
        Masonry = 5,
        Custom = 999
    }

    public class ContentRollupWebPartProperties
    {
        [JsonProperty(PropertyName = "displayMaps")]
        public string DisplayMaps { get; set; } // will be populated from json string
        [JsonProperty(PropertyName = "query")]
        public SearchQuery Query { get; set; }
        [JsonProperty(PropertyName = "listId")]
        public string ListId { get; set; } // selected list to view the contents of
        [JsonProperty(PropertyName = "lastListId")]
        public string LastListId { get; set; } // used to detect list view selection changes
        [JsonProperty(PropertyName = "listTitle")]
        public string ListTitle { get; set; } // selected document library to view the contents of
        [JsonProperty(PropertyName = "isDefaultDocumentLibrary")]
        public bool? IsDefaultDocumentLibrary { get; set; }
        [JsonProperty(PropertyName = "documentLibrarySortField")]
        public string DocumentLibrarySortField { get; set; }
        [JsonProperty(PropertyName = "caml")]
        public string Caml { get; set; }
        [JsonProperty(PropertyName = "templateId")]
        public ContentRollupLayout? TemplateId { get; set; }
        [JsonProperty(PropertyName = "maxItemsPerPage")]
        public int? MaxItemsPerPage { get; set; }
        [JsonProperty(PropertyName = "isSeeAllPage")]
        public bool? IsSeeAllPage { get; set; }
        [JsonProperty(PropertyName = "isAdvancedFilterMode")]
        public bool? IsAdvancedFilterMode { get; set; }
        [JsonProperty(PropertyName = "sites")]
        public List<SiteMetadata> Sites { get; set; } // The list of sites metadata added by user as content source
        // IBaseCollectionWebPartProperties props
        [JsonProperty(PropertyName = "layoutId")] // Set when there are more than one layouts to indicate which layout is in use.
        public string LayoutId { get; set; }
        [JsonProperty(PropertyName = "dataProviderId")] // Set when there are more than one data providers to indicate which data provider is in use.
        public string DataProviderId { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "addToPageScreenReaderLabel")]
        public string AddToPageScreenReaderLabel { get; set; }
        [JsonProperty(PropertyName = "hideWebPartWhenEmpty")]
        public bool? HideWebPartWhenEmpty { get; set; }
        [JsonProperty(PropertyName = "webId")]
        public string WebId { get; set; }
        [JsonProperty(PropertyName = "siteId")]
        public string SiteId { get; set; }
        [JsonProperty(PropertyName = "baseUrl")]
        public string BaseUrl { get; set; } // base url to locate web part resources such has file icons
        [JsonProperty(PropertyName = "queryMode")]
        public string QueryMode { get; set; }
        [JsonProperty(PropertyName = "audienceTarget")]
        public bool AudienceTarget { get; set; }

        public ContentRollupWebPartProperties()
        {
            this.DisplayMaps = "%%DisplayMapsPlaceholder%%";
        }

    }

    #region ISearchQuery enums and classes

    public enum ContentLocation
    {
        CurrentSite = 1,
        CurrentSiteCollection = 2,
        AllSites = 3,
        CurrentSiteDocumentLibrary = 4,
        AllSitesInTheHub = 5,
        CurrentSitePageLibrary = 6,
        SelectedSites = 99
    }

    public enum DocumentType
    {
        Word = 1,
        Excel = 2,
        PowerPoint = 3,
        OneNote = 4,
        Visio = 5,
        PDF = 10,
        Any = 99
    }

    public enum FilterType
    {
        TitleContaining = 1,            // title like "*value*"
        AnyTextContaining = 2,          // any field like "*value*
        TaggedWith = 3,                 // value in list (tags)
        CreatedBy = 4,                  // created by = value
        ModifiedBy = 5,                 // created by = value
        Field = 6,                      // field operator value(s)
        RecentlyChanged = 7,
        RecentlyAdded = 8
    }

    public enum ContentType
    {
        Document = 1,
        Page = 2,
        Video = 3,
        Event = 4,
        Issue = 5,
        Task = 6,
        Link = 7,
        Contact = 8,
        Image = 9,
        News = 10,
        All = 99
    }

    public enum UserType
    {
        CurrentUser = 1, //Indicates user is the current session user.
        SpecificUser = 2 //Indicates user is who match the given match text (e.g. name).
    }

    public enum FilterOperator
    {
        Equals = 1,
        NotEqual = 2,
        BeginsWith = 3,
        EndsWith = 4,
        Contains = 5,
        DoesNotContain = 6,
        LessThan = 7,
        GreaterThan = 8,
        Between = 9
    }

    public enum SortType
    {
        MostRecent = 1,
        MostViewed = 2,
        Trending = 3,
        FieldAscending = 4,
        FieldDescending = 5
    }

    public class ManagedPropertiesRefinerOptions
    {
        [JsonProperty(PropertyName = "number")]
        public int? Number { get; set; } // How many unique managed properties to return
        [JsonProperty(PropertyName = "managedPropertyMatchText")]
        public string ManagedPropertyMatchText { get; set; } // Text to filter managed properties by name
    }

    public class SearchQueryFilter
    {
        [JsonProperty(PropertyName = "filterType")]
        public FilterType? FilterType { get; set; }
        [JsonProperty(PropertyName = "userType")]
        public UserType? UserType { get; set; }
        [JsonProperty(PropertyName = "fieldNameMatchText")]
        public string FieldNameMatchText { get; set; }
        [JsonProperty(PropertyName = "lastFieldNameMatchText")]
        public string LastFieldNameMatchText { get; set; }
        [JsonProperty(PropertyName = "fieldname")]
        public string Fieldname { get; set; }
        [JsonProperty(PropertyName = "op")]
        public FilterOperator? Op { get; set; }
        [JsonProperty(PropertyName = "value")]
        public object Value { get; set; }
        [JsonProperty(PropertyName = "value2")]
        public object Value2 { get; set; }
        [JsonProperty(PropertyName = "values")]
        public List<object> Values { get; set; }
        [JsonIgnore]
        public FilterChainingOperator? ChainingOperatorUsedInCQWP { get; set; }

        public SearchQueryFilter()
        {
            this.Values = new List<object>();
        }

    }

    public class SearchQuery
    {
        [JsonProperty(PropertyName = "advancedQueryText")]
        public string AdvancedQueryText { get; set; }
        [JsonProperty(PropertyName = "contentLocation")]
        public ContentLocation? ContentLocation { get; set; } // search scope
        [JsonProperty(PropertyName = "contentTypes")] // content types to include in query result
        public List<ContentType> ContentTypes { get; set; }
        [JsonProperty(PropertyName = "documentTypes")]
        public List<DocumentType> DocumentTypes { get; set; }
        [JsonProperty(PropertyName = "filters")]
        public List<SearchQueryFilter> Filters { get; set; }
        [JsonProperty(PropertyName = "sortField")]
        public string SortField { get; set; }
        [JsonProperty(PropertyName = "sortFieldMatchText")]
        public string SortFieldMatchText { get; set; }
        [JsonProperty(PropertyName = "sortType")]
        public SortType? SortType { get; set; }
        [JsonProperty(PropertyName = "managedPropertiesRefinerOptions")]
        public ManagedPropertiesRefinerOptions ManagedPropertiesRefinerOptions { get; set; }

        public SearchQuery()
        {
            this.ContentTypes = new List<ContentType>();
            this.DocumentTypes = new List<DocumentType>();
            this.Filters = new List<SearchQueryFilter>();
        }
    }

    #endregion

    #region ISiteMetadata enums and classes

    public class SiteReference
    {
        [JsonProperty(PropertyName = "WebId")]
        public string WebId { get; set; }
        [JsonProperty(PropertyName = "IndexId")]
        public long IndexId { get; set; }
        [JsonProperty(PropertyName = "ExchangeId")]
        public string ExchangeId { get; set; } // eg SPO_M2NlYjY4MDctYTZlZS00NDNjLWE4M2ItZWIyNjM2YzYyZTgyLGU...
        [JsonProperty(PropertyName = "Source")]
        public string Source { get; set; } //'Users' | string;
        [JsonProperty(PropertyName = "SiteId")]
        public string SiteId { get; set; }
        [JsonProperty(PropertyName = "Type")]
        public string Type { get; set; } //'SiteReference' | 'GroupReference' | string;
        [JsonProperty(PropertyName = "GroupId")]
        public string GroupId { get; set; } // The id of the group when site reference type is 'GroupReference, It's not available for other site reference types
    }

    public class SiteMetadata
    {
        [JsonProperty(PropertyName = "Acronym")]
        public string Acronym { get; set; } // The acronym of the site. Used in banner image if the banner image url is not available
        //[JsonProperty(PropertyName = "title")]
        //public string Title { get; set; } // Site title
        [JsonProperty(PropertyName = "BannerColor")]
        public string BannerColor { get; set; } // The color represents the site theme
        [JsonProperty(PropertyName = "bannerImageUrl")]
        public string BannerImageUrl { get; set; } // The url of the site logo
        //[JsonProperty(PropertyName = "url")]
        //public string Url { get; set; } // The url points to the site
        [JsonProperty(PropertyName = "Type")]
        public string Type { get; set; } //Type of the site. E.g. 'Site', 'Group'.
        [JsonProperty(PropertyName = "ItemReference")]
        public SiteReference ItemReference { get; set; } // Identifiers of the site
    }

    #endregion

    #endregion

    #region ContentByQuery model

    public enum SortDirection
    {
        Asc,
        Desc
    }

    public enum FilterChainingOperator
    {
        /// <summary>
        /// Filter is chained using an And operator
        /// </summary>
        And,

        /// <summary>
        /// Filter is chained using an Or operator
        /// </summary>
        Or
    }

    public enum FilterFieldQueryOperator
    {
        /// <summary>
        /// Equal to
        /// </summary>
        Eq,

        /// <summary>
        /// Not equal to
        /// </summary>
        Neq,

        /// <summary>
        /// Greater than
        /// </summary>
        Gt,

        /// <summary>
        /// Greater than or equal to
        /// </summary>
        Geq,

        /// <summary>
        /// Less than
        /// </summary>
        Lt,

        /// <summary>
        /// Less than or equal to
        /// </summary>
        Leq,

        /// <summary>
        /// Begins with
        /// </summary>
        BeginsWith,

        /// <summary>
        /// Contains
        /// </summary>
        Contains,

        /// <summary>
        /// Contains any of
        /// </summary>
        ContainsAny,

        /// <summary>
        /// Contains all of
        /// </summary>
        ContainsAll
    }

    public class ContentByQuery
    {
        // query scope

        // ~sitecollection, ~site, ~sitecollection/sub1 to indicate scope from a given subsite or empty (= current site)    
        public string WebUrl { get; set; }
        // will be set whenever the query is scoped to a list in the current web
        public string ListName { get; set; }
        // will be set whenever the query is scoped to a list in the current web
        public string ListGuid { get; set; }
        // contains list template which we're filtering on (e.g. 119 for wiki list). Must be set!
        public string ServerTemplate { get; set; }
        // contains prefix for content type filtering (e.g. 0x010108 for all wiki page content types and descendants)
        public string ContentTypeBeginsWithId { get; set; }

        // extra filters

        // e.g.: {8553196d-ec8d-4564-9861-3dbe931050c8}
        public string FilterField1 { get; set; }
        // e.g.: d
        public string FilterField1Value { get; set; }
        // e.g.: BeginsWith
        public FilterFieldQueryOperator FilterOperator1 { get; set; }
        public FilterChainingOperator Filter1ChainingOperator { get; set; }
        public string FilterField2 { get; set; }
        public string FilterField2Value { get; set; }
        public FilterFieldQueryOperator FilterOperator2 { get; set; }
        public FilterChainingOperator Filter2ChainingOperator { get; set; }
        public string FilterField3 { get; set; }
        public string FilterField3Value { get; set; }
        public FilterFieldQueryOperator FilterOperator3 { get; set; }
        //public FilterChainingOperator Filter3ChainingOperator { get; set; }

        // sorting & grouping

        // e.g. {8553196d-ec8d-4564-9861-3dbe931050c8}
        public string SortBy { get; set; }
        public SortDirection SortByDirection { get; set; }
        // e.g. {8553196d-ec8d-4564-9861-3dbe931050c8}
        public string GroupBy { get; set; }
        public SortDirection GroupByDirection { get; set; }

        // number of items to display (e.g. 15)
        public int ItemLimit { get; set; }

        // number of columns used to present the data (e.g. 1)
        public int DisplayColumns { get; set; }

        //DataMappings (= fields used for the respective Description, ImageUrl, Title and LinkUrl slots) E.g.:
        // 
        //Description:{691b9a4b-512e-4341-b3f1-68914130d5b2},ShortComment,Text;|
        //ImageUrl:{b9e6f3ae-5632-4b13-b636-9d1a2bd67120},EncodedAbsThumbnailUrl,Computed;{543bc2cf-1f30-488e-8f25-6fe3b689d9ac},PublishingRollupImage,Image;|
        //Title:{fa564e0f-0c70-4ab9-b863-0177e6ddd247},Title,Text;|
        //LinkUrl:{94f89715-e097-4e8b-ba79-ea02aa8b7adb},FileRef,Lookup;|              
        public string DataMappings { get; set; }

    }

    #endregion

    #region ContentBySearch model

    public class ContentBySearch
    {
        public string DataProviderJson { get; set; }
        public string SelectedPropertiesJson { get; set; }
        public int ResultsPerPage { get; set; }
        public string RenderTemplateId { get; set; }
    }

    #endregion

    #region Result class

    public class ContentByQuerySearchTransformatorResult
    {
        public string Properties { get; set; }
        public string SearchablePlainTexts { get; set; }
        public string ImageSources { get; set; }
        public string Links { get; set; }
    }

    #endregion

    /// <summary>
    /// Class used to generate contentrollup (=highlighted content) web part properties coming from either a content by query or content by search web part
    /// </summary>
    public class ContentByQuerySearchTransformator
    {
        private ContentRollupWebPartProperties properties;
        private ClientContext clientContext;
        private const string displayMapJson = "{\"1\":{\"headingText\":{\"sources\":[\"SiteTitle\"]},\"headingUrl\":{\"sources\":[\"SitePath\"]},\"title\":{\"sources\":[\"UserName\",\"Title\"]},\"personImageUrl\":{\"sources\":[\"ProfileImageSrc\"]},\"name\":{\"sources\":[\"Name\"]},\"initials\":{\"sources\":[\"Initials\"]},\"itemUrl\":{\"sources\":[\"WebPath\"]},\"activity\":{\"sources\":[\"ModifiedDate\"]},\"previewUrl\":{\"sources\":[\"PreviewUrl\",\"PictureThumbnailURL\"]},\"iconUrl\":{\"sources\":[\"IconUrl\"]},\"accentColor\":{\"sources\":[\"AccentColor\"]},\"cardType\":{\"sources\":[\"CardType\"]},\"tipActionLabel\":{\"sources\":[\"TipActionLabel\"]},\"tipActionButtonIcon\":{\"sources\":[\"TipActionButtonIcon\"]},\"className\":{\"sources\":[\"ClassName\"]}},\"2\":{\"column1\":{\"heading\":\"\",\"sources\":[\"FileExtension\"],\"width\":34},\"column2\":{\"heading\":\"Title\",\"sources\":[\"Title\"],\"linkUrls\":[\"WebPath\"],\"width\":250},\"column3\":{\"heading\":\"Modified\",\"sources\":[\"ModifiedDate\"],\"width\":100},\"column4\":{\"heading\":\"Modified By\",\"sources\":[\"Name\"],\"width\":150}},\"3\":{\"id\":{\"sources\":[\"UniqueID\"]},\"edit\":{\"sources\":[\"edit\"]},\"DefaultEncodingURL\":{\"sources\":[\"DefaultEncodingURL\"]},\"FileExtension\":{\"sources\":[\"FileExtension\"]},\"FileType\":{\"sources\":[\"FileType\"]},\"Path\":{\"sources\":[\"Path\"]},\"PictureThumbnailURL\":{\"sources\":[\"PictureThumbnailURL\"]},\"SiteID\":{\"sources\":[\"SiteID\"]},\"SiteTitle\":{\"sources\":[\"SiteTitle\"]},\"Title\":{\"sources\":[\"Title\"]},\"UniqueID\":{\"sources\":[\"UniqueID\"]},\"WebId\":{\"sources\":[\"WebId\"]},\"WebPath\":{\"sources\":[\"WebPath\"]},\"PreviewUrl\":{\"sources\":[\"PreviewUrl\"]}},\"4\":{\"headingText\":{\"sources\":[\"SiteTitle\"]},\"headingUrl\":{\"sources\":[\"SitePath\"]},\"title\":{\"sources\":[\"UserName\",\"Title\"]},\"personImageUrl\":{\"sources\":[\"ProfileImageSrc\"]},\"name\":{\"sources\":[\"Name\"]},\"initials\":{\"sources\":[\"Initials\"]},\"itemUrl\":{\"sources\":[\"WebPath\"]},\"activity\":{\"sources\":[\"ModifiedDate\"]},\"previewUrl\":{\"sources\":[\"PreviewUrl\",\"PictureThumbnailURL\"]},\"iconUrl\":{\"sources\":[\"IconUrl\"]},\"accentColor\":{\"sources\":[\"AccentColor\"]},\"cardType\":{\"sources\":[\"CardType\"]},\"tipActionLabel\":{\"sources\":[\"TipActionLabel\"]},\"tipActionButtonIcon\":{\"sources\":[\"TipActionButtonIcon\"]},\"className\":{\"sources\":[\"ClassName\"]}}}";
        private List<Field> queryFields;

        #region Construction

        /// <summary>
        /// Instantiates the class
        /// </summary>
        /// <param name="cc">Client context for the web holding the source page</param>
        public ContentByQuerySearchTransformator(ClientContext cc)
        {
            this.clientContext = cc;
            this.properties = new ContentRollupWebPartProperties();
            this.queryFields = new List<Field>();

            cc.Web.EnsureProperties(p => p.Id, p => p.Url);
            cc.Site.EnsureProperties(p => p.Id, p => p.RootWeb);
            cc.Site.RootWeb.EnsureProperty(p => p.Url);

            // Default initialization of the configuration
            this.properties.WebId = cc.Web.Id.ToString();
            this.properties.SiteId = cc.Site.Id.ToString();
            this.properties.MaxItemsPerPage = 8;
            this.properties.HideWebPartWhenEmpty = false;
            this.properties.Sites = new List<SiteMetadata>();
        }

        #endregion

        /// <summary>
        /// Generate contentrollup (=highlighted content) web part properties coming from a content by search web part
        /// </summary>
        /// <returns>Properties for highlighted content web part</returns>
        public ContentByQuerySearchTransformatorResult TransformUserDocuments()
        {
            ContentByQuerySearchTransformatorResult result = new ContentByQuerySearchTransformatorResult()
            {
                Properties = "",
                ImageSources = "",
                SearchablePlainTexts = "",
                Links = ""
            };

            // Set basic site properties
            this.properties.DataProviderId = "Search";
            this.properties.MaxItemsPerPage = 8;
            this.properties.TemplateId = ContentRollupLayout.Card;

            // construct query
            SearchQuery query = new SearchQuery
            {
                ContentLocation = MapToContentLocation(this.clientContext.Web.EnsureProperty(p => p.Url)),
            };

            query.ContentTypes.Add(ContentType.Page);
            query.DocumentTypes.Add(DocumentType.Any);

            // scope to pages modified by the current user
            query.Filters.Add(new SearchQueryFilter()
            {
                FilterType = FilterType.ModifiedBy,
                UserType = UserType.CurrentUser,
            });

            query.AdvancedQueryText = "";

            // assign query
            this.properties.Query = query;

            // Prep output
            result.Properties = HighlightedContentProperties();

            // Return the json properties for the converted web part
            return result;
        }

        /// <summary>
        /// Generate contentrollup (=highlighted content) web part properties coming from a content by search web part
        /// </summary>
        /// <param name="cbs">Properties coming from the content by search web part</param>
        /// <returns>Properties for highlighted content web part</returns>
        public ContentByQuerySearchTransformatorResult TransformContentBySearchWebPartToHighlightedContent(ContentBySearch cbs)
        {
            ContentByQuerySearchTransformatorResult result = new ContentByQuerySearchTransformatorResult()
            {
                Properties = "",
                ImageSources = "",
                SearchablePlainTexts = "",
                Links = ""
            };

            // Set basic site properties
            this.properties.DataProviderId = "Search";

            if (cbs.ResultsPerPage < 1)
            {
                cbs.ResultsPerPage = 1;
            }
            this.properties.MaxItemsPerPage = cbs.ResultsPerPage;

            // Load the Json
            var dataProviderJson = JObject.Parse(cbs.DataProviderJson);

            // Determine needed content location
            ContentLocation contentLocation = ContentLocation.CurrentSiteCollection;
            if (dataProviderJson["Properties"] != null && IsPropertyAvailable(dataProviderJson["Properties"]["Scope"]))
            {
                string scope = GetPropertyAsString(dataProviderJson["Properties"]["Scope"]);

                if (!string.IsNullOrEmpty(scope))
                {
                    this.clientContext.Site.RootWeb.EnsureProperty(w => w.Url);

                    // replace site collection url by ~sitecollection so we can reuse the existing logic
                    scope = scope.Replace(this.clientContext.Site.RootWeb.Url, "~sitecollection");

                    // TODO: seems like CSWP scopes to site and sub sites while the Site option in HCWP scopes to just the site
                    //if (scope.Equals("{Site.URL}", StringComparison.InvariantCultureIgnoreCase))
                    //{
                    //    scope = "~site";
                    //}

                    // determine location
                    contentLocation = MapToContentLocation(scope);
                }

                if (contentLocation == ContentLocation.SelectedSites)
                {
                    // Fill the needed structure to scope the search query to a sub site
                    var parts = scope.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    var subSiteUrl = parts[1];

                    var siteMetadata = MapToSiteMetadata(subSiteUrl, ref result);

                    if (siteMetadata != null)
                    {
                        this.properties.Sites.Add(siteMetadata);
                    }
                    else
                    {
                        // something went wrong, fall back to full site collection
                        contentLocation = ContentLocation.CurrentSiteCollection;
                    }
                }
            }

            // construct query
            SearchQuery query = new SearchQuery
            {
                // Libraries always equal to this
                ContentLocation = contentLocation
            };

            // Layout properties
            if (!string.IsNullOrEmpty(cbs.RenderTemplateId) && cbs.RenderTemplateId.ToLower().Contains("control_slideshow.js"))
            {
                SetLayoutTemplate(ContentRollupLayout.Carousel);
            }
            else
            {
                SetLayoutTemplate(ContentRollupLayout.Card);
            }

            // There's no document type filtering in CSWP
            query.DocumentTypes.Add(DocumentType.Any);

            var queryTemplate = GetPropertyAsString(dataProviderJson["QueryTemplate"]);
            var sourceName = GetPropertyAsString(dataProviderJson["SourceName"]);

            string contentTypeId = null;
            if (dataProviderJson["Properties"] != null && IsPropertyAvailable(dataProviderJson["Properties"]["ContentTypeId"]))
            {
                contentTypeId = GetPropertyAsString(dataProviderJson["Properties"]["ContentTypeId"]);
            }

            // Assign content types depending on the provided source and optional contenttypeid
            query.ContentTypes.AddRange(MapToContentTypesFromResourceSource(sourceName, contentTypeId));

            // Set sort type - default is MostRecent, but can be different depending on the choosen source
            query.SortType = MapToSortTypeFromResourceSource(sourceName);

            // Handle query (queryTemplate) and refinement filters
            if (!string.IsNullOrEmpty(queryTemplate) || (dataProviderJson["FallbackRefinementFilters"] != null && dataProviderJson["FallbackRefinementFilters"].HasValues))
            {
                MapSearchQueryToFilters(ref query, queryTemplate, dataProviderJson["FallbackRefinementFilters"]);
            }

            // Keep the search query if there was one set in the web part as that allows to retain the same result set
            if (!string.IsNullOrEmpty(queryTemplate))
            {
                query.AdvancedQueryText = $"{queryTemplate}";
                this.properties.QueryMode = "Advanced";
            }
            else
            {
                this.properties.QueryMode = "Basic";
            }

            // assign query
            this.properties.Query = query;

            // Prep output
            result.Properties = HighlightedContentProperties();

            // Return the json properties for the converted web part
            return result;
        }

        /// <summary>
        /// Generate contentrollup (=highlighted content) web part properties coming from a content by query web part
        /// </summary>
        /// <param name="cbq">Properties coming from the content by query web part</param>
        /// <returns>Properties for highlighted content web part</returns>
        public ContentByQuerySearchTransformatorResult TransformContentByQueryWebPartToHighlightedContent(ContentByQuery cbq)
        {
            // Transformation logic
            ContentByQuerySearchTransformatorResult result = new ContentByQuerySearchTransformatorResult()
            {
                ImageSources = "",
                SearchablePlainTexts = "",
                Links = ""
            };

            // Scoped to list?
            if (!Guid.TryParse(cbq.ListGuid, out Guid listId))
            {
                listId = Guid.Empty;
            }

            if (!string.IsNullOrEmpty(cbq.ListName) || listId != Guid.Empty)
            {
                // Scope to list
                List list = null;
                if (listId != Guid.Empty)
                {
                    list = this.clientContext.Web.GetListById(listId);
                }
                else
                {
                    list = this.clientContext.Web.GetListByTitle(cbq.ListName);
                }

                var defaultDocLib = this.clientContext.Web.DefaultDocumentLibrary();
                this.clientContext.Load(defaultDocLib, p => p.Id);
                this.clientContext.Load(list, p => p.BaseType, p => p.Title, p => p.Id, p => p.Fields);
                this.clientContext.ExecuteQueryRetry();

                // Set basic list properties
                this.properties.ListId = list.Id.ToString();
                this.properties.LastListId = this.properties.ListId;
                this.properties.ListTitle = list.Title;
                this.properties.DataProviderId = "List";

                this.properties.IsDefaultDocumentLibrary = defaultDocLib.Id.Equals(list.Id);

                // TODO: verify upper limit bound
                if (cbq.ItemLimit < 1)
                {
                    cbq.ItemLimit = 1;
                }
                this.properties.MaxItemsPerPage = cbq.ItemLimit;

                // Layout properties
                if (cbq.DisplayColumns > 1)
                {
                    SetLayoutTemplate(ContentRollupLayout.Card);
                }
                else
                {
                    SetLayoutTemplate(ContentRollupLayout.List);
                }

                // construct query
                SearchQuery query = new SearchQuery();

                if (list.BaseTemplate == (int)ListTemplateType.WebPageLibrary)
                {
                    // for SitePages library we only support the Page content type, document types are not relevant here
                    query.ContentLocation = ContentLocation.CurrentSitePageLibrary;
                    query.ContentTypes.Add(ContentType.Page);
                }
                else
                {
                    query.ContentLocation = ContentLocation.CurrentSiteDocumentLibrary;
                    // There's no document type filtering in CWQP
                    query.DocumentTypes.Add(DocumentType.Any);
                    // Map contenttypeid to 'default' content types if possible
                    query.ContentTypes.AddRange(MapToContentTypesFromContentType(cbq.ContentTypeBeginsWithId));
                }


                // Filtering
                var filter1 = MapToFilter(list, cbq.FilterOperator1, cbq.FilterField1, cbq.FilterField1Value, FilterChainingOperator.And);
                if (filter1 != null)
                {
                    query.Filters.Add(filter1);
                }

                var filter2 = MapToFilter(list, cbq.FilterOperator2, cbq.FilterField2, cbq.FilterField2Value, cbq.Filter1ChainingOperator);
                if (filter2 != null)
                {
                    query.Filters.Add(filter2);
                }

                var filter3 = MapToFilter(list, cbq.FilterOperator3, cbq.FilterField3, cbq.FilterField3Value, cbq.Filter2ChainingOperator);
                if (filter3 != null)
                {
                    query.Filters.Add(filter3);
                }

                query.AdvancedQueryText = "";

                // Set sort field 
                // Possible sort fields are: Title, FileLeafRef, Author and Modified. Sort direction is always the same (Ascending=\"true\") except for sorting on Modified
                if (!string.IsNullOrEmpty(cbq.SortBy))
                {
                    if (cbq.SortBy.Equals("Modified") || cbq.SortBy.Equals("Title") || cbq.SortBy.Equals("FileLeafRef") || cbq.SortBy.Equals("Author"))
                    {
                        this.properties.DocumentLibrarySortField = cbq.SortBy;
                    }
                    else
                    {
                        // Fall back to default if the original CBQ uses sorting that was not allowed
                        this.properties.DocumentLibrarySortField = "Modified";
                    }
                }

                // assign query
                this.properties.Query = query;

                if (this.properties.Query.Filters.Any())
                {
                    // Use the advanced query mode when we set the Caml query ourselves, this allows for more complex operations with freedom of choosing AND/OR operators
                    query.AdvancedQueryText = CamlQueryBuilder(list, cbq);
                    this.properties.QueryMode = "Advanced";
                    this.properties.Caml = "";
                }
            }
            else
            {
                // scope to site(s)

                // Set basic site properties
                this.properties.DataProviderId = "Search";

                // TODO: verify upper limit bound
                if (cbq.ItemLimit < 1)
                {
                    cbq.ItemLimit = 1;
                }
                this.properties.MaxItemsPerPage = cbq.ItemLimit;

                // Layout properties
                if (cbq.DisplayColumns > 1)
                {
                    SetLayoutTemplate(ContentRollupLayout.Card);
                }
                else
                {
                    SetLayoutTemplate(ContentRollupLayout.List);
                }

                var contentLocation = MapToContentLocation(cbq.WebUrl);

                if (contentLocation == ContentLocation.SelectedSites)
                {
                    // Fill the needed structure to scope the search query to a sub site
                    var parts = cbq.WebUrl.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    var subSiteUrl = parts[1];

                    var siteMetadata = MapToSiteMetadata(subSiteUrl, ref result);

                    if (siteMetadata != null)
                    {
                        this.properties.Sites.Add(siteMetadata);
                    }
                    else
                    {
                        // something went wrong, fall back to full site collection
                        contentLocation = ContentLocation.CurrentSiteCollection;
                    }
                }

                // construct query
                SearchQuery query = new SearchQuery
                {
                    // Libraries always equal to this
                    ContentLocation = contentLocation
                };

                // There's no document type filtering in CWQP
                query.DocumentTypes.Add(DocumentType.Any);

                // Map contenttypeid to 'default' content types if possible
                if (!string.IsNullOrEmpty(cbq.ServerTemplate))
                {
                    query.ContentTypes.AddRange(MapToContentTypesFromListTemplate(cbq.ServerTemplate));
                }
                else
                {
                    query.ContentTypes.AddRange(MapToContentTypesFromContentType(cbq.ContentTypeBeginsWithId));
                }

                if (!string.IsNullOrEmpty(cbq.FilterField1) || !string.IsNullOrEmpty(cbq.FilterField2) || !string.IsNullOrEmpty(cbq.FilterField3))
                {
                    // Process CBQ filters
                    var filter1 = MapToFilter(null, cbq.FilterOperator1, cbq.FilterField1, cbq.FilterField1Value, FilterChainingOperator.And);
                    if (filter1 != null)
                    {
                        query.Filters.Add(filter1);
                    }

                    var filter2 = MapToFilter(null, cbq.FilterOperator2, cbq.FilterField2, cbq.FilterField2Value, cbq.Filter1ChainingOperator);
                    if (filter2 != null)
                    {
                        query.Filters.Add(filter2);
                    }

                    var filter3 = MapToFilter(null, cbq.FilterOperator3, cbq.FilterField3, cbq.FilterField3Value, cbq.Filter2ChainingOperator);
                    if (filter3 != null)
                    {
                        query.Filters.Add(filter3);
                    }
                }
                else
                {
                    // Add default filter element (needed to show up the filters pane in the web part)
                    query.Filters.Add(new SearchQueryFilter()
                    {
                        FilterType = FilterType.TitleContaining,
                        Value = "",
                    });
                }

                // Set sort field 
                // Possible sort by's are: Most recent, Most viewed, Trending, Managed property ascending, Managed property descending
                if (!string.IsNullOrEmpty(cbq.SortBy))
                {
                    var sortField = MapToSort(cbq.SortBy);
                    if (sortField != null)
                    {
                        query.SortField = sortField;
                        query.SortFieldMatchText = sortField;
                        query.SortType = cbq.SortByDirection == SortDirection.Asc ? SortType.FieldAscending : SortType.FieldDescending;
                    }
                }
                else
                {
                    // Set sort type - default is MostRecent
                    query.SortType = SortType.MostRecent;
                }

                query.AdvancedQueryText = "";
                this.properties.QueryMode = "Basic";

                // assign query
                this.properties.Query = query;
            }

            // Prep output
            result.Properties = HighlightedContentProperties();

            // Return the json properties for the converted web part
            return result;
        }

        #region Helper methods

        #region CAML Query Builder

        private string CamlQueryBuilder(List list, ContentByQuery cbq)
        {
            // Copy the CBQW filters
            List<SearchQueryFilter> filters = new List<SearchQueryFilter>();
            filters.AddRange(this.properties.Query.Filters);
            // Add the default filter
            filters.Add(new SearchQueryFilter()
            {
                ChainingOperatorUsedInCQWP = FilterChainingOperator.And,
                Fieldname = "FSObjType",
                Op = FilterOperator.Equals,
                Value = 0
            });

            // Sorting: if CBQW was sorted on one of the 4 allowed fields then take over the setting, else fall back to default sort (= Modified)
            string sortField = "Modified";
            if (!string.IsNullOrEmpty(cbq.SortBy))
            {
                if (cbq.SortBy.Equals("Title") || cbq.SortBy.Equals("FileLeafRef") || cbq.SortBy.Equals("Author"))
                {
                    sortField = cbq.SortBy;
                }
            }

            // Sort order cannot be choosen: Modified = descending, others are ascending
            string sortOrder = "True";
            if (sortField == "Modified")
            {
                sortOrder = "False";
            }

            string query = "";
            Query queryCaml = null;
            var and = LogicalJoin.And();
            var or = LogicalJoin.Or();

            // Do we have filters to apply?
            if (filters.Any())
            {
                for (int i = 0; i < filters.Count; i++)
                {
                    var queryFilter = filters[i];
                    var nextQueryFilter = filters[i];
                    if (i < filters.Count - 1)
                    {
                        nextQueryFilter = filters[i + 1];
                    }

                    if (queryFilter.ChainingOperatorUsedInCQWP == FilterChainingOperator.And && nextQueryFilter.ChainingOperatorUsedInCQWP == FilterChainingOperator.And)
                    {
                        and.AddStatement(CamlFilterBuilder(queryFilter));
                    }
                    else
                    {
                        or.AddStatement(CamlFilterBuilder(queryFilter));
                    }
                }

                if (or.HasStatements())
                {
                    and.AddStatement(or);
                }
            }

            queryCaml = Query.Build(and);
            query = queryCaml.GetCaml(true).Replace("\r", "").Replace("\n", "");
            return $"<View Scope=\"RecursiveAll\"><Query>{query}<OrderBy><FieldRef Name=\"{sortField}\" Ascending=\"{sortOrder}\" /></OrderBy></Query><ViewFields><FieldRef Name=\"Editor\" /><FieldRef Name=\"FileLeafRef\" /><FieldRef Name=\"File_x0020_Type\" /><FieldRef Name=\"ID\" /><FieldRef Name=\"Modified\" /><FieldRef Name=\"Title\" /><FieldRef Name=\"UniqueID\" /></ViewFields><RowLimit Paged=\"false\">{cbq.ItemLimit}</RowLimit></View>";
        }

        private string CamlFilterValueBuilder(string fieldName, string fieldValue)
        {
            // default to Text
            string value = fieldValue;

            var field = this.queryFields.Where(p => p.InternalName == fieldName).FirstOrDefault();
            if (field != null)
            {
                if (field.FieldTypeKind == FieldType.User)
                {
                    if (fieldValue.Equals("[me]", StringComparison.InvariantCultureIgnoreCase))
                    {
                        value = "<UserID Type=\"Integer\" />";
                    }
                }
                else if (field.FieldTypeKind == FieldType.DateTime && fieldValue.StartsWith("[today]", StringComparison.InvariantCultureIgnoreCase))
                {
                    string days = fieldValue.ToLower().Replace("[today]", "");

                    if (string.IsNullOrEmpty(days) || !int.TryParse(days, out int offset))
                    {
                        offset = 0;
                    }

                    value = $"<Today OffsetDays=\"{offset}\" />";
                }
            }

            return value;
        }

        private CamlBuilder.ValueType CamlFilterValueTypeBuilder(string fieldName, string fieldValue)
        {
            // default to Text
            CamlBuilder.ValueType valueType = CamlBuilder.ValueType.Text;

            var field = this.queryFields.Where(p => p.InternalName == fieldName).FirstOrDefault();
            if (field != null)
            {
                if (field.FieldTypeKind == FieldType.User)
                {
                    if (fieldValue.Equals("[me]", StringComparison.InvariantCultureIgnoreCase))
                    {
                        valueType = CamlBuilder.ValueType.Integer;
                    }
                    else
                    {
                        valueType = CamlBuilder.ValueType.User;
                    }
                }
                else if (field.FieldTypeKind == FieldType.DateTime && fieldValue.StartsWith("[today]", StringComparison.InvariantCultureIgnoreCase))
                {
                    valueType = CamlBuilder.ValueType.DateTime;
                }
            }

            // Special case
            if (fieldName == "FSObjType")
            {
                valueType = CamlBuilder.ValueType.Integer;
            }

            return valueType;
        }

        private Operator CamlFilterBuilder(SearchQueryFilter queryFilter)
        {
            if (queryFilter.Op == FilterOperator.BeginsWith)
            {
                return Operator.BeginsWith(queryFilter.Fieldname, CamlFilterValueTypeBuilder(queryFilter.Fieldname, queryFilter.Value.ToString()), CamlFilterValueBuilder(queryFilter.Fieldname, queryFilter.Value.ToString()));
            }
            else if (queryFilter.Op == FilterOperator.Contains)
            {
                return Operator.Contains(queryFilter.Fieldname, CamlFilterValueTypeBuilder(queryFilter.Fieldname, queryFilter.Value.ToString()), CamlFilterValueBuilder(queryFilter.Fieldname, queryFilter.Value.ToString()));
            }
            else if (queryFilter.Op == FilterOperator.Equals)
            {
                return Operator.Equal(queryFilter.Fieldname, CamlFilterValueTypeBuilder(queryFilter.Fieldname, queryFilter.Value.ToString()), CamlFilterValueBuilder(queryFilter.Fieldname, queryFilter.Value.ToString()));
            }
            else if (queryFilter.Op == FilterOperator.NotEqual)
            {
                return Operator.NotEqual(queryFilter.Fieldname, CamlFilterValueTypeBuilder(queryFilter.Fieldname, queryFilter.Value.ToString()), CamlFilterValueBuilder(queryFilter.Fieldname, queryFilter.Value.ToString()));
            }
            else if (queryFilter.Op == FilterOperator.GreaterThan)
            {
                return Operator.GreaterThan(queryFilter.Fieldname, CamlFilterValueTypeBuilder(queryFilter.Fieldname, queryFilter.Value.ToString()), CamlFilterValueBuilder(queryFilter.Fieldname, queryFilter.Value.ToString()));
            }
            else if (queryFilter.Op == FilterOperator.LessThan)
            {
                return Operator.LowerThan(queryFilter.Fieldname, CamlFilterValueTypeBuilder(queryFilter.Fieldname, queryFilter.Value.ToString()), CamlFilterValueBuilder(queryFilter.Fieldname, queryFilter.Value.ToString()));
            }

            return null;
        }
        #endregion

        private static ContentLocation MapToContentLocation(string webUrl)
        {
            if (string.IsNullOrEmpty(webUrl) || webUrl.StartsWith("~sitecollection", StringComparison.InvariantCultureIgnoreCase))
            {
                if (webUrl.StartsWith("~sitecollection", StringComparison.InvariantCultureIgnoreCase))
                {
                    var parts = webUrl.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1 && parts[1] != "/")
                    {
                        return ContentLocation.SelectedSites;
                    }
                }
                else
                {
                    return ContentLocation.CurrentSiteCollection;
                }
            }
            else if (webUrl.Equals("~site", StringComparison.InvariantCultureIgnoreCase))
            {
                return ContentLocation.CurrentSite;
            }

            return ContentLocation.CurrentSiteCollection;
        }

        private SiteMetadata MapToSiteMetadata(string subSiteUrl, ref ContentByQuerySearchTransformatorResult result)
        {
            SiteMetadata siteMetadata = new SiteMetadata();

            try
            {
                this.clientContext.Site.EnsureProperties(p => p.Id, p => p.RootWeb);
                this.clientContext.Site.RootWeb.EnsureProperties(p => p.Id, p => p.Url, p => p.ServerRelativeUrl);

                string subSiteUrlToClone = $"{this.clientContext.Site.RootWeb.Url}/{subSiteUrl}";
                using (var subSiteContext = this.clientContext.Clone(subSiteUrlToClone))
                {
                    subSiteContext.Web.EnsureProperties(p => p.Id, p => p.Url, p => p.ServerRelativeUrl, p => p.Title);

                    // Prep site structure data
                    siteMetadata.Acronym = GetAcronym(subSiteContext.Web.Title);
                    siteMetadata.BannerColor = "#986f0b";
                    siteMetadata.Type = "Site";
                    siteMetadata.ItemReference = new SiteReference()
                    {
                        WebId = subSiteContext.Web.Id.ToString(),
                        //Source = "Users",
                        SiteId = this.clientContext.Site.Id.ToString(),
                        Type = "SiteReference",
                    };

                    // Prep data for in the ServerProcessedContent node
                    result.SearchablePlainTexts = $",\"sites[0].Title\": \"{subSiteContext.Web.Title}\"";
                    result.ImageSources = $"\"sites[0].BannerImageUrl\": null";
                    result.Links = $",\"sites[0].Url\": \"{subSiteContext.Web.ServerRelativeUrl}\"";
                }

                return siteMetadata;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetAcronym(string text)
        {
            // return something if no input
            if (string.IsNullOrEmpty(text))
            {
                return "AB";
            }

            text = text.Trim();

            // if we've less than 2 chars then return the text + something
            if (text.Length < 2)
            {
                return $"{text}B";
            }

            var split = text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 1)
            {
                return $"{split[0].Substring(0, 1)}{split[1].Substring(0, 1)}";
            }
            else
            {
                // Take the first two characters from the text
                return text.Substring(0, 2);
            }
        }

        private void MapSearchQueryToFilters(ref SearchQuery query, string searchQuery, JToken fallbackRefinementFilters)
        {
            var parsedQuery = new KQLParser().Parse(searchQuery);

            // if we've refiners defined then transform them to property filters
            if (fallbackRefinementFilters != null)
            {
                foreach (var refinementFilter in fallbackRefinementFilters)
                {
                    var filter = GetPropertyAsString(refinementFilter["n"]);
                    var token = GetPropertyAsString(refinementFilter["t"].First());

                    // Convert token from HEX to ASCII
                    token = ConvertHexToAscii(token.Trim(new char[] { 'ǂ' }));

                    KQLElement k = new KQLElement()
                    {
                        Filter = filter,
                        Value = token,
                        Operator = KQLPropertyOperator.EqualTo,
                        Type = KQLFilterType.PropertyFilter
                    };
                    parsedQuery.Add(k);
                }
            }

            if (parsedQuery == null || parsedQuery.Count == 0)
            {
                return;
            }

            // Handle text filters
            var textFilters = parsedQuery.Where(p => p.Type == KQLFilterType.Text);
            if (textFilters != null && textFilters.Any())
            {
                foreach (var textFilter in textFilters)
                {
                    var s = new SearchQueryFilter
                    {
                        Value = textFilter.Value,
                        FilterType = FilterType.TitleContaining,
                    };

                    query.Filters.Add(s);
                }
            }

            // Handle property filters
            var propertyFilters = parsedQuery.Where(p => p.Type == KQLFilterType.PropertyFilter);
            if (propertyFilters != null && propertyFilters.Any())
            {
                foreach (var propertyFilter in propertyFilters)
                {
                    var s = new SearchQueryFilter
                    {
                        Fieldname = ManagedPropertyCaseFixup(propertyFilter.Filter),
                        Value = ManagedPropertyValueFixup(propertyFilter.Value),
                        Op = KQLOperatorToFilterOperator(propertyFilter.Operator),
                        FilterType = FilterType.Field
                    };

                    s.FieldNameMatchText = s.Fieldname;
                    query.Filters.Add(s);
                }
            }
        }

        private static object ManagedPropertyValueFixup(string value)
        {
            var valueToCheck = value.ToLower();
            switch (valueToCheck)
            {
                case "true": return 1;
                case "false": return 0;
                case "yes": return 1;
                case "no": return 0;
                default: return value;
            }
        }

        private static FilterOperator KQLOperatorToFilterOperator(KQLPropertyOperator op)
        {
            switch (op)
            {
                case KQLPropertyOperator.Matches: return FilterOperator.Contains;
                case KQLPropertyOperator.EqualTo: return FilterOperator.Equals;
                case KQLPropertyOperator.LesserThan: return FilterOperator.Equals;
                case KQLPropertyOperator.GreaterThan: return FilterOperator.BeginsWith;
                case KQLPropertyOperator.GreaterThanOrEqualTo: return FilterOperator.BeginsWith;
                case KQLPropertyOperator.DoesNoEqual: return FilterOperator.NotEqual;
                default: return FilterOperator.Contains;
            }
        }

        private static SortType MapToSortTypeFromResourceSource(string resultSource)
        {
            if (!string.IsNullOrEmpty(resultSource))
            {
                resultSource = resultSource.ToLower();

                switch (resultSource)
                {
                    case "recently changed items":
                        return SortType.MostRecent;
                    case "popular":
                        return SortType.Trending;
                    case "recommended items":
                        return SortType.MostViewed;
                    default:
                        return SortType.MostRecent;
                }
            }
            return SortType.MostRecent;
        }

        private static List<ContentType> MapToContentTypesFromResourceSource(string resultSource, string contentTypeId)
        {
            List<ContentType> cts = new List<ContentType>();

            if (!string.IsNullOrEmpty(resultSource))
            {
                resultSource = resultSource.ToLower();

                switch (resultSource)
                {
                    case "documents":
                        cts.Add(ContentType.Document);
                        cts.Add(ContentType.Image);
                        cts.Add(ContentType.Video);
                        break;
                    case "wiki":
                    case "pages":
                        cts.Add(ContentType.Page);
                        break;
                    case "pictures":
                        cts.Add(ContentType.Image);
                        break;
                    case "local video results":
                        cts.Add(ContentType.Video);
                        break;
                    case "popular":
                    case "recommended items":
                    case "items matching a content type":
                    case "items related to a current user":
                        {
                            cts = MapToContentTypesFromContentType(contentTypeId);
                            break;
                        }
                    default:
                        cts.Add(ContentType.All);
                        break;
                }
            }

            return cts;
        }

        private static List<ContentType> MapToContentTypesFromListTemplate(string listTemplate)
        {
            List<ContentType> cts = new List<ContentType>();

            if (!string.IsNullOrEmpty(listTemplate))
            {
                if (int.TryParse(listTemplate, out int baseListTemplate))
                {
                    switch (baseListTemplate)
                    {
                        case (int)ListTemplateType.DocumentLibrary:
                            cts.Add(ContentType.Document);
                            cts.Add(ContentType.Image);
                            cts.Add(ContentType.Video);
                            break;
                        case (int)ListTemplateType.WebPageLibrary:
                            cts.Add(ContentType.Page);
                            break;
                        case (int)ListTemplateType.Events:
                            cts.Add(ContentType.Event);
                            break;
                        case (int)ListTemplateType.Announcements:
                            cts.Add(ContentType.News);
                            break;
                        case (int)ListTemplateType.Contacts:
                            cts.Add(ContentType.Contact);
                            break;
                        case (int)ListTemplateType.Tasks:
                        case (int)ListTemplateType.TasksWithTimelineAndHierarchy:
                            cts.Add(ContentType.Task);
                            break;
                        case (int)ListTemplateType.Links:
                        case 170: // Promoted Links
                            cts.Add(ContentType.Link);
                            break;
                        case (int)ListTemplateType.PictureLibrary:
                            cts.Add(ContentType.Image);
                            cts.Add(ContentType.Video);
                            break;
                        case (int)ListTemplateType.IssueTracking:
                            cts.Add(ContentType.Issue);
                            break;
                        default:
                            cts.Add(ContentType.All);
                            break;
                    }
                }
            }

            return cts;
        }

        private static List<ContentType> MapToContentTypesFromContentType(string contentTypeId)
        {
            List<ContentType> cts = new List<ContentType>();

            // some easy matching

            // pages
            if (contentTypeId.StartsWith("0x010108") || // wiki page
                contentTypeId.StartsWith("0x010109") || // web part page
                contentTypeId.StartsWith("0x0101009D1CB255DA76424F860D91F20E6C4118")) // modern page and it's childs
            {
                cts.Add(ContentType.Page);
            }
            // Media assets
            else if (contentTypeId.StartsWith("0x0120D520A808") || // video
                     contentTypeId.StartsWith("0x0101009148F5A04DDD49CBA7127AADA5FB792B")) // Rich Media Asset and it's childs
            {
                cts.Add(ContentType.Video);
                cts.Add(ContentType.Image);
            }
            else if (contentTypeId.StartsWith("0x0104")) //Announcement
            {
                // Base document content type
                cts.Add(ContentType.News);
            }
            else if (contentTypeId.StartsWith("0x0106")) //Contact
            {
                // Base document content type
                cts.Add(ContentType.Contact);
            }
            else if (contentTypeId.StartsWith("0x0106")) //Contact
            {
                // Base document content type
                cts.Add(ContentType.Contact);
            }
            else if (contentTypeId.StartsWith("0x0102")) //Event
            {
                // Base document content type
                cts.Add(ContentType.Event);
            }
            else if (contentTypeId.StartsWith("0x0103")) //Issue
            {
                // Base document content type
                cts.Add(ContentType.Issue);
            }
            else if (contentTypeId.StartsWith("0x0105")) //Link
            {
                // Base document content type
                cts.Add(ContentType.Link);
            }
            else if (contentTypeId.StartsWith("0x0108")) //Task
            {
                // Base document content type
                cts.Add(ContentType.Task);
            }
            else if (contentTypeId.StartsWith("0x0101"))
            {
                // Base document content type
                cts.Add(ContentType.Document);
            }

            if (cts.Count == 0)
            {
                cts.Add(ContentType.All);
            }

            return cts;
        }

        private string MapToSort(string sortField)
        {
            if (string.IsNullOrEmpty(sortField))
            {
                return null;
            }

            IEnumerable<Field> foundFields = null;
            this.clientContext.Web.EnsureProperty(p => p.Fields);

            if (Guid.TryParse(sortField, out Guid fieldId))
            {
                foundFields = this.clientContext.Web.Fields.Where(item => item.Id.Equals(fieldId));
            }
            else
            {
                foundFields = this.clientContext.Web.Fields.Where(item => item.InternalName == sortField);
            }

            if (foundFields.FirstOrDefault() != null)
            {
                return foundFields.FirstOrDefault().InternalName;
            }

            return null;
        }

        private SearchQueryFilter MapToFilter(List list, FilterFieldQueryOperator filterOperator, string filterField, string filterFieldValue, FilterChainingOperator? chainingOperatorUsedInCQWP, FilterType filterType = FilterType.Field)
        {
            if (string.IsNullOrEmpty(filterField))
            {
                return null;
            }

            IEnumerable<Field> foundFields = null;

            if (list != null)
            {
                foundFields = list.Context.LoadQuery(list.Fields.Where(item => item.InternalName == filterField));
                list.Context.ExecuteQueryRetry();
            }
            else
            {
                this.clientContext.Web.EnsureProperty(p => p.Fields);

                if (Guid.TryParse(filterField, out Guid fieldId))
                {
                    foundFields = this.clientContext.Web.Fields.Where(item => item.Id.Equals(fieldId));
                }
                else
                {
                    foundFields = this.clientContext.Web.Fields.Where(item => item.InternalName == filterField);
                }
            }

            if (foundFields.FirstOrDefault() != null)
            {
                // Store field for future reference
                this.queryFields.Add(foundFields.FirstOrDefault());

                // Resolve user ID to name
                if (foundFields.FirstOrDefault().FieldTypeKind == FieldType.User)
                {
                    //[11;#i:0#.f|membership|kevinc@bertonline.onmicrosoft.com]
                    if (filterFieldValue.Contains("|"))
                    {
                        // grab UPN
                        string[] accountParts = filterFieldValue.Replace("[", "").Replace("]", "").Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        //string upn = accountParts[accountParts.Length - 1];
                        if (int.TryParse(accountParts[0], out int userId))
                        {
                            var user = this.clientContext.Web.GetUserById(userId);
                            this.clientContext.Load(user, p => p.Title);
                            this.clientContext.ExecuteQueryRetry();
                            filterFieldValue = user.Title;
                        }
                    }
                }

                // Build the search query filter object
                var s = new SearchQueryFilter
                {
                    Value = filterFieldValue,
                    FilterType = filterType,
                    Op = MapQueryFilterOperator(filterOperator),
                    Fieldname = foundFields.FirstOrDefault().InternalName,
                    ChainingOperatorUsedInCQWP = chainingOperatorUsedInCQWP,
                    FieldNameMatchText = ""
                };

                return s;
            }

            return null;
        }

        private static FilterOperator? MapQueryFilterOperator(FilterFieldQueryOperator filterOperator)
        {
            switch (filterOperator)
            {
                case FilterFieldQueryOperator.BeginsWith:
                    {
                        return FilterOperator.BeginsWith;
                    }
                case FilterFieldQueryOperator.Contains:
                    {
                        return FilterOperator.Contains;
                    }
                case FilterFieldQueryOperator.Eq:
                    {
                        return FilterOperator.Equals;
                    }
                case FilterFieldQueryOperator.Neq:
                    {
                        return FilterOperator.NotEqual;
                    }
                case FilterFieldQueryOperator.Gt:
                case FilterFieldQueryOperator.Geq:
                    {
                        return FilterOperator.GreaterThan;
                    }
                case FilterFieldQueryOperator.Lt:
                case FilterFieldQueryOperator.Leq:
                    {
                        return FilterOperator.LessThan;
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        private void SetLayoutTemplate(ContentRollupLayout template)
        {
            this.properties.TemplateId = template;
            this.properties.LayoutId = template.ToString();
        }

        internal string HighlightedContentProperties()
        {
            // Don't serialize null values
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(this.properties, jsonSerializerSettings);

            // Embed DisplayMaps via a placeholder replace
            json = json.Replace("\"%%DisplayMapsPlaceholder%%\"", displayMapJson);
            return json;
        }

        private static bool IsPropertyAvailable(JToken property)
        {
            if (property != null && property.Value<string>() != null)
            {
                return true;
            }

            return false;
        }

        private static string ConvertHexToAscii(String hexString)
        {
            try
            {
                string ascii = string.Empty;

                for (int i = 0; i < hexString.Length; i += 2)
                {
                    string hs = string.Empty;

                    hs = hexString.Substring(i, 2);
                    uint decval = Convert.ToUInt32(hs, 16);
                    char character = Convert.ToChar(decval);
                    ascii += character;

                }

                return ascii;
            }
            catch
            {
                //Eat exceptions
            }

            return string.Empty;
        }

        private static string GetPropertyAsString(JToken property)
        {
            if (IsPropertyAvailable(property))
            {
                var propertyAsString = property.ToString(Formatting.None);

                if (!string.IsNullOrEmpty(propertyAsString))
                {
                    // Clean the surrounding escaped double quotes
                    propertyAsString = propertyAsString.Replace("\"", "").Replace("\\", "");
                    return propertyAsString;
                }
            }

            return null;
        }

        private static bool GetPropertyAsBoolean(JToken property)
        {
            if (IsPropertyAvailable(property))
            {
                var propertyAsString = property.ToString(Formatting.None);

                if (!string.IsNullOrEmpty(propertyAsString))
                {
                    // Clean the surrounding escaped double quotes
                    propertyAsString = propertyAsString.Replace("\"", "").Replace("\\", "");

                    if (Boolean.TryParse(propertyAsString, out bool result))
                    {
                        return result;
                    }
                    else
                    {
                        throw new Exception($"Property value {propertyAsString} could not be converted to boolean.");
                    }
                }
            }

            return false;
        }

        private static string ManagedPropertyCaseFixup(string property)
        {
            var propertyToCheck = property.ToLower();

            switch (propertyToCheck)
            {
                case "account": return "Account";
                case "aboutme": return "AboutMe";
                case "accountname": return "AccountName";
                case "acronymaggre": return "acronymaggre";
                case "assignedto": return "AssignedTo";
                case "attachmenttype": return "AttachmentType";
                case "author": return "Author";
                case "baseofficelocation": return "BaseOfficeLocation";
                case "categorynavigationurl": return "CategoryNavigationUrl";
                case "charset": return "charset";
                case "colleagues": return "colleagues";
                case "combineduserprofilenames": return "CombinedUserProfileNames";
                case "companies": return "companies";
                // changed
                case "contentclass": return "ContentClass";
                case "contentshidden": return "ContentsHidden";
                case "contentsource": return "ContentSource";
                case "contenttype": return "ContentType";
                case "contenttypeid": return "ContentTypeId";
                case "created": return "Created";
                case "createdby": return "CreatedBy";
                case "date00": return "Date00";
                case "date01": return "Date01";
                case "date02": return "Date02";
                case "date03": return "Date03";
                case "date04": return "Date04";
                case "date05": return "Date05";
                case "date06": return "Date06";
                case "date07": return "Date07";
                case "date08": return "Date08";
                case "date09": return "Date09";
                case "decimal00": return "Decimal00";
                case "decimal01": return "Decimal01";
                case "decimal02": return "Decimal02";
                case "decimal03": return "Decimal03";
                case "decimal04": return "Decimal04";
                case "decimal05": return "Decimal05";
                case "decimal06": return "Decimal06";
                case "decimal07": return "Decimal07";
                case "decimal08": return "Decimal08";
                case "decimal09": return "Decimal09";
                case "deeplinks": return "deeplinks";
                case "defaggre": return "defaggre";
                case "department": return "Department";
                case "description": return "Description";
                case "detectedlanguage": return "DetectedLanguage";
                case "displayauthor": return "DisplayAuthor";
                case "displaydate": return "DisplayDate";
                case "dmsdocaccessright": return "DMSDocAccessRight";
                case "dmsdocauthor": return "DMSDocAuthor";
                case "dmsdoctitle": return "DMSDocTitle";
                case "docacl": return "docacl";
                case "doccomments": return "DocComments";
                case "docid": return "DocId";
                case "dockeywords": return "DocKeywords";
                case "docsignature": return "DocSignature";
                case "docsubject": return "DocSubject";
                case "documentsignature": return "DocumentSignature";
                case "domain": return "domain";
                case "double00": return "Double00";
                case "double01": return "Double01";
                case "double02": return "Double02";
                case "double03": return "Double03";
                case "double04": return "Double04";
                case "double05": return "Double05";
                case "double06": return "Double06";
                case "double07": return "Double07";
                case "double08": return "Double08";
                case "double09": return "Double09";
                case "duplicatehash": return "DuplicateHash";
                case "eduassignmentcategory": return "EduAssignmentCategory";
                case "eduassignmentformat": return "EduAssignmentFormat";
                case "edumaximumscore": return "EduMaximumScore";
                case "enddate": return "EndDate";
                case "expirationtime": return "ExpirationTime";
                case "extractedauthor": return "ExtractedAuthor";
                case "extracteddate": return "ExtractedDate";
                case "fileextension": return "FileExtension";
                case "filename": return "Filename";
                case "filetype": return "FileType";
                case "firstlevelcolleagues": return "FirstLevelColleagues";
                case "firstlevelmutualfollowings": return "FirstLevelMutualFollowings";
                case "firstname": return "FirstName";
                case "followallanchor": return "FollowAllAnchor";
                case "format": return "format";
                case "generatedtitle": return "GeneratedTitle";
                case "genre": return "Genre";
                case "hierarchyurl": return "HierarchyUrl";
                case "hithighlightedproperties": return "HitHighlightedProperties";
                case "hithighlightedsummary": return "HitHighlightedSummary";
                case "hostingpartition": return "HostingPartition";
                case "hwboost": return "hwboost";
                case "imagedatecreated": return "ImageDateCreated";
                case "importance": return "importance";
                case "int00": return "Int00";
                case "int01": return "Int01";
                case "int02": return "Int02";
                case "int03": return "Int03";
                case "int04": return "Int04";
                case "int05": return "Int05";
                case "int06": return "Int06";
                case "int07": return "Int07";
                case "int08": return "Int08";
                case "int09": return "Int09";
                case "int10": return "Int10";
                case "int11": return "Int11";
                case "int12": return "Int12";
                case "int13": return "Int13";
                case "int14": return "Int14";
                case "int15": return "Int15";
                case "int16": return "Int16";
                case "int17": return "Int17";
                case "int18": return "Int18";
                case "int19": return "Int19";
                case "int20": return "Int20";
                case "int21": return "Int21";
                case "int22": return "Int22";
                case "int23": return "Int23";
                case "int24": return "Int24";
                case "int25": return "Int25";
                case "int26": return "Int26";
                case "int27": return "Int27";
                case "int28": return "Int28";
                case "int29": return "Int29";
                case "int30": return "Int30";
                case "int31": return "Int31";
                case "int32": return "Int32";
                case "int33": return "Int33";
                case "int34": return "Int34";
                case "int35": return "Int35";
                case "int36": return "Int36";
                case "int37": return "Int37";
                case "int38": return "Int38";
                case "int39": return "Int39";
                case "int40": return "Int40";
                case "int41": return "Int41";
                case "int42": return "Int42";
                case "int43": return "Int43";
                case "int44": return "Int44";
                case "int45": return "Int45";
                case "int46": return "Int46";
                case "int47": return "Int47";
                case "int48": return "Int48";
                case "int49": return "Int49";
                case "interests": return "Interests";
                case "iscontainer": return "IsContainer";
                case "isdata": return "IsData";
                case "isdocument": return "IsDocument";
                case "ismydocuments": return "IsMyDocuments";
                case "ispublishingcatalog": return "IsPublishingCatalog";
                case "isreport": return "IsReport";
                case "jobtitle": return "JobTitle";
                case "keywords": return "Keywords";
                case "language": return "language";
                case "languages": return "languages";
                case "lastmodifiedtime": return "LastModifiedTime";
                case "lastname": return "LastName";
                case "listid": return "ListID";
                case "listitemid": return "ListItemID";
                case "listurl": return "ListUrl";
                case "location": return "Location";
                case "managedproperties": return "ManagedProperties";
                case "mediaduration": return "MediaDuration";
                case "memberships": return "Memberships";
                case "metadataauthor": return "MetadataAuthor";
                case "microblogtype": return "MicroBlogType";
                case "mobilephone": return "MobilePhone";
                case "modifiedby": return "ModifiedBy";
                case "nlcodepage": return "NLCodePage";
                case "notes": return "Notes";
                case "officenumber": return "OfficeNumber";
                case "orgnames": return "OrgNames";
                case "orgparentnames": return "OrgParentNames";
                case "orgparenturls": return "OrgParentUrls";
                case "orgurls": return "OrgUrls";
                case "ows_url": return "OWS_URL";
                // changed
                case "owsmetadatafacetinfo": return "OWSMetadataFacetInfo";
                // changed
                case "owstaxidmetadataalltagsinfo": return "OWSTaxIdMetadataAllTagsInfo";
                // changed
                case "owstaxidproductcatalogitemcategory": return "OWSTaxIdProductCatalogItemCategory";
                case "parentlink": return "ParentLink";
                case "pastprojects": return "PastProjects";
                case "path": return "Path";
                case "people": return "People";
                case "peopleinmedia": return "PeopleInMedia";
                case "peoplekeywords": return "PeopleKeywords";
                case "phonenumber": return "PhoneNumber";
                case "pictureheight": return "PictureHeight";
                case "picturethumbnailurl": return "PictureThumbnailURL";
                case "pictureurl": return "PictureURL";
                case "picturewidth": return "PictureWidth";
                case "postauthor": return "PostAuthor";
                case "preferredname": return "PreferredName";
                case "priority": return "Priority";
                case "privatecolleagues": return "PrivateColleagues";
                case "processingtime": return "processingtime";
                case "productcataloggroupnumberowstext": return "ProductCatalogGroupNumberOWSTEXT";
                case "profileexpertise": return "ProfileExpertise";
                case "profilename": return "ProfileName";
                case "pronunciations": return "Pronunciations";
                case "purpose": return "Purpose";
                case "rankdetail": return "RankDetail";
                case "rankingweighthigh": return "RankingWeightHigh";
                case "rankingweightlow": return "RankingWeightLow";
                case "rankingweightname": return "RankingWeightName";
                case "recommendedfor": return "recommendedfor";
                case "refinabledate00": return "RefinableDate00";
                case "refinabledate01": return "RefinableDate01";
                case "refinabledate02": return "RefinableDate02";
                case "refinabledate03": return "RefinableDate03";
                case "refinabledate04": return "RefinableDate04";
                case "refinabledate05": return "RefinableDate05";
                case "refinabledate06": return "RefinableDate06";
                case "refinabledate07": return "RefinableDate07";
                case "refinabledate08": return "RefinableDate08";
                case "refinabledate09": return "RefinableDate09";
                case "refinabledate10": return "RefinableDate10";
                case "refinabledate11": return "RefinableDate11";
                case "refinabledate12": return "RefinableDate12";
                case "refinabledate13": return "RefinableDate13";
                case "refinabledate14": return "RefinableDate14";
                case "refinabledate15": return "RefinableDate15";
                case "refinabledate16": return "RefinableDate16";
                case "refinabledate17": return "RefinableDate17";
                case "refinabledate18": return "RefinableDate18";
                case "refinabledate19": return "RefinableDate19";
                case "refinabledecimal00": return "RefinableDecimal00";
                case "refinabledecimal01": return "RefinableDecimal01";
                case "refinabledecimal02": return "RefinableDecimal02";
                case "refinabledecimal03": return "RefinableDecimal03";
                case "refinabledecimal04": return "RefinableDecimal04";
                case "refinabledecimal05": return "RefinableDecimal05";
                case "refinabledecimal06": return "RefinableDecimal06";
                case "refinabledecimal07": return "RefinableDecimal07";
                case "refinabledecimal08": return "RefinableDecimal08";
                case "refinabledecimal09": return "RefinableDecimal09";
                case "refinabledouble00": return "RefinableDouble00";
                case "refinabledouble01": return "RefinableDouble01";
                case "refinabledouble02": return "RefinableDouble02";
                case "refinabledouble03": return "RefinableDouble03";
                case "refinabledouble04": return "RefinableDouble04";
                case "refinabledouble05": return "RefinableDouble05";
                case "refinabledouble06": return "RefinableDouble06";
                case "refinabledouble07": return "RefinableDouble07";
                case "refinabledouble08": return "RefinableDouble08";
                case "refinabledouble09": return "RefinableDouble09";
                case "refinableint00": return "RefinableInt00";
                case "refinableint01": return "RefinableInt01";
                case "refinableint02": return "RefinableInt02";
                case "refinableint03": return "RefinableInt03";
                case "refinableint04": return "RefinableInt04";
                case "refinableint05": return "RefinableInt05";
                case "refinableint06": return "RefinableInt06";
                case "refinableint07": return "RefinableInt07";
                case "refinableint08": return "RefinableInt08";
                case "refinableint09": return "RefinableInt09";
                case "refinableint10": return "RefinableInt10";
                case "refinableint11": return "RefinableInt11";
                case "refinableint12": return "RefinableInt12";
                case "refinableint13": return "RefinableInt13";
                case "refinableint14": return "RefinableInt14";
                case "refinableint15": return "RefinableInt15";
                case "refinableint16": return "RefinableInt16";
                case "refinableint17": return "RefinableInt17";
                case "refinableint18": return "RefinableInt18";
                case "refinableint19": return "RefinableInt19";
                case "refinableint20": return "RefinableInt20";
                case "refinableint21": return "RefinableInt21";
                case "refinableint22": return "RefinableInt22";
                case "refinableint23": return "RefinableInt23";
                case "refinableint24": return "RefinableInt24";
                case "refinableint25": return "RefinableInt25";
                case "refinableint26": return "RefinableInt26";
                case "refinableint27": return "RefinableInt27";
                case "refinableint28": return "RefinableInt28";
                case "refinableint29": return "RefinableInt29";
                case "refinableint30": return "RefinableInt30";
                case "refinableint31": return "RefinableInt31";
                case "refinableint32": return "RefinableInt32";
                case "refinableint33": return "RefinableInt33";
                case "refinableint34": return "RefinableInt34";
                case "refinableint35": return "RefinableInt35";
                case "refinableint36": return "RefinableInt36";
                case "refinableint37": return "RefinableInt37";
                case "refinableint38": return "RefinableInt38";
                case "refinableint39": return "RefinableInt39";
                case "refinableint40": return "RefinableInt40";
                case "refinableint41": return "RefinableInt41";
                case "refinableint42": return "RefinableInt42";
                case "refinableint43": return "RefinableInt43";
                case "refinableint44": return "RefinableInt44";
                case "refinableint45": return "RefinableInt45";
                case "refinableint46": return "RefinableInt46";
                case "refinableint47": return "RefinableInt47";
                case "refinableint48": return "RefinableInt48";
                case "refinableint49": return "RefinableInt49";
                case "refinablestring00": return "RefinableString00";
                case "refinablestring01": return "RefinableString01";
                case "refinablestring02": return "RefinableString02";
                case "refinablestring03": return "RefinableString03";
                case "refinablestring04": return "RefinableString04";
                case "refinablestring05": return "RefinableString05";
                case "refinablestring06": return "RefinableString06";
                case "refinablestring07": return "RefinableString07";
                case "refinablestring08": return "RefinableString08";
                case "refinablestring09": return "RefinableString09";
                case "refinablestring10": return "RefinableString10";
                case "refinablestring11": return "RefinableString11";
                case "refinablestring12": return "RefinableString12";
                case "refinablestring13": return "RefinableString13";
                case "refinablestring14": return "RefinableString14";
                case "refinablestring15": return "RefinableString15";
                case "refinablestring16": return "RefinableString16";
                case "refinablestring17": return "RefinableString17";
                case "refinablestring18": return "RefinableString18";
                case "refinablestring19": return "RefinableString19";
                case "refinablestring20": return "RefinableString20";
                case "refinablestring21": return "RefinableString21";
                case "refinablestring22": return "RefinableString22";
                case "refinablestring23": return "RefinableString23";
                case "refinablestring24": return "RefinableString24";
                case "refinablestring25": return "RefinableString25";
                case "refinablestring26": return "RefinableString26";
                case "refinablestring27": return "RefinableString27";
                case "refinablestring28": return "RefinableString28";
                case "refinablestring29": return "RefinableString29";
                case "refinablestring30": return "RefinableString30";
                case "refinablestring31": return "RefinableString31";
                case "refinablestring32": return "RefinableString32";
                case "refinablestring33": return "RefinableString33";
                case "refinablestring34": return "RefinableString34";
                case "refinablestring35": return "RefinableString35";
                case "refinablestring36": return "RefinableString36";
                case "refinablestring37": return "RefinableString37";
                case "refinablestring38": return "RefinableString38";
                case "refinablestring39": return "RefinableString39";
                case "refinablestring40": return "RefinableString40";
                case "refinablestring41": return "RefinableString41";
                case "refinablestring42": return "RefinableString42";
                case "refinablestring43": return "RefinableString43";
                case "refinablestring44": return "RefinableString44";
                case "refinablestring45": return "RefinableString45";
                case "refinablestring46": return "RefinableString46";
                case "refinablestring47": return "RefinableString47";
                case "refinablestring48": return "RefinableString48";
                case "refinablestring49": return "RefinableString49";
                case "refinablestring50": return "RefinableString50";
                case "refinablestring51": return "RefinableString51";
                case "refinablestring52": return "RefinableString52";
                case "refinablestring53": return "RefinableString53";
                case "refinablestring54": return "RefinableString54";
                case "refinablestring55": return "RefinableString55";
                case "refinablestring56": return "RefinableString56";
                case "refinablestring57": return "RefinableString57";
                case "refinablestring58": return "RefinableString58";
                case "refinablestring59": return "RefinableString59";
                case "refinablestring60": return "RefinableString60";
                case "refinablestring61": return "RefinableString61";
                case "refinablestring62": return "RefinableString62";
                case "refinablestring63": return "RefinableString63";
                case "refinablestring64": return "RefinableString64";
                case "refinablestring65": return "RefinableString65";
                case "refinablestring66": return "RefinableString66";
                case "refinablestring67": return "RefinableString67";
                case "refinablestring68": return "RefinableString68";
                case "refinablestring69": return "RefinableString69";
                case "refinablestring70": return "RefinableString70";
                case "refinablestring71": return "RefinableString71";
                case "refinablestring72": return "RefinableString72";
                case "refinablestring73": return "RefinableString73";
                case "refinablestring74": return "RefinableString74";
                case "refinablestring75": return "RefinableString75";
                case "refinablestring76": return "RefinableString76";
                case "refinablestring77": return "RefinableString77";
                case "refinablestring78": return "RefinableString78";
                case "refinablestring79": return "RefinableString79";
                case "refinablestring80": return "RefinableString80";
                case "refinablestring81": return "RefinableString81";
                case "refinablestring82": return "RefinableString82";
                case "refinablestring83": return "RefinableString83";
                case "refinablestring84": return "RefinableString84";
                case "refinablestring85": return "RefinableString85";
                case "refinablestring86": return "RefinableString86";
                case "refinablestring87": return "RefinableString87";
                case "refinablestring88": return "RefinableString88";
                case "refinablestring89": return "RefinableString89";
                case "refinablestring90": return "RefinableString90";
                case "refinablestring91": return "RefinableString91";
                case "refinablestring92": return "RefinableString92";
                case "refinablestring93": return "RefinableString93";
                case "refinablestring94": return "RefinableString94";
                case "refinablestring95": return "RefinableString95";
                case "refinablestring96": return "RefinableString96";
                case "refinablestring97": return "RefinableString97";
                case "refinablestring98": return "RefinableString98";
                case "refinablestring99": return "RefinableString99";
                case "responsibilities": return "Responsibilities";
                case "robotsnoindex": return "RobotsNoIndex";
                case "rootpostid": return "RootPostID";
                case "rootpostownerid": return "RootPostOwnerID";
                case "rootpostuniqueid": return "RootPostUniqueID";
                case "schools": return "Schools";
                case "secondaryfileextension": return "SecondaryFileExtension";
                case "secondlevelcolleagues": return "SecondLevelColleagues";
                case "serverredirectedurl": return "ServerRedirectedURL";
                case "serviceapplicationid": return "ServiceApplicationID";
                case "sharedwithinternal": return "SharedWithInternal";
                case "sipaddress": return "SipAddress";
                case "site": return "Site";
                case "siteclosed": return "SiteClosed";
                case "siteid": return "SiteID";
                case "sitename": return "sitename";
                case "sitetitle": return "SiteTitle";
                case "size": return "Size";
                case "skills": return "Skills";
                case "socialtag": return "SocialTag";
                case "socialtagtexturl": return "SocialTagTextUrl";
                case "spcontenttype": return "SPContentType";
                case "spsiteurl": return "SPSiteURL";
                case "startdate": return "StartDate";
                case "status": return "Status";
                case "tags": return "Tags";
                case "title": return "Title";
                case "tld": return "tld";
                case "urldepth": return "UrlDepth";
                case "urlkeywords": return "urlkeywords";
                case "urls": return "urls";
                case "usageanalyticsid": return "UsageAnalyticsId";
                case "usageeventitemid": return "UsageEventItemId";
                case "username": return "UserName";
                case "userprofile_guid": return "UserProfile_GUID";
                case "webid": return "WebId";
                case "webtemplate": return "WebTemplate";
                case "wikicategory": return "WikiCategory";
                case "wordcustomrefiner1": return "WordCustomRefiner1";
                case "wordcustomrefiner2": return "WordCustomRefiner2";
                case "wordcustomrefiner3": return "WordCustomRefiner3";
                case "wordcustomrefiner4": return "WordCustomRefiner4";
                case "wordcustomrefiner5": return "WordCustomRefiner5";
                case "wordexactcustomrefiner": return "WordExactCustomRefiner";
                case "wordpartcustomrefiner1": return "WordPartCustomRefiner1";
                case "wordpartcustomrefiner2": return "WordPartCustomRefiner2";
                case "wordpartcustomrefiner3": return "WordPartCustomRefiner3";
                case "wordpartcustomrefiner4": return "WordPartCustomRefiner4";
                case "wordpartcustomrefiner5": return "WordPartCustomRefiner5";
                case "wordpartexactcustomrefiner": return "WordPartExactCustomRefiner";
                case "workemail": return "WorkEmail";
                case "workphone": return "WorkPhone";
                case "yomidisplayname": return "YomiDisplayName";
                default: return property;
            }
        }

        #endregion
    }
}
