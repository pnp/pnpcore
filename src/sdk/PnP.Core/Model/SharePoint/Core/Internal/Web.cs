using PnP.Core.Model.Security;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Web class, write your custom code here
    /// </summary>
    [SharePointType("SP.Web", Uri = V, LinqGet = "_api/web/webinfos")]
    [GraphType(Get = "sites/{hostname}:{serverrelativepath}")]
    internal partial class Web : BaseDataModel<IWeb>, IWeb
    {
        private const string V = "_api/web";
        private static readonly Guid MultilingualPagesFeature = new Guid("24611c05-ee19-45da-955f-6602264abaf8");
        private static readonly Guid PageSchedulingFeature = new Guid("e87ca965-5e07-4a23-b007-ddd4b5afb9c7");
        internal const string WebOptionsAdditionalInformationKey = "WebOptions";

        #region Construction
        public Web()
        {
            // Handler to construct the Add request for this list
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

                string body = JsonSerializer.Serialize(webCreationInformation, typeof(ExpandoObject), new JsonSerializerOptions() { IgnoreNullValues = true });

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

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }
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

        public async Task<IPage> NewPageAsync(PageLayoutType pageLayoutType = PageLayoutType.Article)
        {
            return await Page.NewPageAsync(PnPContext, pageLayoutType).ConfigureAwait(false);
        }

        public IPage NewPage(PageLayoutType pageLayoutType = PageLayoutType.Article)
        {
            return NewPageAsync(pageLayoutType).GetAwaiter().GetResult();
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
            string encodedServerRelativeUrl = WebUtility.UrlEncode(serverRelativeUrl.Replace("'", "''")).Replace("+", "%20");
            var apiCall = new ApiCall($"_api/Web/getFolderByServerRelativePath(decodedUrl='{encodedServerRelativeUrl}')", ApiType.SPORest);
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
                if (error.HttpResponseCode == 404 && error.ServerErrorCode == -2130575338)
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
            string encodedServerRelativeUrl = WebUtility.UrlEncode(serverRelativeUrl.Replace("'", "''")).Replace("+", "%20");
            var apiCall = new ApiCall($"_api/Web/getFileByServerRelativePath(decodedUrl='{encodedServerRelativeUrl}')", ApiType.SPORest);
            return apiCall;
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
        public ISharePointUser EnsureUser(string userPrincipalName)
        {
            return EnsureUserAsync(userPrincipalName).GetAwaiter().GetResult();
        }

        public async Task<ISharePointUser> EnsureUserAsync(string userPrincipalName)
        {
            ApiCall apiCall = BuildEnsureUserApiCall(userPrincipalName);

            SharePointUser sharePointUser = new SharePointUser
            {
                PnPContext = PnPContext
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
                PnPContext = PnPContext
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
                PnPContext = PnPContext
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
                PnPContext = PnPContext
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
                PnPContext = PnPContext
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
                PnPContext = PnPContext
            };

            await sharePointUser.RequestBatchAsync(batch, apiCall, HttpMethod.Get).ConfigureAwait(false);

            return sharePointUser;
        }


        private static ApiCall BuildGetUserByIdApiCall(int userId)
        {
            return new ApiCall($"_api/Web/GetUserById({userId})", ApiType.SPORest);
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
        public async Task<bool> IsSyntexContentCenterAsync()
        {
            await EnsurePropertiesAsync(p => p.WebTemplate).ConfigureAwait(false);

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

        #endregion

        #endregion
    }
}
