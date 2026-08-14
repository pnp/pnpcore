using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Connectors;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using PnP.Core.Model.Security;
using PnP.Core.QueryModel;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Resolves the <c>{placeholder}</c> tokens a provisioning template is written in.
    /// </summary>
    public class TokenParser : ICloneable
    {
        private PnPContext _context;
        private List<TokenDefinition> _tokens;
        private Dictionary<string, string> _tokenDictionary;
        private Dictionary<string, TokenDefinition> _nonCacheableTokenDictionary;
        private Dictionary<string, TokenDefinition> _listTokenDictionary;
        private readonly Dictionary<string, string> _listsTitles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private bool _initializedFromHierarchy;
        private int _webLanguage;
        private Guid _webId;

        private static readonly Regex ReToken = new Regex(@"(?:(\{(?:\1??[^{]*?\})))|(?:(\{(?:\1??[^{]*?:)))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ReTokenFallback = new Regex(@"\{.*?\}", RegexOptions.Compiled);
        private static readonly Regex ReGuid = new Regex("(?<guid>\\{\\S{8}-\\S{4}-\\S{4}-\\S{4}-\\S{12}?\\})", RegexOptions.Compiled);
        private static readonly char[] InternalTokenDelimiters = { ':' };
        private static readonly char[] TokenChars = { '{', '~' };
        private static readonly char[] TokenBoundaryChars = { '{', '}' };
        private static readonly char[] UrlSeparators = { '/' };

        /// <summary>
        /// The token definitions this parser knows about.
        /// </summary>
        public List<TokenDefinition> Tokens
        {
            get { return _tokens; }
            private set { _tokens = value; }
        }

        /// <summary>
        /// The context tokens resolve against.
        /// </summary>
        internal PnPContext Context => _context;

        #region Construction

        private TokenParser()
        {
        }

        /// <summary>
        /// Creates a parser for applying or extracting a single template.
        /// </summary>
        /// <param name="context">The context of the site being provisioned</param>
        /// <param name="template">The template whose tokens should be resolvable</param>
        public static async Task<TokenParser> CreateAsync(PnPContext context, ProvisioningTemplate template)
        {
            return await CreateAsync(context, template, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a parser for applying or extracting a single template.
        /// </summary>
        /// <param name="context">The context of the site being provisioned</param>
        /// <param name="template">The template whose tokens should be resolvable</param>
        /// <param name="applyingInformation">Optional applying information</param>
        public static async Task<TokenParser> CreateAsync(PnPContext context, ProvisioningTemplate template, ProvisioningTemplateApplyingInformation applyingInformation)
        {
            TokenParser parser = new TokenParser();
            await parser.InitializeAsync(context, template, applyingInformation).ConfigureAwait(false);
            return parser;
        }

        /// <summary>
        /// Creates a parser for applying a tenant template (a hierarchy).
        /// </summary>
        /// <param name="context">A context against which hierarchy level tokens resolve</param>
        /// <param name="hierarchy">The hierarchy being applied</param>
        public static async Task<TokenParser> CreateAsync(PnPContext context, ProvisioningHierarchy hierarchy)
        {
            TokenParser parser = new TokenParser();
            await parser.InitializeFromHierarchyAsync(context, hierarchy).ConfigureAwait(false);
            return parser;
        }

        /// <summary>
        /// Creates a parser from an existing token list. Only used by <see cref="Clone"/>.
        /// </summary>
        private TokenParser(PnPContext context, List<TokenDefinition> tokens, int webLanguage, Guid webId)
        {
            _context = context;
            _tokens = tokens;
            _webLanguage = webLanguage;
            _webId = webId;

            CalculateTokenCount(_tokens, out int cacheableCount, out int nonCacheableCount);
            BuildTokenCacheAsync(cacheableCount, nonCacheableCount).GetAwaiter().GetResult();
        }

        private async Task InitializeFromHierarchyAsync(PnPContext context, ProvisioningHierarchy hierarchy)
        {
            IWeb web = await context.Web.GetAsync(w => w.ServerRelativeUrl, w => w.Url, w => w.Language, w => w.Id).ConfigureAwait(false);

            _context = context;
            _webLanguage = web.Language;
            _webId = web.Id;
            _tokens = new List<TokenDefinition>();

            foreach (KeyValuePair<string, string> parameter in hierarchy.Parameters)
            {
                _tokens.Add(new ParameterToken(null, parameter.Key, parameter.Value ?? string.Empty));
            }

            _tokens.Add(new GuidToken(null));
            _tokens.Add(new CurrentUserIdToken(context));
            _tokens.Add(new CurrentUserLoginNameToken(context));
            _tokens.Add(new CurrentUserFullNameToken(context));
            _tokens.Add(new AuthenticationRealmToken(context));
            _tokens.Add(new HostUrlToken(context));
            _tokens.Add(new FqdnToken(context));

            AddResourceTokens(context, hierarchy.Localizations, hierarchy.Connector);

            CalculateTokenCount(_tokens, out int cacheableCount, out int nonCacheableCount);
            await BuildTokenCacheAsync(cacheableCount, nonCacheableCount).ConfigureAwait(false);

            _initializedFromHierarchy = true;
        }

        private async Task InitializeAsync(PnPContext context, ProvisioningTemplate template, ProvisioningTemplateApplyingInformation applyingInformation)
        {
            List<string> tokenIds = ParseTemplate(template);

            IWeb web = await context.Web.GetAsync(w => w.ServerRelativeUrl, w => w.Url, w => w.Language, w => w.Id).ConfigureAwait(false);

            _context = context;
            _webLanguage = web.Language;
            _webId = web.Id;
            _tokens = new List<TokenDefinition>();

            if (tokenIds.Contains("sitecollection"))
                _tokens.Add(new SiteCollectionToken(context));
            if (tokenIds.Contains("sitecollectionid"))
                _tokens.Add(new SiteCollectionIdToken(context));
            if (tokenIds.Contains("sitecollectionidencoded"))
                _tokens.Add(new SiteCollectionIdEncodedToken(context));
            if (tokenIds.Contains("site"))
                _tokens.Add(new SiteToken(context));
            if (tokenIds.Contains("masterpagecatalog"))
                _tokens.Add(new MasterPageCatalogToken(context));
            if (tokenIds.Contains("sitecollectiontermstoreid"))
                _tokens.Add(new SiteCollectionTermStoreIdToken(context));
            if (tokenIds.Contains("keywordstermstoreid"))
                _tokens.Add(new KeywordsTermStoreIdToken(context));
            if (tokenIds.Contains("themecatalog"))
                _tokens.Add(new ThemeCatalogToken(context));
            if (tokenIds.Contains("webname"))
                _tokens.Add(new WebNameToken(context));
            if (tokenIds.Contains("siteid"))
                _tokens.Add(new SiteIdToken(context));
            if (tokenIds.Contains("siteidencoded"))
                _tokens.Add(new SiteIdEncodedToken(context));
            if (tokenIds.Contains("siteowner"))
                _tokens.Add(new SiteOwnerToken(context));
            if (tokenIds.Contains("sitetitle") || tokenIds.Contains("sitename"))
                _tokens.Add(new SiteTitleToken(context));
            if (tokenIds.Contains("groupsitetitle") || tokenIds.Contains("groupsitename"))
                _tokens.Add(new GroupSiteTitleToken(context));
            if (tokenIds.Contains("associatedownergroupid"))
                _tokens.Add(new AssociatedGroupIdToken(context, AssociatedGroupIdToken.AssociatedGroupType.owners));
            if (tokenIds.Contains("associatedmembergroupid"))
                _tokens.Add(new AssociatedGroupIdToken(context, AssociatedGroupIdToken.AssociatedGroupType.members));
            if (tokenIds.Contains("associatedvisitorgroupid"))
                _tokens.Add(new AssociatedGroupIdToken(context, AssociatedGroupIdToken.AssociatedGroupType.visitors));
            if (tokenIds.Contains("associatedownergroup"))
                _tokens.Add(new AssociatedGroupToken(context, AssociatedGroupToken.AssociatedGroupType.owners));
            if (tokenIds.Contains("associatedmembergroup"))
                _tokens.Add(new AssociatedGroupToken(context, AssociatedGroupToken.AssociatedGroupType.members));
            if (tokenIds.Contains("associatedvisitorgroup"))
                _tokens.Add(new AssociatedGroupToken(context, AssociatedGroupToken.AssociatedGroupType.visitors));
            if (tokenIds.Contains("guid"))
                _tokens.Add(new GuidToken(context));
            if (tokenIds.Contains("now"))
                _tokens.Add(new DateNowToken(context));
            if (tokenIds.Contains("currentuserid"))
                _tokens.Add(new CurrentUserIdToken(context));
            if (tokenIds.Contains("currentuserloginname"))
                _tokens.Add(new CurrentUserLoginNameToken(context));
            if (tokenIds.Contains("currentuserfullname"))
                _tokens.Add(new CurrentUserFullNameToken(context));
            if (tokenIds.Contains("authenticationrealm"))
                _tokens.Add(new AuthenticationRealmToken(context));
            if (tokenIds.Contains("hosturl"))
                _tokens.Add(new HostUrlToken(context));
            if (tokenIds.Contains("fqdn"))
                _tokens.Add(new FqdnToken(context));
            if (tokenIds.Contains("sitecollectionconnectedoffice365groupid"))
                _tokens.Add(new SiteCollectionConnectedOffice365GroupId(context));
            if (tokenIds.Contains("everyone"))
                _tokens.Add(new EveryoneToken(context));
            if (tokenIds.Contains("everyonebutexternalusers"))
                _tokens.Add(new EveryoneButExternalUsersToken(context));

            if (tokenIds.Contains("listid") || tokenIds.Contains("listurl") || tokenIds.Contains("viewid"))
                await RebuildListTokensAsync(context, AddTokenToList).ConfigureAwait(false);
            if (tokenIds.Contains("contenttypeid"))
                await AddContentTypeTokensAsync(context).ConfigureAwait(false);

            if (!_initializedFromHierarchy && tokenIds.Contains("parameter"))
            {
                foreach (KeyValuePair<string, string> parameter in template.Parameters)
                {
                    _tokens.Add(new ParameterToken(context, parameter.Key, parameter.Value ?? string.Empty));
                }
            }

            if (tokenIds.Contains("sitedesignid"))
                AddSiteDesignTokens(context);
            if (tokenIds.Contains("sitescriptid"))
                AddSiteScriptTokens(context);
            if (tokenIds.Contains("storageentityvalue"))
                await AddStorageEntityTokensAsync(context).ConfigureAwait(false);

            if (tokenIds.Contains("fieldtitle") || tokenIds.Contains("fieldid"))
                await AddFieldTokensAsync(context).ConfigureAwait(false);

            if (tokenIds.Contains("loc") || tokenIds.Contains("localize") || tokenIds.Contains("localization") || tokenIds.Contains("resource") || tokenIds.Contains("res"))
                AddResourceTokens(context, template.Localizations, template.Connector);

            if (tokenIds.Contains("roledefinition") || tokenIds.Contains("roledefinitionid"))
                await AddRoleDefinitionTokensAsync(context).ConfigureAwait(false);

            if (tokenIds.Contains("groupid"))
                await AddGroupTokensAsync(context).ConfigureAwait(false);

            if (tokenIds.Contains("apppackageid"))
                AddAppPackagesTokens(context);
            if (tokenIds.Contains("pageuniqueid"))
                await AddPageUniqueIdTokensAsync(context).ConfigureAwait(false);
            if (tokenIds.Contains("propertybagvalue"))
                await AddPropertyBagTokensAsync(context).ConfigureAwait(false);

            await AddTermStoreTokensAsync(context, tokenIds).ConfigureAwait(false);

            CalculateTokenCount(_tokens, out int cacheableCount, out int nonCacheableCount);
            await BuildTokenCacheAsync(cacheableCount, nonCacheableCount).ConfigureAwait(false);
        }

        #endregion

        #region Rebasing

        /// <summary>
        /// Discards every resolved token value, forcing the next resolution to go back to the site.
        /// </summary>
        public async Task RebaseAsync()
        {
            foreach (TokenDefinition token in _tokens)
            {
                token.ClearCache();
            }

            CalculateTokenCount(_tokens, out int cacheableCount, out int nonCacheableCount);
            await BuildTokenCacheAsync(cacheableCount, nonCacheableCount).ConfigureAwait(false);
        }

        /// <summary>
        /// Repoints the parser at a different web and rebuilds the tokens the new template needs.
        /// </summary>
        public async Task RebaseAsync(PnPContext context, ProvisioningTemplate template, ProvisioningTemplateApplyingInformation applyingInformation = null)
        {
            List<string> tokenIds = ParseTemplate(template);

            IWeb web = await context.Web.GetAsync(w => w.ServerRelativeUrl, w => w.Language, w => w.Id).ConfigureAwait(false);

            _context = context;
            _webLanguage = web.Language;
            _webId = web.Id;

            foreach (VolatileTokenDefinition token in _tokens.OfType<VolatileTokenDefinition>())
            {
                token.ClearVolatileCache(context);
            }

            _tokens.RemoveAll(t => t is SiteToken);
            _tokens.Add(new SiteToken(context));

            if (tokenIds.Contains("listid")
                || tokenIds.Contains("listurl")
                || tokenIds.Contains("viewid")
                || tokenIds.Contains("listcontenttypeid"))
            {
                await RebuildListTokensAsync(context, AddTokenToList).ConfigureAwait(false);
            }

            if (tokenIds.Contains("contenttypeid"))
            {
                await AddContentTypeTokensAsync(context).ConfigureAwait(false);
            }

            if (tokenIds.Contains("fieldid"))
            {
                _tokens.RemoveAll(t => t is FieldTitleToken || t is FieldIdToken);
                await AddFieldTokensAsync(context).ConfigureAwait(false);
            }

            if (tokenIds.Contains("groupid")
                || tokenIds.FindIndex(t => t.StartsWith("associated", StringComparison.OrdinalIgnoreCase)) > -1)
            {
                _tokens.RemoveAll(t => t is GroupIdToken || t is AssociatedGroupToken);
                await AddGroupTokensAsync(context).ConfigureAwait(false);
            }

            if (tokenIds.Contains("roledefinition"))
            {
                _tokens.RemoveAll(t => t is RoleDefinitionToken || t is RoleDefinitionIdToken);
                await AddRoleDefinitionTokensAsync(context).ConfigureAwait(false);
            }

            CalculateTokenCount(_tokens, out int cacheableCount, out int nonCacheableCount);
            await BuildTokenCacheAsync(cacheableCount, nonCacheableCount).ConfigureAwait(false);
        }

        #endregion

        #region Token registration

        /// <summary>
        /// Registers a token definition and resolves its value into the cache.
        /// </summary>
        /// <param name="tokenDefinition">The definition to add</param>
        public void AddToken(TokenDefinition tokenDefinition)
        {
            AddTokenAsync(tokenDefinition).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Registers a token definition and resolves its value into the cache.
        /// </summary>
        /// <param name="tokenDefinition">The definition to add</param>
        public async Task AddTokenAsync(TokenDefinition tokenDefinition)
        {
            _tokens.Add(tokenDefinition);
            await AddToTokenCacheAsync(tokenDefinition).ConfigureAwait(false);
        }

        private void AddTokenToList(TokenDefinition tokenDefinition)
        {
            _tokens.Add(tokenDefinition);
        }

        internal void RemoveToken<T>(T oldToken) where T : TokenDefinition
        {
            for (int i = 0; i < _tokens.Count; i++)
            {
                TokenDefinition tokenDefinition = _tokens[i];
                if (!tokenDefinition.GetTokens().SequenceEqual(oldToken.GetTokens()))
                {
                    continue;
                }

                _tokens.RemoveAt(i);

                foreach (string token in tokenDefinition.GetUnescapedTokens())
                {
                    _tokenDictionary.Remove(token);
                    _nonCacheableTokenDictionary.Remove(token);
                }

                break;
            }
        }

        #endregion

        #region Site enumeration - populating the token set

        private void AddResourceTokens(PnPContext context, LocalizationCollection localizations, FileConnectorBase connector)
        {
            if (localizations == null || localizations.Count == 0)
            {
                return;
            }

            if (connector == null)
            {
                throw new ArgumentNullException(nameof(connector), "Template or Hierarchy File Connector cannot be null");
            }

            var resourceEntries = new Dictionary<string, List<ResourceEntry>>(StringComparer.InvariantCulture);

            foreach (Localization localizationEntry in localizations)
            {
                string filePath = localizationEntry.ResourceFile;
                int lcid = localizationEntry.LCID;

                if (filePath.EndsWith(".resx", StringComparison.OrdinalIgnoreCase))
                {
                    using (System.IO.Stream stream = connector.GetFileStream(filePath))
                    {
                        if (stream == null)
                        {
                            continue;
                        }

                        XElement xElement = XElement.Load(stream);
                        foreach (XElement dataElement in xElement.Descendants("data"))
                        {
                            string key = dataElement.Attribute("name").Value;
                            string value = dataElement.Descendants().First().Value;
                            string escapedValue = value.Replace("\"", "&quot;");

                            AddResourceEntry($"{localizationEntry.Name}:{key}", lcid, escapedValue, resourceEntries);
                            AddResourceEntry(key, lcid, escapedValue, resourceEntries);
                        }
                    }
                }
                else if (filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    string jsonString = connector.GetFile(filePath);
                    if (string.IsNullOrEmpty(jsonString))
                    {
                        continue;
                    }

                    Dictionary<string, string> dict = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                    foreach (KeyValuePair<string, string> entry in dict)
                    {
                        string escapedValue = entry.Value.Replace("\"", "&quot;");

                        AddResourceEntry($"{localizationEntry.Name}:{entry.Key}", lcid, escapedValue, resourceEntries);
                        AddResourceEntry(entry.Key, lcid, escapedValue, resourceEntries);
                    }
                }
            }

            _tokens.Capacity = _tokens.Count + resourceEntries.Count + 1;

            foreach (KeyValuePair<string, List<ResourceEntry>> pair in resourceEntries)
            {
                _tokens.Add(new LocalizationToken(context, _webLanguage, pair.Key, pair.Value, localizations.DefaultLCID));
            }
        }

        private async Task AddFieldTokensAsync(PnPContext context)
        {
            _tokens.RemoveAll(t => t is FieldTitleToken || t is FieldIdToken);

            await context.Web.LoadAsync(w => w.AvailableFields.QueryProperties(
                f => f.Title, f => f.InternalName, f => f.Id)).ConfigureAwait(false);

            foreach (IField field in context.Web.AvailableFields.AsRequested())
            {
                _tokens.Add(new FieldTitleToken(context, field.InternalName, field.Title));
                _tokens.Add(new FieldIdToken(context, field.InternalName, field.Id));
            }
        }

        private async Task AddRoleDefinitionTokensAsync(PnPContext context)
        {
            await context.Web.LoadAsync(w => w.RoleDefinitions.QueryProperties(
                r => r.RoleTypeKind, r => r.Name, r => r.Id)).ConfigureAwait(false);

            foreach (IRoleDefinition roleDef in context.Web.RoleDefinitions.AsRequested().Where(r => r.RoleTypeKind != RoleType.None))
            {
                _tokens.Add(new RoleDefinitionToken(context, roleDef));
            }

            foreach (IRoleDefinition roleDef in context.Web.RoleDefinitions.AsRequested())
            {
                _tokens.Add(new RoleDefinitionIdToken(context, roleDef.Name, roleDef.Id));
            }
        }

        private async Task AddPropertyBagTokensAsync(PnPContext context)
        {
            await context.Web.LoadAsync(w => w.AllProperties).ConfigureAwait(false);

            foreach (KeyValuePair<string, object> keyValue in context.Web.AllProperties.Values)
            {
                _tokens.Add(new PropertyBagValueToken(context, keyValue.Key, keyValue.Value?.ToString() ?? string.Empty));
            }
        }

        private async Task AddGroupTokensAsync(PnPContext context)
        {
            await context.Web.LoadAsync(w => w.SiteGroups.QueryProperties(g => g.Title, g => g.Id)).ConfigureAwait(false);

            foreach (ISharePointGroup siteGroup in context.Web.SiteGroups.AsRequested())
            {
                _tokens.Add(new GroupIdToken(context, siteGroup.Title, siteGroup.Id.ToString()));
            }

            IWeb web = await context.Web.GetAsync(
                w => w.AssociatedVisitorGroup,
                w => w.AssociatedMemberGroup,
                w => w.AssociatedOwnerGroup).ConfigureAwait(false);

            if (web.AssociatedVisitorGroup != null && web.AssociatedVisitorGroup.Id != 0)
            {
                _tokens.Add(new GroupIdToken(context, "associatedvisitorgroup", web.AssociatedVisitorGroup.Id.ToString()));
            }

            if (web.AssociatedMemberGroup != null && web.AssociatedMemberGroup.Id != 0)
            {
                _tokens.Add(new GroupIdToken(context, "associatedmembergroup", web.AssociatedMemberGroup.Id.ToString()));
            }

            if (web.AssociatedOwnerGroup != null && web.AssociatedOwnerGroup.Id != 0)
            {
                _tokens.Add(new GroupIdToken(context, "associatedownergroup", web.AssociatedOwnerGroup.Id.ToString()));
            }

        }

        private async Task AddTermStoreTokensAsync(PnPContext context, List<string> tokenIds)
        {
            if (!tokenIds.Contains("termstoreid")
                && !tokenIds.Contains("termsetid")
                && !tokenIds.Contains("sitecollectiontermgroupid")
                && !tokenIds.Contains("sitecollectiontermgroupname")
                && !tokenIds.Contains("sitecollectiontermsetid"))
            {
                return;
            }

            try
            {
                if (tokenIds.Contains("termstoreid"))
                {
                    ITermStore store = await context.TermStore.GetAsync(t => t.Id).ConfigureAwait(false);
                    _tokens.Add(new TermStoreIdToken(context, "Taxonomy_", store.Id));
                }

                if (tokenIds.Contains("termsetid"))
                {
                    await context.TermStore.LoadAsync(t => t.Groups.QueryProperties(
                        g => g.Name,
                        g => g.Sets.QueryProperties(s => s.Id, s => s.LocalizedNames))).ConfigureAwait(false);

                    foreach (ITermGroup termGroup in context.TermStore.Groups.AsRequested())
                    {
                        foreach (ITermSet termSet in termGroup.Sets.AsRequested())
                        {
                            string name = termSet.LocalizedNames?.FirstOrDefault()?.Name;
                            if (!string.IsNullOrEmpty(name))
                            {
                                _tokens.Add(new TermSetIdToken(context, termGroup.Name, name, termSet.Id));
                            }
                        }
                    }
                }

                if (tokenIds.Contains("sitecollectiontermgroupid"))
                {
                    _tokens.Add(new SiteCollectionTermGroupIdToken(context));
                }

                if (tokenIds.Contains("sitecollectiontermgroupname"))
                {
                    _tokens.Add(new SiteCollectionTermGroupNameToken(context));
                }

                if (!tokenIds.Contains("sitecollectiontermsetid"))
                {
                    return;
                }

                ITermGroup siteCollectionGroup = await SiteCollectionTermGroupResolver.GetAsync(context).ConfigureAwait(false);
                if (siteCollectionGroup != null)
                {
                    await siteCollectionGroup.LoadAsync(g => g.Sets.QueryProperties(
                        s => s.Id, s => s.LocalizedNames)).ConfigureAwait(false);

                    foreach (ITermSet termSet in siteCollectionGroup.Sets.AsRequested())
                    {
                        string name = termSet.LocalizedNames?.FirstOrDefault()?.Name;
                        if (!string.IsNullOrEmpty(name))
                        {
                            _tokens.Add(new SiteCollectionTermSetIdToken(context, name, termSet.Id));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, PnPCoreProvisioningResources.TermGroup_No_Access);
            }
        }

        private void AddAppPackagesTokens(PnPContext context)
        {
            _tokens.RemoveAll(t => t.GetType() == typeof(AppPackageIdToken));

            _ = context;
        }

        private async Task AddStorageEntityTokensAsync(PnPContext context)
        {
            try
            {
                IWeb rootWeb = await context.Site.RootWeb.GetAsync(w => w.AllProperties).ConfigureAwait(false);
                string storageEntitiesIndex = rootWeb.AllProperties.GetString("storageentitiesindex", string.Empty);

                foreach (StorageEntity entity in ParseStorageEntitiesString(storageEntitiesIndex))
                {
                    _tokens.Add(new StorageEntityValueToken(context, entity.Key, entity.Value));
                }
            }
            catch (Exception)
            {
            }
        }

        private static List<StorageEntity> ParseStorageEntitiesString(string storageEntitiesIndex)
        {
            if (string.IsNullOrWhiteSpace(storageEntitiesIndex))
            {
                return new List<StorageEntity>();
            }

            Dictionary<string, Dictionary<string, string>> storageEntitiesDict =
                JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(storageEntitiesIndex);

            if (storageEntitiesDict == null)
            {
                return new List<StorageEntity>();
            }

            var storageEntities = new List<StorageEntity>(storageEntitiesDict.Count + 1);

            foreach (KeyValuePair<string, Dictionary<string, string>> pair in storageEntitiesDict)
            {
                storageEntities.Add(new StorageEntity
                {
                    Key = pair.Key,
                    Value = pair.Value.TryGetValue("Value", out string value) ? value : null,
                    Comment = pair.Value.TryGetValue("Comment", out string comment) ? comment : null,
                    Description = pair.Value.TryGetValue("Description", out string description) ? description : null
                });
            }

            return storageEntities;
        }

        private void AddSiteDesignTokens(PnPContext context)
        {
            _ = context;
        }

        private void AddSiteScriptTokens(PnPContext context)
        {
            _ = context;
        }

        private async Task AddPageUniqueIdTokensAsync(PnPContext context)
        {
            try
            {
                IList pagesList = await context.Web.Lists.GetByServerRelativeUrlAsync(
                    $"{context.Web.ServerRelativeUrl.TrimEnd(UrlSeparators)}/SitePages").ConfigureAwait(false);

                await pagesList.LoadListDataAsStreamAsync(new RenderListDataOptions
                {
                    ViewXml = "<View><ViewFields><FieldRef Name='UniqueId'/><FieldRef Name='FileLeafRef' /></ViewFields><RowLimit Paged='TRUE'>100</RowLimit></View>",
                    RenderOptions = RenderListDataOptionsFlags.ListData
                }).ConfigureAwait(false);

                foreach (IListItem item in pagesList.Items.AsRequested())
                {
                    if (item["UniqueId"] == null || item["FileLeafRef"] == null)
                    {
                        continue;
                    }

                    if (Guid.TryParse(item["UniqueId"].ToString(), out Guid uniqueId))
                    {
                        _tokens.Add(new PageUniqueIdToken(context, $"SitePages/{item["FileLeafRef"]}", uniqueId));
                        _tokens.Add(new PageUniqueIdEncodedToken(context, $"SitePages/{item["FileLeafRef"]}", uniqueId));
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private async Task AddContentTypeTokensAsync(PnPContext context)
        {
            _tokens.RemoveAll(t => t.GetType() == typeof(ContentTypeIdToken));

            await context.Web.LoadAsync(w => w.AvailableContentTypes.QueryProperties(
                ct => ct.StringId, ct => ct.Name)).ConfigureAwait(false);

            foreach (IContentType ct in context.Web.AvailableContentTypes.AsRequested())
            {
                _tokens.Add(new ContentTypeIdToken(context, ct.Name, ct.StringId));
            }
        }

        internal async Task RebuildListTokensAsync(PnPContext context)
        {
            await RebuildListTokensAsync(context, t => AddTokenAsync(t).GetAwaiter().GetResult()).ConfigureAwait(false);
        }

        private async Task RebuildListTokensAsync(PnPContext context, Action<TokenDefinition> addToken)
        {
            IWeb web = await context.Web.GetAsync(w => w.ServerRelativeUrl, w => w.Language).ConfigureAwait(false);

            Predicate<TokenDefinition> listTokenTypes = t => t.GetType() == typeof(ListIdToken)
                                                             || t.GetType() == typeof(ListUrlToken)
                                                             || t.GetType() == typeof(ListViewIdToken)
                                                             || t.GetType() == typeof(ListContentTypeIdToken);

            foreach (TokenDefinition listToken in _tokens.FindAll(listTokenTypes))
            {
                foreach (string token in listToken.GetUnescapedTokens())
                {
                    _tokenDictionary.Remove(token);
                    if (listToken is ListIdToken)
                    {
                        _listTokenDictionary.Remove(token);
                    }
                }
            }

            _tokens.RemoveAll(listTokenTypes);

            await context.Web.LoadAsync(w => w.Lists.QueryProperties(
                l => l.Id,
                l => l.Title,
                l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl),
                l => l.Views.QueryProperties(v => v.Id, v => v.Title),
                l => l.ContentTypes.QueryProperties(ct => ct.Id, ct => ct.StringId, ct => ct.Name))).ConfigureAwait(false);

            string webUrlPrefix = web.ServerRelativeUrl.TrimEnd(UrlSeparators);

            foreach (IList list in context.Web.Lists.AsRequested())
            {
                addToken(new ListIdToken(context, list.Title, list.Id));

                string mainLanguageName = await GetListTitleForMainLanguageAsync(context, list.Title).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(mainLanguageName) && mainLanguageName != list.Title)
                {
                    addToken(new ListIdToken(context, mainLanguageName, list.Id));
                }

                addToken(new ListUrlToken(context, list.Title, list.RootFolder.ServerRelativeUrl.Substring(webUrlPrefix.Length + 1)));

                foreach (IView view in list.Views.AsRequested())
                {
                    addToken(new ListViewIdToken(context, list.Title, view.Title, view.Id));
                }

                foreach (IContentType contentType in list.ContentTypes.AsRequested())
                {
                    addToken(new ListContentTypeIdToken(context, list.Title, contentType));
                }
            }

        }

        #endregion

        #region Parsing

        /// <summary>
        /// Replaces every token in the input with its resolved value.
        /// </summary>
        /// <param name="input">The string to parse</param>
        /// <returns>The parsed string</returns>
        public string ParseString(string input)
        {
            return ParseString(input, null);
        }

        /// <summary>
        /// Replaces every token in the input with its resolved value.
        /// </summary>
        /// <param name="input">The string to parse</param>
        /// <param name="tokensToSkip">Tokens that should be left in place</param>
        /// <returns>The parsed string</returns>
        public string ParseString(string input, params string[] tokensToSkip)
        {
            if (string.IsNullOrWhiteSpace(input) || input.IndexOfAny(TokenChars) == -1)
            {
                return input;
            }

            if (_tokenDictionary.TryGetValue(input, out string directMatch))
            {
                return directMatch;
            }

            if (TryGetNonCacheableToken(input, out string directMatchNonCached))
            {
                return directMatchNonCached;
            }

            string output = input;
            bool hasMatch;

            do
            {
                hasMatch = false;
                output = ReToken.Replace(output, match =>
                {
                    string tokenString = match.Groups[0].Value;

                    if (!_tokenDictionary.TryGetValue(tokenString, out string val))
                    {
                        return tokenString;
                    }

                    hasMatch = true;
                    return val;
                });
            } while (hasMatch && input != output);

            if (hasMatch)
            {
                return output;
            }

            MatchCollection fallbackMatches = ReTokenFallback.Matches(output);
            if (fallbackMatches.Count == 0)
            {
                return output;
            }

            bool needFallback = false;
            foreach (Match match in fallbackMatches)
            {
                if (!ReGuid.IsMatch(match.Value))
                {
                    needFallback = true;
                }
            }

            if (!needFallback)
            {
                return output;
            }

            foreach (KeyValuePair<string, string> pair in _tokenDictionary)
            {
                int idx = output.IndexOf(pair.Key, StringComparison.CurrentCultureIgnoreCase);
                if (idx != -1)
                {
                    output = output.Remove(idx, pair.Key.Length).Insert(idx, pair.Value);
                }

                if (!ReTokenFallback.IsMatch(output))
                {
                    break;
                }
            }

            return output;
        }

        /// <summary>
        /// Parses a string on behalf of a web part, resolving list tokens only when they belong to
        /// the supplied web.
        /// </summary>
        /// <param name="input">The string to parse</param>
        /// <param name="web">The web whose list tokens may be resolved</param>
        /// <param name="tokensToSkip">Tokens that should be left in place</param>
        /// <returns>The parsed string</returns>
        public string ParseStringWebPart(string input, IWeb web, params string[] tokensToSkip)
        {
            if (string.IsNullOrEmpty(input) || input.IndexOfAny(TokenChars) == -1)
            {
                return input;
            }

            if (_tokenDictionary.TryGetValue(input, out string directMatch))
            {
                return directMatch;
            }

            if (TryGetNonCacheableToken(input, out string directMatchNonCached))
            {
                return directMatchNonCached;
            }

            string output = input;
            bool hasMatch;

            do
            {
                hasMatch = false;
                output = ReToken.Replace(output, match =>
                {
                    string tokenString = match.Groups[0].Value;

                    if (!_tokenDictionary.TryGetValue(tokenString, out string val))
                    {
                        return tokenString;
                    }

                    if (tokenString.IndexOf("listid", StringComparison.OrdinalIgnoreCase) != -1
                        && _listTokenDictionary.TryGetValue(tokenString, out TokenDefinition token)
                        && token.Context != null
                        && !token.Context.Web.Id.Equals(web.Id))
                    {
                        return tokenString;
                    }

                    hasMatch = true;
                    return val;
                });
            } while (hasMatch && input != output);

            return output;
        }

        /// <summary>
        /// Replaces every token in the attribute values and element text of an XML document.
        /// </summary>
        /// <param name="inputXml">The XML to parse</param>
        /// <param name="tokensToSkip">Tokens that should be left in place</param>
        /// <returns>The parsed XML</returns>
        public string ParseXmlString(string inputXml, params string[] tokensToSkip)
        {
            return ParseXmlDocument(inputXml, value => ParseString(value, tokensToSkip));
        }

        /// <summary>
        /// Replaces every token in an XML document on behalf of a web part.
        /// </summary>
        /// <param name="inputXml">The XML to parse</param>
        /// <param name="web">The web whose list tokens may be resolved</param>
        /// <param name="tokensToSkip">Tokens that should be left in place</param>
        /// <returns>The parsed XML</returns>
        public string ParseXmlStringWebpart(string inputXml, IWeb web, params string[] tokensToSkip)
        {
            return ParseXmlDocument(inputXml, value => ParseStringWebPart(value, web, tokensToSkip));
        }

        private static string ParseXmlDocument(string inputXml, Func<string, string> parse)
        {
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(inputXml);

            System.Xml.XmlNodeList nodes = xmlDoc.SelectNodes("//*");
            if (nodes != null)
            {
                foreach (System.Xml.XmlElement node in nodes.OfType<System.Xml.XmlElement>().Where(n => n.HasAttributes))
                {
                    foreach (System.Xml.XmlAttribute attribute in node.Attributes.OfType<System.Xml.XmlAttribute>()
                        .Where(a => !a.Name.Equals("xmlns", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(a.Value)))
                    {
                        attribute.Value = parse(attribute.Value);
                    }
                }
            }

            nodes = xmlDoc.SelectNodes("//*[text()]");
            if (nodes != null)
            {
                foreach (System.Xml.XmlElement node in nodes.OfType<System.Xml.XmlElement>())
                {
                    if (!string.IsNullOrEmpty(node.InnerText))
                    {
                        node.InnerText = parse(node.InnerText);
                    }
                }
            }

            return xmlDoc.OuterXml;
        }

        /// <summary>
        /// Returns the tokens in the input that look like tokens but are not valid GUIDs - i.e.
        /// the ones that were never replaced.
        /// </summary>
        /// <param name="input">The string to inspect</param>
        public IEnumerable<string> GetLeftOverTokens(string input)
        {
            var values = new List<string>();

            foreach (Match match in ReGuid.Matches(input))
            {
                string value = match.Value;
                if (!Guid.TryParse(value, out _))
                {
                    values.Add(value);
                }
            }

            return values;
        }

        /// <summary>
        /// Returns every culture/value pair a localization token resolves to.
        /// </summary>
        /// <param name="tokenValue">The token, with or without its braces</param>
        public List<Tuple<string, string>> GetResourceTokenResourceValues(string tokenValue)
        {
            var resourceValues = new List<Tuple<string, string>>();
            tokenValue = $"{{{Regex.Escape(tokenValue.Trim(TokenBoundaryChars))}}}"; // since LocalizationToken are Regex.Escaped before load

            foreach (LocalizationToken token in _tokens.OfType<LocalizationToken>())
            {
                if (Array.IndexOf(token.GetTokens(), tokenValue) == -1)
                {
                    continue;
                }

                foreach (ResourceEntry entry in token.ResourceEntries)
                {
                    var ci = new CultureInfo(entry.LCID);
                    resourceValues.Add(new Tuple<string, string>(ci.Name, ParseString(entry.Value)));
                }
            }

            return resourceValues;
        }

        #endregion

        #region Token cache

        private async Task BuildTokenCacheAsync(int cacheableCount, int nonCacheableCount)
        {
            _tokenDictionary = new Dictionary<string, string>(capacity: cacheableCount > 0 ? cacheableCount + 1 : 0, StringComparer.OrdinalIgnoreCase);
            _nonCacheableTokenDictionary = new Dictionary<string, TokenDefinition>(capacity: nonCacheableCount > 0 ? nonCacheableCount + 1 : 0, StringComparer.OrdinalIgnoreCase);
            _listTokenDictionary = new Dictionary<string, TokenDefinition>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < _tokens.Count; index++)
            {
                await AddToTokenCacheAsync(_tokens[index]).ConfigureAwait(false);
            }
        }

        private async Task AddToTokenCacheAsync(TokenDefinition definition)
        {
            IReadOnlyList<string> tokens = definition.GetUnescapedTokens();
            for (var index = 0; index < tokens.Count; index++)
            {
                string token = tokens[index];

                if (!definition.IsCacheable)
                {
                    _nonCacheableTokenDictionary[token] = definition;
                    continue;
                }

                _tokenDictionary[token] = await definition.GetReplaceValueAsync().ConfigureAwait(false);

                if (definition is ListIdToken)
                {
                    _listTokenDictionary[token] = definition;
                }
            }
        }

        private bool TryGetNonCacheableToken(string input, out string value)
        {
            if (_nonCacheableTokenDictionary.TryGetValue(input, out TokenDefinition definition))
            {
                value = definition.GetReplaceValue();
                return true;
            }

            value = null;
            return false;
        }

        private static void CalculateTokenCount(IReadOnlyList<TokenDefinition> tokens, out int cacheableCount, out int nonCacheableCount)
        {
            cacheableCount = 0;
            nonCacheableCount = 0;

            for (var index = 0; index < tokens.Count; index++)
            {
                TokenDefinition definition = tokens[index];

                if (definition.IsCacheable)
                {
                    cacheableCount += definition.TokenCount;
                }
                else
                {
                    nonCacheableCount += definition.TokenCount;
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns the title of a list in the site's main language, so that a template written
        /// against the default language still resolves on a site the current user sees translated.
        /// </summary>
        private async Task<string> GetListTitleForMainLanguageAsync(PnPContext context, string name)
        {
            if (_listsTitles.TryGetValue(name, out string title))
            {
                return title;
            }

            _ = context;
            _ = _webLanguage;
            return null;
        }

        private static List<string> ParseTemplate(ProvisioningTemplate template)
        {
            var tokenIds = new List<string>();

            if (template.Parameters != null && template.Parameters.Count > 0)
            {
                tokenIds.Add("parameter");
            }

            string xml = template.ToXML();

            if (xml.IndexOfAny(TokenChars) == -1) return tokenIds;

            bool hasMatch;
            string tempXml;

            do
            {
                hasMatch = false;
                tempXml = ReToken.Replace(xml, match =>
                {
                    for (int i = 0; i < match.Groups.Count; i++)
                    {
                        if (ReGuid.IsMatch(match.Groups[i].Value))
                        {
                            continue;
                        }

                        string originalTokenString = match.Groups[i].Value.Replace("{", "").Replace("}", "").ToLowerInvariant();

                        string tokenStringToAdd = originalTokenString;
                        int colonIndex = tokenStringToAdd.IndexOf(":", StringComparison.Ordinal);
                        if (colonIndex > -1)
                        {
                            tokenStringToAdd = tokenStringToAdd.Substring(0, colonIndex);
                        }
                        if (!tokenIds.Contains(tokenStringToAdd) && !string.IsNullOrEmpty(tokenStringToAdd))
                        {
                            tokenIds.Add(tokenStringToAdd);
                        }

                        if (string.Equals(tokenStringToAdd, "sequencesitetoken", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] sequenceSiteTokenArray = originalTokenString.Split(InternalTokenDelimiters);
                            if (sequenceSiteTokenArray.Length > 2 && !string.IsNullOrWhiteSpace(sequenceSiteTokenArray[2]) && !tokenIds.Contains(sequenceSiteTokenArray[2]))
                            {
                                tokenIds.Add(sequenceSiteTokenArray[2]);
                            }
                        }
                    }

                    return "-";
                });
            } while (hasMatch && xml != tempXml);

            return tokenIds;
        }

        private static void AddResourceEntry(string key, int lcid, string value, Dictionary<string, List<ResourceEntry>> dictionary)
        {
            if (!dictionary.TryGetValue(key, out List<ResourceEntry> entries))
            {
                entries = new List<ResourceEntry>();
                dictionary.Add(key, entries);
            }

            entries.Add(new ResourceEntry { LCID = lcid, Value = value });
        }

        /// <summary>
        /// Creates a copy of this parser sharing the same token definitions.
        /// </summary>
        /// <returns>A new <see cref="TokenParser"/></returns>
        public object Clone()
        {
            return new TokenParser(_context, _tokens, _webLanguage, _webId);
        }

        #endregion
    }
}
