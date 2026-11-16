using Microsoft.Extensions.Caching.Distributed;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Extensions;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using PnP.Core.Modernization.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using PnPCore = PnP.Core.Model.SharePoint;

namespace PnP.Core.Modernization.Cache
{
    /// <summary>
    /// Caching manager, singleton
    /// Important: don't cache SharePoint Client objects as these are tied to a specific client context and hence will fail when there's context switching!
    /// </summary>
    public sealed class CacheManager
    {
        private static readonly Lazy<CacheManager> _lazyInstance = new Lazy<CacheManager>(() => new CacheManager());
        // baseTemplate will be loaded from local assembly and will be the same for any "caller"
        private Provisioning.Model.ProvisioningTemplate baseTemplate;
        // Needs to be refactored together with the logging system
        private BasePageTransformator lastUsedTransformator;

        private const string Publishing = "publishing";
        private const string Blog = "blog";

        // Cache keys
        private const string keyAadTenantId = "aadTenantId";
        private const string keySharePointVersions = "sharepointVersions";
        private const string keyExactSharepointVersions = "exactSharepointVersions";
        private const string keyAssetsTransferred = "assetsTransferred";
        private const string keyClientSideComponents = "clientSideComponents";
        private const string keySiteToComponentMapping = "siteToComponentMapping";
        private const string keyFieldsToCopy = "fieldsToCopy";
        private const string keyPublishingPagesLibraryNames = "publishingPagesLibraryNames";
        private const string keyBlogListNames = "blogListNames";
        private const string keyWebType = "webType";
        private const string keyResourceStrings = "resourceStrings";
        private const string keyGeneratedPageLayoutMappings = "generatedPageLayoutMappings";
        private const string keyUserJsonStrings = "userJsonStrings";
        private const string keyUserJsonStringsViaUpn = "userJsonStringsViaUpn";
        private const string keyEnsuredUsers = "ensuredUsers";
        private const string keyContentTypes = "contentTypes";
        private const string keyPublishingContentTypeFields = "publishingContentTypeFields";
        private const string keyUrlMapping = "urlMapping";
        private const string keyUserMappings = "userMappings";
        private const string keyMappedUsers = "mappedUsers";
        private const string keyTermCache = "termCache";
        private const string keyTermMappings = "termMappings";

        private const string keyTermTransformatorCache = "keyTermTransformatorCache";

        /// <summary>
        /// Gets the single cachemanager instance, singleton pattern
        /// </summary>
        public static CacheManager Instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }

        #region Construction
        private CacheManager()
        {
            // setup default cache store
            var defaultCacheOptions = new CacheOptions();
            this.Store = new MemoryDistributedCache(defaultCacheOptions);
            this.StoreOptions = defaultCacheOptions;

            // place for instance initialization code
            baseTemplate = null;
        }
        #endregion

        #region Cache implementation setup
        public IDistributedCache Store { get; set; }
        public ICacheOptions StoreOptions { get; set; }
        #endregion

        #region SharePoint Versions and AAD
        /// <summary>
        /// Gets the cached SharePoint version for a given site
        /// </summary>
        /// <param name="site">Site to get the SharePoint version for</param>
        /// <returns>Found SharePoint version or "Unknown" if not found in cache</returns>
        public SPVersion GetSharePointVersion(Uri site)
        {
            var sharepointVersions = Store.GetAndInitialize<Dictionary<string, SPVersion>>(StoreOptions.GetKey(keySharePointVersions));
            if (sharepointVersions.ContainsKey(site.ToString()))
            {
                return sharepointVersions[site.ToString()];
            }

            return SPVersion.Unknown;
        }

        /// <summary>
        /// Sets the SharePoint version in cache
        /// </summary>
        /// <param name="site">Site to the set the SharePoint version for</param>
        /// <param name="version">SharePoint version of the site</param>
        public void SetSharePointVersion(Uri site, SPVersion version)
        {
            var sharepointVersions = Store.GetAndInitialize<Dictionary<string, SPVersion>>(StoreOptions.GetKey(keySharePointVersions));
            if (!sharepointVersions.ContainsKey(site.ToString()))
            {
                sharepointVersions.Add(site.ToString(), version);
                Store.Set<Dictionary<string, SPVersion>>(StoreOptions.GetKey(keySharePointVersions), sharepointVersions, StoreOptions.EntryOptions);
            }
        }

        /// <summary>
        /// Gets the exact SharePoint version from cache
        /// </summary>
        /// <param name="site">Site to get the exact version for</param>
        /// <returns>Exact version from cache</returns>
        public string GetExactSharePointVersion(Uri site)
        {
            var exactSharepointVersions = Store.GetAndInitialize<Dictionary<string, string>>(StoreOptions.GetKey(keyExactSharepointVersions));
            if (exactSharepointVersions.ContainsKey(site.ToString()))
            {
                return exactSharepointVersions[site.ToString()];
            }

            return null;
        }

        /// <summary>
        /// Adds exact SharePoint version for a given site to cache
        /// </summary>
        /// <param name="site">Site to add the SharePoint version for to cache</param>
        /// <param name="version">Version to add</param>
        public void SetExactSharePointVersion(Uri site, string version)
        {
            var exactSharepointVersions = Store.GetAndInitialize<Dictionary<string, string>>(StoreOptions.GetKey(keyExactSharepointVersions));
            if (!exactSharepointVersions.ContainsKey(site.ToString()))
            {
                exactSharepointVersions.Add(site.ToString(), version);
                Store.Set<Dictionary<string, string>>(StoreOptions.GetKey(keyExactSharepointVersions), exactSharepointVersions, StoreOptions.EntryOptions);
            }
        }

