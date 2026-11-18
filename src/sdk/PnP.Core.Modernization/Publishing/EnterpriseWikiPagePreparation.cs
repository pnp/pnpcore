using PnP.Core.Modernization.Cache;
using PnP.Core.Provisioning.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PnP.Core.Modernization.Publishing
{
    internal sealed class EnterpriseWikiPagePreparation
    {
        internal const string CategoryFieldName = "Wiki_x0020_Page_x0020_Categories";
        internal static readonly Guid SitePagesFeatureId = new Guid("b6917cb1-93a0-4b97-a84d-7cf49975d4ec");
        private readonly HashSet<(Guid SiteId, Guid WebId)> preparedWebs = new HashSet<(Guid, Guid)>();

        private static readonly (Guid Id, string Name, string Type)[] RatingFields =
        {
            (new Guid("4d64b067-08c3-43dc-a87b-8b8e01673313"), "RatedBy", "UserMulti"),
            (new Guid("434f51fb-ffd2-4a0e-a03b-ca3131ac67ba"), "Ratings", "Note"),
            (new Guid("b1996002-9167-45e5-a4df-b2c41c6723c7"), "RatingCount", "RatingCount"),
            (new Guid("5a14d1ab-1513-48c7-97b3-657a5ba6c742"), "AverageRating", "AverageRating"),
        };

        internal static bool IsEnterpriseWikiWeb(string webTemplate)
        {
            return string.Equals(webTemplate, "ENTERWIKI", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(webTemplate, "ESWIKI", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsEnterpriseWikiPage(string contentTypeId, string pageLayoutUrl)
        {
            if (contentTypeId?.StartsWith(BuiltInContentTypeId.EnterpriseWikiPage, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            var layoutPath = (pageLayoutUrl ?? string.Empty).Split('?', '#')[0];
            var fileName = layoutPath.Substring(layoutPath.LastIndexOf('/') + 1);
            return string.Equals(fileName, "EnterpriseWiki.aspx", StringComparison.OrdinalIgnoreCase);
        }

        internal void EnsureReady(Guid siteId, Guid webId, string webTemplate, bool isEnterpriseWikiPage,
            bool enableEnterpriseWikiCapabilities, Action prepare)
        {
            if (!enableEnterpriseWikiCapabilities)
            {
                return;
            }
            var isEnterpriseWikiWeb = IsEnterpriseWikiWeb(webTemplate);
            if (!isEnterpriseWikiWeb && !isEnterpriseWikiPage)
            {
                return;
            }

            var key = (siteId, webId);
            if (!preparedWebs.Contains(key))
            {
                prepare();
                preparedWebs.Add(key);
            }
        }

        internal static void Prepare(PnPContext context)
        {
            IWeb web = context.Web;
            web.Features.Clear();
            web.LoadBatch(w => w.ServerRelativeUrl);
            web.LoadBatch(w => w.AvailableFields.QueryProperties(
                f => f.Id, f => f.InternalName, f => f.TypeAsString, f => f.SchemaXml));
            web.LoadBatch(w => w.Features.QueryProperties(f => f.DefinitionId));
            context.Execute();

            if (!web.Features.AsRequested().Any(f => f.DefinitionId == SitePagesFeatureId))
            {
                throw new InvalidOperationException(
                    $"Enterprise Wiki preparation cannot continue because the Site Pages feature is not enabled on destination web '{context.Uri.AbsoluteUri}'. " +
                    $"Enable Site Pages ({SitePagesFeatureId}) under Site Settings > Manage site features, then retry.");
            }

            IField category = web.AvailableFields.AsRequested().FirstOrDefault(
                f => string.Equals(f.InternalName, CategoryFieldName, StringComparison.OrdinalIgnoreCase));
            if (category == null || category.TypeAsString != "TaxonomyFieldTypeMulti")
            {
                throw new InvalidOperationException($"The destination requires an existing multi-value taxonomy site column '{CategoryFieldName}'.");
            }

            var categorySchema = XElement.Parse(category.SchemaXml);
            ValidateFieldSchema(category.SchemaXml, category.SchemaXml);
            var textFieldId = RequireTaxonomyGuid(categorySchema, "TextField");
            IField backingField = web.AvailableFields.AsRequested().FirstOrDefault(f => f.Id == textFieldId);
            if (backingField == null || backingField.TypeAsString != "Note")
            {
                throw new InvalidOperationException($"The category site column requires its native hidden Note backing field '{textFieldId}'.");
            }

            // Preflight every definition before adding any list columns.
            var requiredFields = new List<IField> { backingField };
            foreach (var definition in RatingFields)
            {
                IField field = web.AvailableFields.AsRequested().FirstOrDefault(f => f.Id == definition.Id);
                if (field == null || field.InternalName != definition.Name || field.TypeAsString != definition.Type)
                {
                    throw new InvalidOperationException(
                        $"The destination requires the native '{definition.Name}' site column ({definition.Id}, {definition.Type}). " +
                        "Have an administrator provision the site-collection Ratings infrastructure (915c240e-a6cc-49b8-8b2c-0bff8b553ed3).");
                }
                requiredFields.Add(field);
            }
            requiredFields.Add(category);

            var libraryUrl = UrlUtility.Combine(web.ServerRelativeUrl, "SitePages");
            web.Lists.LoadBatch(l => l.Id,l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl));
            context.Execute();
            IList library = web.Lists.AsRequested().FirstOrDefault(l =>
                string.Equals(l.RootFolder.ServerRelativeUrl, libraryUrl, StringComparison.OrdinalIgnoreCase));
            if (library != null)
            {
                LoadLibrary(context, library);
                ValidateExistingFields(library, requiredFields);
            }

            try
            {
                library = web.GetList(libraryUrl);
                LoadLibrary(context, library);
                ValidateExistingFields(library, requiredFields);
                GetSitePageContentType(library);
                var missingFields = requiredFields.Where(source => !library.Fields.AsRequested().Any(f => f.Id == source.Id)).ToList();
                foreach (IField source in missingFields)
                {
                    // Reuse the site column rather than recreating a similarly named field.
                    library.Fields.AddFieldAsXml(source.SchemaXml);
                    context.Execute();
                }

                LoadLibrary(context, library);
                IContentType sitePageContentType = GetSitePageContentType(library);
                sitePageContentType.FieldLinks.LoadBatch(l => l.Id,l => l.Hidden,l => l.ReadOnly);
                context.Execute();

                var linksChanged = false;
                var existingLinks = sitePageContentType.FieldLinks.AsRequested().ToDictionary(link => link.Id);
                foreach (IField source in requiredFields.Where(f => f.Id != backingField.Id))
                {
                    if (!existingLinks.TryGetValue(source.Id, out IFieldLink link))
                    {
                        link = sitePageContentType.FieldLinks.Add(library.Fields.AsRequested().First(f => f.Id == source.Id));
                        link.Hidden = source.InternalName == "RatedBy" || source.InternalName == "Ratings";
                        if (source.Id == category.Id)
                        {
                            link.ReadOnly = false;
                        }
                        link.Update();
                        linksChanged = true;
                    }
                    else
                    {
                        var hidden = source.InternalName == "RatedBy" || source.InternalName == "Ratings";
                        if (link.Hidden != hidden || (source.Id == category.Id && link.ReadOnly))
                        {
                            link.Hidden = hidden;
                            if (source.Id == category.Id)
                            {
                                link.ReadOnly = false;
                            }
                            link.Update();
                            linksChanged = true;
                        }
                    }
                }
                if (linksChanged)
                {
                    sitePageContentType.Update();
                    context.Execute();
                }

                foreach (IField field in library.Fields.AsRequested().Where(f => requiredFields.Any(source => source.Id == f.Id)))
                {
                    var isCategory = field.Id == category.Id;
                    var hidden = field.Id == backingField.Id || field.InternalName == "RatedBy" || field.InternalName == "Ratings";
                    var changed = false;
                    // Native hidden rating fields reject even redundant Hidden assignments.
                    if (field.Hidden != hidden)
                    {
                        field.Hidden = hidden;
                        changed = true;
                    }
                    if (isCategory && field.ReadOnlyField)
                    {
                        field.ReadOnlyField = false;
                        changed = true;
                    }
                    if (!hidden && !HasExpectedFormVisibility(field.SchemaXml, isCategory))
                    {
                        field.SetShowInEditForm(isCategory);
                        field.SetShowInNewForm(isCategory);
                        changed = true;
                    }
                    if (changed)
                    {
                        field.Update();
                        context.Execute();
                    }
                }

                LoadLibrary(context, library);
                ValidateExistingFields(library, requiredFields);
                sitePageContentType = GetSitePageContentType(library);
                sitePageContentType.FieldLinks.LoadBatch(l => l.Id,l => l.Hidden,l => l.ReadOnly);
                context.Execute();

                foreach (IField source in requiredFields)
                {
                    IField field = library.Fields.AsRequested().FirstOrDefault(f => f.Id == source.Id);
                    if (field == null || (source.Id != backingField.Id &&
                        !sitePageContentType.FieldLinks.AsRequested().Any(link => link.Id == source.Id)))
                    {
                        throw new InvalidOperationException($"SitePages preparation did not provision '{source.InternalName}' and its Site Page content-type link.");
                    }

                    var isCategory = field.Id == category.Id;
                    var hidden = field.Id == backingField.Id || field.InternalName == "RatedBy" || field.InternalName == "Ratings";
                    IFieldLink link = sitePageContentType.FieldLinks.AsRequested().FirstOrDefault(l => l.Id == field.Id);
                    if (field.Hidden != hidden || (!hidden && !HasExpectedFormVisibility(field.SchemaXml, isCategory)) ||
                        (isCategory && (field.ReadOnlyField || link.ReadOnly)) ||
                        (source.Id != backingField.Id && link.Hidden != hidden))
                    {
                        throw new InvalidOperationException($"SitePages field '{field.InternalName}' does not have the required visibility/editability.");
                    }
                }
            }
            finally
            {
                // Partial writes also invalidate previously cached library metadata.
                CacheManager.Instance.ClearFieldsToCopy();
            }
        }

        private static void LoadLibrary(PnPContext context, IList library)
        {
            library.LoadBatch(l => l.TemplateType);
            library.Fields.LoadBatch(f => f.Id,f => f.InternalName, f => f.TypeAsString,f => f.SchemaXml,f => f.Hidden,f => f.ReadOnlyField);
            library.ContentTypes.LoadBatch(c => c.Id);
            context.Execute();
            if (library.TemplateType != ListTemplateType.WebPageLibrary ||
                !library.HasPermissions(PermissionKind.AddListItems,
                    PermissionKind.EditListItems))
            {
                throw new InvalidOperationException("Enterprise Wiki preparation requires a writable SitePages Web Page Library.");
            }
        }

        private static IContentType GetSitePageContentType(IList library)
        {
            IContentType contentType = library.ContentTypes.AsRequested().FirstOrDefault(c =>
                c.Id.StartsWith(Constants.ModernPageContentTypeId, StringComparison.OrdinalIgnoreCase));
            return contentType ?? throw new InvalidOperationException("SitePages must contain the native modern Site Page content type.");
        }

        private static void ValidateExistingFields(IList library, IEnumerable<IField> requiredFields)
        {
            foreach (IField source in requiredFields)
            {
                foreach (IField existing in library.Fields.AsRequested().Where(f => f.Id == source.Id ||
                    string.Equals(f.InternalName, source.InternalName, StringComparison.OrdinalIgnoreCase)))
                {
                    ValidateFieldSchema(source.SchemaXml, existing.SchemaXml);
                }
            }
        }

        internal static void ValidateFieldSchema(string expectedSchema, string actualSchema)
        {
            var expected = XElement.Parse(expectedSchema);
            var actual = XElement.Parse(actualSchema);
            if (Guid.Parse((string)expected.Attribute("ID")) != Guid.Parse((string)actual.Attribute("ID")) ||
                !string.Equals((string)expected.Attribute("Name"), (string)actual.Attribute("Name"), StringComparison.OrdinalIgnoreCase) ||
                !string.Equals((string)expected.Attribute("Type"), (string)actual.Attribute("Type"), StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Conflicting SitePages column '{(string)expected.Attribute("Name")}': field ID, internal name and native type must match the site column.");
            }

            if ((string)expected.Attribute("Type") == "TaxonomyFieldTypeMulti")
            {
                foreach (var property in new[] { "SspId", "TermSetId", "TextField" })
                {
                    if (RequireTaxonomyGuid(expected, property) != RequireTaxonomyGuid(actual, property))
                    {
                        throw new InvalidOperationException($"Conflicting SitePages category binding '{property}'.");
                    }
                }
                Guid.TryParse(GetTaxonomyProperty(expected, "AnchorId"), out var expectedAnchor);
                Guid.TryParse(GetTaxonomyProperty(actual, "AnchorId"), out var actualAnchor);
                if (expectedAnchor != actualAnchor)
                {
                    throw new InvalidOperationException("Conflicting SitePages category binding 'AnchorId'.");
                }
                if (!string.Equals((string)actual.Attribute("Mult"), "TRUE", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("The SitePages category column must support multiple values.");
                }
            }
        }

        internal static bool HasExpectedFormVisibility(string fieldSchema, bool isCategory)
        {
            var schema = XElement.Parse(fieldSchema);
            var expected = isCategory ? "TRUE" : "FALSE";
            return string.Equals((string)schema.Attribute("ShowInEditForm"), expected, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals((string)schema.Attribute("ShowInNewForm"), expected, StringComparison.OrdinalIgnoreCase);
        }

        private static Guid RequireTaxonomyGuid(XElement schema, string propertyName)
        {
            if (!Guid.TryParse(GetTaxonomyProperty(schema, propertyName), out var value) || value == Guid.Empty)
            {
                throw new InvalidOperationException($"The category column requires a valid '{propertyName}' binding.");
            }
            return value;
        }

        private static string GetTaxonomyProperty(XElement schema, string propertyName)
        {
            var property = schema.Descendants("Property").FirstOrDefault(p => (string)p.Element("Name") == propertyName);
            return (string)property?.Element("Value");
        }
    }
}
