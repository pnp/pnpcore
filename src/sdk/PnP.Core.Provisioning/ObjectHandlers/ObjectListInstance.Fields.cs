using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
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
using FolderModel = PnP.Core.Provisioning.Model.Folder;
using ViewModel = PnP.Core.Provisioning.Model.View;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// The parts of <see cref="ObjectListInstance"/> that deal with a list's contents rather than
    /// its settings: columns, column references, views, folders and property bag entries.
    /// </summary>
    internal partial class ObjectListInstance
    {
        #region Field references

        /// <summary>
        /// Adds the site columns a list references, and adjusts their per-list display name,
        /// required flag and visibility.
        /// </summary>
        private async Task ProcessFieldRefsAsync(PnPContext context, ProvisioningTemplate template, ListInfo listInfo, TokenParser parser)
        {
            if (!listInfo.TemplateList.FieldRefs.Any())
            {
                return;
            }

            TokenParser listParser = listInfo.TokenParser ?? parser;
            CoreList list = listInfo.SiteList;

            Dictionary<Guid, FieldModel> siteFields = BuildSiteFieldIndex(template, parser);

            List<FieldRef> toProcess = listInfo.TemplateList.FieldRefs
                .Where(fieldRef =>
                    // A reference to a column this template also defines waits for that column's
                    // step; a reference to a column that already exists on the site is done now.
                    !siteFields.TryGetValue(fieldRef.Id, out FieldModel templateField)
                    || templateField.GetFieldProvisioningStep(listParser) == step)
                .ToList();

            if (toProcess.Count == 0)
            {
                return;
            }

            await list.LoadAsync(l => l.Fields.QueryProperties(f => f.Id, f => f.Title, f => f.InternalName,
                f => f.Hidden, f => f.Required)).ConfigureAwait(false);
            List<IField> listFields = list.Fields.AsRequested().ToList();

            int current = 0;

            foreach (FieldRef fieldRef in toProcess)
            {
                current++;
                WriteSubProgress($"Site columns for list {listInfo.TemplateList.Title}", fieldRef.Name, current, toProcess.Count);

                IField onList = listFields.FirstOrDefault(f => f.Id == fieldRef.Id);

                if (onList == null)
                {
                    IField siteColumn = await FindSiteColumnAsync(context, fieldRef.Id).ConfigureAwait(false);
                    if (siteColumn == null)
                    {
                        string warning = $"The list '{listInfo.TemplateList.Title}' references the site column '{fieldRef.Name}' " +
                            $"({fieldRef.Id}), which does not exist on this site.";
                        context.Logger?.LogError("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                        WriteMessage(warning, ProvisioningMessageType.Error);
                        continue;
                    }

                    try
                    {
                        // Adding a site column to a list is expressed by sending that column's own
                        // schema: the id in it is what tells SharePoint this is the existing site
                        // column rather than a new one that happens to look like it.
                        onList = await list.Fields.AddFieldAsXmlAsync(siteColumn.SchemaXml, false,
                            AddFieldOptionsFlags.AddFieldInternalNameHint).ConfigureAwait(false);

                        await onList.LoadAsync(f => f.Id, f => f.Title, f => f.InternalName, f => f.Hidden, f => f.Required)
                            .ConfigureAwait(false);
                        listFields.Add(onList);
                    }
                    catch (Exception ex)
                    {
                        string warning = $"The site column '{fieldRef.Name}' could not be added to list '{list.Title}': {ErrorText.Describe(ex)}";
                        context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                        WriteMessage(warning, ProvisioningMessageType.Warning);
                        continue;
                    }
                }

                await ApplyFieldRefOverridesAsync(context, list, onList, fieldRef, listParser).ConfigureAwait(false);

                parser.AddToken(new FieldTitleToken(context, onList.InternalName, onList.Title));
                parser.AddToken(new FieldIdToken(context, onList.InternalName, onList.Id));
                listParser.AddToken(new FieldTitleToken(context, onList.InternalName, onList.Title));
                listParser.AddToken(new FieldIdToken(context, onList.InternalName, onList.Id));
            }
        }

        /// <summary>
        /// Applies the three overrides a field reference can carry.
        /// </summary>
        private async Task ApplyFieldRefOverridesAsync(PnPContext context, CoreList list, IField field, FieldRef fieldRef, TokenParser parser)
        {
            bool dirty = false;
            bool localizesDisplayName = UserResources.ContainsResourceToken(fieldRef.DisplayName);

            if (!string.IsNullOrEmpty(fieldRef.DisplayName) && !localizesDisplayName)
            {
                dirty |= Set(field.Title, parser.ParseString(fieldRef.DisplayName), v => field.Title = v);
            }

            // The phonetic columns of a contacts list refuse a Hidden change, and the whole update
            // fails with them rather than just that property.
            if (CanConfigureHidden(list, fieldRef))
            {
                dirty |= Set(field.Hidden, fieldRef.Hidden, v => field.Hidden = v);
            }

            dirty |= Set(field.Required, fieldRef.Required, v => field.Required = v);

            if (dirty)
            {
                await field.UpdateAsync().ConfigureAwait(false);
            }

            if (localizesDisplayName)
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                await UserResources.TrySetAsync(context,
                    UserResourcePath.ForListField(siteId, webId, list.Id, field.Id, ResourceProperty.Title),
                    fieldRef.DisplayName, parser, $"the display name of column '{fieldRef.Name}' on list '{list.Title}'",
                    m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);
            }
        }

        private static bool CanConfigureHidden(CoreList list, FieldRef fieldRef)
        {
            if (list.TemplateType != ListTemplateType.Contacts || string.IsNullOrEmpty(fieldRef.Name))
            {
                return true;
            }

            return !fieldRef.Name.Equals("LastNamePhonetic", StringComparison.OrdinalIgnoreCase)
                && !fieldRef.Name.Equals("FirstNamePhonetic", StringComparison.OrdinalIgnoreCase)
                && !fieldRef.Name.Equals("CompanyPhonetic", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<IField> FindSiteColumnAsync(PnPContext context, Guid fieldId)
        {
            try
            {
                return await QueryableExtensions.FirstOrDefaultAsync(
                    context.Web.AvailableFields.QueryProperties(f => f.Id, f => f.InternalName, f => f.Title, f => f.SchemaXml),
                    f => f.Id == fieldId).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// The template's own site columns, indexed by id, so a field reference can tell whether the
        /// column it points at is defined here or already on the site.
        /// </summary>
        private static Dictionary<Guid, FieldModel> BuildSiteFieldIndex(ProvisioningTemplate template, TokenParser parser)
        {
            var index = new Dictionary<Guid, FieldModel>();

            foreach (FieldModel field in template.SiteFields)
            {
                XAttribute id = XElement.Parse(parser.ParseXmlString(field.SchemaXml)).Attribute("ID");
                if (id != null && Guid.TryParse(id.Value, out Guid fieldId))
                {
                    index[fieldId] = field;
                }
            }

            return index;
        }

        #endregion

        #region List columns

        /// <summary>
        /// Creates or updates the columns a list defines for itself.
        /// </summary>
        private async Task ProcessFieldsAsync(PnPContext context, ListInfo listInfo, TokenParser parser)
        {
            if (!listInfo.TemplateList.Fields.Any())
            {
                return;
            }

            TokenParser listParser = listInfo.TokenParser ?? parser;
            CoreList list = listInfo.SiteList;

            FieldModel[] toProcess = listInfo.TemplateList.Fields
                .Select(field => new
                {
                    Field = field,
                    // A FieldRef attribute marks a dependent lookup - the extra column of a lookup
                    // that projects additional values. Ordering by it puts the lookups it depends on
                    // first, because null sorts before a value.
                    DependsOn = XElement.Parse(listParser.ParseXmlString(field.SchemaXml)).Attribute("FieldRef")?.Value,
                    Step = field.GetFieldProvisioningStep(listParser),
                })
                .Where(f => f.Step == step)
                .OrderBy(f => f.DependsOn)
                .Select(f => f.Field)
                .ToArray();

            if (toProcess.Length == 0)
            {
                return;
            }

            await list.LoadAsync(l => l.Fields.QueryProperties(f => f.Id, f => f.Title, f => f.InternalName,
                f => f.SchemaXml, f => f.TypeAsString)).ConfigureAwait(false);
            List<IField> listFields = list.Fields.AsRequested().ToList();

            int current = 0;

            foreach (FieldModel field in toProcess)
            {
                XElement schema = XElement.Parse(listParser.ParseXmlString(field.SchemaXml));
                XAttribute idAttribute = schema.Attribute("ID");

                if (idAttribute == null || !Guid.TryParse(idAttribute.Value, out Guid fieldId))
                {
                    string message = $"A column of list '{listInfo.TemplateList.Title}' has no valid ID attribute: {field.SchemaXml}";
                    context.Logger?.LogError("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    throw new Exception(message);
                }

                string internalName = schema.Attribute("InternalName")?.Value ?? schema.Attribute("Name")?.Value;

                current++;
                WriteSubProgress($"List columns for list {listInfo.TemplateList.Title}",
                    internalName ?? idAttribute.Value, current, toProcess.Length);

                IField existing = listFields.FirstOrDefault(f => f.Id == fieldId);
                IField result = existing == null
                    ? await CreateListFieldAsync(context, listInfo, schema, field.SchemaXml, listParser).ConfigureAwait(false)
                    : await UpdateListFieldAsync(context, listInfo, existing, schema, field.SchemaXml, listParser).ConfigureAwait(false);

                if (result == null)
                {
                    continue;
                }

                await result.LoadAsync(f => f.Id, f => f.Title, f => f.InternalName).ConfigureAwait(false);

                parser.AddToken(new FieldTitleToken(context, result.InternalName, result.Title));
                parser.AddToken(new FieldIdToken(context, result.InternalName, result.Id));
                listParser.AddToken(new FieldTitleToken(context, result.InternalName, result.Title));
                listParser.AddToken(new FieldIdToken(context, result.InternalName, result.Id));
            }
        }

        private async Task<IField> CreateListFieldAsync(PnPContext context, ListInfo listInfo, XElement schema,
            string originalSchemaXml, TokenParser parser)
        {
            string fieldXml = PrepareFieldXml(schema);

            if (!await IsFieldXmlValidAsync(parser.ParseXmlString(originalSchemaXml), parser, context).ConfigureAwait(false))
            {
                string leftOver = string.Join(" ", parser.GetLeftOverTokens(originalSchemaXml));
                string message = $"The column definition for list '{listInfo.TemplateList.Title}' is not valid: {leftOver}";
                context.Logger?.LogError("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                throw new Exception(message);
            }

            fieldXml = await FieldUtilities.FixLookupFieldAsync(context, fieldXml,
                unresolved => WriteMessage(
                    $"The lookup column on list '{listInfo.TemplateList.Title}' points at '{unresolved}', which does not exist on this site.",
                    ProvisioningMessageType.Warning)).ConfigureAwait(false);

            // AddToNoContentType when content types are on: the template's content type bindings
            // decide which content types carry the column, and adding it to the default one as well
            // would put it on items the template never said it should appear on.
            AddFieldOptionsFlags options = AddFieldOptionsFlags.AddFieldInternalNameHint
                | (listInfo.TemplateList.ContentTypesEnabled
                    ? AddFieldOptionsFlags.AddToNoContentType
                    : AddFieldOptionsFlags.AddToDefaultContentType);

            IField created = await listInfo.SiteList.Fields.AddFieldAsXmlAsync(fieldXml, false, options).ConfigureAwait(false);

            await LocalizeFieldAsync(context, listInfo.SiteList, created, originalSchemaXml, parser).ConfigureAwait(false);

            return created;
        }

        private async Task<IField> UpdateListFieldAsync(PnPContext context, ListInfo listInfo, IField existing,
            XElement templateSchema, string originalSchemaXml, TokenParser parser)
        {
            XElement existingSchema = XElement.Parse(existing.SchemaXml);

            if (new XNodeEqualityComparer().GetHashCode(existingSchema) == new XNodeEqualityComparer().GetHashCode(templateSchema))
            {
                return existing;
            }

            if (existingSchema.Attribute("Type")?.Value != templateSchema.Attribute("Type")?.Value)
            {
                string fieldName = (string)existingSchema.Attribute("Name") ?? (string)existingSchema.Attribute("StaticName");
                string warning = $"The column '{fieldName}' exists on list '{listInfo.TemplateList.Title}' but is of a different type, so it was skipped.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return null;
            }

            if (!await IsFieldXmlValidAsync(parser.ParseXmlString(templateSchema.ToString()), parser, context).ConfigureAwait(false))
            {
                string leftOver = string.Join(" ", parser.GetLeftOverTokens(originalSchemaXml));
                string message = $"The column definition for list '{listInfo.TemplateList.Title}' is not valid: {leftOver}";
                context.Logger?.LogError("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                throw new Exception(message);
            }

            MergeInto(existingSchema, PrepareFieldElement(templateSchema));

            if (string.Equals(templateSchema.Attribute("Type")?.Value, "Calculated", StringComparison.OrdinalIgnoreCase))
            {
                // The server recomputes FieldRefs from the formula. Sending stale ones back makes it
                // reject the update.
                existingSchema.Descendants("FieldRefs").FirstOrDefault()?.Remove();
            }

            // Version is server state; echoing it back is treated as a concurrency claim.
            existingSchema.Attributes("Version").Remove();

            existing.SchemaXml = parser.ParseXmlString(
                await FieldUtilities.FixLookupFieldAsync(context, existingSchema.ToString()).ConfigureAwait(false));

            await existing.UpdateAsync().ConfigureAwait(false);
            await LocalizeFieldAsync(context, listInfo.SiteList, existing, originalSchemaXml, parser).ConfigureAwait(false);

            return existing;
        }

        private async Task LocalizeFieldAsync(PnPContext context, CoreList list, IField field, string originalSchemaXml, TokenParser parser)
        {
            if (!UserResources.ContainsResourceToken(originalSchemaXml))
            {
                return;
            }

            XElement original = XElement.Parse(originalSchemaXml);
            string displayName = (string)original.Attribute("DisplayName");
            string description = (string)original.Attribute("Description");

            bool localizesDisplayName = UserResources.ContainsResourceToken(displayName);
            bool localizesDescription = UserResources.ContainsResourceToken(description);

            if (!localizesDisplayName && !localizesDescription)
            {
                return;
            }

            await field.LoadAsync(f => f.Id).ConfigureAwait(false);
            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

            if (localizesDisplayName)
            {
                await UserResources.TrySetAsync(context,
                    UserResourcePath.ForListField(siteId, webId, list.Id, field.Id, ResourceProperty.Title),
                    displayName, parser, $"the display name of a column on list '{list.Title}'",
                    m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);
            }

            if (localizesDescription)
            {
                await UserResources.TrySetAsync(context,
                    UserResourcePath.ForListField(siteId, webId, list.Id, field.Id, ResourceProperty.Description),
                    description, parser, $"the description of a column on list '{list.Title}'",
                    m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Normalises a lookup column's delete behaviour before it is sent.
        /// </summary>
        private static XElement PrepareFieldElement(XElement fieldElement)
        {
            if (fieldElement.Attribute("List") == null)
            {
                return fieldElement;
            }

            XAttribute deleteBehavior = fieldElement.Attribute("RelationshipDeleteBehavior");
            if (deleteBehavior == null)
            {
                return fieldElement;
            }

            if (deleteBehavior.Value.Equals("Restrict", StringComparison.OrdinalIgnoreCase)
                || deleteBehavior.Value.Equals("Cascade", StringComparison.OrdinalIgnoreCase))
            {
                fieldElement.SetAttributeValue("Indexed", "TRUE");
            }

            deleteBehavior.Remove();

            return fieldElement;
        }

        private static string PrepareFieldXml(XElement fieldElement)
        {
            return PrepareFieldElement(fieldElement).ToString();
        }

        /// <summary>
        /// Overlays the template's attributes and child elements onto the existing schema.
        /// </summary>
        private static void MergeInto(XElement existingSchema, XElement templateSchema)
        {
            foreach (XAttribute attribute in templateSchema.Attributes())
            {
                existingSchema.SetAttributeValue(attribute.Name, attribute.Value);
            }

            foreach (XElement element in templateSchema.Elements())
            {
                existingSchema.Element(element.Name)?.Remove();
                existingSchema.Add(element);
            }
        }

        #endregion

        #region Field defaults and default column values

        /// <summary>
        /// Writes the per-column default value a list carries.
        /// </summary>
        private async Task ProcessFieldDefaultsAsync(PnPContext context, ListInfo listInfo)
        {
            if (listInfo.TemplateList.FieldDefaults.Count == 0)
            {
                return;
            }

            CoreList list = listInfo.SiteList;
            await list.LoadAsync(l => l.Fields.QueryProperties(f => f.Id, f => f.InternalName, f => f.Title,
                f => f.TypeAsString, f => f.DefaultValue)).ConfigureAwait(false);

            List<IField> fields = list.Fields.AsRequested().ToList();

            foreach (KeyValuePair<string, string> fieldDefault in listInfo.TemplateList.FieldDefaults)
            {
                IField field = fields.FirstOrDefault(f =>
                    string.Equals(f.InternalName, fieldDefault.Key, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(f.Title, fieldDefault.Key, StringComparison.OrdinalIgnoreCase));

                if (field == null)
                {
                    string warning = $"The list '{list.Title}' sets a default for column '{fieldDefault.Key}', which does not exist on it.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    continue;
                }

                string value = listInfo.TokenParser.ParseString(fieldDefault.Value);

                if (!string.IsNullOrEmpty(value) && field.TypeAsString.StartsWith("TaxonomyField", StringComparison.Ordinal))
                {
                    // A taxonomy default is stored as "<id>;#<label>|<term guid>", where the id is
                    // the term's row in the site's own hidden taxonomy list and therefore differs
                    // per site. Resolving it needs the validated-string call, which is the same gap
                    // ObjectField records - see backlog T10.
                    string warning = $"The default value of taxonomy column '{field.InternalName}' on list '{list.Title}' " +
                        "was written as given. It may need re-validating against this site's term store.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }

                field.DefaultValue = value;
                await field.UpdateAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes the per-folder default column values, for the list root and every folder below it.
        /// </summary>
        private async Task ProcessDefaultColumnValuesAsync(PnPContext context, ListInfo listInfo, TokenParser parser)
        {
            var values = new List<DefaultColumnValueOptions>();

            CollectDefaultColumnValues(listInfo.TemplateList.DefaultColumnValues, listInfo.TemplateList.Folders,
                string.Empty, values, listInfo.TokenParser ?? parser);

            if (values.Count == 0)
            {
                return;
            }

            try
            {
                await listInfo.SiteList.SetDefaultColumnValuesAsync(values).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The default column values of list '{listInfo.SiteList.Title}' could not be set: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        private static void CollectDefaultColumnValues(Dictionary<string, string> defaults, IEnumerable<FolderModel> folders,
            string folderPath, List<DefaultColumnValueOptions> collected, TokenParser parser)
        {
            if (defaults != null)
            {
                foreach (KeyValuePair<string, string> entry in defaults)
                {
                    string value = parser.ParseString(entry.Value);
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    collected.Add(new DefaultColumnValueOptions
                    {
                        FieldInternalName = parser.ParseString(entry.Key),
                        FolderRelativePath = folderPath,
                        DefaultValue = value,
                    });
                }
            }

            if (folders == null)
            {
                return;
            }

            foreach (FolderModel folder in folders)
            {
                string childPath = folder.Name.Length > 0 ? $"{folderPath}/{folder.Name}" : folderPath;
                CollectDefaultColumnValues(folder.DefaultColumnValues, folder.Folders, childPath, collected, parser);
            }
        }

        #endregion

        #region Views

        private async Task ProcessViewsAsync(PnPContext context, ListInfo listInfo)
        {
            ListInstance templateList = listInfo.TemplateList;
            CoreList list = listInfo.SiteList;

            if (!templateList.Views.Any())
            {
                return;
            }

            await list.LoadAsync(l => l.Views.QueryProperties(v => v.Id, v => v.Title, v => v.Hidden,
                v => v.ServerRelativeUrl, v => v.Scope, v => v.JSLink, v => v.Aggregations, v => v.ViewData,
                v => v.CustomFormatter, v => v.MobileView, v => v.MobileDefaultView)).ConfigureAwait(false);

            if (templateList.RemoveExistingViews)
            {
                foreach (IView view in list.Views.AsRequested().ToList())
                {
                    if (ListViewProvisioner.IsWebPartView(view, templateList.Url))
                    {
                        continue;
                    }

                    await view.DeleteAsync().ConfigureAwait(false);
                }

                await list.LoadAsync(l => l.Views.QueryProperties(v => v.Id, v => v.Title, v => v.Hidden,
                    v => v.ServerRelativeUrl)).ConfigureAwait(false);
            }

            int current = 0;

            foreach (ViewModel view in templateList.Views)
            {
                current++;
                WriteSubProgress($"Views for list {list.Title}", $"{current}", current, templateList.Views.Count);

                await ListViewProvisioner.CreateAsync(context, list, view, listInfo.TokenParser,
                    m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);
            }
        }

        #endregion

        #region Folders

        private async Task ProcessFoldersAsync(PnPContext context, ListInfo listInfo)
        {
            CoreList list = listInfo.SiteList;

            if (listInfo.TemplateList.Folders == null || listInfo.TemplateList.Folders.Count == 0)
            {
                return;
            }

            if (list.BaseType != ListBaseType.DocumentLibrary && list.BaseType != ListBaseType.GenericList)
            {
                return;
            }

            // Folder creation has to be on for the folders to be addable, even when the template
            // wants it off afterwards.
            bool folderCreationWasOn = list.EnableFolderCreation;
            if (!folderCreationWasOn)
            {
                list.EnableFolderCreation = true;
                await list.UpdateAsync().ConfigureAwait(false);
            }

            try
            {
                foreach (FolderModel folder in listInfo.TemplateList.Folders)
                {
                    await CreateFolderAsync(context, listInfo, string.Empty, folder).ConfigureAwait(false);
                }
            }
            finally
            {
                if (!folderCreationWasOn)
                {
                    list.EnableFolderCreation = false;
                    await list.UpdateAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Creates one folder and everything below it.
        /// </summary>
        private async Task CreateFolderAsync(PnPContext context, ListInfo listInfo, string parentPath, FolderModel folder)
        {
            TokenParser parser = listInfo.TokenParser;
            string name = parser.ParseString(folder.Name);

            if (name == "/")
            {
                foreach (FolderModel child in folder.Folders ?? Enumerable.Empty<FolderModel>())
                {
                    await CreateFolderAsync(context, listInfo, parentPath, child).ConfigureAwait(false);
                }

                await ApplyFolderPropertyBagAsync(context, listInfo, parentPath, folder).ConfigureAwait(false);
                return;
            }

            string path = string.IsNullOrEmpty(parentPath) ? name : $"{parentPath}/{name}";

            try
            {
                await listInfo.SiteList.AddListFolderAsync(name, string.IsNullOrEmpty(parentPath) ? null : parentPath)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Already there is the common case, and it is not an error - a delta template
                // routinely re-declares folders it created on an earlier run. Anything else is
                // reported, because a folder that silently fails to appear takes its child folders,
                // its default column values and its permissions with it.
                if (await FolderExistsAsync(context, listInfo, path).ConfigureAwait(false))
                {
                    context.Logger?.LogDebug(ex, "{Source}: folder '{Path}' on list '{List}' already exists.",
                        Constants.LOGGING_SOURCE, path, listInfo.SiteList.Title);
                }
                else
                {
                    string warning = $"The folder '{path}' on list '{listInfo.SiteList.Title}' could not be created: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    return;
                }
            }

            foreach (FolderModel child in folder.Folders ?? Enumerable.Empty<FolderModel>())
            {
                await CreateFolderAsync(context, listInfo, path, child).ConfigureAwait(false);
            }

            await ApplyFolderPropertyBagAsync(context, listInfo, path, folder).ConfigureAwait(false);
            await ApplyFolderSecurityAsync(context, listInfo, path, folder).ConfigureAwait(false);
        }

        private async Task ApplyFolderPropertyBagAsync(PnPContext context, ListInfo listInfo, string path, FolderModel folder)
        {
            if (folder.PropertyBagEntries == null || folder.PropertyBagEntries.Count == 0)
            {
                return;
            }

            try
            {
                IFolder target = await GetFolderAsync(context, listInfo, path).ConfigureAwait(false);
                await target.LoadAsync(f => f.Properties).ConfigureAwait(false);

                foreach (PropertyBagEntry entry in folder.PropertyBagEntries)
                {
                    target.Properties[listInfo.TokenParser.ParseString(entry.Key)] =
                        listInfo.TokenParser.ParseString(entry.Value);
                }

                await target.Properties.UpdateAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The property bag of folder '{path}' on list '{listInfo.SiteList.Title}' could not be set: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        private async Task ApplyFolderSecurityAsync(PnPContext context, ListInfo listInfo, string path, FolderModel folder)
        {
            if (folder.Security == null || folder.Security.RoleAssignments.Count == 0)
            {
                return;
            }

            try
            {
                IFolder target = await GetFolderAsync(context, listInfo, path).ConfigureAwait(false);
                await target.LoadAsync(f => f.ListItemAllFields).ConfigureAwait(false);

                await SecurityUtilities.ApplyAsync(context, target.ListItemAllFields, folder.Security,
                    listInfo.TokenParser, $"folder '{path}' on list '{listInfo.SiteList.Title}'",
                    m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The permissions of folder '{path}' on list '{listInfo.SiteList.Title}' could not be set: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        private static async Task<bool> FolderExistsAsync(PnPContext context, ListInfo listInfo, string path)
        {
            try
            {
                return await GetFolderAsync(context, listInfo, path).ConfigureAwait(false) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static async Task<IFolder> GetFolderAsync(PnPContext context, ListInfo listInfo, string path)
        {
            string rootUrl = listInfo.SiteList.RootFolder.ServerRelativeUrl;
            string url = string.IsNullOrEmpty(path) ? rootUrl : $"{rootUrl}/{path}";

            return await context.Web.GetFolderByServerRelativeUrlAsync(url).ConfigureAwait(false);
        }

        #endregion

        #region Property bag and audience targeting

        /// <summary>
        /// Writes the list's own property bag entries, which live on its root folder.
        /// </summary>
        private async Task ProcessPropertyBagEntriesAsync(PnPContext context, ListInfo listInfo)
        {
            PropertyBagEntryCollection entries = listInfo.TemplateList.PropertyBagEntries;

            if (entries == null || entries.Count == 0)
            {
                return;
            }

            CoreList list = listInfo.SiteList;
            IFolder rootFolder = list.RootFolder;
            await rootFolder.LoadAsync(f => f.Properties).ConfigureAwait(false);

            var indexedKeys = ReadIndexedPropertyKeys(rootFolder);
            bool indexChanged = false;
            bool propertiesChanged = false;

            foreach (PropertyBagEntry entry in entries)
            {
                string key = listInfo.TokenParser.ParseString(entry.Key);

                if (rootFolder.Properties.Values.ContainsKey(key) && !entry.Overwrite)
                {
                    continue;
                }

                rootFolder.Properties[key] = listInfo.TokenParser.ParseString(entry.Value);
                propertiesChanged = true;

                if (entry.Indexed && !indexedKeys.Contains(key))
                {
                    indexedKeys.Add(key);
                    indexChanged = true;
                }
                else if (!entry.Indexed && indexedKeys.Remove(key))
                {
                    indexChanged = true;
                }
            }

            if (indexChanged)
            {
                // Indexing has to be written together with the values: the index key list is itself
                // a property bag entry, and a second update would overwrite the values just set.
                rootFolder.Properties[IndexedPropertyKeysName] = EncodeIndexedPropertyKeys(indexedKeys);
                propertiesChanged = true;
            }

            if (propertiesChanged)
            {
                await rootFolder.Properties.UpdateAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The property bag entry SharePoint stores a list's indexed column names in.
        /// </summary>
        private const string IndexedPropertyKeysName = "vti_indexedpropertykeys";

        /// <summary>
        /// Reads the indexed key list, which is stored as pipe-separated base64 of the UTF-16 key.
        /// </summary>
        private static List<string> ReadIndexedPropertyKeys(IFolder folder)
        {
            var keys = new List<string>();

            if (!folder.Properties.Values.TryGetValue(IndexedPropertyKeysName, out object raw) || raw == null)
            {
                return keys;
            }

            foreach (string encoded in raw.ToString().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    keys.Add(System.Text.Encoding.Unicode.GetString(Convert.FromBase64String(encoded)));
                }
                catch (FormatException)
                {
                    // A malformed entry is dropped rather than allowed to break the whole write.
                }
            }

            return keys;
        }

        private static string EncodeIndexedPropertyKeys(IEnumerable<string> keys)
        {
            return string.Concat(keys.Select(k => Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(k)) + "|"));
        }

        /// <summary>
        /// Turns on modern and classic audience targeting.
        /// </summary>
        private async Task ProcessAudienceTargetingAsync(PnPContext context, ListInfo listInfo)
        {
            CoreList list = listInfo.SiteList;

            if (listInfo.TemplateList.EnableAudienceTargeting)
            {
                try
                {
                    await list.EnableAudienceTargetingAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string warning = $"Modern audience targeting could not be enabled on list '{list.Title}': {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }

            if (!listInfo.TemplateList.EnableClassicAudienceTargeting)
            {
                return;
            }

            try
            {
                await list.LoadAsync(l => l.Fields.QueryProperties(f => f.InternalName)).ConfigureAwait(false);

                if (list.Fields.AsRequested().Any(f => f.InternalName == ClassicAudienceTargetingInternalName))
                {
                    return;
                }

                AddFieldOptionsFlags options = AddFieldOptionsFlags.AddFieldInternalNameHint
                    | (list.ContentTypesEnabled
                        ? AddFieldOptionsFlags.AddToNoContentType
                        : AddFieldOptionsFlags.AddToDefaultContentType);

                await list.Fields.AddFieldAsXmlAsync(ClassicAudienceTargetingFieldXml(list.Id), false, options)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"Classic audience targeting could not be enabled on list '{list.Title}': {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        private const string ClassicAudienceTargetingInternalName = "Target_x0020_Audiences";

        private static string ClassicAudienceTargetingFieldXml(Guid listId)
        {
            return $@"<Field ID=""{{61cbb965-1e04-4273-b658-eedaa662f48d}}"" Type=""TargetTo"" Name=""Target_x0020_Audiences""
    DisplayName=""Target Audiences"" Required=""FALSE"" SourceID=""{listId}"" StaticName=""Target_x0020_Audiences"" Version=""2"">
  <Customization><ArrayOfProperty>
    <Property><Name>AllowGlobalAudience</Name><Value xmlns:q1=""http://www.w3.org/2001/XMLSchema"" p4:type=""q1:boolean"" xmlns:p4=""http://www.w3.org/2001/XMLSchema-instance"">true</Value></Property>
    <Property><Name>AllowDL</Name><Value xmlns:q2=""http://www.w3.org/2001/XMLSchema"" p4:type=""q2:boolean"" xmlns:p4=""http://www.w3.org/2001/XMLSchema-instance"">true</Value></Property>
    <Property><Name>AllowSPGroup</Name><Value xmlns:q3=""http://www.w3.org/2001/XMLSchema"" p4:type=""q3:boolean"" xmlns:p4=""http://www.w3.org/2001/XMLSchema-instance"">true</Value></Property>
  </ArrayOfProperty></Customization>
</Field>";
        }

        #endregion
    }
}
