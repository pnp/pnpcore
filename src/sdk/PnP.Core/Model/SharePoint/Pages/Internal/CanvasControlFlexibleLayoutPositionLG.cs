using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class CanvasControlFlexibleLayoutPositionLG
    {
        /// <summary>
        /// x-postion of the control in the section
        /// </summary>
        [JsonPropertyName("x")]
        public double X
        {
            get; set;
        }

        /// <summary>
        /// y-postion of the control in the section
        /// </summary>
        [JsonPropertyName("y")]
        public double Y
        {
            get; set;
        }

        /// <summary>
        /// width of the control in the section
        /// </summary>
        [JsonPropertyName("w")]
        public double W
        {
            get; set;
        }

        /// <summary>
        /// height of the control in the section
        /// </summary>
        [JsonPropertyName("h")]
        public double H
        {
            get; set;
        }
    }
}
