using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Model.SharePoint
{
    internal class ListDataAsStreamProperty
    {
        internal string Name { get; set; }

        internal FieldType Type { get; set; }

        internal string TypeAsString { get; set; }

        internal JsonElement Value { get; set; }

        internal bool IsArray { get; set; }

        internal List<ListDataAsStreamPropertyValue> Values { get; } = new List<ListDataAsStreamPropertyValue>();
    }
}