        /// <summary>
        /// Returns the used AzureAD tenant id
        /// </summary>
        /// <param name="site">Url of the site</param>
        /// <returns>Azure AD tenant id</returns>
        public Guid GetAADTenantId(Uri site)
        {
            var aadTenantId = Store.GetAndInitialize<Dictionary<string, Guid>>(StoreOptions.GetKey(keyAadTenantId));

            if (aadTenantId.ContainsKey(site.ToString()))
            {
                return aadTenantId[site.ToString()];
            }
            else
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Sets the Azure AD tenant Id in cache
        /// </summary>
        /// <param name="tenantId">Tenant Id</param>
        /// <param name="site">Site url</param>
        public void SetAADTenantId(Guid tenantId, Uri site)
        {
            var aadTenantId = Store.GetAndInitialize<Dictionary<string, Guid>>(StoreOptions.GetKey(keyAadTenantId));
            
            if (!aadTenantId.ContainsKey(site.ToString()))
            {
                aadTenantId.Add(site.ToString(), tenantId);
                Store.Set<Dictionary<string, Guid>>(StoreOptions.GetKey(keyAadTenantId), aadTenantId, StoreOptions.EntryOptions);
            }
        }
        #endregion

        #region Asset Transfer
        public List<AssetTransferredEntity> GetAssetsTransferred()
        {
            return Store.GetAndInitialize<List<AssetTransferredEntity>>(StoreOptions.GetKey(keyAssetsTransferred));
        }

        public void AddAssetTransferredEntity(AssetTransferredEntity asset)
        {
            var assetsTransferred = Store.GetAndInitialize<List<AssetTransferredEntity>>(StoreOptions.GetKey(keyAssetsTransferred)); 
            if (!assetsTransferred.Contains(asset))
            {
                assetsTransferred.Add(asset);
                Store.Set<List<AssetTransferredEntity>>(StoreOptions.GetKey(keyAssetsTransferred), assetsTransferred, StoreOptions.EntryOptions);
            }
        }
        #endregion

        #region Client Side Components
        /// <summary>
        /// Gets the clientside components from cache or if needed retrieves and caches them
        /// </summary>
        /// <param name="targetClientContext"></param>
        /// <param name="page">Page to grab the components for</param>
        /// <returns></returns>
        internal List<PnPCore.PageComponent> GetClientSideComponents(ClientContext targetClientContext, PnPCore.IPage page)
        {
            Guid webId = targetClientContext.Web.EnsureProperty(o => o.Id);

            var siteToComponentMapping = Store.GetAndInitialize<Dictionary<Guid, string>>(StoreOptions.GetKey(keySiteToComponentMapping));

            if (siteToComponentMapping.ContainsKey(webId))
            {
                // Components are cached for this site, get the component key
                if (siteToComponentMapping.TryGetValue(webId, out string componentKey))
                {
                    var clientSideComponents2 = Store.GetAndInitialize<Dictionary<string, string>>(StoreOptions.GetKey(keyClientSideComponents));
                    if (clientSideComponents2.TryGetValue(componentKey, out string componentList))
                    {
                        return JsonSerializer.Deserialize<List<PnPCore.PageComponent>>(componentList);
                    }
                }
            }

            // Ok, so nothing in cache so it seems, so let's get the components
            //var componentsToAdd = page.AvailablePageComponents().Cast<PnPCore.IPageComponent>().ToList();
            List<PnPCore.PageComponent> componentsToAdd = new List<PnPCore.PageComponent>();
            foreach (var component in page.AvailablePageComponents())
            {
                componentsToAdd.Add(new PnPCore.PageComponent()
                {
                    ComponentType = component.ComponentType,
                    Id = component.Id,
                    Manifest = component.Manifest,
                    ManifestType = component.ManifestType,
                    Name = component.Name,
                    Status = component.Status
                });
            }

            // calculate the componentkey
            string jsonComponentsToAdd = JsonSerializer.Serialize(componentsToAdd);
            string componentKeyToCache = Sha256(jsonComponentsToAdd);

            // store the retrieved data in cache
            siteToComponentMapping.Add(webId, componentKeyToCache);
            Store.Set<Dictionary<Guid, string>>(StoreOptions.GetKey(keySiteToComponentMapping), siteToComponentMapping, StoreOptions.EntryOptions);

            // Since the components list is big and often the same across webs we only store it in cache if it's different
            var clientSideComponents = Store.GetAndInitialize<Dictionary<string, string>>(StoreOptions.GetKey(keyClientSideComponents));
            if (!clientSideComponents.ContainsKey(componentKeyToCache))
            {
                clientSideComponents.Add(componentKeyToCache, jsonComponentsToAdd);
                Store.Set<Dictionary<string, string>>(StoreOptions.GetKey(keyClientSideComponents), clientSideComponents, StoreOptions.EntryOptions);
            }

            return componentsToAdd;
        }

        /// <summary>
        /// Clear the clientside component cache
        /// </summary>
        public void ClearClientSideComponents()
        {
            Store.Remove(StoreOptions.GetKey(keyClientSideComponents));
            Store.Remove(StoreOptions.GetKey(keySiteToComponentMapping));
        }
        #endregion

        #region Base template and metadata
        /// <summary>
        /// Gets the base template that will be used to filter out "OOB" fields
        /// </summary>
        /// <param name="web">web to operate against</param>
        /// <returns>Provisioning template of the base template of STS#0</returns>
        public Provisioning.Model.ProvisioningTemplate GetBaseTemplate(Web web)
        {
            if (this.baseTemplate == null)
            {
                this.baseTemplate = web.GetBaseTemplate("STS", 0);

                // Ensure certain new fields are there
                var sitePagesInBaseTemplate = baseTemplate.Lists.Where(p => p.Url == "SitePages").FirstOrDefault();
                if (sitePagesInBaseTemplate != null)
                {
                    AddFieldRef(sitePagesInBaseTemplate.FieldRefs, new Guid("ccc1037f-f65e-434a-868e-8c98af31fe29"), "_ComplianceFlags");
                    AddFieldRef(sitePagesInBaseTemplate.FieldRefs, new Guid("d4b6480a-4bed-4094-9a52-30181ea38f1d"), "_ComplianceTag");
                    AddFieldRef(sitePagesInBaseTemplate.FieldRefs, new Guid("92be610e-ddbb-49f4-b3b1-5c2bc768df8f"), "_ComplianceTagWrittenTime");
                    AddFieldRef(sitePagesInBaseTemplate.FieldRefs, new Guid("418d7676-2d6f-42cf-a16a-e43d2971252a"), "_ComplianceTagUserId");
                    AddFieldRef(sitePagesInBaseTemplate.FieldRefs, new Guid("8382d247-72a9-44b1-9794-7b177edc89f3"), "_IsRecord");
                    AddFieldRef(sitePagesInBaseTemplate.FieldRefs, new Guid("d307dff3-340f-44a2-9f4b-fbfe1ba07459"), "_CommentCount");
                    AddFieldRef(sitePagesInBaseTemplate.FieldRefs, new Guid("db8d9d6d-dc9a-4fbd-85f3-4a753bfdc58c"), "_LikeCount");
                    AddFieldRef(sitePagesInBaseTemplate.FieldRefs, new Guid("3a6b296c-3f50-445c-a13f-9c679ea9dda3"), "ComplianceAssetId");
                    AddFieldRef(sitePagesInBaseTemplate.FieldRefs, new Guid("9de685c5-fdf5-4319-b987-3edf55efb36f"), "_SPSitePageFlags");
                    AddFieldRef(sitePagesInBaseTemplate.FieldRefs, new Guid("1a53ab5a-11f9-4b92-a377-8cfaaf6ba7be"), "_DisplayName");
                    AddFieldRef(sitePagesInBaseTemplate.FieldRefs, new Guid("3a8ee3f8-166b-4394-b3e2-e98dcf86a847"), "A2ODMountCount");
                }
            }

            return this.baseTemplate;
        }

        /// <summary>
        /// Clear base template cache
        /// </summary>
        public void ClearBaseTemplate()
        {
            this.baseTemplate = null;
        }

        /// <summary>
        /// Get the list of fields that need to be copied from cache. If cache is empty the list will be calculated
        /// </summary>
        /// <param name="web">Web to operate against</param>
        /// <param name="sourceLibrary">Pages library instance</param>
        /// <param name="pageType">Type of page</param>
        /// <returns>List of fields that need to be copied</returns>
        public List<FieldData> GetFieldsToCopy(Web web, List sourceLibrary, string pageType)
        {
            List<FieldData> fieldsToCopyRetrieved = new List<FieldData>();

            var fieldsToCopy = Store.GetAndInitialize<Dictionary<string, List<FieldData>>>(StoreOptions.GetKey(keyFieldsToCopy));

            // Did we already do the calculation for this sitepages library? If so then return from cache
            if (fieldsToCopy.ContainsKey(sourceLibrary.Id.ToString()))
            {
                if (fieldsToCopy.TryGetValue(sourceLibrary.Id.ToString(), out List<FieldData> fields))
                {
                    return fields;
                }
            }
            else
            {
                // calculate the fields to copy
                var baseTemplate = GetBaseTemplate(web);
                if (baseTemplate != null)
                {
                    var sitePagesInBaseTemplate = baseTemplate.Lists.Where(p => p.Url == "SitePages").FirstOrDefault();

                    // Compare site pages list fields
                    foreach (var sourceField in sourceLibrary.Fields.Where(p => p.Hidden == false).ToList())
                    {
                        // Skip OOB fields
                        if (!IsBuiltInField(pageType.Equals("BlogPage", StringComparison.InvariantCultureIgnoreCase), sourceField.Id))
                        {
                            if (sitePagesInBaseTemplate != null)
                            {
                                var fieldFoundInBaseSitePages = sitePagesInBaseTemplate.FieldRefs.Where(p => p.Name == sourceField.StaticName).FirstOrDefault();
                                if (fieldFoundInBaseSitePages == null)
                                {
                                    // copy metadata for this field
                                    FieldData fieldToAdd = new FieldData()
                                    {
                                        FieldName = sourceField.StaticName,
                                        FieldId = sourceField.Id,
                                        FieldType = sourceField.TypeAsString,
                                    };

                                    fieldsToCopyRetrieved.Add(fieldToAdd);
                                }
                            }
                        }
                    }

                    // Add to cache
                    fieldsToCopy.Add(sourceLibrary.Id.ToString(), fieldsToCopyRetrieved);
                    Store.Set<Dictionary<string, List<FieldData>>>(StoreOptions.GetKey(keyFieldsToCopy), fieldsToCopy, StoreOptions.EntryOptions);
                    return fieldsToCopyRetrieved;

                }
            }

            // We should not get here...
            return null;
        }

        /// <summary>
        /// Get field information of a content type field
        /// </summary>
        /// <param name="pagesLibrary">Pages library list</param>
        /// <param name="contentTypeId">ID of the content type</param>
        /// <param name="fieldName">Name of the field to get information from</param>
        /// <returns>FieldData object holding field information</returns>
        public FieldData GetPublishingContentTypeField(List pagesLibrary, string contentTypeId, string fieldName)
        {
            // Try to get from cache
            var publishingContentTypeFields = Store.GetAndInitialize<Dictionary<string, List<FieldData>>>(StoreOptions.GetKey(keyPublishingContentTypeFields));
            if (publishingContentTypeFields.TryGetValue(contentTypeId, out List<FieldData> fieldsFromCache))
            {
                // return field if found
                return fieldsFromCache.Where(p => p.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }

            ClientContext context = pagesLibrary.Context as ClientContext;

            // Load all fields of the list
            FieldCollection fields = pagesLibrary.Fields;
            context.Load(fields, fs => fs.Include(f => f.Id, f => f.TypeAsString, f => f.InternalName));
            context.ExecuteQueryRetry();

            List<FieldData> contentTypeFieldsInList = new List<FieldData>();
            foreach (var field in fields)
            {
                contentTypeFieldsInList.Add(new FieldData()
                {
                    FieldId = field.Id,
                    FieldName = field.InternalName,
                    FieldType = field.TypeAsString,
                });
            }

            var authorField = new FieldData()
            {
                FieldId = new Guid("1df5e554-ec7e-46a6-901d-d85a3881cb18"),
                FieldName = "Author",
                FieldType = "User",
            };

            var editorField = new FieldData()
            {
                FieldId = new Guid("d31655d1-1d5b-4511-95a1-7a09e9b75bf2"),
                FieldName = "Editor",
                FieldType = "User",
            };

            if (!contentTypeFieldsInList.Contains(authorField))
            {
                contentTypeFieldsInList.Add(authorField);
            }

            if (!contentTypeFieldsInList.Contains(editorField))
            {
                contentTypeFieldsInList.Add(editorField);
            }

            // Store in cache
            publishingContentTypeFields.Add(contentTypeId, contentTypeFieldsInList);
            Store.Set<Dictionary<string, List<FieldData>>>(StoreOptions.GetKey(keyPublishingContentTypeFields), publishingContentTypeFields, StoreOptions.EntryOptions);

            // Return field, if found
            return contentTypeFieldsInList.Where(p => p.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        /// <summary>
        /// Clear the fields to copy cache
        /// </summary>
        public void ClearFieldsToCopy()
        {
            Store.Remove(StoreOptions.GetKey(keyFieldsToCopy));
            Store.Remove(StoreOptions.GetKey(keyPublishingContentTypeFields));
        }

        #endregion

        #region Web type handling
        /// <summary>
        /// Marks this web as a publishing web
        /// </summary>
        /// <param name="webUrl">Url of the web</param>
        public void SetPublishingWeb(string webUrl)
        {
            var webType = Store.GetAndInitialize<Dictionary<string, string>>(StoreOptions.GetKey(keyWebType));

            if (!webType.ContainsKey(webUrl))
            {
                webType.Add(webUrl, CacheManager.Publishing);
                Store.Set<Dictionary<string, string>>(StoreOptions.GetKey(keyWebType), webType, StoreOptions.EntryOptions);
            }
        }

        /// <summary>
        /// Marks this web as a blog web
        /// </summary>
        /// <param name="webUrl">Url of the web</param>
        public void SetBlogWeb(string webUrl)
        {
            var webType = Store.GetAndInitialize<Dictionary<string, string>>(StoreOptions.GetKey(keyWebType));
            if (!webType.ContainsKey(webUrl))
            {
                webType.Add(webUrl, CacheManager.Blog);
                Store.Set<Dictionary<string, string>>(StoreOptions.GetKey(keyWebType), webType, StoreOptions.EntryOptions);
            }
        }

        /// <summary>
        /// Checks if this is publishing web
        /// </summary>
        /// <param name="webUrl">Web url to check</param>
        /// <returns>True if publishing, false otherwise</returns>
        public bool IsPublishingWeb(string webUrl)
        {
            var webType = Store.GetAndInitialize<Dictionary<string, string>>(StoreOptions.GetKey(keyWebType));
            if (webType.ContainsKey(webUrl))
            {
                if (webType.TryGetValue(webUrl, out string type))
                {
                    if (type.Equals(CacheManager.Publishing))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if this is blog web
        /// </summary>
        /// <param name="webUrl">Web url to check</param>
        /// <returns>True if blog, false otherwise</returns>
        public bool IsBlogWeb(string webUrl)
        {
            var webType = Store.GetAndInitialize<Dictionary<string, string>>(StoreOptions.GetKey(keyWebType));
            if (webType.ContainsKey(webUrl))
            {
                if (webType.TryGetValue(webUrl, out string type))
                {
                    if (type.Equals(CacheManager.Blog))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        #region Publishing Pages Library
        /// <summary>
        /// Get translation for the publishing pages library
        /// </summary>
        /// <param name="context">Context of the site</param>
        /// <returns>Translated name of the pages library</returns>
        public string GetPublishingPagesLibraryName(ClientContext context)
        {
            // Simplier implementation - Get the Pages library then get the relative URL of the rootfolder of the library

            //Keys: 
            //  Web Property: __PagesListId
            //  Found in 2010, SPO

            string pagesLibraryName = "pages";

            if (context == null)
            {
                return pagesLibraryName;
            }

            int lcid = (int)context.Web.EnsureProperty(p => p.Language);

            var propertyBagKey = Constants.WebPropertyKeyPagesListId;
            
            var publishingPagesLibraryNames = Store.GetAndInitialize<Dictionary<int, string>>(StoreOptions.GetKey(keyPublishingPagesLibraryNames));
            if (publishingPagesLibraryNames.ContainsKey(lcid))
            {
                if (publishingPagesLibraryNames.TryGetValue(lcid, out string name))
                {
                    return name;
                }
                else
                {
                    // let's fallback to the default...we should never get here unless there's some threading issue
                    return pagesLibraryName;
                }
            }
            else
            {
                if (BaseTransform.GetVersion(context) == SPVersion.SP2010)
                {
                    var keyVal = context.Web.GetPropertyBagValueString(propertyBagKey, string.Empty);
                    if (!string.IsNullOrEmpty(keyVal))
                    {
                        var list = context.Web.GetListById(Guid.Parse(keyVal), o => o.RootFolder.ServerRelativeUrl);
                        var webServerRelativeUrl = context.Web.EnsureProperty(w => w.ServerRelativeUrl);

                        pagesLibraryName = list.RootFolder.ServerRelativeUrl.Replace(webServerRelativeUrl, "").Trim('/').ToLower();

                        // add to cache
                        publishingPagesLibraryNames.Add(lcid, pagesLibraryName);
                        Store.Set(StoreOptions.GetKey(keyPublishingPagesLibraryNames), publishingPagesLibraryNames, StoreOptions.EntryOptions);

                        return pagesLibraryName;
                    }
                }
                else
                {
                    // Fall back to older logic
                    ClientResult<string> result = Microsoft.SharePoint.Client.Utilities.Utility.GetLocalizedString(context, "$Resources:List_Pages_UrlName", "osrvcore", int.Parse(lcid.ToString()));
                    context.ExecuteQueryRetry();

                    var altPagesLibraryName = new Regex(@"['´`]").Replace(result.Value, "");

                    if (string.IsNullOrEmpty(altPagesLibraryName))
                    {
                        return pagesLibraryName;
                    }

                    // add to cache
                    publishingPagesLibraryNames.Add(lcid, altPagesLibraryName.ToLower());
                    Store.Set(StoreOptions.GetKey(keyPublishingPagesLibraryNames), publishingPagesLibraryNames, StoreOptions.EntryOptions);

                    return altPagesLibraryName.ToLower();
                }
            }

            return pagesLibraryName;
        }

        #endregion

        #region Blog list name
        /// <summary>
        /// Get translation for the blog list name
        /// </summary>
        /// <param name="context">Context of the site</param>
        /// <returns>Translated name of the blog list</returns>
        public string GetBlogListName(ClientContext context)
        {
            string blogListName = "posts";

            if (context == null)
            {
                return blogListName;
            }

            int lcid = (int)context.Web.EnsureProperty(p => p.Language);
            var blogListNames = Store.GetAndInitialize<Dictionary<int, string>>(StoreOptions.GetKey(keyBlogListNames));

            if (blogListNames.ContainsKey(lcid))
            {
                if (blogListNames.TryGetValue(lcid, out string name))
                {
                    return name;
                }
                else
                {
                    // let's fallback to the default...we should never get here unless there's some threading issue
                    return blogListName;
                }
            }
            else
            {
                string altBlogListName = null;
                try
                {
                    ClientResult<string> result = Microsoft.SharePoint.Client.Utilities.Utility.GetLocalizedString(context, "$Resources:blogpost_Folder", "core", int.Parse(lcid.ToString()));
                    context.ExecuteQueryRetry();
                    altBlogListName = new Regex(@"['´`]").Replace(result.Value, "");
                }
                catch
                {
                    // Use "simple" method, which also works for SharePoint 2010
                    altBlogListName = PostsTranslation(lcid);
                }

                if (string.IsNullOrEmpty(altBlogListName))
                {
                    return blogListName;
                }

                // add to cache
                blogListNames.Add(lcid, altBlogListName.ToLower());
                Store.Set(StoreOptions.GetKey(keyBlogListNames), blogListNames, StoreOptions.EntryOptions);

                return altBlogListName.ToLower();
            }
        }
        #endregion

        #region Resource strings
        /// <summary>
        /// Returns the translated value for a resource string
        /// </summary>
        /// <param name="context">Context of the site</param>
        /// <param name="resource">Key of the resource (e.g. $Resources:core,ScriptEditorWebPartDescription;) </param>
        /// <returns>Translated string</returns>
        public string GetResourceString(ClientContext context, string resource)
        {
            int lcid = (int)context.Web.EnsureProperty(p => p.Language);

            var resourceStrings = Store.GetAndInitialize<Dictionary<string, Dictionary<int, string>>>(StoreOptions.GetKey(keyResourceStrings));

            if (resourceStrings.ContainsKey(resource))
            {
                if (resourceStrings.TryGetValue(resource, out Dictionary<int, string> resourceValues))
                {
                    if (resourceValues.ContainsKey(lcid))
                    {
                        if (resourceValues.TryGetValue(lcid, out string resourceValue))
                        {
                            return resourceValue;
                        }
                    }
                }
            }

            // If we got here then we need to still add the resource translation
            var resourceString = resource.Replace("$Resources:", "").Replace(";", "");
            var splitResourceString = resourceString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string resourceFile = "core";
            string resourceKey = null;
            if (splitResourceString.Length == 2)
            {
                resourceFile = splitResourceString[0];
                resourceKey = splitResourceString[1];
            }
            else
            {
                resourceKey = splitResourceString[0];
            }

            ClientResult<string> result = Microsoft.SharePoint.Client.Utilities.Utility.GetLocalizedString(context, $"$Resources:{resourceKey}", resourceFile, int.Parse(lcid.ToString()));
            context.ExecuteQueryRetry();

            if (result == null)
            {
                return resource;
            }

            if (resourceStrings.ContainsKey(resource))
            {
                if (resourceStrings.TryGetValue(resource, out Dictionary<int, string> resourceValues))
                {
                    if (!resourceValues.ContainsKey(lcid))
                    {
                        // Add translations in existing array
                        Dictionary<int, string> newResourceValues = new Dictionary<int, string>(resourceValues)
                        {
                            { lcid, result.Value }
                        };
                        resourceStrings[resource] = newResourceValues;
                        Store.Set(StoreOptions.GetKey(keyResourceStrings), resourceStrings, StoreOptions.EntryOptions);
                    }
                }
            }
            else
            {
                // No translations were already retrieved in this language
                Dictionary<int, string> translations = new Dictionary<int, string>
                {
                    { lcid, result.Value }
                };

                resourceStrings.Add(resource, translations);
                Store.Set(StoreOptions.GetKey(keyResourceStrings), resourceStrings, StoreOptions.EntryOptions);
            }

            return result.Value;
        }
        #endregion

        #region PageLayout mappings
        /// <summary>
        /// Generate pagelayout mapping file for given publishing page
        /// </summary>
        /// <param name="page">Publishing page</param>
        /// <returns>Page layout mapping model</returns>
        public PageLayout GetPageLayoutMapping(ListItem page)
        {
            string key = page.PageLayoutFile();

            // Try get the page layout from cache
            var generatedPageLayoutMappings = Store.GetAndInitialize<Dictionary<string, PageLayout>>(StoreOptions.GetKey(keyGeneratedPageLayoutMappings));
            if (generatedPageLayoutMappings.TryGetValue(key, out PageLayout pageLayoutFromCache))
            {
                return pageLayoutFromCache;
            }

            PageLayoutAnalyser pageLayoutAnalyzer = new PageLayoutAnalyser(page.Context as ClientContext);

            // Let's try to generate a 'basic' model and use that...not optimal, but better than bailing out.
            var newPageLayoutMapping = pageLayoutAnalyzer.AnalysePageLayoutFromPublishingPage(page);

            // Add to cache for future reuse
            generatedPageLayoutMappings.Add(key, newPageLayoutMapping);
            Store.Set(StoreOptions.GetKey(keyGeneratedPageLayoutMappings), generatedPageLayoutMappings, StoreOptions.EntryOptions);

            // Return to requestor
            return newPageLayoutMapping;
        }

        #endregion

        #region Generic methods
        /// <summary>
        /// Clear all the caches
        /// </summary>
        public void ClearAllCaches()
        {
            ClearClientSideComponents();
            ClearBaseTemplate();

            Store.Remove(StoreOptions.GetKey(keyUrlMapping));
            Store.Remove(StoreOptions.GetKey(keyUserMappings));
            Store.Remove(StoreOptions.GetKey(keyEnsuredUsers));
            Store.Remove(StoreOptions.GetKey(keyTermMappings));
            Store.Remove(StoreOptions.GetKey(keyAssetsTransferred));
            Store.Remove(StoreOptions.GetKey(keyTermTransformatorCache));

            ClearFieldsToCopy();
            ClearSharePointVersions();
            ClearGeneratedPageLayoutMappings();
        }

        /// <summary>
        /// Clears Cached SharePoint versions
        /// </summary>
        public void ClearSharePointVersions()
        {
            Store.Remove(StoreOptions.GetKey(keySharePointVersions));
        }

        /// <summary>
        /// Clears the cache of generated page layout mappings
        /// </summary>
        public void ClearGeneratedPageLayoutMappings()
        {
            Store.Remove(StoreOptions.GetKey(keyGeneratedPageLayoutMappings));
        }

        #endregion

        #region Users
        /// <summary>
        /// Mapped users
        /// </summary>
        /// <returns>A dictionary of mapped users</returns>
        public Dictionary<string, string> GetMappedUsers()
        {
            var mappedUsers = Store.GetAndInitialize<Dictionary<string, string>>(StoreOptions.GetKey(keyMappedUsers));
            return mappedUsers;
        }

        /// <summary>
        /// Adds a user to the dictionary of mapped users
        /// </summary>
        /// <param name="principal">Principal to map</param>
        /// <param name="user">mapped user</param>
        public void AddMappedUser(string principal, string user)
        {
            var mappedUsers = Store.GetAndInitialize<Dictionary<string, string>>(StoreOptions.GetKey(keyMappedUsers));
            if (!mappedUsers.ContainsKey(principal))
            {
                mappedUsers.Add(principal, user);
                Store.Set(StoreOptions.GetKey(keyMappedUsers), mappedUsers, StoreOptions.EntryOptions);
            }
        }

        /// <summary>
        /// Run and cache the output value of EnsureUser for a given user
        /// </summary>
        /// <param name="context">ClientContext to operate on</param>
        /// <param name="userValue">User name of user to ensure</param>
        /// <returns>ResolvedUser instance holding information about the ensured user</returns>
        public ResolvedUser GetEnsuredUser(ClientContext context, string userValue)
        {
            if (string.IsNullOrEmpty(userValue))
            {
                return null;
            }

            string key = context.Web.GetUrl();

            var ensuredUsers = Store.GetAndInitialize<Dictionary<string, Dictionary<string, ResolvedUser>>>(StoreOptions.GetKey(keyEnsuredUsers));
            if (ensuredUsers.TryGetValue(key, out Dictionary<string, ResolvedUser> ensuredUsersFromCache))
            {
                if (ensuredUsersFromCache.TryGetValue(userValue, out ResolvedUser userLoginName))
                {
                    return userLoginName;
                }
            }

            try
            {
                using (var clonedContext = context.Clone(context.Web.GetUrl()))
                {
                    var userToResolve = clonedContext.Web.EnsureUser(userValue);
                    clonedContext.Load(userToResolve);
                    clonedContext.ExecuteQueryRetry();

                    ResolvedUser resolvedUser = new ResolvedUser()
                    {
                        LoginName = userToResolve.LoginName,
                        Id = userToResolve.Id,
                    };

                    // Store in cache
                    if (ensuredUsersFromCache != null)
                    {
                        // We already has a user list, simply add this one
                        Dictionary<string, ResolvedUser> newEnsuredUsersFromCache = new Dictionary<string, ResolvedUser>(ensuredUsersFromCache)
                            {
                                { userValue, resolvedUser }
                            };

                        ensuredUsers[key] = newEnsuredUsersFromCache;
                        Store.Set(StoreOptions.GetKey(keyEnsuredUsers), ensuredUsers, StoreOptions.EntryOptions);
                    }
                    else
                    {
                        // First user for this key (= web)
                        Dictionary<string, ResolvedUser> newEnsuredUsersFromCache = new Dictionary<string, ResolvedUser>()
                            {
                                { userValue, resolvedUser }
                            };

                        ensuredUsers.Add(key, newEnsuredUsersFromCache);
                        Store.Set(StoreOptions.GetKey(keyEnsuredUsers), ensuredUsers, StoreOptions.EntryOptions);
                    }

                    return resolvedUser;
                }
            }
            catch
            {
                // Logging is not needed as an "empty" ensured user is handled by the callers of this method
            }

            return null;
        }

        /// <summary>
        /// Lookup a user from the site's user list based upon the user's upn
        /// </summary>
        /// <param name="context">Context of the web holding the user list</param>
        /// <param name="userUpn">Upn of the user to fetch</param>
        /// <returns>A UserEntity instance holding information about the user</returns>
        public UserEntity GetUserFromUserList(ClientContext context, string userUpn)
        {
            if (string.IsNullOrEmpty(userUpn))
            {
                return null;
            }

            string key = context.Web.GetUrl();

            if (!userUpn.StartsWith("i:0#.f|membership|"))
            {
                userUpn = $"i:0#.f|membership|{userUpn}";
            }

            var userJsonStringsViaUpn = Store.GetAndInitialize<Dictionary<string, Dictionary<string, UserEntity>>>(StoreOptions.GetKey(keyUserJsonStringsViaUpn));
            if (userJsonStringsViaUpn.TryGetValue(key, out Dictionary<string, UserEntity> userListFromCache))
            {
                if (userListFromCache.TryGetValue(userUpn, out UserEntity userJsonFromCache))
                {
                    return userJsonFromCache;
                }
            }

            try
            {
                string CAMLQueryByName = @"
                <View Scope='Recursive'>
                  <Query>
                    <Where>
                      <Eq>
                        <FieldRef Name='Name'/>
                        <Value Type='Text'>{0}</Value>
                      </Eq>
                    </Where>
                  </Query>
                </View>";

                List siteUserInfoList = context.Web.SiteUserInfoList;
                CamlQuery query = new CamlQuery
                {
                    ViewXml = string.Format(CAMLQueryByName, userUpn)
                };
                var loadedUsers = context.LoadQuery(siteUserInfoList.GetItems(query));
                context.ExecuteQueryRetry();

                UserEntity author = null;
                if (loadedUsers != null)
                {
                    var loadedUser = loadedUsers.FirstOrDefault();
                    if (loadedUser != null)
                    {
                        // Does not work for groups
                        if (loadedUser["Name"] == null)
                        {
                            return null;
                        }

                        bool isGroup = loadedUser["Name"].ToString().StartsWith("c:0t.c|tenant|");
                        string userUpnValue = loadedUser["Name"].ToString().GetUserName();

                        author = new UserEntity()
                        {
                            Upn = userUpnValue,
                            Name = loadedUser["Title"] != null ? loadedUser["Title"].ToString() : "",
                            Role = loadedUser["JobTitle"] != null ? loadedUser["JobTitle"].ToString() : "",
                            LoginName = loadedUser["Name"] != null ? loadedUser["Name"].ToString() : "",
                            IsGroup = isGroup || IsGroup(userUpnValue),
                        };

                        author.Id = $"i:0#.f|membership|{author.Upn}";

                        // Store in cache
                        if (userListFromCache != null)
                        {
                            // We already has a user list, simply add this one
                            Dictionary<string, UserEntity> newUserListToCache = new Dictionary<string, UserEntity>(userListFromCache)
                            {
                                { userUpn, author }
                            };

                            userJsonStringsViaUpn[key] = newUserListToCache;
                            Store.Set(StoreOptions.GetKey(keyUserJsonStringsViaUpn), userJsonStringsViaUpn, StoreOptions.EntryOptions);
                        }
                        else
                        {
                            // First user for this key (= web)
                            Dictionary<string, UserEntity> newUserListToCache = new Dictionary<string, UserEntity>()
                            {
                                { userUpn, author }
                            };

                            userJsonStringsViaUpn.Add(key, newUserListToCache);
                            Store.Set(StoreOptions.GetKey(keyUserJsonStringsViaUpn), userJsonStringsViaUpn, StoreOptions.EntryOptions);
                        }

                        // return 
                        return author;
                    }
                }
            }
            catch
            {
                // Logging is not needed as an "empty" ensured user is handled by the callers of this method
            }

            return null;
        }

        /// <summary>
        /// Lookup a user from the site's user list based upon the user's id
        /// </summary>
        /// <param name="context">Context of the web holding the user list</param>
        /// <param name="userListId">Id of the user to fetch</param>
        /// <returns>A UserEntity instance holding information about the user</returns>
        public UserEntity GetUserFromUserList(ClientContext context, int userListId)
        {
            string key = context.Web.GetUrl();

            var userJsonStrings = Store.GetAndInitialize<Dictionary<string, Dictionary<string, UserEntity>>>(StoreOptions.GetKey(keyUserJsonStrings));
            if (userJsonStrings.TryGetValue(key, out Dictionary<string, UserEntity> userListFromCache))
            {
                if (userListFromCache.TryGetValue(userListId.ToString(), out UserEntity userJsonFromCache))
                {
                    return userJsonFromCache;
                }
            }

            try
            {
                string CAMLQueryByName = @"
                <View Scope='Recursive'>
                  <Query>
                    <Where>
                      <Contains>
                        <FieldRef Name='ID'/>
                        <Value Type='Integer'>{0}</Value>
                      </Contains>
                    </Where>
                  </Query>
                </View>";

                List siteUserInfoList = context.Web.SiteUserInfoList;
                CamlQuery query = new CamlQuery
                {
                    ViewXml = string.Format(CAMLQueryByName, userListId)
                };
                var loadedUsers = context.LoadQuery(siteUserInfoList.GetItems(query));
                context.ExecuteQueryRetry();

                UserEntity author = null;
                if (loadedUsers != null)
                {
                    var loadedUser = loadedUsers.FirstOrDefault();
                    if (loadedUser != null)
                    {
                        if (loadedUser["Name"] == null)
                        {
                            return null;
                        }

                        bool isGroup = loadedUser["Name"].ToString().StartsWith("c:0t.c|tenant|");
                        string userUpnValue = loadedUser["Name"].ToString().GetUserName();

                        author = new UserEntity()
                        {
                            Upn = userUpnValue,
                            Name = loadedUser["Title"] != null ? loadedUser["Title"].ToString() : "",
                            Role = loadedUser["JobTitle"] != null ? loadedUser["JobTitle"].ToString() : "",
                            LoginName = loadedUser["Name"] != null ? loadedUser["Name"].ToString() : "",
                            IsGroup = isGroup || IsGroup(userUpnValue),
                        };

                        author.Id = $"i:0#.f|membership|{author.Upn}";

                        // Store in cache
                        if (userListFromCache != null)
                        {
                            // We already has a user list, simply add this one
                            Dictionary<string, UserEntity> newUserListToCache = new Dictionary<string, UserEntity>(userListFromCache)
                            {
                                { userListId.ToString(), author }
                            };

                            userJsonStrings[key] = newUserListToCache;
                            Store.Set(StoreOptions.GetKey(keyUserJsonStrings), userJsonStrings, StoreOptions.EntryOptions);
                        }
                        else
                        {
                            // First user for this key (= web)
                            Dictionary<string, UserEntity> newUserListToCache = new Dictionary<string, UserEntity>()
                            {
                                { userListId.ToString(), author }
                            };

                            userJsonStrings.Add(key, newUserListToCache);
                            Store.Set(StoreOptions.GetKey(keyUserJsonStrings), userJsonStrings, StoreOptions.EntryOptions);
                        }

                        // return 
                        return author;
                    }
                }
            }
            catch(Exception)
            {
                // Logging is not needed as an "empty" ensured user is handled by the callers of this method
            }

            return null;
        }
        #endregion

        #region Content types
        /// <summary>
        /// Gets the ID of a contenttype
        /// </summary>
        /// <param name="pagesLibrary">Pages library holding the content type</param>
        /// <param name="contentTypeName">Name of the content type</param>
        /// <returns>ID of the content type</returns>
        public string GetContentTypeId(List pagesLibrary, string contentTypeName)
        {
            string contentTypeId = null;

            // try to get from cache
            var contentTypes = Store.GetAndInitialize<Dictionary<string, string>>(StoreOptions.GetKey(keyContentTypes));
            contentTypes.TryGetValue(contentTypeName, out string contentTypeIdFromCache);
            if (!string.IsNullOrEmpty(contentTypeIdFromCache))
            {
                return contentTypeIdFromCache;
            }

            // Load content type
            var ctCol = pagesLibrary.ContentTypes;
            var results = pagesLibrary.Context.LoadQuery(ctCol.Where(item => item.Name == contentTypeName));
            pagesLibrary.Context.ExecuteQueryRetry();

            if (results.FirstOrDefault() != null)
            {
                contentTypeId = results.FirstOrDefault().StringId;

                // We only allow content types that inherit from the OOB Site Page content type
                if (!contentTypeId.StartsWith(Constants.ModernPageContentTypeId, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                // add to cache
                contentTypes.Add(contentTypeName, contentTypeId);
                Store.Set(StoreOptions.GetKey(keyContentTypes), contentTypes, StoreOptions.EntryOptions);
            }

            return contentTypeId;
        }
        #endregion

        #region Taxonomy 
        /// <summary>
        /// Return information for the given term
        /// </summary>
        /// <param name="context">ClientContext object to operate on</param>
        /// <param name="termId">Id of the term to lookup</param>
        /// <returns>Term label</returns>
        public string GetTermFromId(ClientContext context, Guid termId)
        {
            string termInfo = null;

            var termCache = Store.GetAndInitialize<Dictionary<Guid, string>>(StoreOptions.GetKey(keyTermCache));
            termCache.TryGetValue(termId, out string termInfoFromCache);
            if (!string.IsNullOrEmpty(termInfoFromCache))
            {
                return termInfoFromCache;
            }

            // Lookup the provided term
            TaxonomySession taxSession = TaxonomySession.GetTaxonomySession(context);
            var loadedTerm = taxSession.GetTerm(termId);
            context.Load(loadedTerm, p => p.Name);
            context.ExecuteQueryRetry();

            if (loadedTerm != null)
            {
                termInfo = loadedTerm.Name;
                
                // add to cache
                termCache.Add(termId, termInfo);
                Store.Set(StoreOptions.GetKey(keyTermCache), termCache, StoreOptions.EntryOptions);
            }

            return termInfo;
        }

        #region Term Transformator Caching

        /// <summary>
        /// Get Term Path by ID
        /// </summary>
        /// <param name="context"></param>
        /// <param name="termSetId"></param>
        /// <param name="sourceSspId"></param>
        /// <param name="isSP2010"></param>
        /// <param name="isSourceTerm"></param>
        /// <returns></returns>
        public void StoreTermSetTerms(ClientContext context, Guid termSetId, Guid sourceSspId, bool isSP2010, bool isSourceTerm)
        {
            var termsAlreadyInCache = GetTransformTermCacheTermsByTermSet(context, termSetId);
            if(termsAlreadyInCache == default)
            {
                var termCache = Store.GetAndInitialize<Dictionary<Guid, TermData>>(StoreOptions.GetKey(keyTermTransformatorCache));
                var termSetTerms = isSP2010 ? TermTransformator.CallTaxonomyWebServiceFindTermSetId(context, sourceSspId, termSetId) 
                    : TermTransformator.GetAllTermsFromTermSet(termSetId, context);

                foreach (var termSetTerm in termSetTerms)
                {
                    var term = termSetTerm.Value;
                    term.IsSourceTerm = isSourceTerm;
                    termCache.Add(termSetTerm.Key, term);
                }
                Store.Set(StoreOptions.GetKey(keyTermTransformatorCache), termCache, StoreOptions.EntryOptions);
            }
        }

        /// <summary>
        /// Get Term from Term Cache
        /// </summary>
        /// <param name="context"></param>
        /// <param name="termId"></param>
        /// <returns></returns>
        public TermData GetTransformTermCacheTermById(ClientContext context, Guid termId)
        {
            var termCache = Store.GetAndInitialize<Dictionary<Guid, TermData>>(StoreOptions.GetKey(keyTermTransformatorCache));
            termCache.TryGetValue(termId, out TermData termInfoFromCache);
            if (termInfoFromCache != default)
            {
                return termInfoFromCache;
            }

            return default;
        }

        /// <summary>
        /// Search Cached term by name
        /// </summary>
        /// <param name="context"></param>
        /// <param name="termLabel"></param>
        /// <param name="termPath"></param>
        /// <returns></returns>
        public List<TermData> GetTransformTermCacheTermByName(ClientContext context, string termLabel = "", string termPath = "")
        {
            var termCache = Store.GetAndInitialize<Dictionary<Guid, TermData>>(StoreOptions.GetKey(keyTermTransformatorCache));

            var candidateTerms = termCache.Where(o => o.Value.TermLabel == termLabel || o.Value.TermPath == termPath);
            if (candidateTerms.Any())
            {
                return candidateTerms.Select(o => o.Value).ToList();
            }
            
            return default;
        }

        /// <summary>
        /// Get Term by Term Set Id
        /// </summary>
        /// <param name="context"></param>
        /// <param name="termSetId"></param>
        /// <returns></returns>
        public Dictionary<Guid, TermData> GetTransformTermCacheTermsByTermSet(ClientContext context, Guid termSetId)
        {
            var termCache = Store.GetAndInitialize<Dictionary<Guid, TermData>>(StoreOptions.GetKey(keyTermTransformatorCache));

            var candidateTerms = termCache.Where(o => o.Value.TermSetId == termSetId);
            if (candidateTerms.Any())
            {
                return candidateTerms.ToDictionary(o=>o.Key, o=>o.Value);
            }

            return default;
        }

        #endregion

        #endregion

        #region Last used transformator
        /// <summary>
        /// Caches the last used page transformator instance, needed to postpone log writing when transforming multiple pages
        /// </summary>
        /// <param name="transformator"></param>
        public void SetLastUsedTransformator(BasePageTransformator transformator)
        {
            this.lastUsedTransformator = transformator;
        }

        /// <summary>
        /// Gets the last used page transformator instance
        /// </summary>
        /// <returns></returns>
        public BasePageTransformator GetLastUsedTransformator()
        {
            return this.lastUsedTransformator;
        }
        #endregion

        #region URL rewriting
        /// <summary>
        /// Returns a list of url mappings
        /// </summary>
        /// <param name="urlMappingFile">File with url mappings</param>
        /// <param name="logObservers">Attached list of log observers</param>
        /// <returns>List of url mappings</returns>
        public List<UrlMapping> GetUrlMapping(string urlMappingFile, IList<ILogObserver> logObservers = null)
        {
            var urlMapping = Store.GetAndInitialize<List<UrlMapping>>(StoreOptions.GetKey(keyUrlMapping));
            if (urlMapping != null && urlMapping.Count > 0)
            {
                return urlMapping;
            }

            FileManager fileManager = new FileManager(logObservers);
            urlMapping = fileManager.LoadUrlMappingFile(urlMappingFile);
            Store.Set(StoreOptions.GetKey(keyUrlMapping), urlMapping, StoreOptions.EntryOptions);

            return urlMapping;
        }
        #endregion

        #region Get User Mapping
        /// <summary>
        /// Gets the list of user mappings, if first time file will be laoded
        /// </summary>
        /// <param name="userMappingFile">File with the user mappings</param>
        /// <param name="logObservers">Attached list of log observers</param>
        /// <returns>List of user mappings</returns>
        public List<UserMappingEntity> GetUserMapping(string userMappingFile, IList<ILogObserver> logObservers = null)
        {
            var userMappings = Store.GetAndInitialize<List<UserMappingEntity>>(StoreOptions.GetKey(keyUserMappings));
            if (userMappings != null && userMappings.Count > 0)
            {
                return userMappings;
            }

            FileManager fileManager = new FileManager(logObservers);
            userMappings = fileManager.LoadUserMappingFile(userMappingFile);

            Store.Set(StoreOptions.GetKey(keyUserMappings), userMappings, StoreOptions.EntryOptions);

            return userMappings;
        }
        #endregion

        /// <summary>
        /// Returns a cached list of mapped items
        /// </summary>
        /// <param name="mappingFile"></param>
        /// <param name="logObservers"></param>
        /// <returns></returns>
        public List<TermMapping> GetTermMapping(string mappingFile, IList<ILogObserver> logObservers = null)
        {
            var termMapping = Store.GetAndInitialize<List<TermMapping>>(StoreOptions.GetKey(keyTermMappings));
            if (termMapping != null && termMapping.Count > 0)
            {
                return termMapping;
            }

            FileManager fileManager = new FileManager(logObservers);
            termMapping = fileManager.LoadTermMappingFile(mappingFile);
            Store.Set(StoreOptions.GetKey(keyTermMappings), termMapping, StoreOptions.EntryOptions);

            return termMapping;
        }

        #region Helper methods

        private static bool IsGroup(string loginName)
        {
            // Possible input
            // c:0t.c|tenant|b0f984d9-e9d5-432a-bec9-896f910254ba (group in SPO)
            // S-5-1-76-1812374880-3438888550-261701130-6117 (group in SPO on-premises)

            if (loginName.StartsWith("c:0t.c|tenant|") || IsSID(loginName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsSID(string loginName)
        {
            return Regex.IsMatch(loginName.ToUpper(), @"^S-\d-\d+-(\d+-){1,14}\d+$");
        }

        private static string PostsTranslation(int lcid)
        {
            // See https://capacreative.co.uk/resources/reference-sharepoint-online-languages-ids/ for list of language id's
            return lcid switch
            {
                1033 => "Posts",
                1027 => "Publicacions",
                1029 => "Prispevky",
                1030 => "Blogmeddelelser",
                1031 => "Beitraege",
                1035 => "Viestit",
                1036 => "Billets",
                1038 => "Bejegyzesek",
                1040 => "Post",
                1043 => "Berichten",
                1044 => "Innlegg",
                1046 => "Postagens",
                1050 => "Clanci",
                1053 => "Anslag",
                1057 => "Pos",
                1060 => "Obvestila",
                1061 => "Postitused",
                1069 => "Blog-sarrerak",
                1086 => "Catatan",
                1106 => "Postiadau",
                1110 => "Mensaxes",
                2070 => "Artigos",
                2074 => "ObjavljenePoruke",
                2108 => "Poist",
                3082 => "EntradasDeBlog",
                5146 => "Objave",
                9424 => "ObjavljenePoruke",
                _ => "Posts",
            };
        }

        private static string Sha256(string randomString)
        {
            using (SHA256CryptoServiceProvider provider = new SHA256CryptoServiceProvider())
            {
                byte[] hash = provider.ComputeHash(Encoding.Unicode.GetBytes(JsonSerializer.Serialize(randomString)));
                string componentKeyToCache = BitConverter.ToString(hash).Replace("-", "");
                return componentKeyToCache;
            }
        }

        private void AddFieldRef(Provisioning.Model.FieldRefCollection fieldRefs, Guid Id, string name)
        {
            if (fieldRefs.Where(p => p.Id.Equals(Id)).FirstOrDefault() == null)
            {
                fieldRefs.Add(new Provisioning.Model.FieldRef(name) { Id = Id });
            }
        }

        private bool IsBuiltInField(bool isBlog, Guid fieldId)
        {
            if (Enums.BuiltInFieldId.Contains(fieldId))
            {
                if (isBlog)
                {
                    // Always allow the PostCategory field
                    if (fieldId.Equals(Constants.PostCategory))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
