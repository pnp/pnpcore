using System;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class CanvasControlFlexibleLayoutPosition
    {
        [JsonPropertyName("lg")]
        public CanvasControlFlexibleLayoutPositionLG LG
        {
            get; set;
        }

        /// <summary>
        /// Group of WebParts inside this section, is null if no group is defined
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("groupId")]
        public Guid? GroupId 
        {
            get; set;
        }
    }
}
