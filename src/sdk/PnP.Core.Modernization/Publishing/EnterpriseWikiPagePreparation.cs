using Microsoft.SharePoint.Client;
using PnP.Core.Modernization.Cache;
using PnP.Framework.Utilities;
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

        internal static void Prepare(ClientContext context)
        {
            var web = context.Web;
            web.Features.ClearObjectData();
            context.Load(web, w => w.ServerRelativeUrl);
            context.Load(web.AvailableFields, fields => fields.Include(
                f => f.Id, f => f.InternalName, f => f.TypeAsString, f => f.SchemaXmlWithResourceTokens));
            context.Load(web.Features, features => features.Include(f => f.DefinitionId));
            context.ExecuteQueryRetry();

            if (!web.Features.Any(f => f.DefinitionId == SitePagesFeatureId))
            {
                throw new InvalidOperationException(
                    $"Enterprise Wiki preparation cannot continue because the Site Pages feature is not enabled on destination web '{context.Url}'. " +
                    $"Enable Site Pages ({SitePagesFeatureId}) under Site Settings > Manage site features, then retry.");
            }

            var category = web.AvailableFields.FirstOrDefault(
                f => string.Equals(f.InternalName, CategoryFieldName, StringComparison.OrdinalIgnoreCase));
            if (category == null || category.TypeAsString != "TaxonomyFieldTypeMulti")
            {
                throw new InvalidOperationException($"The destination requires an existing multi-value taxonomy site column '{CategoryFieldName}'.");
            }

            var categorySchema = XElement.Parse(category.SchemaXmlWithResourceTokens);
            ValidateFieldSchema(category.SchemaXmlWithResourceTokens, category.SchemaXmlWithResourceTokens);
            var textFieldId = RequireTaxonomyGuid(categorySchema, "TextField");
            var backingField = web.AvailableFields.FirstOrDefault(f => f.Id == textFieldId);
            if (backingField == null || backingField.TypeAsString != "Note")
            {
                throw new InvalidOperationException($"The category site column requires its native hidden Note backing field '{textFieldId}'.");
            }

            // Preflight every definition before adding any list columns.
            var requiredFields = new List<Field> { backingField };
            foreach (var definition in RatingFields)
            {
                var field = web.AvailableFields.FirstOrDefault(f => f.Id == definition.Id);
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
            context.Load(web.Lists, lists => lists.Include(l => l.Id, l => l.RootFolder.ServerRelativeUrl));
            context.ExecuteQueryRetry();
            var library = web.Lists.FirstOrDefault(l =>
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
                var missingFields = requiredFields.Where(source => !library.Fields.Any(f => f.Id == source.Id)).ToList();
                foreach (var source in missingFields)
                {
                    // Reuse the site column rather than recreating a similarly named field.
                    library.Fields.Add(source);
                    context.ExecuteQueryRetry();
                }

                LoadLibrary(context, library);
                var sitePageContentType = GetSitePageContentType(library);
                context.Load(sitePageContentType.FieldLinks, links => links.Include(l => l.Id, l => l.Hidden, l => l.ReadOnly));
                context.ExecuteQueryRetry();

                var linksChanged = false;
                var existingLinks = sitePageContentType.FieldLinks.ToDictionary(link => link.Id);
                foreach (var source in requiredFields.Where(f => f.Id != backingField.Id))
                {
                    if (!existingLinks.TryGetValue(source.Id, out var link))
                    {
                        link = sitePageContentType.FieldLinks.Add(new FieldLinkCreationInformation
                        {
                            Field = library.Fields.First(f => f.Id == source.Id)
                        });
                        link.Hidden = source.InternalName == "RatedBy" || source.InternalName == "Ratings";
                        if (source.Id == category.Id)
                        {
                            link.ReadOnly = false;
                        }
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
                            linksChanged = true;
                        }
                    }
                }
                if (linksChanged)
                {
                    sitePageContentType.Update(false);
                    context.ExecuteQueryRetry();
                }

                foreach (var field in library.Fields.Where(f => requiredFields.Any(source => source.Id == f.Id)))
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
                    if (!hidden && !HasExpectedFormVisibility(field.SchemaXmlWithResourceTokens, isCategory))
                    {
                        field.SetShowInEditForm(isCategory);
                        field.SetShowInNewForm(isCategory);
                        changed = true;
                    }
                    if (changed)
                    {
                        field.Update();
                        context.ExecuteQueryRetry();
                    }
                }

                LoadLibrary(context, library);
                ValidateExistingFields(library, requiredFields);
                sitePageContentType = GetSitePageContentType(library);
                context.Load(sitePageContentType.FieldLinks, links => links.Include(l => l.Id, l => l.Hidden, l => l.ReadOnly));
                context.ExecuteQueryRetry();

                foreach (var source in requiredFields)
                {
                    var field = library.Fields.FirstOrDefault(f => f.Id == source.Id);
                    if (field == null || (source.Id != backingField.Id &&
                        !sitePageContentType.FieldLinks.Any(link => link.Id == source.Id)))
                    {
                        throw new InvalidOperationException($"SitePages preparation did not provision '{source.InternalName}' and its Site Page content-type link.");
                    }

                    var isCategory = field.Id == category.Id;
                    var hidden = field.Id == backingField.Id || field.InternalName == "RatedBy" || field.InternalName == "Ratings";
                    var link = sitePageContentType.FieldLinks.FirstOrDefault(l => l.Id == field.Id);
                    if (field.Hidden != hidden || (!hidden && !HasExpectedFormVisibility(field.SchemaXmlWithResourceTokens, isCategory)) ||
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

        private static void LoadLibrary(ClientContext context, List library)
        {
            context.Load(library, l => l.BaseTemplate, l => l.EffectiveBasePermissions);
            context.Load(library.Fields, fields => fields.Include(f => f.Id, f => f.InternalName,
                f => f.TypeAsString, f => f.SchemaXmlWithResourceTokens, f => f.Hidden, f => f.ReadOnlyField));
            context.Load(library.ContentTypes, types => types.Include(c => c.Id, c => c.Parent.Id));
            context.ExecuteQueryRetry();
            if (library.BaseTemplate != (int)ListTemplateType.WebPageLibrary ||
                !library.EffectiveBasePermissions.Has(PermissionKind.AddListItems) ||
                !library.EffectiveBasePermissions.Has(PermissionKind.EditListItems))
            {
                throw new InvalidOperationException("Enterprise Wiki preparation requires a writable SitePages Web Page Library.");
            }
        }

        private static ContentType GetSitePageContentType(List library)
        {
            var contentType = library.ContentTypes.FirstOrDefault(c =>
                string.Equals(c.Id.StringValue, Constants.ModernPageContentTypeId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Parent.Id.StringValue, Constants.ModernPageContentTypeId, StringComparison.OrdinalIgnoreCase));
            return contentType ?? throw new InvalidOperationException("SitePages must contain the native modern Site Page content type.");
        }

        private static void ValidateExistingFields(List library, IEnumerable<Field> requiredFields)
        {
            foreach (var source in requiredFields)
            {
                foreach (var existing in library.Fields.Where(f => f.Id == source.Id ||
                    string.Equals(f.InternalName, source.InternalName, StringComparison.OrdinalIgnoreCase)))
                {
                    ValidateFieldSchema(source.SchemaXmlWithResourceTokens, existing.SchemaXmlWithResourceTokens);
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
