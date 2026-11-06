using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ViewModel = PnP.Core.Provisioning.Model.View;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Creates a list view from the <c>&lt;View&gt;</c> schema XML a template carries.
    /// </summary>
    internal static class ListViewProvisioner
    {
        /// <summary>
        /// Creates one view on a list, replacing any existing view with the same title.
        /// </summary>
        /// <returns>The created view, or null when the schema could not be honoured</returns>
        internal static async Task<IView> CreateAsync(PnPContext context, IList list, ViewModel view,
            TokenParser parser, Action<string> reportWarning = null)
        {
            XElement rawElement = XElement.Parse(view.SchemaXml);
            XAttribute displayName = rawElement.Attribute("DisplayName");

            if (displayName == null)
            {
                throw new ArgumentException("A view element must carry a DisplayName attribute.", nameof(view));
            }

            XElement element = XElement.Parse(parser.ParseString(view.SchemaXml));
            string viewTitle = parser.ParseString(displayName.Value);

            IView existing = list.Views.AsRequested()
                .FirstOrDefault(v => string.Equals(v.Title, viewTitle, StringComparison.Ordinal));

            if (existing != null)
            {
                await existing.DeleteAsync().ConfigureAwait(false);
            }

            var options = new ViewOptions
            {
                Title = viewTitle,
                ViewFields = ReadViewFields(element),
                Query = ReadQuery(element),
                ViewTypeKind = ReadViewType(element),
                SetAsDefaultView = ReadBool(element.Attribute("DefaultView")),
                PersonalView = false,
                AssociatedContentTypeId = ReadAssociatedContentType(element),
            };

            (bool paged, int rowLimit) = ReadRowLimit(element);
            options.Paged = paged;
            options.RowLimit = rowLimit;

            string urlName = ReadUrlName(element);
            if (urlName != null)
            {
                options.Title = urlName;
            }

            IView created;
            try
            {
                created = await list.Views.AddAsync(options).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string message = $"The view '{viewTitle}' on list '{list.Title}' could not be created: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                reportWarning?.Invoke(message);
                return null;
            }

            await ApplySchemaAsync(context, created, element, viewTitle, urlName != null).ConfigureAwait(false);

            parser.AddToken(new ListViewIdToken(context, list.Title, created.Title, created.Id));

            return created;
        }

        /// <summary>
        /// Pushes the view's XML and the properties the create call does not honour.
        /// </summary>
        private static async Task ApplySchemaAsync(PnPContext context, IView created, XElement element,
            string viewTitle, bool renameNeeded)
        {
            System.Xml.XmlReader reader = element.CreateReader();
            reader.MoveToContent();
            created.ListViewXml = reader.ReadInnerXml();

            if (ReadBool(element.Attribute("Hidden")))
            {
                created.Hidden = true;
            }

            await created.UpdateAsync().ConfigureAwait(false);

            if (renameNeeded && !string.Equals(created.Title, viewTitle, StringComparison.Ordinal))
            {
                created.Title = viewTitle;
                await created.UpdateAsync().ConfigureAwait(false);
            }

            bool dirty = false;

            if (bool.TryParse((string)element.Attribute("DefaultViewForContentType"), out bool defaultForContentType))
            {
                created.DefaultViewForContentType = defaultForContentType;
                dirty = true;
            }

            if (Enum.TryParse((string)element.Attribute("Scope"), out ViewScope scope) && created.Scope != scope)
            {
                created.Scope = scope;
                dirty = true;
            }

            if (ReadBool(element.Attribute("MobileView")))
            {
                created.MobileView = true;
                dirty = true;
            }

            if (ReadBool(element.Attribute("MobileDefaultView")))
            {
                created.MobileDefaultView = true;
                dirty = true;
            }

            dirty |= SetIfDifferent(ConcatFieldRefs(element, "Aggregations"), created.Aggregations, v => created.Aggregations = v);
            dirty |= SetIfDifferent(ConcatFieldRefs(element, "ViewData"), created.ViewData, v => created.ViewData = v);
            dirty |= SetIfDifferent(element.Descendants("JSLink").FirstOrDefault()?.Value, created.JSLink, v => created.JSLink = v);

            string customFormatter = element.Descendants("CustomFormatter").FirstOrDefault()?.Value;
            if (customFormatter != null)
            {
                customFormatter = System.Net.WebUtility.HtmlEncode(customFormatter);
                dirty |= SetIfDifferent(customFormatter, created.CustomFormatter, v => created.CustomFormatter = v);
            }

            if (dirty)
            {
                await created.UpdateAsync().ConfigureAwait(false);
            }
        }

        private static bool SetIfDifferent(string wanted, string current, Action<string> set)
        {
            if (wanted == null || string.Equals(wanted, current, StringComparison.Ordinal))
            {
                return false;
            }

            set(wanted);
            return true;
        }

        private static string ConcatFieldRefs(XElement element, string containerName)
        {
            XElement container = element.Descendants(containerName).FirstOrDefault();
            if (container == null || !container.HasElements)
            {
                return null;
            }

            var builder = new StringBuilder();
            foreach (XElement fieldRef in container.Descendants("FieldRef"))
            {
                builder.Append(fieldRef.ToString());
            }

            return builder.ToString();
        }

        private static string[] ReadViewFields(XElement element)
        {
            XElement viewFields = element.Descendants("ViewFields").FirstOrDefault();
            if (viewFields == null)
            {
                return null;
            }

            return viewFields.Descendants("FieldRef")
                .Select(f => (string)f.Attribute("Name"))
                .Where(n => !string.IsNullOrEmpty(n))
                .ToArray();
        }

        private static string ReadQuery(XElement element)
        {
            XElement query = element.Descendants("Query").FirstOrDefault();
            if (query == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            foreach (XElement child in query.Elements())
            {
                builder.Append(child.ToString());
            }

            return builder.ToString();
        }

        private static ViewTypeKind ReadViewType(XElement element)
        {
            string type = (string)element.Attribute("Type");
            if (string.IsNullOrEmpty(type))
            {
                return ViewTypeKind.None;
            }

            string normalized = type.Substring(0, 1).ToUpperInvariant() + type.Substring(1).ToLowerInvariant();

            if (!Enum.TryParse(normalized, out ViewTypeKind parsed))
            {
                return ViewTypeKind.None;
            }

            return parsed == ViewTypeKind.Calendar ? ViewTypeKind.Calendar | ViewTypeKind.Recurrence : parsed;
        }

        private static (bool paged, int rowLimit) ReadRowLimit(XElement element)
        {
            XElement rowLimit = element.Descendants("RowLimit").FirstOrDefault();
            if (rowLimit == null)
            {
                return (true, 30);
            }

            bool paged = rowLimit.Attribute("Paged") == null || ReadBool(rowLimit.Attribute("Paged"));
            int limit = int.TryParse(rowLimit.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : 30;

            return (paged, limit);
        }

        private static string ReadAssociatedContentType(XElement element)
        {
            string contentTypeId = (string)element.Attribute("ContentTypeID");

            if (string.IsNullOrEmpty(contentTypeId)
                || contentTypeId.Equals("0x", StringComparison.OrdinalIgnoreCase)
                || contentTypeId.Equals(BuiltInContentTypeId.System, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return contentTypeId;
        }

        private static string ReadUrlName(XElement element)
        {
            string url = (string)element.Attribute("Url");
            return string.IsNullOrEmpty(url) ? null : Path.GetFileNameWithoutExtension(url);
        }

        private static bool ReadBool(XAttribute attribute)
        {
            return attribute != null && bool.TryParse(attribute.Value, out bool value) && value;
        }

        /// <summary>
        /// Whether a view should be dropped when a template asks to remove the existing ones.
        /// </summary>
        internal static bool IsWebPartView(IView view, string listUrl)
        {
            return view.Hidden
                && !string.IsNullOrEmpty(listUrl)
                && view.ServerRelativeUrl.IndexOf(listUrl, StringComparison.OrdinalIgnoreCase) == -1;
        }
    }
}
