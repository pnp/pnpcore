using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PnP.Core.Model.SharePoint.Core.Internal
{
    internal static class SharePointFieldTypeMapping
    {
        private readonly static Dictionary<FieldType, string> _mapping = new Dictionary<FieldType, string>()
        {
            { FieldType.Text, "SP.FieldText" },
            { FieldType.Currency, "SP.FieldCurrency" },
            { FieldType.Computed, "SP.FieldComputed" },
            // TODO Continue here...
        };

        internal static string GetEntityTypeFromFieldType(FieldType fieldType)
        {
            return _mapping.ContainsKey(fieldType) ? _mapping[fieldType] : string.Empty;
        }

        internal static FieldType GetFieldTypeFromEntityType(string entityType)
        {
            var found = _mapping.FirstOrDefault(kvp => kvp.Value == entityType).Key;
            return found == default ? FieldType.Invalid : found;
        }
    }
}
