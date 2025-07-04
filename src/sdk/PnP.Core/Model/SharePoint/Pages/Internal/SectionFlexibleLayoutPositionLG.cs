using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class SectionFlexibleLayoutPositionLG
    {
        /// <summary>
        /// x-postion of the control in the section
        /// </summary>
        [JsonPropertyName("x")]
        public int X
        {
            get; set;
        }

        /// <summary>
        /// y-postion of the control in the section
        /// </summary>
        [JsonPropertyName("y")]
        public int Y
        {
            get; set;
        }

        /// <summary>
        /// width of the control in the section
        /// </summary>
        [JsonPropertyName("w")]
        public int W
        {
            get; set;
        }

        /// <summary>
        /// height of the control in the section
        /// </summary>
        [JsonPropertyName("h")]
        public int H
        {
            get; set;
        }
    }
}
