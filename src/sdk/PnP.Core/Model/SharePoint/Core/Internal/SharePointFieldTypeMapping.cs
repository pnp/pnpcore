using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    internal static class SharePointFieldType
    {
        internal const string FieldCreationInformation = "SP.FieldCreationInformation";

        private readonly static Dictionary<FieldType, string> _mapping = new Dictionary<FieldType, string>()
        {
            { FieldType.Text, "SP.FieldText" },
            { FieldType.Note, "SP.FieldMultiLineText" },
            { FieldType.Currency, "SP.FieldCurrency" },
            { FieldType.MultiChoice, "SP.FieldMultiChoice"},
            { FieldType.Choice, "SP.FieldChoice" },
            { FieldType.Number, "SP.FieldNumber" },
            { FieldType.DateTime , "SP.FieldDateTime" },
            { FieldType.Calculated, "SP.FieldCalculated" },
            { FieldType.Guid, "SP.FieldGuid" },
            { FieldType.URL, "SP.FieldUrl" },
            { FieldType.User, "SP.FieldUser" },
            { FieldType.Lookup, "SP.FieldLookup" },
            // TODO Double-check mapping
            //{ FieldType.Computed, "SP.FieldComputed" },
            //{ FieldType.Geolocation, "SP.FieldGeolocation" },
            //{ FieldType.Location, "SP.FieldLocation" },
            //{ FieldType.Thumbnail, "SP.FieldThumbnail" },
            //{ FieldType.Boolean, "SP.FieldNumber" }
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
