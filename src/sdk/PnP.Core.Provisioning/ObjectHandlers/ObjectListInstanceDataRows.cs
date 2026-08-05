using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.SharePoint.InformationArchitecture;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreList = PnP.Core.Model.SharePoint.IList;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    using UrlUtility = PnP.Core.Provisioning.Utilities.UrlUtility;

    /// <summary>
    /// Writes the <c>&lt;pnp:DataRows&gt;</c> a list instance carries - the list items a template
    /// ships as content rather than as structure.
    /// </summary>
    internal class ObjectListInstanceDataRows : ObjectHandlerBase
    {
        public override string Name => "List instance data rows";

        public override string InternalName => "ListInstanceDataRows";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.Lists.Any(l => l.DataRows != null && l.DataRows.Any());
            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            // Rows are only ever extracted for lists a caller named explicitly, with IncludeItems on.
            _willExtract ??= configuration?.Lists != null
                && configuration.Lists.HasLists
                && configuration.Lists.Lists.Any(l => l.IncludeItems);

            return _willExtract.Value;
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template,
            TokenParser parser, ApplyConfiguration configuration)
        {
            if (!WillProvision(context, template, configuration))
            {
                return parser;
            }

            IWeb web = context.Web;
            await web.LoadAsync(w => w.ServerRelativeUrl,
                w => w.Lists.QueryProperties(l => l.Id, l => l.Title, l => l.BaseType,
                    l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl))).ConfigureAwait(false);

            string webUrl = web.ServerRelativeUrl.TrimEnd('/');
            List<CoreList> lists = web.Lists.AsRequested().ToList();

            bool ignoreDuplicates = configuration?.Lists?.IgnoreDuplicateDataRowErrors ?? false;

            foreach (ListInstance listInstance in template.Lists)
            {
                if (listInstance.DataRows == null || !listInstance.DataRows.Any())
                {
                    continue;
                }

                string url = UrlUtility.Combine(webUrl, parser.ParseString(listInstance.Url));

                CoreList list = lists.FirstOrDefault(
                    l => string.Equals(l.RootFolder.ServerRelativeUrl, url, StringComparison.OrdinalIgnoreCase));

                if (list == null)
                {
                    string warning = $"The template has data rows for the list at '{listInstance.Url}', which does not exist on this site.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    continue;
                }

                await ProcessRowsAsync(context, template, listInstance, list, parser, ignoreDuplicates).ConfigureAwait(false);
            }

            WriteMessage("Done processing list data rows", ProvisioningMessageType.Completed);

            return parser;
        }

        private async Task ProcessRowsAsync(PnPContext context, ProvisioningTemplate template, ListInstance listInstance,
            CoreList list, TokenParser parser, bool ignoreDuplicates)
        {
            string keyColumn = parser.ParseString(listInstance.DataRows.KeyColumn);
            string keyColumnType = await ResolveKeyColumnTypeAsync(list, keyColumn).ConfigureAwait(false);

            int index = 0;
            int total = listInstance.DataRows.Count;

            foreach (DataRow dataRow in listInstance.DataRows)
            {
                index++;
                WriteSubProgress($"Data rows for list {listInstance.Title}", $"{index}", index, total);

                try
                {
                    IListItem existing = await FindExistingItemAsync(context, list, dataRow, keyColumn, keyColumnType, parser)
                        .ConfigureAwait(false);

                    if (existing != null && listInstance.DataRows.UpdateBehavior == UpdateBehavior.Skip)
                    {
                        continue;
                    }

                    Dictionary<string, object> values = await ListItemUtilities.BuildValuesAsync(
                        context, list, dataRow.Values, parser,
                        m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);

                    IListItem item = existing;

                    if (item == null)
                    {
                        item = await list.Items.AddAsync(values).ConfigureAwait(false);
                    }
                    else
                    {
                        foreach (KeyValuePair<string, object> value in values)
                        {
                            item.Values[value.Key] = value.Value;
                        }

                        // Overwrite rather than create a version: a provisioning run is not an edit
                        // by a person, and a template applied repeatedly would otherwise inflate the
                        // version history of every row it touches.
                        await item.UpdateOverwriteVersionAsync().ConfigureAwait(false);
                    }

                    await ApplyAttachmentsAsync(context, template, item, dataRow, parser, isNew: existing == null)
                        .ConfigureAwait(false);

                    if (HasSecurity(dataRow))
                    {
                        await SecurityUtilities.ApplyAsync(context, item, dataRow.Security, parser,
                            $"an item of list '{list.Title}'", m => WriteMessage(m, ProvisioningMessageType.Warning))
                            .ConfigureAwait(false);
                    }
                }
                catch (Exception ex) when (ignoreDuplicates && IsDuplicateValue(ex))
                {
                    string warning = $"A data row of list '{listInstance.Title}' duplicates a value in a column that " +
                        "enforces unique values, and was skipped.";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
                catch (Exception ex) when (IsAddToDocumentLibrary(ex))
                {
                    // A document library's items are files; there is nothing to add without content.
                    // The template is wrong, but failing the whole run over it helps nobody.
                    string warning = $"The list '{listInstance.Title}' is a document library, so its items cannot be " +
                        "created from data rows. Use <pnp:Files> instead.";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    return;
                }
            }
        }

        /// <summary>
        /// The CAML type name the key column's value has to be compared as.
        /// </summary>
        private static async Task<string> ResolveKeyColumnTypeAsync(CoreList list, string keyColumn)
        {
            if (string.IsNullOrEmpty(keyColumn))
            {
                return "Text";
            }

            await list.LoadAsync(l => l.Fields.QueryProperties(f => f.InternalName, f => f.FieldTypeKind)).ConfigureAwait(false);

            IField field = list.Fields.AsRequested()
                .FirstOrDefault(f => string.Equals(f.InternalName, keyColumn, StringComparison.OrdinalIgnoreCase));

            if (field == null)
            {
                return "Text";
            }

            return field.FieldTypeKind switch
            {
                FieldType.User or FieldType.Lookup => "Lookup",
                FieldType.URL => "Url",
                FieldType.DateTime => "DateTime",
                FieldType.Number or FieldType.Counter => "Number",
                _ => "Text",
            };
        }

        private static async Task<IListItem> FindExistingItemAsync(PnPContext context, CoreList list, DataRow dataRow,
            string keyColumn, string keyColumnType, TokenParser parser)
        {
            if (string.IsNullOrEmpty(keyColumn) || dataRow.Values == null)
            {
                return null;
            }

            KeyValuePair<string, string> keyValue = dataRow.Values
                .FirstOrDefault(v => string.Equals(v.Key, keyColumn, StringComparison.OrdinalIgnoreCase));

            if (keyValue.Key == null)
            {
                return null;
            }

            string value = parser.ParseString(keyValue.Value);

            if (keyColumnType == "DateTime" && DateTime.TryParse(value, out DateTime parsed))
            {
                // CAML wants the ISO sortable form; anything else compares as text and never matches.
                value = parsed.ToString("s", CultureInfo.InvariantCulture) + "Z";
            }

            string timeAttribute = keyColumnType == "DateTime" ? " IncludeTimeValue='TRUE'" : string.Empty;
            string query = $"<View><Query><Where><Eq><FieldRef Name=\"{keyColumn}\"/>" +
                $"<Value{timeAttribute} Type=\"{keyColumnType}\">{System.Security.SecurityElement.Escape(value)}</Value>" +
                "</Eq></Where></Query><RowLimit>1</RowLimit></View>";

            try
            {
                await list.LoadItemsByCamlQueryAsync(query).ConfigureAwait(false);

                // 🔴 The match is re-checked against the key value, not taken as "the first item the
                // collection now holds". PnP Core materialises IListItemCollection once and a CAML
                // load merges into it rather than replacing it, so after row one is added the
                // collection still holds row one when row two's query returns nothing - and row two
                // would silently overwrite row one. Two rows in, one row out.
                return list.Items.AsRequested()
                    .FirstOrDefault(i => MatchesKey(i, keyColumn, value, keyColumnType));
            }
            catch (Exception ex)
            {
                context.Logger?.LogWarning(ex, "{Source}: the key column query on list '{List}' failed; the row is treated as new.",
                    Constants.LOGGING_SOURCE, list.Title);
                return null;
            }
        }

        private static bool MatchesKey(IListItem item, string keyColumn, string wanted, string keyColumnType)
        {
            if (!item.Values.TryGetValue(keyColumn, out object stored) || stored == null)
            {
                return false;
            }

            if (keyColumnType == "DateTime" && stored is DateTime storedDate
                && DateTime.TryParse(wanted.TrimEnd('Z'), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime wantedDate))
            {
                return storedDate == wantedDate;
            }

            if (stored is IFieldLookupValue lookup)
            {
                return lookup.LookupId.ToString(CultureInfo.InvariantCulture) == wanted;
            }

            return string.Equals(Convert.ToString(stored, CultureInfo.InvariantCulture), wanted, StringComparison.Ordinal);
        }

        private async Task ApplyAttachmentsAsync(PnPContext context, ProvisioningTemplate template, IListItem item,
            DataRow dataRow, TokenParser parser, bool isNew)
        {
            if (dataRow.Attachments == null || dataRow.Attachments.Count == 0)
            {
                return;
            }

            List<IAttachment> existing = new List<IAttachment>();

            if (!isNew)
            {
                await item.LoadAsync(i => i.AttachmentFiles.QueryProperties(a => a.FileName)).ConfigureAwait(false);
                existing = item.AttachmentFiles.AsRequested().ToList();
            }

            foreach (DataRowAttachment attachment in dataRow.Attachments)
            {
                string name = parser.ParseString(attachment.Name);
                string source = parser.ParseString(attachment.Src);

                IAttachment match = existing.FirstOrDefault(
                    a => string.Equals(a.FileName, name, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    if (!attachment.Overwrite)
                    {
                        continue;
                    }

                    // There is no replace: an attachment of the same name has to go first.
                    await match.DeleteAsync().ConfigureAwait(false);
                    existing.Remove(match);
                }

                try
                {
                    if (template.Connector == null)
                    {
                        string warning = $"The attachment '{source}' cannot be read: the template has no file connector.";
                        context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                        WriteMessage(warning, ProvisioningMessageType.Warning);
                        continue;
                    }

                    byte[] bytes = ConnectorFileHelper.GetFileBytes(template.Connector, source);

                    using (var content = new MemoryStream(bytes))
                    {
                        await item.AttachmentFiles.AddAsync(name, content).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    string warning = $"The attachment '{name}' could not be added: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }
        }

        private static bool HasSecurity(DataRow dataRow)
        {
            return dataRow.Security != null
                && (dataRow.Security.ClearSubscopes
                    || dataRow.Security.CopyRoleAssignments
                    || dataRow.Security.RoleAssignments.Count > 0);
        }

        /// <summary>
        /// Whether the failure is a unique-value violation, which a template may choose to tolerate.
        /// </summary>
        private static bool IsDuplicateValue(Exception ex)
        {
            string text = ErrorText.Describe(ex);

            return text.IndexOf("SPDuplicateValuesFoundException", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("must have unique values", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("already exists", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsAddToDocumentLibrary(Exception ex)
        {
            return ErrorText.Describe(ex)
                .IndexOf("use SPFileCollection.Add()", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion

        #region Extract

        /// <summary>
        /// Reads the items of the lists a caller asked for back into their list instances.
        /// </summary>
        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            if (!WillExtract(context, template, configuration))
            {
                return template;
            }

            IWeb web = context.Web;
            await web.LoadAsync(w => w.ServerRelativeUrl, w => w.Url,
                w => w.Lists.QueryProperties(l => l.Id, l => l.Title, l => l.BaseType, l => l.EnableAttachments,
                    l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl))).ConfigureAwait(false);

            string webUrl = web.ServerRelativeUrl.TrimEnd('/');
            List<CoreList> lists = web.Lists.AsRequested().ToList();

            foreach (Model.Configuration.Lists.Lists.ExtractListsListsConfiguration listConfig
                in configuration.Lists.Lists.Where(l => l.IncludeItems))
            {
                CoreList siteList = lists.FirstOrDefault(l =>
                    string.Equals(l.Title, listConfig.Title, StringComparison.Ordinal)
                    || l.RootFolder.ServerRelativeUrl.EndsWith(listConfig.Title, StringComparison.OrdinalIgnoreCase)
                    || (Guid.TryParse(listConfig.Title, out Guid id) && l.Id == id));

                if (siteList == null)
                {
                    string warning = $"Items were requested for the list '{listConfig.Title}', which does not exist on this site.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    continue;
                }

                ListInstance listInstance = template.Lists.FirstOrDefault(l =>
                    string.Equals(UrlUtility.Combine(webUrl, l.Url), siteList.RootFolder.ServerRelativeUrl,
                        StringComparison.OrdinalIgnoreCase));

                if (listInstance == null)
                {
                    // The list itself was not extracted, so there is nothing to hang the rows off.
                    // That happens when the Lists handler is excluded, and is the caller's choice.
                    context.Logger?.LogDebug("{Source}: list '{List}' has no instance in the template; its items were skipped.",
                        Constants.LOGGING_SOURCE, siteList.Title);
                    continue;
                }

                if (siteList.BaseType == ListBaseType.DocumentLibrary)
                {
                    string warning = $"Items were requested for '{siteList.Title}', which is a document library. " +
                        "Library contents are extracted as files, not as data rows.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    continue;
                }

                await ExtractRowsAsync(context, siteList, listInstance, listConfig).ConfigureAwait(false);
            }

            WriteMessage("Done processing list data rows", ProvisioningMessageType.Completed);

            return template;
        }

        private async Task ExtractRowsAsync(PnPContext context, CoreList siteList, ListInstance listInstance,
            Model.Configuration.Lists.Lists.ExtractListsListsConfiguration listConfig)
        {
            if (!string.IsNullOrEmpty(listConfig.KeyColumn))
            {
                // Carried into the template so a re-apply updates rather than duplicates.
                listInstance.DataRows.KeyColumn = listConfig.KeyColumn;
                listInstance.DataRows.UpdateBehavior = listConfig.UpdateBehavior;
            }

            Model.Configuration.Lists.Lists.ExtractListsQueryConfiguration queryConfig = listConfig.Query;

            await siteList.LoadAsync(l => l.Fields.QueryProperties(f => f.InternalName, f => f.TypeAsString,
                f => f.ReadOnlyField, f => f.Hidden)).ConfigureAwait(false);

            Dictionary<string, IField> fields = siteList.Fields.AsRequested()
                .GroupBy(f => f.InternalName)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

            try
            {
                await siteList.LoadItemsByCamlQueryAsync(BuildQuery(queryConfig)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The items of list '{siteList.Title}' could not be read: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return;
            }

            List<IListItem> items = siteList.Items.AsRequested().ToList();
            int index = 0;

            foreach (IListItem item in items)
            {
                index++;
                WriteSubProgress($"Data rows of list {siteList.Title}", $"{index}", index, items.Count);

                listInstance.DataRows.Add(BuildDataRow(context, item, fields, listConfig, queryConfig));
            }
        }

        /// <summary>
        /// Builds the CAML the configuration asks for, defaulting to everything ordered by id.
        /// </summary>
        private static string BuildQuery(Model.Configuration.Lists.Lists.ExtractListsQueryConfiguration queryConfig)
        {
            string inner = string.IsNullOrEmpty(queryConfig?.CamlQuery)
                ? "<OrderBy><FieldRef Name='ID' /></OrderBy>"
                : queryConfig.CamlQuery;

            var query = new System.Text.StringBuilder();
            query.Append("<View Scope=\"RecursiveAll\"><Query>").Append(inner).Append("</Query>");

            if (queryConfig?.ViewFields != null && queryConfig.ViewFields.Count > 0)
            {
                query.Append("<ViewFields>");
                foreach (string viewField in queryConfig.ViewFields)
                {
                    query.Append($"<FieldRef Name='{viewField}' />");
                }

                query.Append("</ViewFields>");
            }

            int rowLimit = queryConfig?.PageSize > 0 ? queryConfig.PageSize : queryConfig?.RowLimit ?? 0;
            if (rowLimit > 0)
            {
                string paged = queryConfig.PageSize > 0 ? " Paged=\"TRUE\"" : string.Empty;
                query.Append($"<RowLimit{paged}>{rowLimit}</RowLimit>");
            }

            return query.Append("</View>").ToString();
        }

        private static DataRow BuildDataRow(PnPContext context, IListItem item, Dictionary<string, IField> fields,
            Model.Configuration.Lists.Lists.ExtractListsListsConfiguration listConfig,
            Model.Configuration.Lists.Lists.ExtractListsQueryConfiguration queryConfig)
        {
            var dataRow = new DataRow();

            foreach (KeyValuePair<string, object> value in item.Values)
            {
                if (!fields.TryGetValue(value.Key, out IField field))
                {
                    continue;
                }

                if (queryConfig?.ViewFields != null && queryConfig.ViewFields.Count > 0
                    && !queryConfig.ViewFields.Contains(value.Key))
                {
                    continue;
                }

                if (listConfig.SkipEmptyFields && value.Value == null)
                {
                    continue;
                }

                string text = RenderValue(context, field, value.Value);

                if (listConfig.SkipEmptyFields && string.IsNullOrEmpty(text))
                {
                    continue;
                }

                dataRow.Values.Add(value.Key, text);
            }

            return dataRow;
        }

        /// <summary>
        /// Renders one stored value in the form the apply path parses back.
        /// </summary>
        private static string RenderValue(PnPContext context, IField field, object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            switch (field.TypeAsString)
            {
                case "User":
                case "UserMulti":
                    return string.Join(",", Flatten(value).OfType<IFieldUserValue>()
                        .Select(NameOfUser)
                        .Where(v => !string.IsNullOrEmpty(v)));

                case "Lookup":
                case "LookupMulti":
                    return string.Join(",", Flatten(value).OfType<IFieldLookupValue>()
                        .Select(l => Safe(() => l.LookupId))
                        .Where(id => id > 0)
                        .Select(id => id.ToString(CultureInfo.InvariantCulture)));

                case "TaxonomyFieldType":
                case "TaxonomyFieldTypeMulti":
                    return string.Join(";", Flatten(value).OfType<IFieldTaxonomyValue>()
                        .Select(t => $"{Safe(() => t.Label)}|{Safe(() => t.TermId)}"));

                case "URL":
                    return value is IFieldUrlValue url
                        ? (string.IsNullOrEmpty(url.Description) || url.Description == url.Url
                            ? url.Url
                            : $"{url.Url},{url.Description}")
                        : value.ToString();

                case "MultiChoice":
                    return value is IEnumerable<string> choices
                        ? string.Join(";#", choices)
                        : value.ToString();

                case "DateTime":
                    // A round-trippable, culture-independent form. The apply path parses it with
                    // the invariant culture first, so anything else would depend on the machine.
                    return value is DateTime date
                        ? date.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)
                        : value.ToString();

                case "Boolean":
                    return value is bool flag
                        ? (flag ? "true" : "false")
                        : value.ToString();

                default:
                    return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            }
        }

        /// <summary>
        /// The most portable name available for a person value.
        /// </summary>
        private static string NameOfUser(IFieldUserValue user)
        {
            string email = Safe(() => user.Email);
            if (!string.IsNullOrEmpty(email))
            {
                return email;
            }

            string displayName = Safe(() => user.LookupValue);
            if (!string.IsNullOrEmpty(displayName))
            {
                return displayName;
            }

            int lookupId = Safe(() => user.LookupId);
            return lookupId > 0 ? lookupId.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        /// <summary>
        /// Reads a model property that may not have been loaded, yielding the default instead of
        /// throwing.
        /// </summary>
        private static T Safe<T>(Func<T> read)
        {
            try
            {
                return read();
            }
            catch (ClientException)
            {
                return default;
            }
        }

        /// <summary>
        /// Yields a single field value or the members of a multi-value collection, uniformly.
        /// </summary>
        private static IEnumerable<object> Flatten(object value)
        {
            if (value is IFieldValueCollection collection)
            {
                return collection.Values.Cast<object>();
            }

            return new[] { value };
        }

        #endregion
    }
}
