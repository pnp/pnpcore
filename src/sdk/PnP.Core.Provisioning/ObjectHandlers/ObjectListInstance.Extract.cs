using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.UserResources;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using CoreList = PnP.Core.Model.SharePoint.IList;
using FieldModel = PnP.Core.Provisioning.Model.Field;
using CoreListExperience = PnP.Core.Model.SharePoint.ListExperience;
using CoreListReadingDirection = PnP.Core.Model.SharePoint.ListReadingDirection;
using ListExperienceModel = PnP.Core.Provisioning.Model.ListExperience;
using ListReadingDirectionModel = PnP.Core.Provisioning.Model.ListReadingDirection;
using ViewModel = PnP.Core.Provisioning.Model.View;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    internal partial class ObjectListInstance
    {
        #region Extract

        /// <summary>
        /// Columns that are hidden but still worth exporting, because a template can meaningfully
        /// ask for them.
        /// </summary>
        private static readonly string[] SpecialFields = { "LikedBy", "RatedBy", "Ratings" };

        /// <summary>
        /// Columns SharePoint puts on every list. Exporting them produces a template that fails to
        /// apply, because the server refuses to create a column it already owns.
        /// </summary>
        private static readonly HashSet<string> BuiltInFieldNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "Editor", "Author", "ID", "Created", "Modified", "Attachments", "_UIVersionString", "DocIcon",
            "LinkTitleNoMenu", "LinkTitle", "Edit", "AppAuthor", "AppEditor", "ContentType", "ItemChildCount",
            "FolderChildCount", "LinkFilenameNoMenu", "LinkFilename", "_CopySource", "ParentVersionString",
            "ParentLeafName", "_CheckinComment", "FileLeafRef", "FileSizeDisplay", "Preview", "ThumbnailOnForm",
            "CheckoutUser", "Modified_x0020_By", "Created_x0020_By", "_DisplayName", "ComplianceAssetId",
            "_ComplianceFlags", "_ComplianceTag", "_ComplianceTagWrittenTime", "_ComplianceTagUserId", "_IsRecord",
        };

        private const string SharePointV3SourceId = "http://schemas.microsoft.com/sharepoint/v3";

        private const string ModernAudienceTargetingInternalName = "_ModernAudienceTargetUserField";

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            ProvisioningTemplateCreationInformation creationInfo =
                configuration?.ToCreationInformation() ?? new ProvisioningTemplateCreationInformation();

            IWeb web = context.Web;
            await web.LoadAsync(w => w.ServerRelativeUrl, w => w.Url, w => w.IsMultilingual).ConfigureAwait(false);

            string webUrl = web.ServerRelativeUrl.TrimEnd('/');

            // The list's own properties only. Its child collections - views, columns, custom
            // actions, role assignments - are loaded per list further down, deliberately:
            //
            //   1. Expanding all of them here builds a $select/$expand long enough that SharePoint
            //      rejects the request outright with HTTP 401 and "the length of the query string
            //      for this request exceeds the configured maxQueryStringLength value". On a site
            //      with a realistic number of lists that is not a corner case, it is the norm.
            //   2. Most lists on a site are filtered out before their contents are ever read.
            //
            // These have to be written inline rather than hoisted into a field. QueryProperties is
            // parsed out of the expression tree, and the parser reaches for a NewArrayExpression at
            // argument one; passing an array variable makes that a member access instead, and the
            // parse fails with a NullReferenceException from inside the SDK that names nothing.
            await web.LoadAsync(w => w.Lists.QueryProperties(
                l => l.Id,
                l => l.Title,
                l => l.Description,
                l => l.TemplateType,
                l => l.TemplateFeatureId,
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
                l => l.HasUniqueRoleAssignments,
                // The root folder's property bag is an expando model and is loaded separately, when
                // the property bag entries are read.
                l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl))).ConfigureAwait(false);

            List<CoreList> allLists = web.Lists.AsRequested().ToList();

            List<CoreList> toProcess = allLists
                .Where(l => !l.Hidden || IncludesHiddenLists(configuration, creationInfo))
                .ToList();

            // The content types a binding may legitimately name, read once for the whole extract.
            // Without it the extract cannot tell a content type the target site will also have from
            // one that exists only inside the list that declared it.
            HashSet<string> siteContentTypeIds = await ReadSiteContentTypeIdsAsync(context).ConfigureAwait(false);

            int listCount = 0;
            int totalCount = toProcess.Count;

            foreach (CoreList siteList in toProcess)
            {
                if (ShouldNotExtract(configuration, creationInfo, siteList))
                {
                    continue;
                }

                listCount++;
                WriteSubProgress("List", siteList.Title, listCount, totalCount);

                ListInstance list = await ExtractListAsync(context, template, creationInfo, siteList, allLists, webUrl, siteContentTypeIds)
                    .ConfigureAwait(false);

                ListInstance baseTemplateList = FindInBaseTemplate(creationInfo, siteList, webUrl);

                if (baseTemplateList != null && baseTemplateList.Equals(list))
                {
                    // Identical to what the base template already provides, so exporting it would
                    // only add noise to the result.
                    continue;
                }

                context.Logger?.LogDebug("{Source}: adding list '{Title}' ({Url}).", Constants.LOGGING_SOURCE, list.Title, list.Url);
                template.Lists.Add(list);
            }

            WriteMessage("Done processing lists", ProvisioningMessageType.Completed);

            return template;
        }

        private async Task<ListInstance> ExtractListAsync(PnPContext context, ProvisioningTemplate template,
            ProvisioningTemplateCreationInformation creationInfo, CoreList siteList, List<CoreList> allLists, string webUrl,
            HashSet<string> siteContentTypeIds)
        {
            var list = new ListInstance
            {
                Title = siteList.Title,
                Description = siteList.Description,
                TemplateType = (int)siteList.TemplateType,
                TemplateFeatureID = siteList.TemplateFeatureId,
                Url = siteList.RootFolder.ServerRelativeUrl.Substring(webUrl.Length).TrimStart('/'),
                Hidden = siteList.Hidden,
                OnQuickLaunch = siteList.OnQuickLaunch,
                NoCrawl = siteList.NoCrawl,
                ContentTypesEnabled = siteList.ContentTypesEnabled,
                EnableAttachments = siteList.EnableAttachments,
                EnableFolderCreation = siteList.EnableFolderCreation,
                EnableModeration = siteList.EnableModeration,
                EnableVersioning = siteList.EnableVersioning,
                EnableMinorVersions = siteList.EnableMinorVersions,
                MaxVersionLimit = siteList.MaxVersionLimit,
                MinorVersionLimit = siteList.MinorVersionLimit,
                DraftVersionVisibility = (int)siteList.DraftVersionVisibility,
                ForceCheckout = siteList.ForceCheckout,
                DocumentTemplate = Tokenize(siteList.DocumentTemplate, webUrl),
                DefaultDisplayFormUrl = Tokenize(siteList.DefaultDisplayFormUrl, webUrl),
                DefaultEditFormUrl = Tokenize(siteList.DefaultEditFormUrl, webUrl),
                DefaultNewFormUrl = Tokenize(siteList.DefaultNewFormUrl, webUrl),
                ImageUrl = Tokenize(siteList.ImageUrl, webUrl),
                Direction = ToModelDirection(siteList.Direction),
                ListExperience = ToModelListExperience(siteList.ListExperience),
                IrmExpire = siteList.IrmExpire,
                IrmReject = siteList.IrmReject,
                IsApplicationList = siteList.IsApplicationList,
                ReadSecurity = siteList.ReadSecurity,
                WriteSecurity = siteList.WriteSecurity,
                ValidationFormula = siteList.ValidationFormula,
                ValidationMessage = siteList.ValidationMessage,
            };

            await PersistListResourcesAsync(context, siteList, list, template, creationInfo).ConfigureAwait(false);

            var contentTypeFields = new List<FieldRef>();
            await ExtractContentTypesAsync(context, siteList, list, contentTypeFields, siteContentTypeIds).ConfigureAwait(false);
            await ExtractViewsAsync(context, siteList, list).ConfigureAwait(false);
            await ExtractFieldsAsync(context, siteList, list, contentTypeFields, allLists).ConfigureAwait(false);
            await ExtractUserCustomActionsAsync(siteList, list).ConfigureAwait(false);
            await ExtractWebhooksAsync(context, siteList, list).ConfigureAwait(false);
            await ExtractIrmSettingsAsync(context, siteList, list).ConfigureAwait(false);
            await ExtractPropertyBagEntriesAsync(context, siteList, list).ConfigureAwait(false);

            list.Security = await SecurityUtilities.ExtractAsync(context, siteList).ConfigureAwait(false);

            return list;
        }

        private static async Task PersistListResourcesAsync(PnPContext context, CoreList siteList, ListInstance list,
            ProvisioningTemplate template, ProvisioningTemplateCreationInformation creationInfo)
        {
            if (!creationInfo.PersistMultiLanguageResources)
            {
                return;
            }

            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);
            string key = siteList.Title.Replace(" ", "_");

            if (await UserResources.PersistAsync(context,
                UserResourcePath.ForList(siteId, webId, siteList.Id, ResourceProperty.Title),
                $"List_{key}_Title", template, creationInfo).ConfigureAwait(false))
            {
                list.Title = UserResources.TokenFor($"List_{key}_Title");
            }

            if (await UserResources.PersistAsync(context,
                UserResourcePath.ForList(siteId, webId, siteList.Id, ResourceProperty.Description),
                $"List_{key}_Description", template, creationInfo).ConfigureAwait(false))
            {
                list.Description = UserResources.TokenFor($"List_{key}_Description");
            }
        }

        /// <summary>
        /// Reads the list's content type bindings, and collects the columns those content types
        /// carry so <see cref="ExtractFieldsAsync"/> can tell a column that belongs to a content
        /// type from one that only exists on the list.
        /// </summary>
        private static async Task ExtractContentTypesAsync(PnPContext context, CoreList siteList, ListInstance list,
            List<FieldRef> contentTypeFields, ISet<string> siteContentTypeIds)
        {
            await siteList.LoadAsync(l => l.ContentTypes.QueryProperties(ct => ct.Id, ct => ct.StringId, ct => ct.Name,
                ct => ct.FieldLinks.QueryProperties(fl => fl.Id, fl => fl.Hidden))).ConfigureAwait(false);

            List<string> order;
            try
            {
                order = await siteList.GetContentTypeOrderAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the content type order of list '{List}' could not be read.",
                    Constants.LOGGING_SOURCE, siteList.Title);
                order = null;
            }

            int index = 0;

            foreach (IContentType contentType in siteList.ContentTypes.AsRequested())
            {
                string parentId = BindableContentTypeId(contentType.StringId, siteContentTypeIds);

                if (string.Equals(parentId, BuiltInContentTypeId.System, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Absent from the unique content type order means hidden in the new button. A list
                // with no unique order at all shows everything.
                bool hidden = order != null && order.Count > 0
                    && !order.Any(id => string.Equals(id, contentType.StringId, StringComparison.OrdinalIgnoreCase));

                list.ContentTypeBindings.Add(new ContentTypeBinding
                {
                    ContentTypeId = parentId,
                    Default = index == 0,
                    Hidden = hidden,
                });

                foreach (IFieldLink fieldLink in contentType.FieldLinks.AsRequested().Where(fl => !fl.Hidden))
                {
                    contentTypeFields.Add(new FieldRef { Id = fieldLink.Id });
                }

                index++;
            }
        }

        /// <summary>
        /// The ids of the content types this site offers, or <c>null</c> if they could not be read.
        /// </summary>
        private static async Task<HashSet<string>> ReadSiteContentTypeIdsAsync(PnPContext context)
        {
            try
            {
                await context.Web.LoadAsync(w => w.AvailableContentTypes.QueryProperties(ct => ct.StringId))
                    .ConfigureAwait(false);

                return new HashSet<string>(
                    context.Web.AvailableContentTypes.AsRequested().Select(ct => ct.StringId),
                    StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                // Falls back to the immediate parent, which is what PnP Framework records. Worth
                // degrading rather than failing: the extract still produces a template, and the
                // apply reports any binding it cannot satisfy.
                context.Logger?.LogDebug(ex, "{Source}: the site's available content types could not be read.",
                    Constants.LOGGING_SOURCE);

                return null;
            }
        }

        /// <summary>
        /// The nearest ancestor of a list content type that a template can actually bind to.
        /// </summary>
        internal static string BindableContentTypeId(string listContentTypeId, ISet<string> siteContentTypeIds)
        {
            string id = ParentContentTypeId(listContentTypeId);

            if (siteContentTypeIds == null)
            {
                return id;
            }

            while (!string.Equals(id, BuiltInContentTypeId.System, StringComparison.OrdinalIgnoreCase))
            {
                if (siteContentTypeIds.Contains(id))
                {
                    return id;
                }

                id = ParentContentTypeId(id);
            }

            return BuiltInContentTypeId.System;
        }

        /// <summary>
        /// The id of the content type a list content type derives from.
        /// </summary>
        internal static string ParentContentTypeId(string listContentTypeId)
        {
            const int listSuffixLength = 34;

            if (string.IsNullOrEmpty(listContentTypeId) || listContentTypeId.Length <= 2)
            {
                return BuiltInContentTypeId.System;
            }

            if (listContentTypeId.Length > listSuffixLength
                && listContentTypeId.Substring(listContentTypeId.Length - listSuffixLength, 2) == "00")
            {
                return listContentTypeId.Substring(0, listContentTypeId.Length - listSuffixLength);
            }

            return listContentTypeId.Substring(0, listContentTypeId.Length - 2);
        }

        private async Task ExtractViewsAsync(PnPContext context, CoreList siteList, ListInstance list)
        {
            await siteList.LoadAsync(l => l.Views.QueryProperties(v => v.Id, v => v.Title, v => v.Hidden,
                v => v.ListViewXml)).ConfigureAwait(false);

            foreach (IView view in siteList.Views.AsRequested())
            {
                if (view.Hidden || string.IsNullOrEmpty(view.ListViewXml))
                {
                    continue;
                }

                XElement schema = XElement.Parse(view.ListViewXml);

                // Neither is honoured on apply, and both carry server-generated content that makes
                // an extract-apply-extract cycle produce a different template each time.
                schema.Descendants("Toolbar").FirstOrDefault()?.Remove();
                schema.Descendants("XslLink").FirstOrDefault()?.Remove();

                list.Views.Add(new ViewModel { SchemaXml = TokenizeListView(schema.ToString(), siteList, context.Web) });
            }
        }

        /// <summary>
        /// Splits the list's columns into references to site columns and full definitions.
        /// </summary>
        private async Task ExtractFieldsAsync(PnPContext context, CoreList siteList, ListInstance list,
            List<FieldRef> contentTypeFields, List<CoreList> allLists)
        {
            await siteList.LoadAsync(l => l.Fields.QueryProperties(f => f.Id, f => f.Title, f => f.InternalName,
                f => f.Hidden, f => f.Required, f => f.DefaultValue, f => f.SchemaXml, f => f.TypeAsString,
                f => f.CustomFormatter, f => f.ShowInFiltersPane)).ConfigureAwait(false);

            await context.Web.LoadAsync(w => w.AvailableFields.QueryProperties(f => f.Id, f => f.Title,
                f => f.DefaultValue, f => f.CustomFormatter, f => f.ShowInFiltersPane)).ConfigureAwait(false);

            Dictionary<Guid, IField> siteColumns = context.Web.AvailableFields.AsRequested()
                .GroupBy(f => f.Id)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (IField field in siteList.Fields.AsRequested())
            {
                if (field.Hidden && !SpecialFields.Contains(field.InternalName))
                {
                    continue;
                }

                if (field.InternalName == ModernAudienceTargetingInternalName)
                {
                    list.EnableAudienceTargeting = true;
                }

                if (field.InternalName == ClassicAudienceTargetingInternalName)
                {
                    list.EnableClassicAudienceTargeting = true;
                }

                siteColumns.TryGetValue(field.Id, out IField siteColumn);

                if (siteColumn != null && !DiffersFromSiteColumn(siteColumn, field))
                {
                    AddFieldRef(list, siteList, field, siteColumn, contentTypeFields);
                }
                else
                {
                    await AddFieldDefinitionAsync(context, list, field, allLists).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Whether the list has customised the column beyond what the site column defines.
        /// </summary>
        private static bool DiffersFromSiteColumn(IField siteColumn, IField listField)
        {
            return siteColumn.Title != listField.Title
                || siteColumn.ShowInFiltersPane != listField.ShowInFiltersPane
                || !string.Equals(siteColumn.CustomFormatter ?? string.Empty, listField.CustomFormatter ?? string.Empty, StringComparison.Ordinal);
        }

        private static void AddFieldRef(ListInstance list, CoreList siteList, IField field, IField siteColumn,
            List<FieldRef> contentTypeFields)
        {
            bool include = true;

            // With content types on, the content types decide the columns; a column that no content
            // type carries is not part of the list's schema and re-adding it changes the list.
            if (siteList.ContentTypesEnabled && contentTypeFields.All(c => c.Id != field.Id))
            {
                include = false;
            }

            // DefaultValue is typed object because a column's default is not always textual, but the
            // schema records it as a string, so both sides are compared and stored as one.
            string siteDefault = siteColumn.DefaultValue?.ToString() ?? string.Empty;
            string listDefault = field.DefaultValue?.ToString() ?? string.Empty;

            if (!string.Equals(siteDefault, listDefault, StringComparison.Ordinal))
            {
                // A per-list default is a real difference and has to be carried, even for a column
                // the content types would otherwise account for.
                list.FieldDefaults.Add(field.InternalName, listDefault);
                include = true;
            }

            if (IsBuiltInField(field))
            {
                include = false;
            }

            if (!include)
            {
                return;
            }

            list.FieldRefs.Add(new FieldRef(field.InternalName)
            {
                Id = field.Id,
                DisplayName = field.Title,
                Required = field.Required,
                Hidden = field.Hidden,
            });
        }

        private async Task AddFieldDefinitionAsync(PnPContext context, ListInstance list, IField field, List<CoreList> allLists)
        {
            XElement schema = XElement.Parse(field.SchemaXml);

            if (IsBuiltInField(field))
            {
                return;
            }

            // A lookup addresses its target list by id, which is different on every site. Replacing
            // it with the target's title makes the template portable.
            XAttribute listAttribute = schema.Attribute("List");
            if (listAttribute != null && Guid.TryParse(listAttribute.Value, out Guid targetListId))
            {
                CoreList target = allLists.FirstOrDefault(l => l.Id == targetListId);
                if (target != null)
                {
                    listAttribute.SetValue($"{{listid:{target.Title}}}");
                }
            }

            string schemaXml = field.TypeAsString.StartsWith("TaxonomyField", StringComparison.Ordinal)
                ? await TokenizeTaxonomyFieldAsync(context, schema).ConfigureAwait(false)
                : schema.ToString();

            list.Fields.Add(new FieldModel { SchemaXml = schemaXml });
        }

        private static bool IsBuiltInField(IField field)
        {
            XElement schema = XElement.Parse(field.SchemaXml);
            string sourceId = (string)schema.Attribute("SourceID");

            return sourceId == SharePointV3SourceId && BuiltInFieldNames.Contains(field.InternalName);
        }

        private static async Task ExtractUserCustomActionsAsync(CoreList siteList, ListInstance list)
        {
            await siteList.LoadAsync(l => l.UserCustomActions.QueryProperties(a => a.Id, a => a.Name, a => a.Title,
                a => a.Description, a => a.Location, a => a.Group, a => a.Sequence, a => a.Url, a => a.ImageUrl,
                a => a.ScriptBlock, a => a.ScriptSrc, a => a.CommandUIExtension, a => a.ClientSideComponentId,
                a => a.ClientSideComponentProperties)).ConfigureAwait(false);

            foreach (IUserCustomAction action in siteList.UserCustomActions.AsRequested())
            {
                list.UserCustomActions.Add(new CustomAction
                {
                    Name = action.Name,
                    Title = action.Title,
                    Description = action.Description,
                    Enabled = true,
                    Location = action.Location,
                    Group = action.Group,
                    Sequence = action.Sequence,
                    Url = action.Url,
                    ImageUrl = action.ImageUrl,
                    ScriptBlock = action.ScriptBlock,
                    ScriptSrc = action.ScriptSrc,
                    ClientSideComponentId = action.ClientSideComponentId,
                    ClientSideComponentProperties = action.ClientSideComponentProperties,
                    CommandUIExtension = !string.IsNullOrEmpty(action.CommandUIExtension)
                        ? XElement.Parse(action.CommandUIExtension)
                        : null,
                });
            }
        }

        private static async Task ExtractWebhooksAsync(PnPContext context, CoreList siteList, ListInstance list)
        {
            try
            {
                await siteList.LoadAsync(l => l.Webhooks.QueryProperties(w => w.NotificationUrl, w => w.ExpirationDateTime))
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // App-only contexts cannot read subscriptions.
                context.Logger?.LogDebug(ex, "{Source}: the webhooks of list '{List}' could not be read.",
                    Constants.LOGGING_SOURCE, siteList.Title);
                return;
            }

            foreach (IListSubscription webhook in siteList.Webhooks.AsRequested())
            {
                if (string.IsNullOrEmpty(webhook.NotificationUrl))
                {
                    continue;
                }

                // The schema carries a lifetime, not a date, so an export records what is left of it.
                int expiresInDays = webhook.ExpirationDateTime.Subtract(DateTime.Now).Days + 1;
                if (expiresInDays <= 0)
                {
                    continue;
                }

                list.Webhooks.Add(new Webhook
                {
                    ServerNotificationUrl = webhook.NotificationUrl,
                    ExpiresInDays = expiresInDays,
                });
            }
        }

        private static async Task ExtractIrmSettingsAsync(PnPContext context, CoreList siteList, ListInstance list)
        {
            if (siteList.TemplateType == ListTemplateType.PictureLibrary || !siteList.IrmEnabled)
            {
                return;
            }

            try
            {
                await siteList.LoadAsync(l => l.InformationRightsManagementSettings).ConfigureAwait(false);
                IInformationRightsManagementSettings irm = siteList.InformationRightsManagementSettings;

                list.IRMSettings = new IRMSettings
                {
                    Enabled = true,
                    AllowPrint = irm.AllowPrint,
                    AllowScript = irm.AllowScript,
                    AllowWriteCopy = irm.AllowWriteCopy,
                    DisableDocumentBrowserView = irm.DisableDocumentBrowserView,
                    DocumentAccessExpireDays = irm.DocumentAccessExpireDays,
                    EnableDocumentAccessExpire = irm.EnableDocumentAccessExpire,
                    EnableDocumentBrowserPublishingView = irm.EnableDocumentBrowserPublishingView,
                    EnableGroupProtection = irm.EnableGroupProtection,
                    EnableLicenseCacheExpire = irm.EnableLicenseCacheExpire,
                    GroupName = irm.GroupName,
                    LicenseCacheExpireDays = irm.LicenseCacheExpireDays,
                    PolicyDescription = irm.PolicyDescription,
                    PolicyTitle = irm.PolicyTitle,
                    DocumentLibraryProtectionExpiresInDays =
                        (int)irm.DocumentLibraryProtectionExpireDate.Subtract(DateTime.Now).TotalDays,
                };
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the IRM settings of list '{List}' could not be read.",
                    Constants.LOGGING_SOURCE, siteList.Title);
            }
        }

        private static async Task ExtractPropertyBagEntriesAsync(PnPContext context, CoreList siteList, ListInstance list)
        {
            try
            {
                await siteList.RootFolder.LoadAsync(f => f.Properties).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the property bag of list '{List}' could not be read.",
                    Constants.LOGGING_SOURCE, siteList.Title);
                return;
            }

            List<string> indexedKeys = ReadIndexedPropertyKeys(siteList.RootFolder);

            foreach (string key in indexedKeys)
            {
                if (siteList.RootFolder.Properties.Values.TryGetValue(key, out object value))
                {
                    // Only the indexed entries are exported. A list's property bag is otherwise full
                    // of server bookkeeping (vti_*), none of which a template should reproduce.
                    list.PropertyBagEntries.Add(new PropertyBagEntry
                    {
                        Key = key,
                        Value = value?.ToString(),
                        Indexed = true,
                        Overwrite = true,
                    });
                }
            }
        }

        #endregion

        #region Extract filtering

        private static bool IncludesHiddenLists(ExtractConfiguration configuration, ProvisioningTemplateCreationInformation creationInfo)
        {
            return creationInfo.IncludeHiddenLists || (configuration?.Lists?.IncludeHiddenLists ?? false);
        }

        /// <summary>
        /// Whether an explicit list selection excludes this list.
        /// </summary>
        private static bool ShouldNotExtract(ExtractConfiguration configuration,
            ProvisioningTemplateCreationInformation creationInfo, CoreList siteList)
        {
            if (creationInfo.ListsToExtract != null && creationInfo.ListsToExtract.Count > 0)
            {
                bool selected = creationInfo.ListsToExtract.Any(entry =>
                    (Guid.TryParse(entry, out Guid id) && id == siteList.Id)
                    || string.Equals(entry, siteList.Title, StringComparison.Ordinal));

                if (!selected)
                {
                    return true;
                }
            }

            if (configuration?.Lists != null && configuration.Lists.HasLists)
            {
                bool selected = configuration.Lists.Lists.Any(entry =>
                    (Guid.TryParse(entry.Title, out Guid id) && id == siteList.Id)
                    || string.Equals(entry.Title, siteList.Title, StringComparison.Ordinal)
                    || siteList.RootFolder.ServerRelativeUrl.EndsWith(entry.Title, StringComparison.OrdinalIgnoreCase));

                if (!selected)
                {
                    return true;
                }
            }

            return false;
        }

        private static ListInstance FindInBaseTemplate(ProvisioningTemplateCreationInformation creationInfo,
            CoreList siteList, string webUrl)
        {
            if (creationInfo.BaseTemplate == null)
            {
                return null;
            }

            string url = siteList.RootFolder.ServerRelativeUrl.Substring(webUrl.Length).TrimStart('/');

            return creationInfo.BaseTemplate.Lists.FirstOrDefault(
                l => string.Equals(l.Url, url, StringComparison.OrdinalIgnoreCase)
                    && l.TemplateType == (int)siteList.TemplateType);
        }

        private static ListReadingDirectionModel ToModelDirection(CoreListReadingDirection direction)
        {
            return direction switch
            {
                CoreListReadingDirection.RTL => ListReadingDirectionModel.RTL,
                CoreListReadingDirection.LTR => ListReadingDirectionModel.LTR,
                _ => ListReadingDirectionModel.None,
            };
        }

        private static ListExperienceModel ToModelListExperience(CoreListExperience experience)
        {
            return experience switch
            {
                CoreListExperience.NewExperience => ListExperienceModel.NewExperience,
                CoreListExperience.ClassicExperience => ListExperienceModel.ClassicExperience,
                _ => ListExperienceModel.Auto,
            };
        }

        #endregion
    }
}
