
using System;
using System.Linq;
using System.Text.Json;

namespace PnP.Core.Modernization.Extensions
{
    internal static class TaxonomyValueExtensions
    {
        internal static object ReadTaxonomyValue(this IField field, IListItem item)
        {
            var value = item[field.InternalName];
            if (value is IFieldTaxonomyValue term)
            {
                try
                {
                    _ = term.TermId;
                }
                catch (ClientException exception) when (exception.Error.Type == ErrorType.PropertyNotLoaded)
                {
                    // Core's CAML reader can leave taxonomy wrappers uninitialized, including populated fields.
                    var raw = SharePointRest.Request(item.PnPContext,
                        $"_api/web/lists(guid'{item.ParentList.Id}')/items({item.Id})?$select={field.EntityPropertyName}");
                    if (raw.TryGetProperty(field.EntityPropertyName, out var property))
                    {
                        if (property.ValueKind == JsonValueKind.Null ||
                            property.ValueKind == JsonValueKind.Array && property.GetArrayLength() == 0)
                        {
                            return null;
                        }
                        if (field.TypeAsString == "TaxonomyFieldTypeMulti" && property.ValueKind == JsonValueKind.Array)
                        {
                            var terms = property.EnumerateArray().Select(restTerm => ReadRestTerm(field, restTerm)).ToArray();
                            return field.NewFieldValueCollection(terms);
                        }
                        if (field.TypeAsString == "TaxonomyFieldType" && property.ValueKind == JsonValueKind.Object)
                        {
                            return ReadRestTerm(field, property);
                        }
                    }
                    throw new InvalidOperationException($"Taxonomy field '{field.InternalName}' has unloaded term data and REST did not return a supported value.", exception);
                }
            }
            return value;
        }

        private static IFieldTaxonomyValue ReadRestTerm(IField field, JsonElement value)
        {
            if (value.ValueKind != JsonValueKind.Object ||
                !value.TryGetProperty("TermGuid", out var id) || id.ValueKind != JsonValueKind.String ||
                !Guid.TryParse(id.GetString(), out var termId) || termId == Guid.Empty ||
                !value.TryGetProperty("Label", out var label) || label.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException($"Taxonomy field '{field.InternalName}' contains invalid REST term data.");
            }
            return field.NewFieldTaxonomyValue(termId, label.GetString());
        }

        internal static IFieldValueCollection ParseTaxonomyValues(this IField field, string values)
        {
            var terms = values.Split(new[] { ";#" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(value => value.Contains("|"))
                .Select(value =>
                {
                    var separator = value.LastIndexOf('|');
                    return field.NewFieldTaxonomyValue(Guid.Parse(value.Substring(separator + 1)), value.Substring(0, separator));
                });
            return field.NewFieldValueCollection(terms);
        }

        internal static void SetFieldValueByValue(this IField field, IListItem item, IFieldTaxonomyValue value)
        {
            var copy = field.NewFieldTaxonomyValue(value.TermId, value.Label);
            item[field.InternalName] = field.TypeAsString == "TaxonomyFieldTypeMulti"
                ? (object)field.NewFieldValueCollection(new[] { copy }) : copy;
        }

        internal static void SetFieldValueByValueCollection(this IField field, IListItem item, IFieldValueCollection values)
        {
            item[field.InternalName] = field.NewFieldValueCollection(values.Values.Cast<IFieldTaxonomyValue>()
                .Select(value => field.NewFieldTaxonomyValue(value.TermId, value.Label)));
        }
    }
}
