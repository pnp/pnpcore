using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.ContentTypes;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.UserResources;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContentTypeModel = PnP.Core.Provisioning.Model.ContentType;
using FieldRefModel = PnP.Core.Provisioning.Model.FieldRef;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Creates and updates the site content types a template declares, including document sets.
    /// </summary>
    internal class ObjectContentType : ObjectHandlerBase
    {
        private readonly FieldAndListProvisioningStepHelper.Step step;

        public ObjectContentType(FieldAndListProvisioningStepHelper.Step step)
        {
            this.step = step;
        }

        public override string Name => $"Content Types ({step})";

        public override string InternalName => "ContentTypes";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.ContentTypes.Any();
            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return true;
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                IWeb web = await context.Web.GetAsync(w => w.ServerRelativeUrl, w => w.Url).ConfigureAwait(false);
                await context.Site.LoadAsync(s => s.ServerRelativeUrl).ConfigureAwait(false);

                bool provisionToSubWebs = configuration?.ToApplyingInformation()?.ProvisionContentTypesToSubWebs ?? false;

                if (IsSubSite(web) && !provisionToSubWebs)
                {
                    const string message = "This template contains content types and the target is a sub site. " +
                        "Set ProvisionContentTypesToSubWebs if you really want them created there.";

                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    WriteMessage(message, ProvisioningMessageType.Warning);

                    return parser;
                }

                bool isNoScriptSite = await context.Web.IsNoScriptSiteAsync().ConfigureAwait(false);

                await LoadWebContentTypesAndFieldsAsync(context).ConfigureAwait(false);

                int currentIndex = 0;

                // By id, so a parent content type is always created before its children.
                foreach (ContentTypeModel contentType in template.ContentTypes.OrderBy(ct => ct.Id, StringComparer.OrdinalIgnoreCase))
                {
                    currentIndex++;
                    string name = parser.ParseString(contentType.Name);
                    WriteSubProgress("Content Type", name, currentIndex, template.ContentTypes.Count);

                    IContentType existing = FindContentType(context, contentType.Id)
                        ?? await FindContentTypeOnServerAsync(context, contentType.Id).ConfigureAwait(false);

                    if (existing == null)
                    {
                        existing = await CreateContentTypeAsync(context, template, contentType, parser, isNoScriptSite).ConfigureAwait(false);
                    }
                    else if (contentType.Overwrite && step == FieldAndListProvisioningStepHelper.Step.ListAndStandardFields)
                    {
                        context.Logger?.LogDebug("{Source}: recreating content type {Id} ({Name}).", Constants.LOGGING_SOURCE, contentType.Id, name);

                        await existing.DeleteAsync().ConfigureAwait(false);

                        existing = await CreateContentTypeAsync(context, template, contentType, parser, isNoScriptSite).ConfigureAwait(false);
                    }
                    else if ((!existing.Sealed || !contentType.Sealed) && (!existing.ReadOnly || !contentType.ReadOnly))
                    {
                        await UpdateContentTypeAsync(context, template, existing, contentType, parser, isNoScriptSite).ConfigureAwait(false);
                    }
                    else
                    {
                        // Logged, not reported - which is what PnP Framework does with it too.
                        //
                        // This branch is reached only when the site and the template AGREE that the
                        // content type is sealed, or agree that it is read only. Nothing the template
                        // asked for has been refused: it wanted a sealed content type and found one.
                        // Every site carries a handful of sealed content types SharePoint ships and
                        // nobody can update, so reporting these made every apply of every classic
                        // template come back "with warnings" over a state that is entirely correct -
                        // five of them on STS#0, two on a communication site - and a caller acting on
                        // the warning count could do nothing about any of them.
                        //
                        // The log still records each one, so the detail is there when it is wanted.
                        context.Logger?.LogWarning("{Source}: the content type '{Name}' ({Id}) is sealed or " +
                            "read only on this site, so it was not updated.",
                            Constants.LOGGING_SOURCE, name, contentType.Id);
                    }

                    // Last, and only in the final pass: a read-only content type cannot be updated,
                    // so setting this earlier would lock out everything after it.
                    if (existing != null
                        && step == FieldAndListProvisioningStepHelper.Step.LookupFields
                        && !existing.ReadOnly && contentType.ReadOnly)
                    {
                        existing.ReadOnly = true;
                        await existing.UpdateAsync().ConfigureAwait(false);
                    }
                }

                WriteMessage("Done processing Content Types", ProvisioningMessageType.Completed);

                return parser;
            }
        }

        /// <summary>
        /// Loads the web's content types and columns with everything both passes will need.
        /// </summary>
        private static async Task LoadWebContentTypesAndFieldsAsync(PnPContext context)
        {
            await context.Web.LoadAsync(
                w => w.ContentTypes.QueryProperties(ct => ct.Id, ct => ct.StringId, ct => ct.Name, ct => ct.Description,
                    ct => ct.Group, ct => ct.Hidden, ct => ct.Sealed, ct => ct.ReadOnly, ct => ct.DocumentTemplate,
                    ct => ct.NewFormUrl, ct => ct.EditFormUrl, ct => ct.DisplayFormUrl,
                    ct => ct.NewFormClientSideComponentId, ct => ct.NewFormClientSideComponentProperties,
                    ct => ct.EditFormClientSideComponentId, ct => ct.EditFormClientSideComponentProperties,
                    ct => ct.DisplayFormClientSideComponentId, ct => ct.DisplayFormClientSideComponentProperties,
                    ct => ct.FieldLinks.QueryProperties(fl => fl.Id, fl => fl.Name, fl => fl.Required, fl => fl.Hidden)),
                w => w.Fields.QueryProperties(f => f.Id, f => f.InternalName, f => f.Title, f => f.SchemaXml)).ConfigureAwait(false);
        }

        private static IContentType FindContentType(PnPContext context, string contentTypeId)
        {
            return context.Web.ContentTypes.AsRequested()
                .FirstOrDefault(ct => string.Equals(ct.StringId, contentTypeId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Asks the server whether a content type exists, rather than the loaded collection.
        /// </summary>
        private static async Task<IContentType> FindContentTypeOnServerAsync(PnPContext context, string contentTypeId)
        {
            try
            {
                return await QueryableExtensions.FirstOrDefaultAsync(
                    context.Web.ContentTypes.QueryProperties(
                        ct => ct.Id, ct => ct.StringId, ct => ct.Name, ct => ct.Description, ct => ct.Group,
                        ct => ct.Hidden, ct => ct.Sealed, ct => ct.ReadOnly, ct => ct.DocumentTemplate,
                        ct => ct.NewFormUrl, ct => ct.EditFormUrl, ct => ct.DisplayFormUrl,
                        ct => ct.NewFormClientSideComponentId, ct => ct.NewFormClientSideComponentProperties,
                        ct => ct.EditFormClientSideComponentId, ct => ct.EditFormClientSideComponentProperties,
                        ct => ct.DisplayFormClientSideComponentId, ct => ct.DisplayFormClientSideComponentProperties,
                        ct => ct.FieldLinks.QueryProperties(fl => fl.Id, fl => fl.Name, fl => fl.Required, fl => fl.Hidden)),
                    ct => ct.StringId == contentTypeId).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<IContentType> CreateContentTypeAsync(PnPContext context, ProvisioningTemplate template,
            ContentTypeModel contentType, TokenParser parser, bool isNoScriptSite)
        {
            string name = parser.ParseString(contentType.Name);
            string description = parser.ParseString(contentType.Description);
            string id = parser.ParseString(contentType.Id);
            string group = parser.ParseString(contentType.Group);

            context.Logger?.LogDebug("{Source}: creating content type {Id} ({Name}).", Constants.LOGGING_SOURCE, id, name);

            IContentType created;

            if (contentType.DocumentSetTemplate != null)
            {
                if (!await EnsureDocumentSetsFeatureAsync(context).ConfigureAwait(false))
                {
                    return null;
                }

                // A document set is a content type with extra structure, and PnP Core creates both
                // in one call - which is backlog T15, and it turned out to be already covered.
                IDocumentSet documentSet = await context.Web.ContentTypes
                    .AddDocumentSetAsync(id, name, description, group).ConfigureAwait(false);

                // Taken from the document set rather than by re-reading Web.ContentTypes.
                //
                // Re-loading the collection here replaces the client-side entry the create added
                // with whatever the server returns - and the server does not list a brand new
                // content type immediately. The second provisioning pass then does not find it,
                // creates it again, and SharePoint answers "a duplicate content type was found".
                created = documentSet.Parent as IContentType;

                if (created != null)
                {
                    await ApplyDocumentSetAsync(context, template, created, contentType, parser, isNoScriptSite).ConfigureAwait(false);
                }
            }
            else
            {
                created = await context.Web.ContentTypes.AddAsync(id, name, description, group).ConfigureAwait(false);
            }

            if (created != null)
            {
                // Whichever route created it, the object carries only what the create response held.
                // Reading Hidden or Sealed off it without this throws "property was not yet loaded",
                // and everything below reads them.
                await created.LoadAsync(ct => ct.StringId, ct => ct.Name, ct => ct.Description, ct => ct.Group,
                    ct => ct.Hidden, ct => ct.Sealed, ct => ct.ReadOnly, ct => ct.DocumentTemplate,
                    ct => ct.NewFormUrl, ct => ct.EditFormUrl, ct => ct.DisplayFormUrl,
                    ct => ct.NewFormClientSideComponentId, ct => ct.NewFormClientSideComponentProperties,
                    ct => ct.EditFormClientSideComponentId, ct => ct.EditFormClientSideComponentProperties,
                    ct => ct.DisplayFormClientSideComponentId, ct => ct.DisplayFormClientSideComponentProperties,
                    ct => ct.FieldLinks).ConfigureAwait(false);
            }

            if (created == null)
            {
                string message = $"The content type '{name}' ({id}) could not be created.";
                context.Logger?.LogError("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Error);

                return null;
            }

            // Registered straight away so a later content type - or a list - can reference it.
            parser.AddToken(new ContentTypeIdToken(context, name, id));

            await AddFieldRefsAsync(context, template, created, contentType, parser).ConfigureAwait(false);
            await ReorderFieldsAsync(context, created, contentType, parser).ConfigureAwait(false);

            await SendPropertyUpdateAsync(context, created, contentType, parser, isNoScriptSite, updateChildren: true).ConfigureAwait(false);

            await LocalizeAsync(context, created, contentType, parser).ConfigureAwait(false);

            WarnOnUnsupportedDocumentTemplate(context, contentType, parser, name);

            return created;
        }

        private async Task UpdateContentTypeAsync(PnPContext context, ProvisioningTemplate template,
            IContentType existing, ContentTypeModel contentType, TokenParser parser, bool isNoScriptSite)
        {
            string name = parser.ParseString(contentType.Name);

            context.Logger?.LogDebug("{Source}: updating content type {Id} ({Name}).", Constants.LOGGING_SOURCE, contentType.Id, name);

            string oldName = existing.Name;
            string newName = parser.ParseString(contentType.Name);

            await SendPropertyUpdateAsync(context, existing, contentType, parser, isNoScriptSite,
                updateChildren: contentType.UpdateChildren).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(newName) && !string.Equals(oldName, newName, StringComparison.Ordinal))
            {
                // The content type is being renamed, so the token that resolves its id by name has
                // to follow - anything referencing the new name would otherwise not resolve.
                parser.RemoveToken(new ContentTypeIdToken(context, oldName, existing.StringId));
                parser.AddToken(new ContentTypeIdToken(context, newName, existing.StringId));
            }

            bool fieldsAdded = await AddFieldRefsAsync(context, template, existing, contentType, parser).ConfigureAwait(false);

            await UpdateExistingFieldLinksAsync(context, existing, contentType).ConfigureAwait(false);

            if (fieldsAdded || FieldOrderDiffers(existing, contentType, parser))
            {
                await ReorderFieldsAsync(context, existing, contentType, parser).ConfigureAwait(false);
            }

            if (contentType.DocumentSetTemplate != null)
            {
                await ApplyDocumentSetAsync(context, template, existing, contentType, parser, isNoScriptSite).ConfigureAwait(false);
            }

            await LocalizeAsync(context, existing, contentType, parser).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the template's simple properties to SharePoint, if any of them differ.
        /// </summary>
        private async Task SendPropertyUpdateAsync(PnPContext context, IContentType target, ContentTypeModel contentType,
            TokenParser parser, bool isNoScriptSite, bool updateChildren)
        {
            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);
            var request = new UpdateContentTypeRequest(siteId, webId, target.StringId, updateChildren);

            AddIfChanged(request.SetBoolean, "Hidden", target.Hidden, contentType.Hidden);
            AddIfChanged(request.SetBoolean, "Sealed", target.Sealed, contentType.Sealed);

            // Only ever turned OFF here. Turning it on happens last, after every other pass - see
            // the remarks on the class.
            if (target.ReadOnly && !contentType.ReadOnly)
            {
                request.SetBoolean("ReadOnly", false);
            }

            AddStringIfChanged(request, "Description", target.Description, parser.ParseString(contentType.Description), contentType.Description);
            AddStringIfChanged(request, "Name", target.Name, parser.ParseString(contentType.Name), contentType.Name);
            AddStringIfChanged(request, "Group", target.Group, parser.ParseString(contentType.Group), contentType.Group);

            // A document set gets its document template from its default contents, not from here.
            if (contentType.DocumentSetTemplate == null)
            {
                AddStringIfChanged(request, "DocumentTemplate", target.DocumentTemplate,
                    parser.ParseString(contentType.DocumentTemplate), contentType.DocumentTemplate);
            }

            AddFormUrls(context, request, target, contentType, parser, isNoScriptSite);
            AddFormCustomizers(request, target, contentType, parser);

            if (!request.HasChanges)
            {
                return;
            }

            await CsomRequestSender.SendAsync(context, request).ConfigureAwait(false);
        }

        /// <summary>
        /// Queues the custom form urls, which a NoScript site refuses.
        /// </summary>
        private void AddFormUrls(PnPContext context, UpdateContentTypeRequest request, IContentType target,
            ContentTypeModel contentType, TokenParser parser, bool isNoScriptSite)
        {
            string newFormUrl = parser.ParseString(contentType.NewFormUrl);
            string editFormUrl = parser.ParseString(contentType.EditFormUrl);
            string displayFormUrl = parser.ParseString(contentType.DisplayFormUrl);

            bool anyRequested = !string.IsNullOrEmpty(newFormUrl)
                || !string.IsNullOrEmpty(editFormUrl)
                || !string.IsNullOrEmpty(displayFormUrl);

            if (!anyRequested)
            {
                return;
            }

            if (isNoScriptSite)
            {
                // Custom form pages live in a library this site type does not permit writing to, so
                // the urls would point at pages that cannot exist.
                string message = $"This is a NoScript site, so the custom form urls on content type '{target.Name}' were not applied.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Warning);

                return;
            }

            AddStringIfChanged(request, "NewFormUrl", target.NewFormUrl, newFormUrl, contentType.NewFormUrl);
            AddStringIfChanged(request, "EditFormUrl", target.EditFormUrl, editFormUrl, contentType.EditFormUrl);
            AddStringIfChanged(request, "DisplayFormUrl", target.DisplayFormUrl, displayFormUrl, contentType.DisplayFormUrl);
        }

        /// <summary>
        /// Queues the SPFx form customizer settings.
        /// </summary>
        private static void AddFormCustomizers(UpdateContentTypeRequest request, IContentType target,
            ContentTypeModel contentType, TokenParser parser)
        {
            AddStringIfChanged(request, "DisplayFormClientSideComponentId", target.DisplayFormClientSideComponentId, parser.ParseString(contentType.DisplayFormClientSideComponentId), contentType.DisplayFormClientSideComponentId);
            AddStringIfChanged(request, "DisplayFormClientSideComponentProperties", target.DisplayFormClientSideComponentProperties, parser.ParseString(contentType.DisplayFormClientSideComponentProperties), contentType.DisplayFormClientSideComponentProperties);
            AddStringIfChanged(request, "NewFormClientSideComponentId", target.NewFormClientSideComponentId, parser.ParseString(contentType.NewFormClientSideComponentId), contentType.NewFormClientSideComponentId);
            AddStringIfChanged(request, "NewFormClientSideComponentProperties", target.NewFormClientSideComponentProperties, parser.ParseString(contentType.NewFormClientSideComponentProperties), contentType.NewFormClientSideComponentProperties);
            AddStringIfChanged(request, "EditFormClientSideComponentId", target.EditFormClientSideComponentId, parser.ParseString(contentType.EditFormClientSideComponentId), contentType.EditFormClientSideComponentId);
            AddStringIfChanged(request, "EditFormClientSideComponentProperties", target.EditFormClientSideComponentProperties, parser.ParseString(contentType.EditFormClientSideComponentProperties), contentType.EditFormClientSideComponentProperties);
        }

        private static void AddIfChanged(Action<string, bool> add, string name, bool current, bool wanted)
        {
            if (current != wanted)
            {
                add(name, wanted);
            }
        }

        /// <summary>
        /// Queues a string property, when the template supplies one and it differs.
        /// </summary>
        private static void AddStringIfChanged(UpdateContentTypeRequest request, string name, string current, string wanted, string templateValue)
        {
            if (templateValue == null || string.Equals(current, wanted, StringComparison.Ordinal))
            {
                return;
            }

            request.SetString(name, wanted);
        }

        #endregion

        #region Field links

        /// <summary>
        /// Adds the column links this pass is responsible for.
        /// </summary>
        /// <returns>Whether any link was added</returns>
        private async Task<bool> AddFieldRefsAsync(PnPContext context, ProvisioningTemplate template,
            IContentType contentType, ContentTypeModel templateContentType, TokenParser parser)
        {
            List<Guid> existingLinks = contentType.FieldLinks.AsRequested().Select(fl => fl.Id).ToList();
            bool added = false;

            foreach (FieldRefModel fieldRef in templateContentType.FieldRefs)
            {
                if (existingLinks.Contains(fieldRef.Id))
                {
                    continue;
                }

                Model.Field templateField = template.SiteFields
                    .FirstOrDefault(f => f.GetFieldId(parser) == fieldRef.Id);

                FieldAndListProvisioningStepHelper.Step fieldStep = templateField != null
                    ? templateField.GetFieldProvisioningStep(parser)
                    : FieldAndListProvisioningStepHelper.Step.ListAndStandardFields;

                if (fieldStep != step)
                {
                    continue;
                }

                IField field = context.Web.Fields.AsRequested().FirstOrDefault(f => f.Id == fieldRef.Id)
                    ?? (!string.IsNullOrEmpty(fieldRef.Name)
                        ? context.Web.Fields.AsRequested().FirstOrDefault(f => f.InternalName == fieldRef.Name)
                        : null);

                if (field == null)
                {
                    // Says which of the two it is. "The column is not on this site" is equally
                    // consistent with the template never carrying it - so nothing in the run would
                    // ever have created it - and with the template carrying it and its creation
                    // having failed earlier, which is a defect. Those need different answers, and a
                    // message that cannot tell them apart costs a run against a live tenant to
                    // establish something the engine already knew when it wrote it.
                    string diagnosis = templateField == null
                        ? "and the template does not carry that column either, so nothing in this run would have created it"
                        : "even though the template carries that column, so creating it earlier in this run must have failed";

                    string message = $"The column '{fieldRef.Name ?? fieldRef.Id.ToString()}' is not on this site, " +
                        $"so it was not added to content type '{contentType.Name}' - {diagnosis}.";

                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    WriteMessage(message, ProvisioningMessageType.Warning);
                    continue;
                }

                await contentType.FieldLinks.AddAsync(field, fieldRef.DisplayName, fieldRef.Hidden,
                    fieldRef.Required, fieldRef.ReadOnly, fieldRef.ShowInDisplayForm).ConfigureAwait(false);

                added = true;
            }

            return added;
        }

        /// <summary>
        /// Brings the <c>Required</c> and <c>Hidden</c> flags of existing links in line.
        /// </summary>
        private async Task UpdateExistingFieldLinksAsync(PnPContext context, IContentType contentType, ContentTypeModel templateContentType)
        {
            bool dirty = false;

            foreach (IFieldLink fieldLink in contentType.FieldLinks.AsRequested())
            {
                FieldRefModel fieldRef = templateContentType.FieldRefs.FirstOrDefault(fr => fr.Id == fieldLink.Id);
                if (fieldRef == null)
                {
                    continue;
                }

                dirty |= SetIfChanged(fieldLink.Required, fieldRef.Required, v => fieldLink.Required = v);
                dirty |= SetIfChanged(fieldLink.Hidden, fieldRef.Hidden, v => fieldLink.Hidden = v);
            }

            if (!dirty)
            {
                return;
            }

            try
            {
                await contentType.UpdateAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Same PnP Core limitation as the property update - see SendPropertyUpdateAsync -
                // but changing an existing link's Required or Hidden flag is a refinement rather
                // than the main path, so it is reported instead of failing the run.
                string message = $"The required/hidden flags on the existing columns of content type '{contentType.Name}' could not be changed: {ex.Message}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Warning);
            }
        }

        private static bool FieldOrderDiffers(IContentType contentType, ContentTypeModel templateContentType, TokenParser parser)
        {
            string[] wanted = templateContentType.FieldRefs.Select(fr => parser.ParseString(fr.Name)).ToArray();
            if (wanted.Length == 0)
            {
                return false;
            }

            string[] actual = contentType.FieldLinks.AsRequested().Select(fl => fl.Name).ToArray();

            return !actual.SequenceEqual(wanted, StringComparer.Ordinal);
        }

        /// <summary>
        /// Puts the content type's columns into the template's order.
        /// </summary>
        private async Task ReorderFieldsAsync(PnPContext context, IContentType contentType, ContentTypeModel templateContentType, TokenParser parser)
        {
            List<string> wanted = templateContentType.FieldRefs
                .Select(fr => parser.ParseString(fr.Name))
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            if (wanted.Count == 0)
            {
                return;
            }

            try
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                await CsomRequestSender.SendAsync(context,
                    new ReorderFieldLinksRequest(siteId, webId, contentType.StringId, wanted)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string message = $"The columns of content type '{contentType.Name}' could not be reordered: {ex.Message}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Warning);
            }
        }

        #endregion

        #region Document sets

        /// <summary>
        /// Applies the document set structure a template describes.
        /// </summary>
        private async Task ApplyDocumentSetAsync(PnPContext context, ProvisioningTemplate template, IContentType contentType,
            ContentTypeModel templateContentType, TokenParser parser, bool isNoScriptSite)
        {
            DocumentSetTemplate documentSetTemplate = templateContentType.DocumentSetTemplate;

            try
            {
                IDocumentSet documentSet = await contentType.AsDocumentSetAsync().ConfigureAwait(false);

                var options = new DocumentSetOptions
                {
                    KeepExistingContentTypes = !documentSetTemplate.RemoveExistingContentTypes,
                    PropagateWelcomePageChanges = documentSetTemplate.UpdateChildren,
                };

                if (!string.IsNullOrEmpty(documentSetTemplate.WelcomePage))
                {
                    options.WelcomePageUrl = parser.ParseString(documentSetTemplate.WelcomePage);
                }

                // Remove=true entries are the template's way of saying "not this one", which maps
                // onto simply leaving them out of the replacement set.
                List<IContentType> allowed = documentSetTemplate.AllowedContentTypes
                    .Where(reference => !reference.Remove && !string.IsNullOrEmpty(reference.ContentTypeId))
                    .Select(reference => FindContentType(context, parser.ParseString(reference.ContentTypeId)))
                    .Where(ct => ct != null)
                    .ToList();

                if (allowed.Count > 0)
                {
                    options.AllowedContentTypes = allowed;
                }

                List<IField> sharedColumns = await ResolveFieldsAsync(context, documentSetTemplate.SharedFields, "shared", contentType.Name).ConfigureAwait(false);
                if (sharedColumns.Count > 0)
                {
                    options.SharedColumns = sharedColumns;
                }

                List<IField> welcomePageColumns = await ResolveFieldsAsync(context, documentSetTemplate.WelcomePageFields, "welcome page", contentType.Name).ConfigureAwait(false);
                if (welcomePageColumns.Count > 0)
                {
                    options.WelcomePageColumns = welcomePageColumns;
                }

                if (documentSetTemplate.DefaultDocuments.Any())
                {
                    if (isNoScriptSite)
                    {
                        // Default documents are files uploaded into the content type's own folder,
                        // which a NoScript site does not permit.
                        string message = $"This is a NoScript site, so the default documents of document set '{contentType.Name}' were not uploaded.";
                        context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                        WriteMessage(message, ProvisioningMessageType.Warning);
                    }
                    else
                    {
                        options.DefaultContents = BuildDefaultContents(context, template, documentSetTemplate, parser);
                    }
                }

                await documentSet.UpdateAsync(options).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string message = $"The document set settings of content type '{contentType.Name}' could not be applied: {ex.Message}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Warning);
            }
        }

        /// <summary>
        /// Makes sure the site collection can host document sets.
        /// </summary>
        private async Task<bool> EnsureDocumentSetsFeatureAsync(PnPContext context)
        {
            var documentSetsFeature = new Guid("3bae86a2-776d-499d-9db8-fa4cdc7884f8");

            try
            {
                await context.Site.LoadAsync(s => s.Features.QueryProperties(f => f.DefinitionId)).ConfigureAwait(false);

                if (context.Site.Features.AsRequested().Any(f => f.DefinitionId == documentSetsFeature))
                {
                    return true;
                }

                context.Logger?.LogInformation("{Source}: activating the Document Sets site feature.", Constants.LOGGING_SOURCE);
                await context.Site.Features.EnableAsync(documentSetsFeature).ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                string message = "This template contains a document set, but the Document Sets site collection feature " +
                    $"is not active and could not be activated: {ex.Message}. The document set was NOT created.";

                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Warning);

                return false;
            }
        }

        /// <summary>
        /// Resolves a document set's column references against the site's columns.
        /// </summary>
        private async Task<List<IField>> ResolveFieldsAsync(PnPContext context, IEnumerable<FieldReference> fieldReferences, string kind, string contentTypeName)
        {
            var resolved = new List<IField>();

            foreach (FieldReference reference in fieldReferences.Where(r => !r.Remove))
            {
                // The loaded collection is not authoritative for a column ObjectField created
                // moments ago in this same run, so a miss is re-checked against the server before
                // it is reported. Same caching rule as content types.
                IField field = context.Web.Fields.AsRequested().FirstOrDefault(f => f.Id == reference.Id)
                    ?? await FindFieldOnServerAsync(context, reference.Id).ConfigureAwait(false);

                if (field == null)
                {
                    string message = $"The {kind} column {reference.Id} of document set '{contentTypeName}' is not on this site and was skipped.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    WriteMessage(message, ProvisioningMessageType.Warning);
                    continue;
                }

                resolved.Add(field);
            }

            return resolved;
        }

        private static async Task<IField> FindFieldOnServerAsync(PnPContext context, Guid fieldId)
        {
            try
            {
                return await QueryableExtensions.FirstOrDefaultAsync(
                    context.Web.Fields.QueryProperties(f => f.Id, f => f.InternalName, f => f.Title),
                    f => f.Id == fieldId).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<DocumentSetContentOptions> BuildDefaultContents(PnPContext context, ProvisioningTemplate template,
            DocumentSetTemplate documentSetTemplate, TokenParser parser)
        {
            var contents = new List<DocumentSetContentOptions>();

            foreach (DefaultDocument document in documentSetTemplate.DefaultDocuments)
            {
                IContentType contentType = FindContentType(context, parser.ParseString(document.ContentTypeId));

                if (contentType == null)
                {
                    string message = $"The default document '{document.Name}' names content type '{document.ContentTypeId}', which is not on this site - it was skipped.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    WriteMessage(message, ProvisioningMessageType.Warning);
                    continue;
                }

                if (template.Connector == null || string.IsNullOrEmpty(document.FileSourcePath))
                {
                    continue;
                }

                contents.Add(new DocumentSetContentOptions
                {
                    FileName = document.Name,
                    ContentTypeId = contentType.StringId,
                    FolderName = string.Empty,
                });
            }

            return contents;
        }

        /// <summary>
        /// Reports a document template that cannot be uploaded from here.
        /// </summary>
        private void WarnOnUnsupportedDocumentTemplate(PnPContext context, ContentTypeModel contentType, TokenParser parser, string name)
        {
            if (contentType.DocumentSetTemplate != null || string.IsNullOrEmpty(parser.ParseString(contentType.DocumentTemplate)))
            {
                return;
            }

            string message = $"The content type '{name}' names a document template. Uploading it into _cts/{name} needs the file handler, " +
                "which lands later in phase 6 - the property was set but the file was NOT uploaded.";

            context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
            WriteMessage(message, ProvisioningMessageType.Warning);
        }

        #endregion

        #region Localization

        private async Task LocalizeAsync(PnPContext context, IContentType contentType, ContentTypeModel templateContentType, TokenParser parser)
        {
            bool localizesName = UserResources.ContainsResourceToken(templateContentType.Name);
            bool localizesDescription = UserResources.ContainsResourceToken(templateContentType.Description);

            if (!localizesName && !localizesDescription)
            {
                return;
            }

            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

            if (localizesName)
            {
                // NameResource, not TitleResource - a content type's display name is its Name.
                await UserResources.TrySetAsync(context,
                    UserResourcePath.ForContentType(siteId, webId, contentType.StringId, ResourceProperty.ContentTypeName),
                    templateContentType.Name, parser, $"the name of content type '{contentType.StringId}'",
                    m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);
            }

            if (localizesDescription)
            {
                await UserResources.TrySetAsync(context,
                    UserResourcePath.ForContentType(siteId, webId, contentType.StringId, ResourceProperty.Description),
                    templateContentType.Description, parser, $"the description of content type '{contentType.StringId}'",
                    m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);
            }
        }

        #endregion

        #region Extract

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                IWeb web = await context.Web.GetAsync(w => w.ServerRelativeUrl, w => w.Url).ConfigureAwait(false);
                await context.Site.LoadAsync(s => s.ServerRelativeUrl).ConfigureAwait(false);

                await LoadWebContentTypesAndFieldsAsync(context).ConfigureAwait(false);

                ProvisioningTemplateCreationInformation creationInformation = configuration?.ToCreationInformation();
                List<string> groupsToInclude = creationInformation?.ContentTypeGroupsToInclude ?? new List<string>();

                List<IContentType> contentTypes = context.Web.ContentTypes.AsRequested()
                    .Where(ct => !BuiltInContentTypeId.Contains(ct.StringId))
                    .Where(ct => groupsToInclude.Count == 0 || groupsToInclude.Contains(ct.Group))
                    .ToList();

                if (contentTypes.Count > 0 && IsSubSite(web))
                {
                    WriteMessage(
                        "This sub site has its own content types. That works, but they belong on the root site - " +
                        "consider excluding them from this template.", ProvisioningMessageType.Warning);
                }

                int currentIndex = 0;
                foreach (IContentType contentType in contentTypes)
                {
                    currentIndex++;
                    WriteSubProgress("Content Type", contentType.Name, currentIndex, contentTypes.Count);

                    ContentTypeModel extracted = Copy(contentType);

                    await PersistResourcesAsync(context, contentType, extracted, template, creationInformation).ConfigureAwait(false);
                    await ExtractDocumentSetAsync(context, contentType, extracted).ConfigureAwait(false);

                    template.ContentTypes.Add(extracted);
                }

                if (creationInformation?.BaseTemplate != null)
                {
                    RemoveBaseTemplateContentTypes(template, creationInformation.BaseTemplate);
                }

                WriteMessage("Done processing Content Types", ProvisioningMessageType.Completed);

                return template;
            }
        }

        private static ContentTypeModel Copy(IContentType contentType)
        {
            IEnumerable<FieldRefModel> fieldRefs = contentType.FieldLinks.AsRequested()
                .Select(fieldLink => new FieldRefModel(fieldLink.Name)
                {
                    Id = fieldLink.Id,
                    Hidden = fieldLink.Hidden,
                    Required = fieldLink.Required,
                });

            // A document template that lives under _cts/ is the content type's own copy, generated
            // per site - carrying it would point the target at a path that does not exist there.
            string documentTemplate = !string.IsNullOrEmpty(contentType.DocumentTemplate)
                && !contentType.DocumentTemplate.StartsWith("_cts/", StringComparison.Ordinal)
                    ? contentType.DocumentTemplate
                    : null;

            return new ContentTypeModel(
                contentType.StringId,
                contentType.Name,
                contentType.Description,
                contentType.Group,
                contentType.Sealed,
                contentType.Hidden,
                contentType.ReadOnly,
                documentTemplate,
                false,
                fieldRefs)
            {
                DisplayFormUrl = contentType.DisplayFormUrl,
                EditFormUrl = contentType.EditFormUrl,
                NewFormUrl = contentType.NewFormUrl,
                DisplayFormClientSideComponentId = contentType.DisplayFormClientSideComponentId,
                DisplayFormClientSideComponentProperties = contentType.DisplayFormClientSideComponentProperties,
                NewFormClientSideComponentId = contentType.NewFormClientSideComponentId,
                NewFormClientSideComponentProperties = contentType.NewFormClientSideComponentProperties,
                EditFormClientSideComponentId = contentType.EditFormClientSideComponentId,
                EditFormClientSideComponentProperties = contentType.EditFormClientSideComponentProperties,
            };
        }

        private async Task ExtractDocumentSetAsync(PnPContext context, IContentType contentType, ContentTypeModel extracted)
        {
            if (!contentType.StringId.StartsWith(BuiltInContentTypeId.DocumentSet, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                IDocumentSet documentSet = await contentType.AsDocumentSetAsync().ConfigureAwait(false);

                extracted.DocumentSetTemplate = new DocumentSetTemplate(
                    documentSet.WelcomePageUrl,
                    documentSet.AllowedContentTypes?.Select(ct => ct.Id).ToList() ?? new List<string>(),

                    // MIGRATION PHASE 6: the default documents' file bytes are exported by
                    // ObjectFiles. Their names and content types travel here so the two halves meet.
                    documentSet.DefaultContents?.Select(content => new DefaultDocument
                    {
                        Name = content.FileName,
                        ContentTypeId = content.ContentType?.Id,
                        FileSourcePath = string.Empty,
                    }).ToList() ?? new List<DefaultDocument>(),

                    documentSet.SharedColumns?.Select(f => f.Id).ToList() ?? new List<Guid>(),
                    documentSet.WelcomePageColumns?.Select(f => f.Id).ToList() ?? new List<Guid>());
            }
            catch (Exception ex)
            {
                context.Logger?.LogWarning(ex, "{Source}: could not read the document set settings of {ContentType}.",
                    Constants.LOGGING_SOURCE, contentType.Name);
            }
        }

        private static async Task PersistResourcesAsync(PnPContext context, IContentType contentType, ContentTypeModel extracted,
            ProvisioningTemplate template, ProvisioningTemplateCreationInformation creationInformation)
        {
            if (creationInformation?.PersistMultiLanguageResources != true || template.SupportedUILanguages.Count == 0)
            {
                return;
            }

            // A content type the base template already provides is dropped later, so reading its
            // translations would be wasted work.
            if (creationInformation.BaseTemplate != null
                && creationInformation.BaseTemplate.ContentTypes.Any(c => c.Id.Equals(contentType.StringId, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);
            string key = contentType.Name.Replace(" ", "_");

            string nameToken = $"ContentType_{key}_Title";
            if (await UserResources.PersistAsync(context,
                UserResourcePath.ForContentType(siteId, webId, contentType.StringId, ResourceProperty.ContentTypeName),
                nameToken, template, creationInformation).ConfigureAwait(false))
            {
                extracted.Name = UserResources.TokenFor(nameToken);
            }

            string descriptionToken = $"ContentType_{key}_Description";
            if (await UserResources.PersistAsync(context,
                UserResourcePath.ForContentType(siteId, webId, contentType.StringId, ResourceProperty.Description),
                descriptionToken, template, creationInformation).ConfigureAwait(false))
            {
                extracted.Description = UserResources.TokenFor(descriptionToken);
            }
        }

        private static void RemoveBaseTemplateContentTypes(ProvisioningTemplate template, ProvisioningTemplate baseTemplate)
        {
            foreach (ContentTypeModel contentType in baseTemplate.ContentTypes)
            {
                template.ContentTypes.RemoveAll(ct => ct.Id.Equals(contentType.Id, StringComparison.OrdinalIgnoreCase));
            }
        }

        #endregion

        #region Helpers

        private static bool SetIfChanged<T>(T current, T wanted, Action<T> set)
        {
            if (EqualityComparer<T>.Default.Equals(current, wanted))
            {
                return false;
            }

            set(wanted);
            return true;
        }

        private static bool SetIfNotEmpty(string wanted, string current, Action<string> set)
        {
            if (string.IsNullOrEmpty(wanted) || string.Equals(current, wanted, StringComparison.Ordinal))
            {
                return false;
            }

            set(wanted);
            return true;
        }

        #endregion
    }
}
