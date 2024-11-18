using Microsoft.Extensions.Logging;
using PnP.Core.Model.Security;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Requests.SearchConfiguration;
using PnP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Web class, write your custom code here
    /// </summary>
    [SharePointType("SP.Web", Uri = V, LinqGet = "_api/web/webs")]
    [GraphType(Get = "sites/{hostname}:{serverrelativepath}")]
    internal sealed class Web : BaseDataModel<IWeb>, IWeb
    {
        private const string V = "_api/web";
        private static readonly Guid MultilingualPagesFeature = new Guid("24611c05-ee19-45da-955f-6602264abaf8");
        private static readonly Guid PageSchedulingFeature = new Guid("e87ca965-5e07-4a23-b007-ddd4b5afb9c7");
        internal const string WebOptionsAdditionalInformationKey = "WebOptions";
        internal const string IndexedPropertyKeysName = "vti_indexedpropertykeys";

        #region Construction
        public Web()
        {
            // Handler to construct the Add request for this web
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (additionalInformation) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var webOptions = (WebOptions)additionalInformation[WebOptionsAdditionalInformationKey];

                // Build body
                var webCreationInformation = new
                {
                    parameters = new
                    {
                        __metadata = new { type = "SP.WebCreationInformation" },
                        Title = webOptions.Title,
                        Url = webOptions.Url,
                        WebTemplate = webOptions.Template,
                        Description = webOptions.Description,
                        Language = webOptions.Language,
                        UseSamePermissionsAsParentSite = webOptions.InheritPermissions
                    }
                }.AsExpando();

                string body = JsonSerializer.Serialize(webCreationInformation, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);

                return new ApiCall($"{V}/Webs/Add", ApiType.SPORest, body);
            };

            DeleteApiCallOverrideHandler = async (ApiCallRequest apiCallRequest) =>
            {
                await EnsurePropertiesAsync(p => p.Url).ConfigureAwait(false);

                string webRelativePath = Url.ToString().Replace(PnPContext.Web.Url.ToString(), "");

                // We don't allow to delete the web loaded in the current context as that would lead to a useless context
                if (string.IsNullOrEmpty(webRelativePath))
                {
                    apiCallRequest.CancelRequest("Can't delete the web of the loaded context");
                    return apiCallRequest;
                }

                // Ensure we've the correct path to the subsite to delete, since batching always calls the batch endpoint of the site to delete we 
                // can't delete the web calling it's batch endpoint. Using an interactive approach instead
                ApiCall deleteApiCall = new ApiCall($"{Url}/{V}", apiCallRequest.ApiCall.Type)
                {
                    Interactive = true
                };

                return new ApiCallRequest(deleteApiCall);
            };

            PostMappingHandler = (json) =>
            {
                // implement post mapping handler in case you want to do extra data loading/mapping work
            };

            MappingHandler = (FromJson input) =>
            {
                // implement custom mapping logic

                //// Sample of field override, done by setting the UseCustomMapping = true field attribute
                //if (input.FieldName == "NoCrawl")
                //{
                //    return true;
                //}

                //input.Log.LogDebug(PnPCoreResources.Log_Debug_JsonCannotMapField, input.FieldName);

                return null;
            };

            //GetApiCallOverrideHandler = (ApiCallRequest api) =>
            //{
            //    return api;
            //};

        }
        #endregion

        #region Properties
        [GraphProperty("sharepointIds", JsonPath = "webId")]
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public string AccessRequestListUrl { get => GetValue<string>(); set => SetValue(value); }

        public string AccessRequestSiteDescription { get => GetValue<string>(); set => SetValue(value); }

        public bool AllowAutomaticASPXPageIndexing { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowCreateDeclarativeWorkflowForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowDesignerForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowMasterPageEditingForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowRevertFromTemplateForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowRssFeeds { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowSaveDeclarativeWorkflowAsTemplateForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowSavePublishDeclarativeWorkflowForCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public Guid AppInstanceId { get => GetValue<Guid>(); set => SetValue(value); }

        public bool ContainsConfidentialInfo { get => GetValue<bool>(); set => SetValue(value); }

        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        public Guid DefaultNewPageTemplateId { get => GetValue<Guid>(); set => SetValue(value); }

        public Guid DesignPackageId { get => GetValue<Guid>(); set => SetValue(value); }

        public bool DisableRecommendedItems { get => GetValue<bool>(); set => SetValue(value); }

        public bool DocumentLibraryCalloutOfficeWebAppPreviewersDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableMinimalDownload { get => GetValue<bool>(); set => SetValue(value); }

        public FooterVariantThemeType FooterEmphasis { get => GetValue<FooterVariantThemeType>(); set => SetValue(value); }

        public bool FooterEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public FooterLayoutType FooterLayout { get => GetValue<FooterLayoutType>(); set => SetValue(value); }

        public VariantThemeType HeaderEmphasis { get => GetValue<VariantThemeType>(); set => SetValue(value); }

        public HeaderLayoutType HeaderLayout { get => GetValue<HeaderLayoutType>(); set => SetValue(value); }

        public bool HideTitleInHeader { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsHomepageModernized { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsProvisioningComplete { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsRevertHomepageLinkHidden { get => GetValue<bool>(); set => SetValue(value); }

        public int Language { get => GetValue<int>(); set => SetValue(value); }

        public DateTime LastItemModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime LastItemUserModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public LogoAlignment LogoAlignment { get => GetValue<LogoAlignment>(); set => SetValue(value); }

        public string MasterUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool MegaMenuEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NavAudienceTargetingEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NextStepsFirstRunEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool WebTemplatesGalleryFirstRunEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NotificationsInOneDriveForBusinessEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool NotificationsInSharePointEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ObjectCacheEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool PreviewFeaturesEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string PrimaryColor { get => GetValue<string>(); set => SetValue(value); }

        public IRecycleBinItemCollection RecycleBin { get => GetModelCollectionValue<IRecycleBinItemCollection>(); }

        public bool RecycleBinEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool SaveSiteAsTemplateEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string SearchBoxPlaceholderText { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public string SiteLogoDescription { get => GetValue<string>(); set => SetValue(value); }

        public string SiteLogoUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool SyndicationEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public SharingState TenantAdminMembersCanShare { get => GetValue<SharingState>(); set => SetValue(value); }

        public string ThemeData { get => GetValue<string>(); set => SetValue(value); }

        public bool ThirdPartyMdmEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool TreeViewEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool UseAccessRequestDefault { get => GetValue<bool>(); set => SetValue(value); }

        public string WebTemplate { get => GetValue<string>(); set => SetValue(value); }

        public string WebTemplateConfiguration { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("displayName")]
        public string Title { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("description")]
        public string Description { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("webUrl")]
        public Uri Url { get => GetValue<Uri>(); set => SetValue(value); }

        public bool NoCrawl { get => GetValue<bool>(); set => SetValue(value); }

        public string RequestAccessEmail { get => GetValue<string>(); set => SetValue(value); }

        public string WelcomePage { get => GetValue<string>(); set => SetValue(value); }

        public string AlternateCssUrl { get => GetValue<string>(); set => SetValue(value); }

        public string MasterPageUrl { get => GetValue<string>(); set => SetValue(value); }

        public string CustomMasterUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool CommentsOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool QuickLaunchEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsMultilingual { get => GetValue<bool>(); set => SetValue(value); }

        public bool OverwriteTranslationsOnChange { get => GetValue<bool>(); set => SetValue(value); }

        public bool ExcludeFromOfflineClient { get => GetValue<bool>(); set => SetValue(value); }

        public bool MembersCanShare { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableFlows { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableAppViews { get => GetValue<bool>(); set => SetValue(value); }

        public bool HorizontalQuickLaunch { get => GetValue<bool>(); set => SetValue(value); }

        public SearchScope SearchScope { get => GetValue<SearchScope>(); set => SetValue(value); }

        public SearchBoxInNavBar SearchBoxInNavBar { get => GetValue<SearchBoxInNavBar>(); set => SetValue(value); }

        public List<int> SupportedUILanguageIds { get => GetValue<List<int>>(); set => SetValue(value); }

        public IFieldCollection Fields { get => GetModelCollectionValue<IFieldCollection>(); }

        public IFieldCollection AvailableFields { get => GetModelCollectionValue<IFieldCollection>(); }

        // BERT/PAOLO: not possible at this moment after refactoring, somethign to reassess later on
        // A special approach is needed to load all lists, comes down to adding the "system" facet to the select
        //[GraphProperty("lists", Get = "sites/{hostname}:{serverrelativepath}:/lists?$select=" + List.DefaultGraphFieldsToLoad, Expandable = true)]
        //[GraphProperty("lists", Expandable = true)]
        public IListCollection Lists { get => GetModelCollectionValue<IListCollection>(); }

        public IContentTypeCollection ContentTypes { get => GetModelCollectionValue<IContentTypeCollection>(); }

        public IContentTypeCollection AvailableContentTypes { get => GetModelCollectionValue<IContentTypeCollection>(); }

        public IWebCollection Webs { get => GetModelCollectionValue<IWebCollection>(); }

        public IList SiteUserInfoList { get => GetModelValue<IList>(); }

        public IFeatureCollection Features { get => GetModelCollectionValue<IFeatureCollection>(); }

        public IFolder RootFolder { get => GetModelValue<IFolder>(); }

        public IFolderCollection Folders { get => GetModelCollectionValue<IFolderCollection>(); }

        public IPropertyValues AllProperties { get => GetModelValue<IPropertyValues>(); }

        public ISharePointUser CurrentUser { get => GetModelValue<ISharePointUser>(); }

        public ISharePointUser Author { get => GetModelValue<ISharePointUser>(); }

        public ISharePointUserCollection SiteUsers { get => GetModelCollectionValue<ISharePointUserCollection>(); }

        public ISharePointGroupCollection SiteGroups { get => GetModelCollectionValue<ISharePointGroupCollection>(); }

        public IUserCustomActionCollection UserCustomActions { get => GetModelCollectionValue<IUserCustomActionCollection>(); }

        public IBasePermissions EffectiveBasePermissions { get => GetModelValue<IBasePermissions>(); }

        public IRegionalSettings RegionalSettings { get => GetModelValue<IRegionalSettings>(); }

        public ISharePointGroup AssociatedMemberGroup { get => GetModelValue<ISharePointGroup>(); }

        public ISharePointGroup AssociatedOwnerGroup { get => GetModelValue<ISharePointGroup>(); }

        public ISharePointGroup AssociatedVisitorGroup { get => GetModelValue<ISharePointGroup>(); }

        public IRoleAssignmentCollection RoleAssignments { get => GetModelCollectionValue<IRoleAssignmentCollection>(); }

        public IRoleDefinitionCollection RoleDefinitions { get => GetModelCollectionValue<IRoleDefinitionCollection>(); }

        public INavigation Navigation { get => GetModelValue<INavigation>(); }

        public bool HasUniqueRoleAssignments { get => GetValue<bool>(); set => SetValue(value); }

        public IEventReceiverDefinitionCollection EventReceivers { get => GetModelCollectionValue<IEventReceiverDefinitionCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }

        internal IList TaxonomyHiddenList { get; set; }
        #endregion

        #region Extension methods        

        #region Delete

        internal override Task BaseDeleteBatchAsync(Batch batch, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Throw an exception to clarify that batch web delete is not supported
            throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_WebDeleteIsInteractive);
        }

        #endregion

        #region Modern Pages
        public async Task<List<IPage>> GetPagesAsync(string pageName = null)
        {
            return await Page.LoadPagesAsync(PnPContext, pageName).ConfigureAwait(false);
        }

        public List<IPage> GetPages(string pageName = null)
        {
            return GetPagesAsync(pageName).GetAwaiter().GetResult();
        }

        public async Task<IPage> NewPageAsync(PageLayoutType pageLayoutType = PageLayoutType.Article, EditorType editorType = EditorType.CK5)
        {
            return await Page.NewPageAsync(PnPContext, pageLayoutType, editorType).ConfigureAwait(false);
        }

        public IPage NewPage(PageLayoutType pageLayoutType = PageLayoutType.Article, EditorType editorType = EditorType.CK5)
        {
            return NewPageAsync(pageLayoutType, editorType).GetAwaiter().GetResult();
        }

        public async Task<IVivaDashboard> GetVivaDashboardAsync()
        {
            IPage dashboardPage = (await Page.LoadPagesAsync(PnPContext, "Dashboard").ConfigureAwait(false)).FirstOrDefault();
            if (dashboardPage == null)
            {
                return null;
            }

            return new VivaDashboard(dashboardPage);
        }

        public IVivaDashboard GetVivaDashboard()
        {
            return GetVivaDashboardAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region Folders

        #region GetFolderByServerRelativeUrl
        public async Task<IFolder> GetFolderByServerRelativeUrlAsync(string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions)
        {
            // Instantiate a folder, link it the Web as parent and provide it a context. This folder will not be included in the current model
            Folder folder = new Folder()
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await folder.BaseRetrieveAsync(apiOverride: BuildGetFolderByRelativeUrlApiCall(serverRelativeUrl), fromJsonCasting: folder.MappingHandler, postMappingJson: folder.PostMappingHandler, expressions: expressions).ConfigureAwait(false);

            return folder;
        }

        public IFolder GetFolderByServerRelativeUrl(string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions)
        {
            return GetFolderByServerRelativeUrlAsync(serverRelativeUrl, expressions).GetAwaiter().GetResult();
        }

        public async Task<IFolder> GetFolderByServerRelativeUrlBatchAsync(Batch batch, string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions)
        {
            // Instantiate a folder, link it the Web as parent and provide it a context. This folder will not be included in the current model
            Folder folder = new Folder()
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await folder.BaseBatchRetrieveAsync(batch, apiOverride: BuildGetFolderByRelativeUrlApiCall(serverRelativeUrl), fromJsonCasting: folder.MappingHandler, postMappingJson: folder.PostMappingHandler, selectors: expressions).ConfigureAwait(false);

            return folder;
        }

        public IFolder GetFolderByServerRelativeUrlBatch(Batch batch, string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions)
        {
            return GetFolderByServerRelativeUrlBatchAsync(batch, serverRelativeUrl, expressions).GetAwaiter().GetResult();
        }

        public async Task<IFolder> GetFolderByServerRelativeUrlBatchAsync(string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions)
        {
            return await GetFolderByServerRelativeUrlBatchAsync(PnPContext.CurrentBatch, serverRelativeUrl, expressions).ConfigureAwait(false);
        }

        public IFolder GetFolderByServerRelativeUrlBatch(string serverRelativeUrl, params Expression<Func<IFolder, object>>[] expressions)
        {
            return GetFolderByServerRelativeUrlBatchAsync(serverRelativeUrl, expressions).GetAwaiter().GetResult();
        }

        private static ApiCall BuildGetFolderByRelativeUrlApiCall(string serverRelativeUrl)
        {
            // NOTE WebUtility encode spaces to "+" instead of %20
            // Replace %20 by space as otherwise %20 gets encoded as %2520 which will break the API request
            // When using the "parameter" model (@u) the server relative URL has to be specified using /, using \ only works in the non parameterized version (#1412)
            string encodedServerRelativeUrl = WebUtility.UrlEncode(serverRelativeUrl.Replace("'", "''").Replace("%20", " ").Replace("\\", "/")).Replace("+", "%20");
            var apiCall = new ApiCall($"_api/Web/getFolderByServerRelativePath(decodedUrl=@u)?@u='{encodedServerRelativeUrl}'", ApiType.SPORest);
            return apiCall;
        }
        #endregion

        #region GetFolderById
        public async Task<IFolder> GetFolderByIdAsync(Guid folderId, params Expression<Func<IFolder, object>>[] expressions)
        {
            // Instantiate a folder, link it the Web as parent and provide it a context. This folder will not be included in the current model
            Folder folder = new Folder()
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await folder.BaseRetrieveAsync(apiOverride: BuildGetFolderByIdApiCall(folderId), fromJsonCasting: folder.MappingHandler, postMappingJson: folder.PostMappingHandler, expressions: expressions).ConfigureAwait(false);

            return folder;
        }

        public IFolder GetFolderById(Guid folderId, params Expression<Func<IFolder, object>>[] expressions)
        {
            return GetFolderByIdAsync(folderId, expressions).GetAwaiter().GetResult();
        }

        public async Task<IFolder> GetFolderByIdBatchAsync(Batch batch, Guid folderId, params Expression<Func<IFolder, object>>[] expressions)
        {
            // Instantiate a folder, link it the Web as parent and provide it a context. This folder will not be included in the current model
            Folder folder = new Folder()
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await folder.BaseBatchRetrieveAsync(batch, apiOverride: BuildGetFolderByIdApiCall(folderId), fromJsonCasting: folder.MappingHandler, postMappingJson: folder.PostMappingHandler, selectors: expressions).ConfigureAwait(false);

            return folder;
        }

        public IFolder GetFolderByIdBatch(Batch batch, Guid folderId, params Expression<Func<IFolder, object>>[] expressions)
        {
            return GetFolderByIdBatchAsync(batch, folderId, expressions).GetAwaiter().GetResult();
        }

        public async Task<IFolder> GetFolderByIdBatchAsync(Guid folderId, params Expression<Func<IFolder, object>>[] expressions)
        {
            return await GetFolderByIdBatchAsync(PnPContext.CurrentBatch, folderId, expressions).ConfigureAwait(false);
        }

        public IFolder GetFolderByIdBatch(Guid folderId, params Expression<Func<IFolder, object>>[] expressions)
        {
            return GetFolderByIdBatchAsync(folderId, expressions).GetAwaiter().GetResult();
        }

        private static ApiCall BuildGetFolderByIdApiCall(Guid folderId)
        {
            return new ApiCall($"_api/Web/getFolderById(guid'{folderId}')", ApiType.SPORest);
        }
        #endregion


        #endregion

        #region Files
        public IFile GetFileByServerRelativeUrlOrDefault(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions)
        {
            return GetFileByServerRelativeUrlOrDefaultAsync(serverRelativeUrl, expressions).GetAwaiter().GetResult();
        }

        public async Task<IFile> GetFileByServerRelativeUrlOrDefaultAsync(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions)
        {
            try
            {
                return await GetFileByServerRelativeUrlAsync(serverRelativeUrl, expressions).ConfigureAwait(false);
            }
            catch (SharePointRestServiceException ex)
            {
                var error = ex.Error as SharePointRestError;
                // Indicates the file did not exist
                if (File.ErrorIndicatesFileDoesNotExists(error))
                {
                    return null;
                }

                throw;
            }
        }

        public IFile GetFileByServerRelativeUrl(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions)
        {
            return GetFileByServerRelativeUrlAsync(serverRelativeUrl, expressions).GetAwaiter().GetResult();
        }

        public async Task<IFile> GetFileByServerRelativeUrlAsync(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions)
        {
            // Instantiate a file, link it the Web as parent and provide it a context. This folder will not be included in the current model
            File file = new File()
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await file.BaseRetrieveAsync(apiOverride: BuildGetFileByRelativeUrlApiCall(serverRelativeUrl), fromJsonCasting: file.MappingHandler, postMappingJson: file.PostMappingHandler, expressions: expressions).ConfigureAwait(false);
            return file;
        }

        public IFile GetFileByServerRelativeUrlBatch(Batch batch, string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions)
        {
            return GetFileByServerRelativeUrlBatchAsync(batch, serverRelativeUrl, expressions).GetAwaiter().GetResult();
        }

        public IFile GetFileByServerRelativeUrlBatch(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions)
        {
            return GetFileByServerRelativeUrlBatchAsync(serverRelativeUrl, expressions).GetAwaiter().GetResult();
        }

        public async Task<IFile> GetFileByServerRelativeUrlBatchAsync(Batch batch, string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions)
        {
            // Instantiate a file, link it the Web as parent and provide it a context. This folder will not be included in the current model
            File file = new File()
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await file.BaseBatchRetrieveAsync(batch, apiOverride: BuildGetFileByRelativeUrlApiCall(serverRelativeUrl), fromJsonCasting: file.MappingHandler, postMappingJson: file.PostMappingHandler, selectors: expressions).ConfigureAwait(false);
            return file;
        }

        public async Task<IFile> GetFileByServerRelativeUrlBatchAsync(string serverRelativeUrl, params Expression<Func<IFile, object>>[] expressions)
        {
            return await GetFileByServerRelativeUrlBatchAsync(PnPContext.CurrentBatch, serverRelativeUrl, expressions).ConfigureAwait(false);
        }

        private static ApiCall BuildGetFileByRelativeUrlApiCall(string serverRelativeUrl)
        {
            // NOTE WebUtility encode spaces to "+" instead of %20
            // Replace %20 by space as otherwise %20 gets encoded as %2520 which will break the API request
            // When using the "parameter" model (@u) the server relative URL has to be specified using /, using \ only works in the non parameterized version (#1412)
            string encodedServerRelativeUrl = WebUtility.UrlEncode(serverRelativeUrl.Replace("'", "''").Replace("%20", " ").Replace("\\", "/")).Replace("+", "%20");
            var apiCall = new ApiCall($"_api/Web/getFileByServerRelativePath(decodedUrl=@u)?@u='{encodedServerRelativeUrl}'", ApiType.SPORest);
            return apiCall;
        }

        public IFile GetFileById(Guid uniqueFileId, params Expression<Func<IFile, object>>[] expressions)
        {
            return GetFileByIdAsync(uniqueFileId, expressions).GetAwaiter().GetResult();
        }

        public async Task<IFile> GetFileByIdAsync(Guid uniqueFileId, params Expression<Func<IFile, object>>[] expressions)
        {
            // Instantiate a file, link it the Web as parent and provide it a context. This folder will not be included in the current model
            File file = new File()
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await file.BaseRetrieveAsync(apiOverride: BuildGetFileByUniqueIdApiCall(uniqueFileId), fromJsonCasting: file.MappingHandler, postMappingJson: file.PostMappingHandler, expressions: expressions).ConfigureAwait(false);
            return file;
        }

        public IFile GetFileByIdBatch(Batch batch, Guid uniqueFileId, params Expression<Func<IFile, object>>[] expressions)
        {
            return GetFileByIdBatchAsync(batch, uniqueFileId, expressions).GetAwaiter().GetResult();
        }

        public IFile GetFileByIdBatch(Guid uniqueFileId, params Expression<Func<IFile, object>>[] expressions)
        {
            return GetFileByIdBatchAsync(uniqueFileId, expressions).GetAwaiter().GetResult();
        }

        public async Task<IFile> GetFileByIdBatchAsync(Batch batch, Guid uniqueFileId, params Expression<Func<IFile, object>>[] expressions)
        {
            // Instantiate a file, link it the Web as parent and provide it a context. This folder will not be included in the current model
            File file = new File()
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await file.BaseBatchRetrieveAsync(batch, apiOverride: BuildGetFileByUniqueIdApiCall(uniqueFileId), fromJsonCasting: file.MappingHandler, postMappingJson: file.PostMappingHandler, selectors: expressions).ConfigureAwait(false);
            return file;
        }

        public async Task<IFile> GetFileByIdBatchAsync(Guid uniqueFileId, params Expression<Func<IFile, object>>[] expressions)
        {
            return await GetFileByIdBatchAsync(PnPContext.CurrentBatch, uniqueFileId, expressions).ConfigureAwait(false);
        }

        private static ApiCall BuildGetFileByUniqueIdApiCall(Guid uniqueFileId)
        {
            return new ApiCall($"_api/Web/getFileById('{uniqueFileId}')", ApiType.SPORest);
        }

        public async Task<IFile> GetFileByLinkAsync(string link, params Expression<Func<IFile, object>>[] expressions)
        {
            // first encode the passed link
            var encodedLink = DriveHelper.EncodeSharingUrl(link);

            // Let's try to get DriveItem information
            var apiCall = new ApiCall($"shares/{encodedLink}/driveitem?$select=sharepointids,parentreference", ApiType.Graph);

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            return await DeserializeGraphDriveItemAsync(response.Json, expressions).ConfigureAwait(false);
        }

        public IFile GetFileByLink(string link, params Expression<Func<IFile, object>>[] expressions)
        {
            return GetFileByLinkAsync(link, expressions).GetAwaiter().GetResult();
        }

        private async Task<IFile> DeserializeGraphDriveItemAsync(string response, params Expression<Func<IFile, object>>[] expressions)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);

            if (json.TryGetProperty("sharepointIds", out JsonElement parentReference))
            {
                Guid fileUniqueId = Guid.Empty;
                Guid siteId = Guid.Empty;
                Guid webId = Guid.Empty;

                if (parentReference.TryGetProperty("listItemUniqueId", out JsonElement listItemUniqueId))
                {
                    fileUniqueId = listItemUniqueId.GetGuid();
                }

                if (parentReference.TryGetProperty("siteId", out JsonElement siteIdElement))
                {
                    siteId = siteIdElement.GetGuid();
                }

                if (parentReference.TryGetProperty("webId", out JsonElement webIdElement))
                {
                    webId = webIdElement.GetGuid();
                }

                if (PnPContext.Site.Id == siteId && PnPContext.Web.Id == webId)
                {
                    return await PnPContext.Web.GetFileByIdAsync(fileUniqueId, expressions).ConfigureAwait(false);
                }
                else
                {
                    // A PnPContext is bound to the web, creating a new one to get the file from the other site
                    if (json.TryGetProperty("sharepointIds", out JsonElement sharepointIds))
                    {
                        if (sharepointIds.TryGetProperty("siteUrl", out JsonElement siteUrl))
                        {
                            var newContext = await PnPContext.CloneAsync(new Uri(siteUrl.ToString())).ConfigureAwait(false);
                            return await newContext.Web.GetFileByIdAsync(fileUniqueId, expressions).ConfigureAwait(false);
                        }
                    }
                }
            }

            return null;
        }
        #endregion

        #region IsNoScriptSite 
        public async Task<bool> IsNoScriptSiteAsync()
        {
            await EnsurePropertiesAsync(w => w.EffectiveBasePermissions).ConfigureAwait(false);

            // Definition of no-script is not having the AddAndCustomizePages permission
            if (!EffectiveBasePermissions.Has(PermissionKind.AddAndCustomizePages))
            {
                return true;
            }

            return false;
        }

        public bool IsNoScriptSite()
        {
            return IsNoScriptSiteAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region Users

        public ISharePointUser EnsureEveryoneExceptExternalUsers()
        {
            return EnsureEveryoneExceptExternalUsersAsync().GetAwaiter().GetResult();
        }

        public async Task<ISharePointUser> EnsureEveryoneExceptExternalUsersAsync()
        {
            try
            {
                var tenantId = await PnPContext.GetTenantIdAsync().ConfigureAwait(false);
                var loginName = $"c:0-.f|rolemanager|spo-grid-all-users/{tenantId}";
                return await EnsureUserAsync(loginName).ConfigureAwait(false);
            }
            catch(SharePointRestServiceException ex) when (ex.HResult == -2146233088)
            {
                var web = await GetAsync(p=>p.Language).ConfigureAwait(false);
                string userIdentity = null;
                switch (web.Language)
                {
                    case 1025: // Arabic
                        userIdentity = "الجميع باستثناء المستخدمين الخارجيين";
                        break;
                    case 1069: // Basque
                        userIdentity = "Guztiak kanpoko erabiltzaileak izan ezik";
                        break;
                    case 1026: // Bulgarian
                        userIdentity = "Всички освен външни потребители";
                        break;
                    case 1027: // Catalan
                        userIdentity = "Tothom excepte els usuaris externs";
                        break;
                    case 2052: // Chinese (Simplified)
                        userIdentity = "除外部用户外的任何人";
                        break;
                    case 1028: // Chinese (Traditional)
                        userIdentity = "外部使用者以外的所有人";
                        break;
                    case 1050: // Croatian
                        userIdentity = "Svi osim vanjskih korisnika";
                        break;
                    case 1029: // Czech
                        userIdentity = "Všichni kromě externích uživatelů";
                        break;
                    case 1030: // Danish
                        userIdentity = "Alle undtagen eksterne brugere";
                        break;
                    case 1043: // Dutch
                        userIdentity = "Iedereen behalve externe gebruikers";
                        break;
                    case 1033: // English
                        userIdentity = "Everyone except external users";
                        break;
                    case 1061: // Estonian
                        userIdentity = "Kõik peale väliskasutajate";
                        break;
                    case 1035: // Finnish
                        userIdentity = "Kaikki paitsi ulkoiset käyttäjät";
                        break;
                    case 1036: // French
                        userIdentity = "Tout le monde sauf les utilisateurs externes";
                        break;
                    case 1110: // Galician
                        userIdentity = "Todo o mundo excepto os usuarios externos";
                        break;
                    case 1031: // German
                        userIdentity = "Jeder, außer externen Benutzern";
                        break;
                    case 1032: // Greek
                        userIdentity = "Όλοι εκτός από εξωτερικούς χρήστες";
                        break;
                    case 1037: // Hebrew
                        userIdentity = "כולם פרט למשתמשים חיצוניים";
                        break;
                    case 1081: // Hindi
                        userIdentity = "बाह्य उपयोगकर्ताओं को छोड़कर सभी";
                        break;
                    case 1038: // Hungarian
                        userIdentity = "Mindenki, kivéve külső felhasználók";
                        break;
                    case 1057: // Indonesian
                        userIdentity = "Semua orang kecuali pengguna eksternal";
                        break;
                    case 1040: // Italian
                        userIdentity = "Tutti tranne gli utenti esterni";
                        break;
                    case 1041: // Japanese
                        userIdentity = "外部ユーザー以外のすべてのユーザー";
                        break;
                    case 1087: // Kazakh
                        userIdentity = "Сыртқы пайдаланушылардан басқасының барлығы";
                        break;
                    case 1042: // Korean
                        userIdentity = "외부 사용자를 제외한 모든 사람";
                        break;
                    case 1062: // Latvian
                        userIdentity = "Visi, izņemot ārējos lietotājus";
                        break;
                    case 1063: // Lithuanian
                        userIdentity = "Visi, išskyrus išorinius vartotojus";
                        break;
                    case 1086: // Malay
                        userIdentity = "Semua orang kecuali pengguna luaran";
                        break;
                    case 1044: // Norwegian (Bokmål)
                        userIdentity = "Alle bortsett fra eksterne brukere";
                        break;
                    case 1045: // Polish
                        userIdentity = "Wszyscy oprócz użytkowników zewnętrznych";
                        break;
                    case 1046: // Portuguese (Brazil)
                        userIdentity = "Todos exceto os usuários externos";
                        break;
                    case 2070: // Portuguese (Portugal)
                        userIdentity = "Todos exceto os utilizadores externos";
                        break;
                    case 1048: // Romanian
                        userIdentity = "Toată lumea, cu excepția utilizatorilor externi";
                        break;
                    case 1049: // Russian
                        userIdentity = "Все, кроме внешних пользователей";
                        break;
                    case 10266: // Serbian (Cyrillic, Serbia)
                        userIdentity = "Сви осим спољних корисника";
                        break;
                    case 2074:// Serbian (Latin)
                        userIdentity = "Svi osim spoljnih korisnika";
                        break;
                    case 1051:// Slovak
                        userIdentity = "Všetci okrem externých používateľov";
                        break;
                    case 1060: // Slovenian
                        userIdentity = "Vsi razen zunanji uporabniki";
                        break;
                    case 3082: // Spanish
                        userIdentity = "Todos excepto los usuarios externos";
                        break;
                    case 1053: // Swedish
                        userIdentity = "Alla utom externa användare";
                        break;
                    case 1054: // Thai
                        userIdentity = "ทุกคนยกเว้นผู้ใช้ภายนอก";
                        break;
                    case 1055: // Turkish
                        userIdentity = "Dış kullanıcılar hariç herkes";
                        break;
                    case 1058: // Ukranian
                        userIdentity = "Усі, крім зовнішніх користувачів";
                        break;
                    case 1066: // Vietnamese
                        userIdentity = "Tất cả mọi người trừ người dùng bên ngoài";
                        break;
                }

                if (!string.IsNullOrEmpty(userIdentity))
                {
                    return await EnsureUserAsync(userIdentity).ConfigureAwait(false);
                }
            }

            throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Web_EveyoneExceptUsersCouldNotBeEnsured);
        }

        public ISharePointUser EnsureUser(string userPrincipalName)
        {
            return EnsureUserAsync(userPrincipalName).GetAwaiter().GetResult();
        }

        public async Task<ISharePointUser> EnsureUserAsync(string userPrincipalName)
        {
            ApiCall apiCall = BuildEnsureUserApiCall(userPrincipalName);

            SharePointUser sharePointUser = new SharePointUser
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await sharePointUser.RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            return sharePointUser;
        }

        public ISharePointUser EnsureUserBatch(string userPrincipalName)
        {
            return EnsureUserBatchAsync(PnPContext.CurrentBatch, userPrincipalName).GetAwaiter().GetResult();
        }

        public async Task<ISharePointUser> EnsureUserBatchAsync(string userPrincipalName)
        {
            return await EnsureUserBatchAsync(PnPContext.CurrentBatch, userPrincipalName).ConfigureAwait(false);
        }

        public ISharePointUser EnsureUserBatch(Batch batch, string userPrincipalName)
        {
            return EnsureUserBatchAsync(batch, userPrincipalName).GetAwaiter().GetResult();
        }

        public async Task<ISharePointUser> EnsureUserBatchAsync(Batch batch, string userPrincipalName)
        {
            ApiCall apiCall = BuildEnsureUserApiCall(userPrincipalName);

            SharePointUser sharePointUser = new SharePointUser
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await sharePointUser.RequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);

            return sharePointUser;
        }

        private static ApiCall BuildEnsureUserApiCall(string userPrincipalName)
        {
            // Possible inputs
            // i:0#.f|membership|bert.jansen@bertonline.onmicrosoft.com
            // bert.jansen@bertonline.onmicrosoft.com
            // Bert Jansen (Cloud)
            // Everyone except external users

            if (string.IsNullOrEmpty(userPrincipalName))
            {
                throw new ArgumentNullException(nameof(userPrincipalName));
            }

            int count = 0;
            foreach (char c in userPrincipalName)
            {
                if (c == '|')
                {
                    count++;
                }
            }

            string logonNameValue;
            if (count < 2)
            {
                if (userPrincipalName.Contains("@"))
                {
                    //bert.jansen@bertonline.onmicrosoft.com
                    logonNameValue = $"i:0#.f|membership|{userPrincipalName}";
                }
                else
                {
                    // Bert Jansen (Cloud)
                    // Everyone except external users
                    logonNameValue = userPrincipalName;
                }
            }
            else
            {
                // i:0#.f|membership|bert.jansen@bertonline.onmicrosoft.com
                logonNameValue = userPrincipalName;
            }

            var parameters = new
            {
                logonName = logonNameValue
            }.AsExpando();

            string body = JsonSerializer.Serialize(parameters, typeof(ExpandoObject));

            var apiCall = new ApiCall("_api/Web/EnsureUser", ApiType.SPORest, body);
            return apiCall;
        }

        public ISharePointUser GetCurrentUser()
        {
            return GetCurrentUserAsync().GetAwaiter().GetResult();
        }

        public async Task<ISharePointUser> GetCurrentUserAsync()
        {
            ApiCall apiCall = BuildGetCurrentUserApiCall();

            SharePointUser sharePointUser = new SharePointUser
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await sharePointUser.RequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            return sharePointUser;
        }

        public ISharePointUser GetCurrentUserBatch()
        {
            return GetCurrentUserBatchAsync(PnPContext.CurrentBatch).GetAwaiter().GetResult();
        }

        public async Task<ISharePointUser> GetCurrentUserBatchAsync()
        {
            return await GetCurrentUserBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public ISharePointUser GetCurrentUserBatch(Batch batch)
        {
            return GetCurrentUserBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task<ISharePointUser> GetCurrentUserBatchAsync(Batch batch)
        {
            ApiCall apiCall = BuildGetCurrentUserApiCall();

            SharePointUser sharePointUser = new SharePointUser
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await sharePointUser.RequestBatchAsync(batch, apiCall, HttpMethod.Get).ConfigureAwait(false);

            return sharePointUser;
        }

        private static ApiCall BuildGetCurrentUserApiCall()
        {
            return new ApiCall("_api/Web/CurrentUser", ApiType.SPORest);
        }

        public ISharePointUser GetUserById(int userId)
        {
            return GetUserByIdAsync(userId).GetAwaiter().GetResult();
        }

        public async Task<ISharePointUser> GetUserByIdAsync(int userId)
        {
            ApiCall apiCall = BuildGetUserByIdApiCall(userId);

            SharePointUser sharePointUser = new SharePointUser
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await sharePointUser.RequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            return sharePointUser;
        }

        public ISharePointUser GetUserByIdBatch(int userId)
        {
            return GetUserByIdBatchAsync(PnPContext.CurrentBatch, userId).GetAwaiter().GetResult();
        }

        public async Task<ISharePointUser> GetUserByIdBatchAsync(int userId)
        {
            return await GetUserByIdBatchAsync(PnPContext.CurrentBatch, userId).ConfigureAwait(false);
        }

        public ISharePointUser GetUserByIdBatch(Batch batch, int userId)
        {
            return GetUserByIdBatchAsync(batch, userId).GetAwaiter().GetResult();
        }

        public async Task<ISharePointUser> GetUserByIdBatchAsync(Batch batch, int userId)
        {
            ApiCall apiCall = BuildGetUserByIdApiCall(userId);

            SharePointUser sharePointUser = new SharePointUser
            {
                PnPContext = PnPContext,
                Parent = this
            };

            await sharePointUser.RequestBatchAsync(batch, apiCall, HttpMethod.Get).ConfigureAwait(false);

            return sharePointUser;
        }


        private static ApiCall BuildGetUserByIdApiCall(int userId)
        {
            return new ApiCall($"_api/Web/GetUserById({userId})", ApiType.SPORest);
        }

        public async Task<IList<string>> ValidateUsersAsync(IList<string> userList)
        {
            List<string> nonExistingUsers = new();

            if (userList == null || userList.Count == 0)
            {
                return nonExistingUsers;
            }

            List<Tuple<string, BatchRequest>> requests = new();
            var batch = PnPContext.NewBatch();
            foreach (var user in userList)
            {
                requests.Add(Tuple.Create(user, await RawRequestBatchAsync(batch, new ApiCall($"users/{user}", ApiType.Graph), HttpMethod.Get, "GetUser").ConfigureAwait(false)));
            }
            await PnPContext.ExecuteAsync(batch, false).ConfigureAwait(false);

            foreach (var request in requests)
            {
                if (request.Item2.ResponseHeaders.Count == 0)
                {
                    nonExistingUsers.Add(request.Item1);
                }
            }

            return nonExistingUsers;
        }

        public IList<string> ValidateUsers(IList<string> userList)
        {
            return ValidateUsersAsync(userList).GetAwaiter().GetResult();
        }

        public async Task<IList<ISharePointUser>> ValidateAndEnsureUsersAsync(IList<string> userList)
        {
            var nonExistingUsers = await ValidateUsersAsync(userList).ConfigureAwait(false);

            List<ISharePointUser> ensuredUsers = new();

            var batch = PnPContext.NewBatch();
            foreach (var user in userList)
            {
                if (!nonExistingUsers.Contains(user))
                {
                    ensuredUsers.Add(await EnsureUserBatchAsync(batch, user).ConfigureAwait(false));
                }
            }
            await PnPContext.ExecuteAsync(batch, false).ConfigureAwait(false);

            return ensuredUsers;
        }

        public IList<ISharePointUser> ValidateAndEnsureUsers(IList<string> userList)
        {
            return ValidateAndEnsureUsersAsync(userList).GetAwaiter().GetResult();
        }

        #endregion

        #region Multilingual

        public void EnsureMultilingual(List<int> requiredLanguageIds)
        {
            EnsureMultilingualAsync(requiredLanguageIds).GetAwaiter().GetResult();
        }

        public async Task EnsureMultilingualAsync(List<int> requiredLanguageIds)
        {
            // Ensure the needed web properties are loaded
            await EnsurePropertiesAsync(p => p.IsMultilingual, p => p.SupportedUILanguageIds, p => p.Features).ConfigureAwait(false);

            bool updated = false;
            // Ensure the multilingual page feature is enabled
            if (Features.AsRequested().FirstOrDefault(p => p.DefinitionId == MultilingualPagesFeature) == null)
            {
                await Features.EnableBatchAsync(MultilingualPagesFeature).ConfigureAwait(false);
                updated = true;
            }

            if (!IsMultilingual)
            {
                // Make site multi-lingual and load the available languages
                IsMultilingual = true;
                updated = true;
            }

            foreach (var language in requiredLanguageIds)
            {
                if (!SupportedUILanguageIds.Contains(language))
                {
                    SupportedUILanguageIds.Add(language);
                    updated = true;
                }
            }

            if (updated)
            {
                await UpdateBatchAsync().ConfigureAwait(false);
                await this.GetBatchAsync(p => p.SupportedUILanguageIds).ConfigureAwait(false);
                await PnPContext.ExecuteAsync().ConfigureAwait(false);
            }
        }

        #endregion

        #region Syntex support

        public async Task<bool> IsSyntexEnabledAsync()
        {
            ApiCall apiCall = new ApiCall($"_api/machinelearning/MachineLearningEnabled/MachineLearningCaptureEnabled", ApiType.SPORest);
            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            var machineLearningCaptureEnabled = JsonSerializer.Deserialize<JsonElement>(response.Json).GetProperty("value");

            return machineLearningCaptureEnabled.GetBoolean();
        }

        public bool IsSyntexEnabled()
        {
            return IsSyntexEnabledAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> IsSyntexEnabledForCurrentUserAsync()
        {
            ApiCall apiCall = new ApiCall($"_api/machinelearning/MachineLearningEnabled/UserSyntexEnabled", ApiType.SPORest);
            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            var machineLearningCaptureEnabled = JsonSerializer.Deserialize<JsonElement>(response.Json).GetProperty("value");

            return machineLearningCaptureEnabled.GetBoolean();
        }

        public bool IsSyntexEnabledForCurrentUser()
        {
            return IsSyntexEnabledForCurrentUserAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> IsSyntexContentCenterAsync()
        {
            await EnsurePropertiesAsync(p => p.WebTemplate).ConfigureAwait(false);
            return IsSyntexContentCenterCheck();
        }

        private bool IsSyntexContentCenterCheck()
        {
            // Syntex Content Center sites use a specific template
            if (WebTemplate == "CONTENTCTR")
            {
                return true;
            }

            return false;
        }

        public bool IsSyntexContentCenter()
        {
            return IsSyntexContentCenterAsync().GetAwaiter().GetResult();
        }

        public async Task<ISyntexContentCenter> AsSyntexContentCenterAsync()
        {
            if (await IsSyntexContentCenterAsync().ConfigureAwait(false))
            {
                SyntexContentCenter syntexContentCenter = new SyntexContentCenter()
                {
                    Web = this
                };

                return syntexContentCenter;
            }
            else
            {
                return null;
            }
        }

        public ISyntexContentCenter AsSyntexContentCenter()
        {
            return AsSyntexContentCenterAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region Hub Sites

        /// <summary>
        /// Sync the hub site theme from parent hub site
        /// </summary>
        public Task SyncHubSiteThemeAsync()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Is Sub site
        public async Task<bool> IsSubSiteAsync()
        {
            await PnPContext.Site.EnsurePropertiesAsync(p => p.RootWeb).ConfigureAwait(false);

            // If the id's differ this implies the current web is a sub site
            return PnPContext.Site.RootWeb.Id != Id;
        }

        public bool IsSubSite()
        {
            return IsSubSiteAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region AccessRequest

        public async Task SetAccessRequest(AccessRequestOption operation, string email = null)
        {
            await SetAccessRequestAsync(operation, email).ConfigureAwait(false);
        }

        public async Task SetAccessRequestAsync(AccessRequestOption operation, string email = null)
        {
            if (string.IsNullOrEmpty(email) && operation == AccessRequestOption.SpecificMail)
            {
                throw new ArgumentNullException(PnPCoreResources.Exception_Unsupported_AccessRequest_NoMail);
            }

            if (!string.IsNullOrEmpty(email) && operation != AccessRequestOption.SpecificMail)
            {
                throw new ArgumentNullException(PnPCoreResources.Exception_Unsupported_AccessRequest_MailNotSupported);
            }

            // the option is always true except when it needs to be disabled
            string targetMail = email;
            if (operation == AccessRequestOption.Enabled)
            {
                targetMail = "someone@someone.com";
            }

            if (operation == AccessRequestOption.Disabled)
            {
                targetMail = "";
            }

            // Build body
            var useAccessRequest = new
            {
                useAccessRequestDefault = operation == AccessRequestOption.Enabled ? "true" : "false"
            }.AsExpando();

            string useAccessRequestBody = JsonSerializer.Serialize(useAccessRequest, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);
            var setUseAccessRequestDefaultAndUpdateRequest = new ApiCall($"_api/web/setUseaccessrequestdefaultandupdate", ApiType.SPORest, useAccessRequestBody);
            await RawRequestAsync(setUseAccessRequestDefaultAndUpdateRequest, HttpMethod.Post).ConfigureAwait(false);

            var updateRequestAccessMail = new
            {
                __metadata = new { type = "SP.Web" },
                RequestAccessEmail = targetMail
            }.AsExpando();

            string updateRequestAccessMailBody = JsonSerializer.Serialize(updateRequestAccessMail, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);
            var updateRequestAccessMailRequest = new ApiCall($"_api/web", ApiType.SPORest, updateRequestAccessMailBody);
            await RawRequestAsync(updateRequestAccessMailRequest, new HttpMethod("PATCH")).ConfigureAwait(false);
        }

        #endregion

        #region Ensure page scheduling

        public async Task EnsurePageSchedulingAsync()
        {
            // Page scheduling only works for the root site of the site collection
            if (await IsSubSiteAsync().ConfigureAwait(false))
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_PagePublishingOnSubWeb);
            }

            // Ensure the needed web properties are loaded
            await EnsurePropertiesAsync(p => p.Features).ConfigureAwait(false);

            // Ensure the page scheduling page feature is enabled
            if (Features.AsRequested().FirstOrDefault(p => p.DefinitionId == PageSchedulingFeature) == null)
            {
                await Features.EnableAsync(PageSchedulingFeature).ConfigureAwait(false);
            }
        }

        public void EnsurePageScheduling()
        {
            EnsurePageSchedulingAsync().GetAwaiter().GetResult();
        }

        #endregion

        #region Get Changes

        public async Task<IList<IChange>> GetChangesAsync(ChangeQueryOptions query)
        {
            var apiCall = ChangeCollectionHandler.GetApiCall(this, query);
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            return ChangeCollectionHandler.Deserialize(response, this, PnPContext).ToList();
        }

        public IList<IChange> GetChanges(ChangeQueryOptions query)
        {
            return GetChangesAsync(query).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<IChange>> GetChangesBatchAsync(Batch batch, ChangeQueryOptions query)
        {
            var apiCall = ChangeCollectionHandler.GetApiCall(this, query);
            apiCall.RawEnumerableResult = new List<IChange>();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                var batchFirstRequest = batch.Requests.First().Value;
                ApiCallResponse response = new ApiCallResponse(apiCall, json, System.Net.HttpStatusCode.OK, batchFirstRequest.Id, batchFirstRequest.ResponseHeaders);
                ((List<IChange>)apiCall.RawEnumerableResult).AddRange(ChangeCollectionHandler.Deserialize(response, this, PnPContext).ToList());
            };

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);

            return new BatchEnumerableBatchResult<IChange>(batch, batchRequest.Id, (IReadOnlyList<IChange>)apiCall.RawEnumerableResult);
        }

        public IEnumerableBatchResult<IChange> GetChangesBatch(Batch batch, ChangeQueryOptions query)
        {
            return GetChangesBatchAsync(batch, query).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<IChange>> GetChangesBatchAsync(ChangeQueryOptions query)
        {
            return await GetChangesBatchAsync(PnPContext.CurrentBatch, query).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<IChange> GetChangesBatch(ChangeQueryOptions query)
        {
            return GetChangesBatchAsync(query).GetAwaiter().GetResult();
        }

        #endregion

        #region Security

        public void BreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes)
        {
            BreakRoleInheritanceAsync(copyRoleAssignments, clearSubscopes).GetAwaiter().GetResult();
        }

        public async Task BreakRoleInheritanceAsync(bool copyRoleAssignments, bool clearSubscopes)
        {
            ApiCall apiCall = BuildBreakRoleInheritanceApiCall(copyRoleAssignments, clearSubscopes);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private static ApiCall BuildBreakRoleInheritanceApiCall(bool copyRoleAssignments, bool clearSubscopes)
        {
            return new ApiCall($"_api/Web/breakroleinheritance(copyRoleAssignments={copyRoleAssignments.ToString().ToLower()},clearSubscopes={clearSubscopes.ToString().ToLower()})", ApiType.SPORest);
        }

        public void BreakRoleInheritanceBatch(Batch batch, bool copyRoleAssignments, bool clearSubscopes)
        {
            BreakRoleInheritanceBatchAsync(batch, copyRoleAssignments, clearSubscopes).GetAwaiter().GetResult();
        }

        public async Task BreakRoleInheritanceBatchAsync(Batch batch, bool copyRoleAssignments, bool clearSubscopes)
        {
            ApiCall apiCall = BuildBreakRoleInheritanceApiCall(copyRoleAssignments, clearSubscopes);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void BreakRoleInheritanceBatch(bool copyRoleAssignments, bool clearSubscopes)
        {
            BreakRoleInheritanceBatchAsync(copyRoleAssignments, clearSubscopes).GetAwaiter().GetResult();
        }

        public async Task BreakRoleInheritanceBatchAsync(bool copyRoleAssignments, bool clearSubscopes)
        {
            await BreakRoleInheritanceBatchAsync(PnPContext.CurrentBatch, copyRoleAssignments, clearSubscopes).ConfigureAwait(false);
        }

        public void ResetRoleInheritance()
        {
            ResetRoleInheritanceAsync().GetAwaiter().GetResult();
        }

        public async Task ResetRoleInheritanceAsync()
        {
            ApiCall apiCall = BuildResetRoleInheritanceApiCall();
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private static ApiCall BuildResetRoleInheritanceApiCall()
        {
            return new ApiCall($"_api/Web/resetroleinheritance", ApiType.SPORest);
        }

        public void ResetRoleInheritanceBatch(Batch batch)
        {
            ResetRoleInheritanceBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task ResetRoleInheritanceBatchAsync(Batch batch)
        {
            ApiCall apiCall = BuildResetRoleInheritanceApiCall();
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void ResetRoleInheritanceBatch()
        {
            ResetRoleInheritanceBatchAsync().GetAwaiter().GetResult();
        }

        public async Task ResetRoleInheritanceBatchAsync()
        {
            await ResetRoleInheritanceBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public IRoleDefinitionCollection GetRoleDefinitions(int principalId)
        {
            return GetRoleDefinitionsAsync(principalId).GetAwaiter().GetResult();
        }

        public async Task<IRoleDefinitionCollection> GetRoleDefinitionsAsync(int principalId)
        {
            await EnsurePropertiesAsync(l => l.RoleAssignments).ConfigureAwait(false);
            var roleAssignment = await RoleAssignments
                .QueryProperties(r => r.RoleDefinitions)
                .FirstOrDefaultAsync(p => p.PrincipalId == principalId)
                .ConfigureAwait(false);
            return roleAssignment?.RoleDefinitions;
        }

        public bool AddRoleDefinitions(int principalId, params string[] names)
        {
            return AddRoleDefinitionsAsync(principalId, names).GetAwaiter().GetResult();
        }

        public async Task<bool> AddRoleDefinitionsAsync(int principalId, params string[] names)
        {
            if (names == null || names.Length == 0)
            {
                return false;
            }

            var roleDefinitions = await PnPContext.Web.RoleDefinitions.ToListAsync().ConfigureAwait(false);
            var batch = PnPContext.NewBatch();
            foreach (var name in names)
            {
                var roleDefinition = roleDefinitions.FirstOrDefault(d => d.Name == name);
                if (roleDefinition != null)
                {
                    await AddRoleDefinitionBatchAsync(batch, principalId, roleDefinition).ConfigureAwait(false);
                }
                else
                {
                    throw new ArgumentException(string.Format(PnPCoreResources.Exception_RoleDefinition_NotFound, name));
                }
            }
            // Send role updates to server
            await PnPContext.ExecuteAsync(batch).ConfigureAwait(false);

            return true;
        }

        private ApiCall BuildAddRoleDefinitionsApiCall(int principalId, IRoleDefinition roleDefinition)
        {
            return new ApiCall($"_api/web/roleassignments/addroleassignment(principalid={principalId},roledefid={roleDefinition.Id})", ApiType.SPORest);
        }

        public async Task AddRoleDefinitionAsync(int principalId, IRoleDefinition roleDefinition)
        {
            ApiCall apiCall = BuildAddRoleDefinitionsApiCall(principalId, roleDefinition);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void AddRoleDefinition(int principalId, IRoleDefinition roleDefinition)
        {
            AddRoleDefinitionAsync(principalId, roleDefinition).GetAwaiter().GetResult();
        }

        public void AddRoleDefinitionBatch(Batch batch, int principalId, IRoleDefinition roleDefinition)
        {
            AddRoleDefinitionBatchAsync(batch, principalId, roleDefinition).GetAwaiter().GetResult();
        }

        public async Task AddRoleDefinitionBatchAsync(Batch batch, int principalId, IRoleDefinition roleDefinition)
        {
            ApiCall apiCall = BuildAddRoleDefinitionsApiCall(principalId, roleDefinition);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void AddRoleDefinitionBatch(int principalId, IRoleDefinition roleDefinition)
        {
            AddRoleDefinitionBatchAsync(principalId, roleDefinition).GetAwaiter().GetResult();
        }

        public async Task AddRoleDefinitionBatchAsync(int principalId, IRoleDefinition roleDefinition)
        {
            await AddRoleDefinitionBatchAsync(PnPContext.CurrentBatch, principalId, roleDefinition).ConfigureAwait(false);
        }

        public bool RemoveRoleDefinitions(int principalId, params string[] names)
        {
            return RemoveRoleDefinitionsAsync(principalId, names).GetAwaiter().GetResult();
        }

        public async Task<bool> RemoveRoleDefinitionsAsync(int principalId, params string[] names)
        {
            if (names == null || names.Length == 0)
            {
                return false;
            }

            var roleDefinitions = await GetRoleDefinitionsAsync(principalId).ConfigureAwait(false);
            var batch = PnPContext.NewBatch();
            foreach (var name in names)
            {
                var roleDefinition = roleDefinitions.AsRequested().FirstOrDefault(r => r.Name == name);
                if (roleDefinition != null)
                {
                    await RemoveRoleDefinitionBatchAsync(batch, principalId, roleDefinition).ConfigureAwait(false);
                }
                else
                {
                    throw new ArgumentException(string.Format(PnPCoreResources.Exception_RoleDefinition_NotFound, name));
                }
            }

            // Send role updates to server
            await PnPContext.ExecuteAsync(batch).ConfigureAwait(false);

            return true;
        }

        public async Task RemoveRoleDefinitionAsync(int principalId, IRoleDefinition roleDefinition)
        {
            ApiCall apiCall = BuildRemoveRoleDefinitionApiCall(principalId, roleDefinition);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void RemoveRoleDefinition(int principalId, IRoleDefinition roleDefinition)
        {
            RemoveRoleDefinitionAsync(principalId, roleDefinition).GetAwaiter().GetResult();
        }

        public void RemoveRoleDefinitionBatch(int principalId, IRoleDefinition roleDefinition)
        {
            RemoveRoleDefinitionBatchAsync(principalId, roleDefinition).GetAwaiter().GetResult();
        }

        public async Task RemoveRoleDefinitionBatchAsync(int principalId, IRoleDefinition roleDefinition)
        {
            await RemoveRoleDefinitionBatchAsync(PnPContext.CurrentBatch, principalId, roleDefinition).ConfigureAwait(false);
        }

        public void RemoveRoleDefinitionBatch(Batch batch, int principalId, IRoleDefinition roleDefinition)
        {
            RemoveRoleDefinitionBatchAsync(batch, principalId, roleDefinition).GetAwaiter().GetResult();
        }

        public async Task RemoveRoleDefinitionBatchAsync(Batch batch, int principalId, IRoleDefinition roleDefinition)
        {
            ApiCall apiCall = BuildRemoveRoleDefinitionApiCall(principalId, roleDefinition);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private ApiCall BuildRemoveRoleDefinitionApiCall(int principalId, IRoleDefinition roleDefinition)
        {
            return new ApiCall($"_api/web/roleassignments/removeroleassignment(principalid={principalId},roledefid={roleDefinition.Id})", ApiType.SPORest);
        }
        #endregion

        #region Has Communication Site features
        public async Task<bool> HasCommunicationSiteFeaturesAsync()
        {
            await EnsurePropertiesAsync(p => p.WebTemplate, p => p.Features).ConfigureAwait(false);

            // Syntex Content Center did enable communication site features in a different manner
            if (IsSyntexContentCenterCheck())
            {
                return true;
            }

            // Was the communication site feature enabled?
            var communicationSiteFeature = Guid.Parse("f39dad74-ea79-46ef-9ef7-fe2370754f6f");
            var feature = Features.AsRequested().FirstOrDefault(p => p.DefinitionId == communicationSiteFeature);
            return feature != null;
        }

        public bool HasCommunicationSiteFeatures()
        {
            return HasCommunicationSiteFeaturesAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region Branding
        public IBrandingManager GetBrandingManager()
        {
            return new BrandingManager(PnPContext);
        }
        #endregion

        #region Search

        public async Task<ISearchResult> SearchAsync(SearchOptions query)
        {
            var apiCall = BuildSearchApiCall(query);
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            SearchResult searchResult = new SearchResult();
            ProcessSearchResults(searchResult, response.Json);
            return searchResult;
        }

        public ISearchResult Search(SearchOptions query)
        {
            return SearchAsync(query).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<ISearchResult>> SearchBatchAsync(SearchOptions query)
        {
            return await SearchBatchAsync(PnPContext.CurrentBatch, query).ConfigureAwait(false);
        }

        public IBatchSingleResult<ISearchResult> SearchBatch(SearchOptions query)
        {
            return SearchBatchAsync(query).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<ISearchResult>> SearchBatchAsync(Batch batch, SearchOptions query)
        {
            var apiCall = BuildSearchApiCall(query);
            apiCall.RawSingleResult = new SearchResult();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                ProcessSearchResults(apiCall.RawSingleResult as SearchResult, json);
            };

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            return new BatchSingleResult<ISearchResult>(batch, batchRequest.Id, apiCall.RawSingleResult as ISearchResult);
        }

        public IBatchSingleResult<ISearchResult> SearchBatch(Batch batch, SearchOptions query)
        {
            return SearchBatchAsync(batch, query).GetAwaiter().GetResult();
        }

        private void ProcessSearchResults(SearchResult searchResult, string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                var parsedSearchResult = JsonSerializer.Deserialize<JsonElement>(json);
                if (parsedSearchResult.ValueKind != JsonValueKind.Null)
                {
                    if (parsedSearchResult.TryGetProperty("ElapsedTime", out JsonElement elapsedTime))
                    {
                        searchResult.ElapsedTime = parsedSearchResult.GetProperty("ElapsedTime").GetInt32();
                    }

                    // Process the result rows
                    if (parsedSearchResult.TryGetProperty("PrimaryQueryResult", out JsonElement primaryQueryResult) &&
                        primaryQueryResult.TryGetProperty("RelevantResults", out JsonElement relevantResults))
                    {
                        searchResult.TotalRows = relevantResults.GetProperty("TotalRows").GetInt64();
                        searchResult.TotalRowsIncludingDuplicates = relevantResults.GetProperty("TotalRowsIncludingDuplicates").GetInt64();

                        if (relevantResults.TryGetProperty("Table", out JsonElement table) && table.TryGetProperty("Rows", out JsonElement rows) && rows.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var row in rows.EnumerateArray())
                            {
                                var processedRow = ProcessSearchResultRow(row);
                                if (processedRow != null)
                                {
                                    searchResult.Rows.Add(processedRow);
                                }
                            }
                        }
                    }

                    // Process the refinement rows
                    if (parsedSearchResult.TryGetProperty("PrimaryQueryResult", out JsonElement primaryQueryResult2) &&
                        primaryQueryResult2.TryGetProperty("RefinementResults", out JsonElement refinementResults) && refinementResults.ValueKind != JsonValueKind.Null &&
                        refinementResults.TryGetProperty("Refiners", out JsonElement refiners) && refiners.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var refiner in refiners.EnumerateArray())
                        {
                            (string usedRefiner, List<ISearchRefinementResult> refinementResultsList) = ProcessSearchRefinementRow(refiner);
                            searchResult.Refinements.Add(usedRefiner, refinementResultsList);
                        }
                    }
                }
            }
        }

        private (string, List<ISearchRefinementResult>) ProcessSearchRefinementRow(JsonElement row)
        {
            List<ISearchRefinementResult> results = new List<ISearchRefinementResult>();
            string refiner = row.GetProperty("Name").GetString();

            if (row.TryGetProperty("Entries", out JsonElement entries) && entries.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in entries.EnumerateArray())
                {
                    SearchRefinementResult result = new SearchRefinementResult()
                    {
                        Count = long.Parse(entry.GetProperty("RefinementCount").GetString()),
                        Name = entry.GetProperty("RefinementName").GetString(),
                        Token = entry.GetProperty("RefinementToken").GetString(),
                        Value = entry.GetProperty("RefinementValue").GetString(),
                    };
                    results.Add(result);
                }
            }

            return (refiner, results);
        }

        private Dictionary<string, object> ProcessSearchResultRow(JsonElement row)
        {
            if (row.TryGetProperty("Cells", out JsonElement cells) && cells.ValueKind == JsonValueKind.Array)
            {
                Dictionary<string, object> result = new Dictionary<string, object>();

                foreach (var cell in cells.EnumerateArray())
                {
                    (string key, object value) = ProcessSearchResultCell(cell);
                    result.Add(key, value);
                }

                return result;
            }

            return null;
        }

        private (string, object) ProcessSearchResultCell(JsonElement cell)
        {
            string valueType = cell.GetProperty("ValueType").GetString();
            string key = cell.GetProperty("Key").GetString();
            string value = cell.GetProperty("Value").GetString();

            switch (valueType)
            {
                case "Null":
                    {
                        return (key, null);
                    };
                case "Edm.Double":
                    {
                        if (double.TryParse(value, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out double result))
                        {
                            return (key, result);
                        }
                        goto default;
                    };
                case "Edm.Decimal":
                    {
                        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out decimal result))
                        {
                            return (key, result);
                        }
                        goto default;
                    };
                case "Edm.Float":
                    {
                        if (float.TryParse(value, NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"), out float result))
                        {
                            return (key, result);
                        }
                        goto default;
                    };
                case "Edm.Int16":
                    {
                        if (short.TryParse(value, out short result))
                        {
                            return (key, result);
                        }
                        goto default;
                    };
                case "Edm.Int32":
                    {
                        if (int.TryParse(value, out int result))
                        {
                            return (key, result);
                        }
                        goto default;
                    };
                case "Edm.Int64":
                    {
                        if (long.TryParse(value, out long result))
                        {
                            return (key, result);
                        }
                        goto default;
                    };
                case "Edm.Guid":
                    {
                        if (Guid.TryParse(value, out Guid result))
                        {
                            return (key, result);
                        }
                        goto default;
                    };
                case "Edm.Boolean":
                    {
                        if (bool.TryParse(value, out bool result))
                        {
                            return (key, result);
                        }
                        goto default;
                    };
                case "Edm.DateTime":
                    {
                        if (DateTime.TryParse(value, out DateTime result))
                        {
                            return (key, result);
                        }
                        goto default;
                    };
                case "Edm.DateTimeOffSet":
                    {
                        if (DateTimeOffset.TryParse(value, out DateTimeOffset result))
                        {
                            return (key, result);
                        }
                        goto default;
                    };
                default:
                    {
                        return (key, value);
                    }
            }
        }

        private ApiCall BuildSearchApiCall(SearchOptions query)
        {
            var endpointUri = $"_api/search/postquery";

            dynamic request = new
            {
                Querytext = query.Query,
                EnableQueryRules = false,
                SourceId = query.ResultSourceId,
            }.AsExpando();

            // The default is true
            if (!query.TrimDuplicates)
            {
                request.TrimDuplicates = false;
            }

            if (query.StartRow.HasValue && query.StartRow.Value > 0)
            {
                request.StartRow = query.StartRow.Value;
            }

            if (query.RowsPerPage.HasValue)
            {
                request.RowsPerPage = query.RowsPerPage.Value;
            }

            if (query.RowLimit.HasValue && query.RowLimit.Value > 0)
            {
                request.RowLimit = query.RowLimit.Value;
            }
            else if (query.RowsPerPage.HasValue)
            {
                request.RowLimit = query.RowsPerPage.Value;
            }

            if (query.SelectProperties.Count > 0)
            {
                request.SelectProperties = new
                {
                    results = query.SelectProperties.ToArray(),
                };
            }

            if (query.SortProperties.Count > 0)
            {
                request.SortList = new
                {
                    results = query.SortProperties.Select(o => new
                    {
                        Property = o.Property,
                        Direction = o.Sort,
                    }).ToArray()
                };
            }

            if (query.RefineProperties.Count > 0)
            {
                request.Refiners = string.Join(",", query.RefineProperties);
            }

            if (query.RefinementFilters.Count > 0)
            {
                request.RefinementFilters = new
                {
                    results = query.RefinementFilters.ToArray()
                };
            }

            if (!string.IsNullOrEmpty(query.ClientType))
            {
                request.ClientType = query.ClientType;
            }

            var body = new
            {
                request
            };

            var jsonBody = JsonSerializer.Serialize(body);
            return new ApiCall(endpointUri, ApiType.SPORest, jsonBody);
        }

        #endregion Search

        #region Web Templates

        public async Task<List<IWebTemplate>> GetWebTemplatesAsync(int lcid, bool includeCrossLanguage)
        {
            var webTemplates = new List<IWebTemplate>();

            var apiCall = BuildWebTemplatesApiCall(lcid, includeCrossLanguage);

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                ProcessWebTemplates(response.Json, webTemplates);
            }

            return webTemplates;
        }

        public List<IWebTemplate> GetWebTemplates(int lcid, bool includeCrossLanguage)
        {
            return GetWebTemplatesAsync(lcid, includeCrossLanguage).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<IWebTemplate>> GetWebTemplatesBatchAsync(int lcid, bool includeCrossLanguage)
        {
            return await GetWebTemplatesBatchAsync(PnPContext.CurrentBatch, lcid, includeCrossLanguage).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<IWebTemplate> GetWebTemplatesBatch(int lcid, bool includeCrossLanguage)
        {
            return GetWebTemplatesBatchAsync(lcid, includeCrossLanguage).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<IWebTemplate>> GetWebTemplatesBatchAsync(Batch batch, int lcid, bool includeCrossLanguage)
        {
            var apiCall = BuildWebTemplatesApiCall(lcid, includeCrossLanguage);

            apiCall.RawEnumerableResult = new List<IWebTemplate>();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                ProcessWebTemplates(json, (List<IWebTemplate>)apiCall.RawEnumerableResult);
            };

            // Add the request to the batch

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Get).ConfigureAwait(false);

            // Return the batch result as Enumerable
            return new BatchEnumerableBatchResult<IWebTemplate>(batch, batchRequest.Id, (IReadOnlyList<IWebTemplate>)apiCall.RawEnumerableResult);
        }

        public IEnumerableBatchResult<IWebTemplate> GetWebTemplatesBatch(Batch batch, int lcid, bool includeCrossLanguage)
        {
            return GetWebTemplatesBatchAsync(batch, lcid, includeCrossLanguage).GetAwaiter().GetResult();
        }

        public async Task<IWebTemplate> GetWebTemplateByNameAsync(string name, int lcid = 1033, bool includeCrossLanguage = false)
        {
            var apiCall = BuildWebTemplatesNameApiCall(lcid, includeCrossLanguage, name: name);
            try
            {
                var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
                return ProcessWebTemplate(response.Json);
            }
            catch (SharePointRestServiceException ex)
            {
                var error = ex.Error as SharePointRestError;
                if (error.ServerErrorCode == -2147024809)
                {
                    throw new ClientException(ErrorType.SharePointRestServiceError, string.Format(PnPCoreResources.Exception_WebTemplate_NotFound, name));
                }
            }
            return null;
        }

        public IWebTemplate GetWebTemplateByName(string name, int lcid = 1033, bool includeCrossLanguage = false)
        {
            return GetWebTemplateByNameAsync(name, lcid, includeCrossLanguage).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<IWebTemplate>> GetWebTemplateByNameBatchAsync(string name, int lcid = 1033, bool includeCrossLanguage = false)
        {
            return await GetWebTemplateByNameBatchAsync(PnPContext.CurrentBatch, name, lcid, includeCrossLanguage).ConfigureAwait(false);
        }

        public IBatchSingleResult<IWebTemplate> GetWebTemplateByNameBatch(string name, int lcid = 1033, bool includeCrossLanguage = false)
        {
            return GetWebTemplateByNameBatchAsync(PnPContext.CurrentBatch, name, lcid, includeCrossLanguage).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<IWebTemplate>> GetWebTemplateByNameBatchAsync(Batch batch, string name, int lcid = 1033, bool includeCrossLanguage = false)
        {
            var apiCall = BuildWebTemplatesNameApiCall(lcid, includeCrossLanguage, name);

            apiCall.RawSingleResult = new WebTemplate();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                var webTemplate = JsonSerializer.Deserialize<WebTemplate>(json, PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue);
                (apiCall.RawSingleResult as WebTemplate).Lcid = webTemplate.Lcid;
                (apiCall.RawSingleResult as WebTemplate).IsHidden = webTemplate.IsHidden;
                (apiCall.RawSingleResult as WebTemplate).Description = webTemplate.Description;
                (apiCall.RawSingleResult as WebTemplate).Name = webTemplate.Name;
                (apiCall.RawSingleResult as WebTemplate).DisplayCategory = webTemplate.DisplayCategory;
                (apiCall.RawSingleResult as WebTemplate).IsSubWebOnly = webTemplate.IsSubWebOnly;
                (apiCall.RawSingleResult as WebTemplate).IsRootWebOnly = webTemplate.IsRootWebOnly;
                (apiCall.RawSingleResult as WebTemplate).Title = webTemplate.Title;
                (apiCall.RawSingleResult as WebTemplate).Id = webTemplate.Id;
                (apiCall.RawSingleResult as WebTemplate).ImageUrl = webTemplate.ImageUrl;
            };

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Get).ConfigureAwait(false);

            return new BatchSingleResult<WebTemplate>(batch, batchRequest.Id, apiCall.RawSingleResult as WebTemplate);

        }

        public IBatchSingleResult<IWebTemplate> GetWebTemplateByNameBatch(Batch batch, string name, int lcid = 1033, bool includeCrossLanguage = false)
        {
            return GetWebTemplateByNameBatchAsync(batch, name, lcid, includeCrossLanguage).GetAwaiter().GetResult();
        }

        private static ApiCall BuildWebTemplatesApiCall(int lcid, bool includeCrossLanguage = false)
        {
            // Have to do includeCrossLanguage ToLower() or error occurs ("The expression \"web/GetAvailableWebTemplates(lcid=1033,doincludecrosslanguage=True)\" is not valid.")
            var baseUrl = $"_api/Web/GetAvailableWebTemplates(lcid={lcid},doincludecrosslanguage={includeCrossLanguage.ToString().ToLower()})";

            return new ApiCall(baseUrl, ApiType.SPORest);
        }

        private static ApiCall BuildWebTemplatesNameApiCall(int lcid, bool includeCrossLanguage, string name)
        {
            // Have to do includeCrossLanguage ToLower() or error occurs ("The expression \"web/GetAvailableWebTemplates(lcid=1033,doincludecrosslanguage=True)\" is not valid.")
            var baseUrl = $"_api/Web/GetAvailableWebTemplates(lcid={lcid},doincludecrosslanguage={includeCrossLanguage.ToString().ToLower()})/GetByName('{name}')";

            return new ApiCall(baseUrl, ApiType.SPORest);
        }

        private static void ProcessWebTemplates(string responseJson, List<IWebTemplate> webTemplatesResult)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(responseJson);

            if (json.TryGetProperty("value", out JsonElement getAvailableWebTemplates))
            {
                var webTemplates = JsonSerializer.Deserialize<IEnumerable<WebTemplate>>(getAvailableWebTemplates.GetRawText(), PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue);
                foreach (var webTemplate in webTemplates)
                {
                    webTemplatesResult.Add(webTemplate);
                }
            }
        }

        private static IWebTemplate ProcessWebTemplate(string responseJson)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(responseJson);

            return JsonSerializer.Deserialize<WebTemplate>(json.GetRawText(), PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue);
        }

        #endregion

        #region Link unfurling        
        public async Task<IUnfurledResource> UnfurlLinkAsync(string link, UnfurlOptions unfurlOptions = null)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                throw new ArgumentNullException(nameof(link));
            }

            return await UnfurlHandler.UnfurlAsync(PnPContext, link, unfurlOptions).ConfigureAwait(false);
        }

        public IUnfurledResource UnfurlLink(string link, UnfurlOptions unfurlOptions = null)
        {
            return UnfurlLinkAsync(link, unfurlOptions).GetAwaiter().GetResult();
        }
        #endregion

        #region Recycle bin

        public async Task<IRecycleBinItemCollection> GetRecycleBinItemsByQueryAsync(RecycleBinQueryOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var apiCall = BuildRecyleBinQueryApiCall(options);

            Web newWeb = new Web
            {
                PnPContext = PnPContext,
                Parent = Parent
            };

            await newWeb.RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            return newWeb.RecycleBin;
        }

        public IRecycleBinItemCollection GetRecycleBinItemsByQuery(RecycleBinQueryOptions options)
        {
            return GetRecycleBinItemsByQueryAsync(options).GetAwaiter().GetResult();
        }

        public async Task<IRecycleBinItemCollection> GetRecycleBinItemsByQueryBatchAsync(RecycleBinQueryOptions options)
        {
            return await GetRecycleBinItemsByQueryBatchAsync(PnPContext.CurrentBatch, options).ConfigureAwait(false);
        }

        public IRecycleBinItemCollection GetRecycleBinItemsByQueryBatch(RecycleBinQueryOptions options)
        {
            return GetRecycleBinItemsByQueryBatchAsync(options).GetAwaiter().GetResult();
        }

        public async Task<IRecycleBinItemCollection> GetRecycleBinItemsByQueryBatchAsync(Batch batch, RecycleBinQueryOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var apiCall = BuildRecyleBinQueryApiCall(options);

            Web newWeb = new Web
            {
                PnPContext = PnPContext,
                Parent = Parent
            };

            await newWeb.RequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);

            return newWeb.RecycleBin;
        }

        public IRecycleBinItemCollection GetRecycleBinItemsByQueryBatch(Batch batch, RecycleBinQueryOptions options)
        {
            return GetRecycleBinItemsByQueryBatchAsync(batch, options).GetAwaiter().GetResult();
        }

        private static ApiCall BuildRecyleBinQueryApiCall(RecycleBinQueryOptions options)
        {
            var baseUrl = $"_api/Web/GetRecycleBinItemsByQueryInfo";

            string queryString = $"?rowLimit=%27{options.RowLimit}%27&isAscending={options.IsAscending.ToString().ToLowerInvariant()}&itemState={(int)options.ItemState}&orderBy={(int)options.OrderBy}&showOnlyMyItems={options.ShowOnlyMyItems.ToString().ToLowerInvariant()}";
            if (!string.IsNullOrEmpty(options.PagingInfo))
            {
                queryString += $"&pagingInfo={options.PagingInfo}";
            }

            return new ApiCall($"{baseUrl}{queryString}", ApiType.SPORest, receivingProperty: nameof(RecycleBin));
        }
        #endregion

        #region Get Search Configuration

        public async Task<string> GetSearchConfigurationXmlAsync()
        {
            ApiCall apiCall = new ApiCall(new List<IRequest<object>> { new ExportSearchConfigurationRequest(SearchObjectLevel.SPWeb) });

            var result = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            return result.ApiCall.CSOMRequests[0].Result.ToString();
        }

        public string GetSearchConfigurationXml()
        {
            return GetSearchConfigurationXmlAsync().GetAwaiter().GetResult();
        }

        public async Task<List<IManagedProperty>> GetSearchConfigurationManagedPropertiesAsync()
        {
            var searchConfiguration = await GetSearchConfigurationXmlAsync().ConfigureAwait(false);

            return SearchConfigurationHandler.GetManagedPropertiesFromConfigurationXml(searchConfiguration);
        }

        public List<IManagedProperty> GetSearchConfigurationManagedProperties()
        {
            return GetSearchConfigurationManagedPropertiesAsync().GetAwaiter().GetResult();
        }

        public async Task SetSearchConfigurationXmlAsync(string configuration)
        {
            if (string.IsNullOrEmpty(configuration))
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            ApiCall apiCall = new ApiCall(new List<IRequest<object>> { new ImportSearchConfigurationRequest(SearchObjectLevel.SPWeb, configuration) });

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void SetSearchConfigurationXml(string configuration)
        {
            SetSearchConfigurationXmlAsync(configuration).GetAwaiter().GetResult();
        }
        #endregion

        #region Get WSS Id for term

        public async Task<int> GetWssIdForTermAsync(string termId)
        {
            if (TaxonomyHiddenList == null)
            {
                await PnPContext.Site.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);
                TaxonomyHiddenList = await PnPContext.Site.RootWeb.Lists.GetByServerRelativeUrlAsync($"{PnPContext.Site.ServerRelativeUrl}/Lists/TaxonomyHiddenList").ConfigureAwait(false);
            }

            var camlQuery = new CamlQueryOptions()
            {
                ViewXml = $@"<View><Query><Where><Eq><FieldRef Name='IdForTerm' /><Value Type='Text'>{termId}</Value></Eq></Where></Query></View>",
                DatesInUtc = true,
            };

            // Clear the cached item from previous runs
            TaxonomyHiddenList.Items.Clear();

            // Fetch the item
            await TaxonomyHiddenList.LoadItemsByCamlQueryAsync(camlQuery).ConfigureAwait(false);
            var items = TaxonomyHiddenList.Items.AsRequested();

            if (items.Any())
            {
                return items.First().Id;
            }
            else
            {
                return -1;
            }
        }

        public int GetWssIdForTerm(string termId)
        {
            return GetWssIdForTermAsync(termId).GetAwaiter().GetResult();
        }

        #endregion

        #region User effective permissions

        public IBasePermissions GetUserEffectivePermissions(string userPrincipalName)
        {
            return GetUserEffectivePermissionsAsync(userPrincipalName).GetAwaiter().GetResult();
        }

        public async Task<IBasePermissions> GetUserEffectivePermissionsAsync(string userPrincipalName)
        {
            if (string.IsNullOrEmpty(userPrincipalName))
            {
                throw new ArgumentNullException(PnPCoreResources.Exception_UserPrincipalNameEmpty);
            }

            var apiCall = BuildGetUserEffectivePermissionsApiCall(userPrincipalName);

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (string.IsNullOrEmpty(response.Json))
            {
                throw new Exception(PnPCoreResources.Exception_EffectivePermissionsNotFound);
            }

            return EffectivePermissionsHandler.ParseGetUserEffectivePermissionsResponse(response.Json);
        }

        private static ApiCall BuildGetUserEffectivePermissionsApiCall(string userPrincipalName)
        {
            return new ApiCall($"_api/web/getusereffectivepermissions('{HttpUtility.UrlEncode("i:0#.f|membership|" + userPrincipalName)}')", ApiType.SPORest)
            {
                SkipCollectionClearing = true
            };
        }

        public bool CheckIfUserHasPermissions(string userPrincipalName, PermissionKind permissionKind)
        {
            return CheckIfUserHasPermissionsAsync(userPrincipalName, permissionKind).GetAwaiter().GetResult();
        }

        public async Task<bool> CheckIfUserHasPermissionsAsync(string userPrincipalName, PermissionKind permissionKind)
        {
            var basePermissions = await GetUserEffectivePermissionsAsync(userPrincipalName).ConfigureAwait(false);
            return basePermissions.Has(permissionKind);
        }

        #endregion

        #region Reindex web
        public async Task ReIndexAsync()
        {
            var webInfo = await GetAsync(p => p.EffectiveBasePermissions,
                                         p => p.AllProperties,
                                         p => p.Lists.QueryProperties(p => p.Title,
                                                                      p => p.NoCrawl,
                                                                      p => p.RootFolder.QueryProperties(p => p.Properties))).ConfigureAwait(false);

            const string reIndexKey = "vti_searchversion";

            // Definition of no-script is not having the AddAndCustomizePages permission
            if (!webInfo.EffectiveBasePermissions.Has(PermissionKind.AddAndCustomizePages))
            {
                // NoScript site, reindex each list separately
                foreach (var list in webInfo.Lists.AsRequested())
                {
                    if (list.NoCrawl)
                    {
                        PnPContext.Logger.LogInformation($"List {list.Title} is configured as NoCrawl, reindex request will be skipped.");
                    }
                    else
                    {
                        int searchVersion = 0;

                        if (list.RootFolder.Properties.Values.ContainsKey(reIndexKey))
                        {
                            searchVersion = list.RootFolder.Properties.GetInteger(reIndexKey, 0);
                        }

                        list.RootFolder.Properties.Values[reIndexKey] = searchVersion + 1;

                        await list.RootFolder.Properties.UpdateAsync().ConfigureAwait(false);
                    }
                }
            }
            else
            {
                // Regular site, we can reindex the site in one go

                int searchVersion = 0;

                if (webInfo.AllProperties.Values.ContainsKey(reIndexKey))
                {
                    searchVersion = webInfo.AllProperties.GetInteger(reIndexKey, 0);
                }

                webInfo.AllProperties.Values[reIndexKey] = searchVersion + 1;

                await webInfo.AllProperties.UpdateAsync().ConfigureAwait(false);
            }
        }

        public void ReIndex()
        {
            ReIndexAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region Indexed properties

        public async Task<bool> AddIndexedPropertyAsync(string propertyName)
        {
            if (AllProperties.Values.ContainsKey(propertyName) == false)
            {
                return false;
            }

            var propertyNameAsBase64String = Convert.ToBase64String(Encoding.Unicode.GetBytes(propertyName));

            var indexedProperties = AllProperties.GetString(IndexedPropertyKeysName, string.Empty)
                .Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();

            if (indexedProperties.Contains<string>(propertyNameAsBase64String))
            {
                return true;
            }

            indexedProperties.Add(propertyNameAsBase64String);

            AllProperties[IndexedPropertyKeysName] = string.Join("|", indexedProperties) + "|";
            await AllProperties.UpdateAsync().ConfigureAwait(false);

            return true;
        }

        public bool AddIndexedProperty(string propertyName)
        {
            return AddIndexedPropertyAsync(propertyName).GetAwaiter().GetResult();
        }

        public async Task<bool> RemoveIndexedPropertyAsync(string propertyName)
        {
            var propertyNameAsBase64String = Convert.ToBase64String(Encoding.Unicode.GetBytes(propertyName));

            var indexedProperties = AllProperties.GetString(IndexedPropertyKeysName, string.Empty)
                .Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (indexedProperties.Contains<string>(propertyNameAsBase64String) == false)
            {
                return false;
            }

            indexedProperties.Remove(propertyNameAsBase64String);

            if (indexedProperties.Any())
            {
                AllProperties[IndexedPropertyKeysName] = string.Join("|", indexedProperties) + "|";
            }
            else
            {
                AllProperties[IndexedPropertyKeysName] = string.Empty;
            }

            await AllProperties.UpdateAsync().ConfigureAwait(false);

            return true;
        }

        public bool RemoveIndexedProperty(string propertyName)
        {
            return RemoveIndexedPropertyAsync(propertyName).GetAwaiter().GetResult();
        }

        #endregion

        #endregion
    }
}
