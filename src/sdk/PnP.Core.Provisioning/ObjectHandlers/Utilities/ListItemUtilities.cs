using Microsoft.Extensions.Logging;
using PnP.Core.Model.Security;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CoreList = PnP.Core.Model.SharePoint.IList;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Turns the strings a template carries into the values SharePoint expects for a list item.
    /// </summary>
    internal static class ListItemUtilities
    {
        /// <summary>
        /// Builds the value dictionary for one row, coercing each value to its column's type.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="list">The list the row belongs to</param>
        /// <param name="values">The template's raw values, keyed by column name or title</param>
        /// <param name="parser">The token parser</param>
        /// <param name="reportWarning">Called for a value that could not be coerced</param>
        internal static async Task<Dictionary<string, object>> BuildValuesAsync(PnPContext context, CoreList list,
            IDictionary<string, string> values, TokenParser parser, Action<string> reportWarning = null)
        {
            var result = new Dictionary<string, object>();

            if (values == null || values.Count == 0)
            {
                return result;
            }

            await list.LoadAsync(l => l.Fields.QueryProperties(f => f.Id, f => f.InternalName, f => f.Title,
                f => f.TypeAsString, f => f.ReadOnlyField)).ConfigureAwait(false);

            List<IField> fields = list.Fields.AsRequested().ToList();

            foreach (KeyValuePair<string, string> entry in values)
            {
                IField field = fields.FirstOrDefault(f =>
                    string.Equals(f.InternalName, entry.Key, StringComparison.Ordinal)
                    || string.Equals(f.Title, entry.Key, StringComparison.Ordinal));

                if (field == null)
                {
                    string message = $"The list '{list.Title}' has no column '{entry.Key}', so that value was skipped.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    reportWarning?.Invoke(message);
                    continue;
                }

                // Only ID was skipped here, and ReadOnlyField was read from the server and then
                // ignored. Anything SharePoint maintains itself fails the write - Attachments was
                // the one that surfaced it, with "There was an exception while writing field
                // Attachments", which sounds like a wrong internal name rather than a column that
                // is not writable at all.
                if (ObjectContentHandlerBase.FieldsToExclude.Contains(field.InternalName)
                    || field.ReadOnlyField)
                {
                    continue;
                }

                string value = parser.ParseString(entry.Value);

                try
                {
                    object coerced = await CoerceAsync(context, field, value, reportWarning).ConfigureAwait(false);
                    if (coerced != NoValue)
                    {
                        result[field.InternalName] = coerced;
                    }
                }
                catch (Exception ex)
                {
                    string message = $"The value '{value}' could not be applied to column '{field.InternalName}' " +
                        $"of list '{list.Title}': {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    reportWarning?.Invoke(message);
                }
            }

            return result;
        }

        /// <summary>
        /// Distinguishes "this column is deliberately not written" from "write null to it".
        /// </summary>
        private static readonly object NoValue = new object();

        private static async Task<object> CoerceAsync(PnPContext context, IField field, string value,
            Action<string> reportWarning)
        {
            switch (field.TypeAsString)
            {
                case "User":
                case "UserMulti":
                    return await CoerceUserAsync(context, field, value, reportWarning).ConfigureAwait(false);

                case "Lookup":
                case "LookupMulti":
                    return CoerceLookup(field, value);

                case "TaxonomyFieldType":
                case "TaxonomyFieldTypeMulti":
                    return await CoerceTaxonomyAsync(context, field, value).ConfigureAwait(false);

                case "MultiChoice":
                    return string.IsNullOrEmpty(value)
                        ? null
                        : value.Split(new[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);

                case "URL":
                    return CoerceUrl(field, value);

                case "DateTime":
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return null;
                    }

                    return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate)
                        ? parsedDate
                        : DateTime.Parse(value, CultureInfo.CurrentCulture);

                case "Boolean":
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return null;
                    }

                    return value.Equals("1", StringComparison.Ordinal)
                        || value.Equals("true", StringComparison.OrdinalIgnoreCase);

                case "Number":
                case "Currency":
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return null;
                    }

                    return double.Parse(value, CultureInfo.InvariantCulture);

                case "Integer":
                case "Counter":
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return null;
                    }

                    return int.Parse(value, CultureInfo.InvariantCulture);

                default:
                    return value;
            }
        }

        private static async Task<object> CoerceUserAsync(PnPContext context, IField field, string value,
            Action<string> reportWarning)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var entries = new List<IFieldUserValue>();

            foreach (string part in value.Split(',').Select(p => p.Trim()).Where(p => p.Length > 0))
            {
                if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out int userId))
                {
                    // A bare id has to be turned into a principal before it can be written. PnP Core
                    // refuses a user value carrying only a LookupId on Add and on the Update methods
                    // - "You need to provide the user via it's principal" - because the REST payload
                    // it builds needs the login name.
                    //
                    // The id is also site specific, so one taken from another site names a different
                    // person here, or nobody. Resolving it is what makes that visible: a warning and
                    // a missing value beats writing the wrong person's name into the item.
                    ISharePointUser byId = await ResolveUserByIdAsync(context, userId).ConfigureAwait(false);

                    if (byId == null)
                    {
                        string message = $"Column '{field.InternalName}' names user id {userId}, which this site " +
                            "does not have - user ids belong to the site they came from. The value was skipped.";

                        context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                        reportWarning?.Invoke(message);
                        continue;
                    }

                    entries.Add(field.NewFieldUserValue(byId));
                    continue;
                }

                ISharePointUser user = await context.Web.EnsureUserAsync(part).ConfigureAwait(false);
                entries.Add(field.NewFieldUserValue(user));
            }

            if (entries.Count == 0)
            {
                return null;
            }

            return field.TypeAsString == "UserMulti"
                ? (object)field.NewFieldValueCollection(entries)
                : entries[0];
        }

        /// <summary>
        /// The site user with this id, or null when the site has no such user.
        /// </summary>
        private static async Task<ISharePointUser> ResolveUserByIdAsync(PnPContext context, int userId)
        {
            try
            {
                return await context.Web.GetUserByIdAsync(userId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: user id {UserId} could not be resolved on this site.",
                    Constants.LOGGING_SOURCE, userId);

                return null;
            }
        }

        private static object CoerceLookup(IField field, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var entries = new List<IFieldValue>();

            foreach (string part in value.Split(',', ';').Select(p => p.Trim()).Where(p => p.Length > 0))
            {
                if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out int lookupId))
                {
                    throw new FormatException(
                        $"A lookup value must be the target item's id; '{part}' is not a number. " +
                        "Use a {listitemid:} token if the target is provisioned by this template.");
                }

                entries.Add(field.NewFieldLookupValue(lookupId));
            }

            if (entries.Count == 0)
            {
                return null;
            }

            if (field.TypeAsString == "Lookup" && entries.Count > 1)
            {
                throw new InvalidOperationException($"Column '{field.InternalName}' does not accept multiple values.");
            }

            return field.TypeAsString == "LookupMulti"
                ? (object)field.NewFieldValueCollection(entries)
                : entries[0];
        }

        private static async Task<object> CoerceTaxonomyAsync(PnPContext context, IField field, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var entries = new List<IFieldValue>();

            foreach (string part in value.Split(',', ';').Select(p => p.Trim()).Where(p => p.Length > 0))
            {
                (Guid termId, string label) = await ResolveTermAsync(context, part).ConfigureAwait(false);

                if (termId == Guid.Empty)
                {
                    throw new InvalidOperationException($"The term '{part}' could not be found in the term store.");
                }

                entries.Add(field.NewFieldTaxonomyValue(termId, label));
            }

            if (entries.Count == 0)
            {
                return null;
            }

            return field.TypeAsString == "TaxonomyFieldTypeMulti"
                ? (object)field.NewFieldValueCollection(entries)
                : entries[0];
        }

        /// <summary>
        /// Resolves a term written either as a guid or as a <c>Group|Set|Term</c> path.
        /// </summary>
        private static async Task<(Guid TermId, string Label)> ResolveTermAsync(PnPContext context, string value)
        {
            if (Guid.TryParse(value, out Guid termId))
            {
                return (termId, string.Empty);
            }

            ITerm byPath = await TaxonomyLookup.FindTermByPathAsync(context, value).ConfigureAwait(false);

            return byPath == null
                ? (Guid.Empty, null)
                : (Guid.Parse(byPath.Id), byPath.Labels?.FirstOrDefault()?.Name ?? string.Empty);
        }

        private static object CoerceUrl(IField field, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string[] parts = value.Split(new[] { ',', ';' }, StringSplitOptions.None);

            return parts.Length == 2
                ? field.NewFieldUrlValue(parts[0].Trim(), parts[1].Trim())
                : field.NewFieldUrlValue(value, value);
        }
    }
}
