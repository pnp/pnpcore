using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class ListDataAsStreamPropertyValue
    {
        internal Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();

        internal FieldValue FieldValue { get; set; }
    }
}
