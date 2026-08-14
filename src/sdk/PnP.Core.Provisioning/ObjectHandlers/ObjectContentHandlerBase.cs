using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Connectors;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PnP.Core.Model;
using PnP.Core.QueryModel;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Base class for the handlers that move file content in and out of a template -
    /// <c>ObjectFiles</c>, <c>ObjectPages</c> and <c>ObjectPageContents</c>.
    /// </summary>
    internal abstract class ObjectContentHandlerBase : ObjectHandlerBase
    {
        /// <summary>
        /// Fields that describe SharePoint's own bookkeeping rather than the file, and so are
        /// never written into a template.
        /// </summary>
        private static readonly HashSet<string> FieldsToExclude = new HashSet<string>(StringComparer.Ordinal)
        {
            "ID", "GUID", "Author", "Editor", "FileLeafRef", "FileRef", "File_x0020_Type",
            "Modified_x0020_By", "Created_x0020_By", "Created", "Modified", "FileDirRef",
            "Last_x0020_Modified", "Created_x0020_Date", "File_x0020_Size", "FSObjType",
            "IsCheckedoutToLocal", "ScopeId", "UniqueId", "VirusStatus", "_Level",
            "_IsCurrentVersion", "ItemChildCount", "FolderChildCount", "SMLastModifiedDate",
            "owshiddenversion", "_UIVersion", "_UIVersionString", "Order", "WorkflowVersion",
            "DocConcurrencyNumber", "ParentUniqueId", "CheckedOutUserId", "SyncClientId",
            "CheckedOutTitle", "SMTotalSize", "SMTotalFileStreamSize", "SMTotalFileCount",
            "ParentVersionString", "ParentLeafName", "SortBehavior", "StreamHash", "TaxCatchAll",
            "TaxCatchAllLabel", "_ModerationStatus", "MetaInfo", "CheckoutUser", "NoExecute",
            "_HasCopyDestinations", "ContentVersion", "UIVersion", "AccessPolicy", "BSN",
            "_ListSchemaVersion", "_Dirty", "_Parsable", "_StubFile", "_VirusStatus",
            "_VirusVendorID", "_CheckinComment"
        };

        /// <summary>
        /// Copies a file's list item field values into the template's file model, tokenizing urls
        /// and content type ids so the result is portable.
        /// </summary>
        internal async Task<Model.File> RetrieveFieldValuesAsync(PnPContext context, IFile file, Model.File modelFile)
        {
            IListItem listItem;
            try
            {
                await file.LoadAsync(f => f.ListItemAllFields).ConfigureAwait(false);
                listItem = file.ListItemAllFields;
            }
            catch (Exception)
            {
                return modelFile;
            }

            if (listItem == null)
            {
                return modelFile;
            }

            IList list = listItem.ParentList;
            await list.LoadAsync(l => l.Fields.QueryProperties(
                f => f.TypeAsString, f => f.InternalName, f => f.Title)).ConfigureAwait(false);

            await listItem.LoadAsync(li => li.FieldValuesAsText).ConfigureAwait(false);

            IWeb web = await context.Web.GetAsync(w => w.Url, w => w.ServerRelativeUrl).ConfigureAwait(false);
            string webUrl = web.Url.ToString();

            foreach (KeyValuePair<string, object> fieldValue in listItem.Values.Where(f => !FieldsToExclude.Contains(f.Key)))
            {
                if (fieldValue.Value == null || string.IsNullOrEmpty(fieldValue.Value.ToString()))
                {
                    continue;
                }

                IField field = list.Fields.AsRequested().FirstOrDefault(fs => fs.InternalName == fieldValue.Key);
                if (field == null)
                {
                    continue;
                }

                string value;
                switch (field.TypeAsString)
                {
                    case "URL":
                        value = Tokenize(listItem.FieldValuesAsText.Values[fieldValue.Key]?.ToString(), webUrl, web);
                        break;
                    case "User":
                        value = (fieldValue.Value as IFieldUserValue)?.Email;
                        break;
                    case "LookupMulti":
                    case "TaxonomyFieldType":
                    case "TaxonomyFieldTypeMulti":
                        value = Tokenize(SerializeFieldValue(fieldValue.Value), webUrl);
                        break;
                    case "ContentTypeIdFieldType":
                    default:
                        value = Tokenize(fieldValue.Value.ToString(), webUrl, web);
                        break;
                }

                if (fieldValue.Key == "ContentTypeId" && !string.IsNullOrEmpty(value))
                {
                    IContentType ct = list.ContentTypes.AsRequested()
                        .FirstOrDefault(c => value.StartsWith(c.StringId, StringComparison.OrdinalIgnoreCase));
                    if (ct != null)
                    {
                        value = $"{{contenttypeid:{ct.Name}}}";
                    }
                }

                if (!string.IsNullOrEmpty(value) && value != "[]")
                {
                    modelFile.Properties.Add(fieldValue.Key, value);
                }
            }

            return modelFile;
        }

        /// <summary>
        /// Serializes a lookup or taxonomy field value to the JSON shape the template stores.
        /// </summary>
        private static string SerializeFieldValue(object fieldValue)
        {
            switch (fieldValue)
            {
                case IFieldLookupValue lookup:
                    return JsonSerializer.Serialize(new { lookup.LookupId, lookup.LookupValue });

                case IFieldTaxonomyValue taxonomy:
                    return JsonSerializer.Serialize(new { taxonomy.TermId, taxonomy.Label });

                case IFieldValueCollection collection:
                    return JsonSerializer.Serialize(collection.Values.Select(SerializeFieldValueObject).ToArray());

                default:
                    return fieldValue?.ToString();
            }
        }

        private static object SerializeFieldValueObject(IFieldValue value)
        {
            switch (value)
            {
                case IFieldLookupValue lookup:
                    return new { lookup.LookupId, lookup.LookupValue };
                case IFieldTaxonomyValue taxonomy:
                    return new { taxonomy.TermId, taxonomy.Label };
                default:
                    return value?.ToString();
            }
        }

        /// <summary>
        /// Copies a file out of SharePoint and into the template's file connector.
        /// </summary>
        internal async Task PersistFileAsync(PnPContext context, ExtractConfiguration configuration, string folderPath, string fileName, bool decodeFileName = false)
        {
            if (configuration?.FileConnector == null)
            {
                WriteMessage($"No connector present to persist {fileName}.", ProvisioningMessageType.Error);
                return;
            }

            FileConnectorBase fileConnector = configuration.FileConnector;
            IWeb web = await context.Web.GetAsync(w => w.Url).ConfigureAwait(false);

            var connector = new SharePointConnector(context, web.Url.ToString(), "dummy");
            var u = new Uri(web.Url.ToString());

            if (u.PathAndQuery != "/" && folderPath.IndexOf(u.PathAndQuery, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                folderPath = folderPath.Replace(u.PathAndQuery, "");
            }

            string container = folderPath.Trim('/').Replace("%20", " ").Replace("/", "\\");
            string persistenceFileName = (decodeFileName ? Uri.UnescapeDataString(fileName) : fileName).Replace("%20", " ");

            if (fileConnector.Parameters.ContainsKey(FileConnectorBase.CONTAINER))
            {
                container = string.Concat(fileConnector.GetContainer(), container);
            }

            using (Stream s = connector.GetFileStream(fileName, folderPath))
            {
                if (s != null)
                {
                    fileConnector.SaveFileStream(persistenceFileName, container, s);
                }
            }
        }
    }
}
