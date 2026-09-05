using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Lists;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.UserResources;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using CoreList = PnP.Core.Model.SharePoint.IList;
using FieldModel = PnP.Core.Provisioning.Model.Field;
using CoreListExperience = PnP.Core.Model.SharePoint.ListExperience;
using CoreListReadingDirection = PnP.Core.Model.SharePoint.ListReadingDirection;
using ListExperienceModel = PnP.Core.Provisioning.Model.ListExperience;
using ListReadingDirectionModel = PnP.Core.Provisioning.Model.ListReadingDirection;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    using UrlUtility = PnP.Core.Provisioning.Utilities.UrlUtility;

    /// <summary>
    /// Provisions and extracts the <c>&lt;pnp:ListInstance&gt;</c> elements of a template - the
    /// lists and libraries themselves, their columns, views, folders, content type bindings and
    /// settings.
    /// </summary>
    internal partial class ObjectListInstance : ObjectHandlerBase
    {
        private readonly FieldAndListProvisioningStepHelper.Step step;

        public ObjectListInstance(FieldAndListProvisioningStepHelper.Step step)
        {
            this.step = step;
        }

        public override string Name => $"List instances ({step})";

        public override string InternalName => "ListInstances";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.Lists.Any();
            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            _willExtract ??= true;
            return _willExtract.Value;
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template,
            TokenParser parser, ApplyConfiguration configuration)
        {
            if (!template.Lists.Any())
            {
                return parser;
            }

            IWeb web = context.Web;
            await web.LoadAsync(
                w => w.ServerRelativeUrl,
                w => w.Url,
                w => w.SupportedUILanguageIds,
                w => w.Lists.QueryProperties(l => l.Id, l => l.Title, l => l.TemplateType,
                    l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl))).ConfigureAwait(false);

            string webUrl = web.ServerRelativeUrl.TrimEnd('/');
            bool isNoScriptSite = await web.IsNoScriptSiteAsync().ConfigureAwait(false);

            List<CoreList> existingLists = web.Lists.AsRequested().ToList();
            var processedLists = new List<ListInfo>();

            int currentListIndex = 0;
            int total = template.Lists.Count;

            HashSet<string> availableContentTypes = null;

            foreach (ListInstance templateList in template.Lists)
            {
                var listParser = (TokenParser)parser.Clone();

                templateList.Url = listParser.ParseString(templateList.Url);
                currentListIndex++;
                WriteSubProgress("List", templateList.Title, currentListIndex, total);

                string wantedTitle = listParser.ParseString(templateList.Title);
                string wantedUrl = UrlUtility.Combine(webUrl, templateList.Url);

                CoreList existing = existingLists.FirstOrDefault(l =>
                    string.Equals(l.Title, wantedTitle, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(l.RootFolder.ServerRelativeUrl, wantedUrl, StringComparison.OrdinalIgnoreCase));

                try
                {
                    availableContentTypes = await CheckContentTypesAsync(context, template, templateList,
                        availableContentTypes).ConfigureAwait(false);

                    if (existing == null)
                    {
                        CoreList created = await CreateListAsync(context, templateList, listParser, isNoScriptSite).ConfigureAwait(false);
                        if (created == null)
                        {
                            continue;
                        }

                        processedLists.Add(new ListInfo { SiteList = created, TemplateList = templateList, TokenParser = listParser });

                        await RegisterListTokensAsync(context, created, webUrl, web.SupportedUILanguageIds, parser, listParser).ConfigureAwait(false);

                        existingLists.Add(created);
                    }
                    else
                    {
                        CoreList updated = await UpdateListAsync(context, existing, templateList, listParser, webUrl, isNoScriptSite).ConfigureAwait(false);
                        if (updated != null)
                        {
                            processedLists.Add(new ListInfo { SiteList = updated, TemplateList = templateList, TokenParser = listParser });
                        }
                    }
                }
                catch (Exception ex)
                {
                    string warning = $"The list '{wantedTitle}' could not be provisioned, so it and " +
                        $"anything depending on it were skipped: {ErrorText.Describe(ex)}";

                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }

            foreach (ListInfo listInfo in processedLists)
            {
                await ProcessFieldRefsAsync(context, template, listInfo, parser).ConfigureAwait(false);
            }

            foreach (ListInfo listInfo in processedLists)
            {
                await ProcessFieldsAsync(context, listInfo, parser).ConfigureAwait(false);
            }

            foreach (ListInfo listInfo in processedLists)
            {
                await ProcessAudienceTargetingAsync(context, listInfo).ConfigureAwait(false);
            }

            if (step == FieldAndListProvisioningStepHelper.Step.ListSettings)
            {
                await parser.RebuildListTokensAsync(context).ConfigureAwait(false);

                foreach (ListInfo listInfo in processedLists)
                {
                    await ProcessFieldDefaultsAsync(context, listInfo).ConfigureAwait(false);
                }

                foreach (ListInfo listInfo in processedLists)
                {
                    await ProcessViewsAsync(context, listInfo).ConfigureAwait(false);
                }

                await parser.RebuildListTokensAsync(context).ConfigureAwait(false);

                foreach (ListInfo listInfo in processedLists)
                {
                    await ProcessFoldersAsync(context, listInfo).ConfigureAwait(false);
                }

                foreach (ListInfo listInfo in processedLists)
                {
                    await ProcessDefaultColumnValuesAsync(context, listInfo, parser).ConfigureAwait(false);
                }

                foreach (ListInfo listInfo in processedLists)
                {
                    await ProcessIrmSettingsAsync(context, listInfo).ConfigureAwait(false);
                }

                foreach (ListInfo listInfo in processedLists)
                {
                    if (listInfo.SiteList.OnQuickLaunch != listInfo.TemplateList.OnQuickLaunch)
                    {
                        listInfo.SiteList.OnQuickLaunch = listInfo.TemplateList.OnQuickLaunch;
                        await listInfo.SiteList.UpdateAsync().ConfigureAwait(false);
                    }
                }

                foreach (ListInfo listInfo in processedLists)
                {
                    await ProcessPropertyBagEntriesAsync(context, listInfo).ConfigureAwait(false);
                }
            }

            WriteMessage("Done processing lists", ProvisioningMessageType.Completed);

            return parser;
        }

        #endregion

        #region Create and update

        /// <summary>
        /// Creates a list and applies every setting the template carries.
        /// </summary>
        private async Task<CoreList> CreateListAsync(PnPContext context, ListInstance templateList,
            TokenParser parser, bool isNoScriptSite)
        {
            context.Logger?.LogDebug("{Source}: creating list '{Title}'.", Constants.LOGGING_SOURCE, templateList.Title);

            CoreList createdList;

            if (string.Equals(templateList.Url, "SiteAssets", StringComparison.OrdinalIgnoreCase)
                && templateList.TemplateType == (int)ListTemplateType.DocumentLibrary)
            {
                createdList = await context.Web.Lists.EnsureSiteAssetsLibraryAsync(
                    l => l.Id, l => l.Title, l => l.Description, l => l.TemplateType).ConfigureAwait(false);
            }
            else
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                CreatedListInfo info = await CsomRequestSender.SendAsync(context, new CreateListRequest(
                    siteId, webId,
                    parser.ParseString(templateList.Title),
                    parser.ParseString(templateList.Url),
                    parser.ParseString(templateList.Description),
                    templateList.TemplateType,
                    templateList.TemplateFeatureID,
                    templateList.OnQuickLaunch)).ConfigureAwait(false);

                if (info == null || info.Id == Guid.Empty)
                {
                    string message = $"The list '{templateList.Title}' was not created - the server returned no list id.";
                    context.Logger?.LogError("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    throw new Exception(message);
                }

                createdList = await context.Web.Lists.GetByIdAsync(info.Id, ListLoadProperties).ConfigureAwait(false);
            }

            await ApplySettingsAsync(context, createdList, templateList, parser, isNew: true).ConfigureAwait(false);
            await LocalizeListAsync(context, createdList, templateList, parser).ConfigureAwait(false);
            await RegisterListFieldTokensAsync(context, createdList, parser).ConfigureAwait(false);

            if (createdList.TemplateType != ListTemplateType.Survey)
            {
                await ConfigureContentTypesAsync(context, createdList, templateList, isNewList: true, parser).ConfigureAwait(false);
            }

            await ApplyCustomActionsAsync(context, createdList, templateList, parser, isNoScriptSite).ConfigureAwait(false);
            await ApplyWebhooksAsync(context, createdList, templateList, parser).ConfigureAwait(false);

            await SecurityUtilities.ApplyAsync(context, createdList, templateList.Security, parser,
                $"list '{createdList.Title}'", m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);

            return createdList;
        }

        /// <summary>
        /// Brings an existing list in line with the template.
        /// </summary>
        /// <returns>The list, or null when it exists but is of a different type</returns>
        private async Task<CoreList> UpdateListAsync(PnPContext context, CoreList existingList, ListInstance templateList,
            TokenParser parser, string webUrl, bool isNoScriptSite)
        {
            context.Logger?.LogDebug("{Source}: updating list '{Title}'.", Constants.LOGGING_SOURCE, templateList.Title);

            CoreList list = await context.Web.Lists.GetByIdAsync(existingList.Id, ListLoadProperties).ConfigureAwait(false);

            if ((int)list.TemplateType != templateList.TemplateType)
            {
                string warning = $"The list '{templateList.Title}' ({templateList.Url}) exists but is of a different type, so it was skipped.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return null;
            }

            await RegisterListFieldTokensAsync(context, list, parser).ConfigureAwait(false);

            string wantedUrl = UrlUtility.Combine(webUrl, templateList.Url);
            if (!string.Equals(list.RootFolder.ServerRelativeUrl, wantedUrl, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await list.RootFolder.MoveToAsync(wantedUrl).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string warning = $"The list '{list.Title}' could not be moved to '{wantedUrl}': {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }

            await ApplySettingsAsync(context, list, templateList, parser, isNew: false).ConfigureAwait(false);
            await LocalizeListAsync(context, list, templateList, parser).ConfigureAwait(false);

            if (list.ContentTypesEnabled)
            {
                await ConfigureContentTypesAsync(context, list, templateList, isNewList: false, parser).ConfigureAwait(false);
            }

            await ApplyCustomActionsAsync(context, list, templateList, parser, isNoScriptSite).ConfigureAwait(false);
            await ApplyWebhooksAsync(context, list, templateList, parser).ConfigureAwait(false);

            await SecurityUtilities.ApplyAsync(context, list, templateList.Security, parser,
                $"list '{list.Title}'", m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);

            return list;
        }

        /// <summary>
        /// Everything the list schema expresses as a plain property.
        /// </summary>
        private async Task ApplySettingsAsync(PnPContext context, CoreList list, ListInstance templateList,
            TokenParser parser, bool isNew)
        {
            int baseTemplate = (int)list.TemplateType;
            bool dirty = false;

            string wantedTitle = parser.ParseString(templateList.Title);
            if (!string.IsNullOrEmpty(wantedTitle))
            {
                dirty |= Set(list.Title, wantedTitle, v => list.Title = v);
            }

            dirty |= Set(list.Description, parser.ParseString(templateList.Description) ?? string.Empty, v => list.Description = v);
            dirty |= Set(list.Hidden, templateList.Hidden, v => list.Hidden = v);
            dirty |= Set(list.OnQuickLaunch, templateList.OnQuickLaunch, v => list.OnQuickLaunch = v);
            dirty |= Set(list.NoCrawl, templateList.NoCrawl, v => list.NoCrawl = v);
            dirty |= Set(list.IsApplicationList, templateList.IsApplicationList, v => list.IsApplicationList = v);
            dirty |= Set(list.IrmExpire, templateList.IrmExpire, v => list.IrmExpire = v);
            dirty |= Set(list.IrmReject, templateList.IrmReject, v => list.IrmReject = v);
            dirty |= Set(list.EnableModeration, templateList.EnableModeration, v => list.EnableModeration = v);
            dirty |= Set(list.ForceCheckout, templateList.ForceCheckout, v => list.ForceCheckout = v);

            dirty |= SetIfNotEmpty(list.DocumentTemplate, parser.ParseString(templateList.DocumentTemplate), v => list.DocumentTemplate = v);
            dirty |= SetIfNotEmpty(list.DefaultDisplayFormUrl, parser.ParseString(templateList.DefaultDisplayFormUrl), v => list.DefaultDisplayFormUrl = v);
            dirty |= SetIfNotEmpty(list.DefaultEditFormUrl, parser.ParseString(templateList.DefaultEditFormUrl), v => list.DefaultEditFormUrl = v);
            dirty |= SetIfNotEmpty(list.DefaultNewFormUrl, parser.ParseString(templateList.DefaultNewFormUrl), v => list.DefaultNewFormUrl = v);
            dirty |= SetIfNotEmpty(list.ImageUrl, parser.ParseString(templateList.ImageUrl), v => list.ImageUrl = v);
            dirty |= SetIfNotEmpty(list.ValidationFormula, parser.ParseString(templateList.ValidationFormula), v => list.ValidationFormula = v);
            dirty |= SetIfNotEmpty(list.ValidationMessage, parser.ParseString(templateList.ValidationMessage), v => list.ValidationMessage = v);

            dirty |= Set(list.Direction, ToCoreDirection(templateList.Direction), v => list.Direction = v);
            dirty |= Set(list.ListExperience, ToCoreListExperience(templateList.ListExperience), v => list.ListExperience = v);

            dirty |= Set(list.ReadSecurity, templateList.ReadSecurity == 0 ? 1 : templateList.ReadSecurity, v => list.ReadSecurity = v);
            dirty |= Set(list.WriteSecurity, templateList.WriteSecurity == 0 ? 1 : templateList.WriteSecurity, v => list.WriteSecurity = v);

            if (baseTemplate != (int)ListTemplateType.Survey
                && baseTemplate != (int)ListTemplateType.DocumentLibrary
                && baseTemplate != (int)ListTemplateType.PictureLibrary
                && baseTemplate != 850)
            {
                dirty |= Set(list.EnableAttachments, templateList.EnableAttachments, v => list.EnableAttachments = v);
            }

            if (baseTemplate != (int)ListTemplateType.DiscussionBoard && baseTemplate != (int)ListTemplateType.Events)
            {
                dirty |= Set(list.EnableFolderCreation, templateList.EnableFolderCreation, v => list.EnableFolderCreation = v);
            }

            if (baseTemplate != (int)ListTemplateType.Survey)
            {
                dirty |= Set(list.ContentTypesEnabled, templateList.ContentTypesEnabled, v => list.ContentTypesEnabled = v);
                dirty |= ApplyVersioning(list, templateList);
            }

            if (dirty)
            {
                await list.UpdateAsync().ConfigureAwait(false);
            }

            if (isNew)
            {
                await ApplyIrmSettingsOnCreateAsync(context, list, templateList, parser).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Versioning, which has to be written as a group because the limits are only accepted while
        /// the corresponding switch is on.
        /// </summary>
        private static bool ApplyVersioning(CoreList list, ListInstance templateList)
        {
            bool dirty = Set(list.EnableVersioning, templateList.EnableVersioning, v => list.EnableVersioning = v);

            if (!templateList.EnableVersioning)
            {
                return dirty;
            }

            dirty |= Set(list.MaxVersionLimit, templateList.MaxVersionLimit > 0 ? templateList.MaxVersionLimit : 500,
                v => list.MaxVersionLimit = v);

            if (list.BaseType != ListBaseType.DocumentLibrary)
            {
                return dirty;
            }

            dirty |= Set(list.EnableMinorVersions, templateList.EnableMinorVersions, v => list.EnableMinorVersions = v);

            var wantedVisibility = (DraftVisibilityType)templateList.DraftVersionVisibility;

            if (wantedVisibility != DraftVisibilityType.Approver || templateList.EnableModeration)
            {
                dirty |= Set(list.DraftVersionVisibility, wantedVisibility, v => list.DraftVersionVisibility = v);
            }

            if (templateList.EnableMinorVersions)
            {
                dirty |= Set(list.MinorVersionLimit, templateList.MinorVersionLimit > 0 ? templateList.MinorVersionLimit : 500,
                    v => list.MinorVersionLimit = v);
            }

            return dirty;
        }

        private async Task ApplyIrmSettingsOnCreateAsync(PnPContext context, CoreList list, ListInstance templateList, TokenParser parser)
        {
            if (templateList.IRMSettings == null
                || !templateList.IRMSettings.Enabled
                || list.TemplateType == ListTemplateType.PictureLibrary)
            {
                return;
            }

            try
            {
                list.IrmEnabled = true;
                await list.UpdateAsync().ConfigureAwait(false);
                await WriteIrmSettingsAsync(context, list, templateList, parser).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"Information rights management could not be configured on list '{list.Title}': {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        private async Task ProcessIrmSettingsAsync(PnPContext context, ListInfo listInfo)
        {
            IRMSettings settings = listInfo.TemplateList.IRMSettings;
            CoreList list = listInfo.SiteList;

            if (settings == null || !settings.Enabled || list.TemplateType == ListTemplateType.PictureLibrary)
            {
                return;
            }

            try
            {
                if (!list.IrmEnabled)
                {
                    list.IrmEnabled = true;
                    await list.UpdateAsync().ConfigureAwait(false);
                }

                await WriteIrmSettingsAsync(context, list, listInfo.TemplateList, listInfo.TokenParser).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"Information rights management could not be configured on list '{list.Title}': {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        /// <summary>
        /// Writes the detailed information rights management settings of a library.
        /// </summary>
        private static async Task WriteIrmSettingsAsync(PnPContext context, CoreList list, ListInstance templateList, TokenParser parser)
        {
            IRMSettings wanted = templateList.IRMSettings;

            var settings = new Dictionary<string, object>
            {
                ["AllowPrint"] = wanted.AllowPrint,
                ["AllowScript"] = wanted.AllowScript,
                ["AllowWriteCopy"] = wanted.AllowWriteCopy,
                ["DisableDocumentBrowserView"] = wanted.DisableDocumentBrowserView,
                ["DocumentAccessExpireDays"] = wanted.DocumentAccessExpireDays,
                ["EnableDocumentAccessExpire"] = wanted.EnableDocumentAccessExpire,
                ["EnableDocumentBrowserPublishingView"] = wanted.EnableDocumentBrowserPublishingView,
                ["EnableGroupProtection"] = wanted.EnableGroupProtection,
                ["EnableLicenseCacheExpire"] = wanted.EnableLicenseCacheExpire,
                ["LicenseCacheExpireDays"] = wanted.LicenseCacheExpireDays,
                ["GroupName"] = parser.ParseString(wanted.GroupName) ?? string.Empty,
                ["PolicyDescription"] = parser.ParseString(wanted.PolicyDescription) ?? string.Empty,
                ["PolicyTitle"] = parser.ParseString(wanted.PolicyTitle) ?? string.Empty,
            };

            if (wanted.DocumentLibraryProtectionExpiresInDays > 0)
            {
                settings["DocumentLibraryProtectionExpireDate"] =
                    DateTime.Now.AddDays(wanted.DocumentLibraryProtectionExpiresInDays).ToString("o", CultureInfo.InvariantCulture);
            }

            string body = JsonSerializer.Serialize(settings);

            await context.Web.ExecuteRequestAsync(new ApiRequest(
                new HttpMethod("MERGE"),
                ApiRequestType.SPORest,
                $"_api/web/lists(guid'{list.Id}')/InformationRightsManagementSettings",
                body,
                new Dictionary<string, string> { { "IF-MATCH", "*" } })).ConfigureAwait(false);
        }

        #endregion

        #region Content types

        /// <summary>
        /// Fails early when a list binds a content type that neither the template nor the site
        /// provides.
        /// </summary>
        private static async Task<HashSet<string>> CheckContentTypesAsync(PnPContext context,
            ProvisioningTemplate template, ListInstance templateList, HashSet<string> availableContentTypes)
        {
            if (!templateList.ContentTypesEnabled || templateList.ContentTypeBindings.Count == 0)
            {
                return availableContentTypes;
            }

            if (availableContentTypes == null)
            {
                await context.Web.LoadAsync(w => w.AvailableContentTypes.QueryProperties(ct => ct.StringId)).ConfigureAwait(false);

                availableContentTypes = new HashSet<string>(
                    context.Web.AvailableContentTypes.AsRequested().Select(ct => ct.StringId),
                    StringComparer.OrdinalIgnoreCase);
            }

            foreach (ContentTypeBinding binding in templateList.ContentTypeBindings)
            {
                bool found = availableContentTypes.Contains(binding.ContentTypeId)
                    || template.ContentTypes.Any(t => string.Equals(t.Id, binding.ContentTypeId, StringComparison.OrdinalIgnoreCase));

                if (!found)
                {
                    throw new Exception(
                        $"The list '{templateList.Title}' is bound to content type {binding.ContentTypeId}, " +
                        $"which is available neither on this site ({availableContentTypes.Count} content type(s) " +
                        $"available) nor in the template ({template.ContentTypes.Count} content type(s)).");
                }
            }

            return availableContentTypes;
        }

        /// <summary>
        /// Binds the template's content types to the list and sets their order, default and
        /// visibility.
        /// </summary>
        private async Task ConfigureContentTypesAsync(PnPContext context, CoreList list, ListInstance templateList,
            bool isNewList, TokenParser parser)
        {
            if (templateList.ContentTypeBindings.Count == 0)
            {
                return;
            }

            await list.LoadAsync(l => l.ContentTypes.QueryProperties(ct => ct.Id, ct => ct.StringId, ct => ct.Name)).ConfigureAwait(false);
            List<IContentType> listContentTypes = list.ContentTypes.AsRequested().ToList();

            if (!isNewList && templateList.RemoveExistingContentTypes && listContentTypes.Count > 0)
            {
                string warning = $"The template asks to remove the existing content types of the list at '{list.RootFolder.ServerRelativeUrl}', " +
                    "but the list already exists. Its content types may be in use by items, so they were left in place.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }

            List<IContentType> toRemove = isNewList && templateList.RemoveExistingContentTypes
                ? listContentTypes.ToList()
                : new List<IContentType>();

            IContentType defaultContentType = null;
            var visible = new List<IContentType>();
            var hidden = new List<IContentType>();

            foreach (ContentTypeBinding binding in templateList.ContentTypeBindings)
            {
                IContentType onList = BestMatch(listContentTypes, binding.ContentTypeId);

                if (binding.Remove)
                {
                    if (onList != null)
                    {
                        await onList.DeleteAsync().ConfigureAwait(false);
                        listContentTypes.Remove(onList);
                        toRemove.Remove(onList);
                    }

                    continue;
                }

                if (onList == null)
                {
                    try
                    {
                        onList = await list.ContentTypes.AddAvailableContentTypeAsync(binding.ContentTypeId).ConfigureAwait(false);
                        listContentTypes.Add(onList);
                    }
                    catch (Exception ex)
                    {
                        string warning = $"Content type '{binding.ContentTypeId}' could not be added to list '{list.Title}': {ErrorText.Describe(ex)}";
                        context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                        WriteMessage(warning, ProvisioningMessageType.Warning);
                        continue;
                    }
                }
                else
                {
                    toRemove.Remove(onList);
                }

                parser.AddToken(new ListContentTypeIdToken(context, list.Title, onList));

                if (binding.Default && defaultContentType == null)
                {
                    defaultContentType = onList;
                }

                (binding.Hidden ? hidden : visible).Add(onList);
            }

            foreach (IContentType contentType in toRemove)
            {
                try
                {
                    await contentType.DeleteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    context.Logger?.LogWarning(ex, "{Source}: content type '{Name}' could not be removed from list '{List}'.",
                        Constants.LOGGING_SOURCE, contentType.Name, list.Title);
                }
            }

            await ApplyContentTypeOrderAsync(context, list, defaultContentType, visible, hidden, listContentTypes).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the list's unique content type order, which is also how "default" and "hidden" are
        /// expressed.
        /// </summary>
        private async Task ApplyContentTypeOrderAsync(PnPContext context, CoreList list, IContentType defaultContentType,
            List<IContentType> visible, List<IContentType> hidden, List<IContentType> listContentTypes)
        {
            if (step != FieldAndListProvisioningStepHelper.Step.ListSettings)
            {
                return;
            }

            if (visible.Count == 0 && hidden.Count == 0)
            {
                return;
            }

            var order = new List<string>();

            if (defaultContentType != null && visible.Any(ct => ct.StringId == defaultContentType.StringId))
            {
                order.Add(defaultContentType.StringId);
            }

            foreach (IContentType contentType in visible)
            {
                if (!order.Contains(contentType.StringId))
                {
                    order.Add(contentType.StringId);
                }
            }

            foreach (IContentType contentType in listContentTypes)
            {
                if (!order.Contains(contentType.StringId)
                    && !hidden.Any(h => h.StringId == contentType.StringId))
                {
                    order.Add(contentType.StringId);
                }
            }

            if (order.Count == 0)
            {
                return;
            }

            try
            {
                await list.ReorderContentTypesAsync(order).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The content type order of list '{list.Title}' could not be set: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        /// <summary>
        /// Finds the list content type derived from a given site content type.
        /// </summary>
        private static IContentType BestMatch(IEnumerable<IContentType> contentTypes, string contentTypeId)
        {
            return contentTypes
                .Where(ct => ct.StringId.StartsWith(contentTypeId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(ct => ct.StringId.Length)
                .FirstOrDefault();
        }

        #endregion

        #region Custom actions and webhooks

        private async Task ApplyCustomActionsAsync(PnPContext context, CoreList list, ListInstance templateList,
            TokenParser parser, bool isNoScriptSite)
        {
            if (templateList.UserCustomActions == null || templateList.UserCustomActions.Count == 0)
            {
                return;
            }

            if (isNoScriptSite)
            {
                string warning = $"This is a NoScript site, so the custom actions of list '{list.Title}' were skipped.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return;
            }

            await list.LoadAsync(l => l.UserCustomActions.QueryProperties(a => a.Id, a => a.Name, a => a.Title,
                a => a.Description, a => a.Location, a => a.Sequence, a => a.Url, a => a.ScriptBlock,
                a => a.ScriptSrc, a => a.ImageUrl, a => a.Group, a => a.CommandUIExtension,
                a => a.ClientSideComponentId, a => a.ClientSideComponentProperties, a => a.RegistrationId,
                a => a.RegistrationType, a => a.Rights)).ConfigureAwait(false);

            List<IUserCustomAction> existingActions = list.UserCustomActions.AsRequested().ToList();

            foreach (CustomAction customAction in templateList.UserCustomActions)
            {
                IUserCustomAction existing = existingActions
                    .FirstOrDefault(a => string.Equals(a.Name, customAction.Name, StringComparison.Ordinal));

                if (existing != null && customAction.Remove)
                {
                    await existing.DeleteAsync().ConfigureAwait(false);
                    existingActions.Remove(existing);
                    continue;
                }

                IUserCustomAction target;

                if (existing == null)
                {
                    if (customAction.Remove || !customAction.Enabled)
                    {
                        continue;
                    }

                    target = await list.UserCustomActions.AddAsync(
                        ObjectCustomActions.BuildOptions(customAction, parser)).ConfigureAwait(false);
                    existingActions.Add(target);
                }
                else
                {
                    UserCustomActionRegistrationType originalType = customAction.RegistrationType;
                    string originalId = customAction.RegistrationId;

                    customAction.RegistrationType = UserCustomActionRegistrationType.List;
                    customAction.RegistrationId = list.Id.ToString("B").ToUpperInvariant();

                    try
                    {
                        await ObjectCustomActions.UpdateAsync(context, existing, customAction, parser).ConfigureAwait(false);
                    }
                    finally
                    {
                        customAction.RegistrationType = originalType;
                        customAction.RegistrationId = originalId;
                    }

                    target = existing;
                }

                await LocalizeCustomActionAsync(context, list, target, customAction, parser).ConfigureAwait(false);
            }
        }

        private async Task LocalizeCustomActionAsync(PnPContext context, CoreList list, IUserCustomAction customActionOnSite,
            CustomAction customAction, TokenParser parser)
        {
            bool localizesTitle = UserResources.ContainsResourceToken(customAction.Title);
            bool localizesDescription = UserResources.ContainsResourceToken(customAction.Description);

            if (!localizesTitle && !localizesDescription)
            {
                return;
            }

            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

            UserResourcePath PathFor(string property) =>
                UserResourcePath.ForListUserCustomAction(siteId, webId, list.Id, customActionOnSite.Id, property);

            if (localizesTitle)
            {
                await UserResources.TrySetAsync(context, PathFor(ResourceProperty.Title), customAction.Title, parser,
                    $"the title of custom action '{customAction.Name}' on list '{list.Title}'",
                    m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);
            }

            if (localizesDescription)
            {
                await UserResources.TrySetAsync(context, PathFor(ResourceProperty.Description), customAction.Description, parser,
                    $"the description of custom action '{customAction.Name}' on list '{list.Title}'",
                    m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);
            }
        }

        private async Task ApplyWebhooksAsync(PnPContext context, CoreList list, ListInstance templateList, TokenParser parser)
        {
            if (step != FieldAndListProvisioningStepHelper.Step.ListSettings)
            {
                return;
            }

            if (templateList.Webhooks == null || templateList.Webhooks.Count == 0)
            {
                return;
            }

            List<IListSubscription> existing;
            try
            {
                await list.LoadAsync(l => l.Webhooks.QueryProperties(w => w.Id, w => w.NotificationUrl, w => w.ExpirationDateTime))
                    .ConfigureAwait(false);
                existing = list.Webhooks.AsRequested().ToList();
            }
            catch (Exception ex)
            {
                context.Logger?.LogWarning(ex, "{Source}: the webhooks of list '{List}' could not be read.",
                    Constants.LOGGING_SOURCE, list.Title);
                return;
            }

            foreach (Webhook webhook in templateList.Webhooks)
            {
                string notificationUrl = parser.ParseString(webhook.ServerNotificationUrl);

                if (webhook.ExpiresInDays <= 0)
                {
                    string warning = $"The webhook '{notificationUrl}' on list '{list.Title}' has already expired, so it was skipped.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    continue;
                }

                try
                {
                    IListSubscription match = existing.FirstOrDefault(
                        w => string.Equals(w.NotificationUrl, notificationUrl, StringComparison.OrdinalIgnoreCase));

                    if (match != null)
                    {
                        match.ExpirationDateTime = DateTime.Now.AddDays(webhook.ExpiresInDays);
                        await match.UpdateAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        await list.Webhooks.AddAsync(notificationUrl, DateTime.Now.AddDays(webhook.ExpiresInDays)).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    string warning = $"The webhook '{notificationUrl}' on list '{list.Title}' could not be registered: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// The properties every code path below reads. Loaded in one query because PnP Core
        /// materialises a model's properties once - a property left out here throws when read.
        /// </summary>
        private static readonly System.Linq.Expressions.Expression<Func<CoreList, object>>[] ListLoadProperties =
        {
            l => l.Id,
            l => l.Title,
            l => l.Description,
            l => l.TemplateType,
            l => l.BaseType,
            l => l.Hidden,
            l => l.OnQuickLaunch,
            l => l.NoCrawl,
            l => l.ContentTypesEnabled,
            l => l.EnableAttachments,
            l => l.EnableFolderCreation,
            l => l.EnableModeration,
            l => l.EnableVersioning,
            l => l.EnableMinorVersions,
            l => l.DraftVersionVisibility,
            l => l.MaxVersionLimit,
            l => l.MinorVersionLimit,
            l => l.ForceCheckout,
            l => l.DocumentTemplate,
            l => l.DefaultDisplayFormUrl,
            l => l.DefaultEditFormUrl,
            l => l.DefaultNewFormUrl,
            l => l.Direction,
            l => l.ImageUrl,
            l => l.IrmEnabled,
            l => l.IrmExpire,
            l => l.IrmReject,
            l => l.IsApplicationList,
            l => l.ListExperience,
            l => l.ReadSecurity,
            l => l.WriteSecurity,
            l => l.ValidationFormula,
            l => l.ValidationMessage,
            l => l.RootFolder,
        };

        private static bool Set<T>(T current, T wanted, Action<T> set)
        {
            if (EqualityComparer<T>.Default.Equals(current, wanted))
            {
                return false;
            }

            set(wanted);
            return true;
        }

        /// <summary>
        /// Applies a value only when the template actually specifies one.
        /// </summary>
        private static bool SetIfNotEmpty(string current, string wanted, Action<string> set)
        {
            if (string.IsNullOrEmpty(wanted))
            {
                return false;
            }

            return Set(current, wanted, set);
        }

        private static CoreListReadingDirection ToCoreDirection(ListReadingDirectionModel direction)
        {
            return direction switch
            {
                ListReadingDirectionModel.RTL => CoreListReadingDirection.RTL,
                ListReadingDirectionModel.LTR => CoreListReadingDirection.LTR,
                _ => CoreListReadingDirection.None,
            };
        }

        private static CoreListExperience ToCoreListExperience(ListExperienceModel experience)
        {
            return experience switch
            {
                ListExperienceModel.NewExperience => CoreListExperience.NewExperience,
                ListExperienceModel.ClassicExperience => CoreListExperience.ClassicExperience,
                _ => CoreListExperience.Auto,
            };
        }

        /// <summary>
        /// Registers the list's id and url tokens, in the shared parser as well as the list's own.
        /// </summary>
        private static async Task RegisterListTokensAsync(PnPContext context, CoreList list, string webUrl,
            IEnumerable<int> supportedLanguageIds, TokenParser parser, TokenParser listParser)
        {
            void AddToBoth(TokenDefinition token)
            {
                parser.AddToken(token);
                listParser.AddToken(token);
            }

            AddToBoth(new ListIdToken(context, list.Title, list.Id));

            string webRelativeUrl = list.RootFolder.ServerRelativeUrl.Substring(webUrl.Length).TrimStart('/');
            AddToBoth(new ListUrlToken(context, list.Title, webRelativeUrl));

            foreach (string title in await ReadLocalizedTitlesAsync(context, list, supportedLanguageIds).ConfigureAwait(false))
            {
                AddToBoth(new ListIdToken(context, title, list.Id));
            }
        }

        /// <summary>
        /// The list's title in every language the site supports, so far as they differ from the
        /// default.
        /// </summary>
        private static async Task<IEnumerable<string>> ReadLocalizedTitlesAsync(PnPContext context, CoreList list,
            IEnumerable<int> supportedLanguageIds)
        {
            var titles = new List<string>();

            try
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                Dictionary<int, string> values = await UserResources.ReadAsync(context,
                    UserResourcePath.ForList(siteId, webId, list.Id, ResourceProperty.Title),
                    supportedLanguageIds).ConfigureAwait(false);

                foreach (string value in values.Values)
                {
                    if (!string.Equals(value, list.Title, StringComparison.Ordinal))
                    {
                        titles.Add(value);
                    }
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the localized titles of list '{List}' could not be read.",
                    Constants.LOGGING_SOURCE, list.Title);
            }

            return titles.Distinct(StringComparer.Ordinal);
        }

        /// <summary>
        /// Registers a token for every column on the list, so settings and actions can refer to
        /// them by name.
        /// </summary>
        private static async Task RegisterListFieldTokensAsync(PnPContext context, CoreList list, TokenParser parser)
        {
            await list.LoadAsync(l => l.Fields.QueryProperties(f => f.Id, f => f.Title, f => f.InternalName)).ConfigureAwait(false);

            foreach (IField field in list.Fields.AsRequested())
            {
                parser.AddToken(new FieldTitleToken(context, field.InternalName, field.Title));
                parser.AddToken(new FieldIdToken(context, field.InternalName, field.Id));
            }
        }

        private async Task LocalizeListAsync(PnPContext context, CoreList list, ListInstance templateList, TokenParser parser)
        {
            bool localizesTitle = UserResources.ContainsResourceToken(templateList.Title);
            bool localizesDescription = UserResources.ContainsResourceToken(templateList.Description);

            if (!localizesTitle && !localizesDescription)
            {
                return;
            }

            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

            if (localizesTitle)
            {
                await UserResources.TrySetAsync(context,
                    UserResourcePath.ForList(siteId, webId, list.Id, ResourceProperty.Title),
                    templateList.Title, parser, $"the title of list '{list.Title}'",
                    m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);
            }

            if (localizesDescription)
            {
                await UserResources.TrySetAsync(context,
                    UserResourcePath.ForList(siteId, webId, list.Id, ResourceProperty.Description),
                    templateList.Description, parser, $"the description of list '{list.Title}'",
                    m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// One list being provisioned, and the parser that carries its column tokens.
        /// </summary>
        private sealed class ListInfo
        {
            internal CoreList SiteList { get; set; }

            internal ListInstance TemplateList { get; set; }

            internal TokenParser TokenParser { get; set; }
        }

        #endregion
    }
}
