namespace PnP.Core.Model.SharePoint
{

    /// <summary>
    /// Defines the options for converting a file to another format
    /// </summary>
    public class ConvertToOptions
    {
        /// <summary>
        /// The desired target format for the converted file. Defaults to PDF
        /// </summary>
        public ConvertToFormat Format { get; set; } = ConvertToFormat.Pdf;

        /// <summary>
        /// Return a streaming response or return all bytes at once. Defaults to false
        /// </summary>
        public bool StreamContent = false;

        /// <summary>
        /// When <see cref="ConvertToFormat.Jpg"/> is used then you also need to specify the jpg width. Defaults to 300
        /// </summary>
        public int JpgFormatWidth = 300;

        /// <summary>
        /// When <see cref="ConvertToFormat.Jpg"/> is used then you also need to specify the jpg height. Defaults to 300
        /// </summary>
        public int JpgFormatHeight = 300;
    }
}
