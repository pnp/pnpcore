namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options that define how the preview URL must be constructed
    /// </summary>
    public class PreviewOptions
    {

        /// <summary>
        /// Optional page number of document to start at, if applicable. Specified as string for future use cases around file types such as ZIP
        /// </summary>
        public string Page { get; set; } = "";

        /// <summary>
        /// Optional zoom level to start at, if applicable
        /// </summary>
        public int Zoom { get; set; }
    }
}
