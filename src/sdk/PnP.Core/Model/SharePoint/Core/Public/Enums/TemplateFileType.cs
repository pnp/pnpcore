namespace PnP.Core.Model.SharePoint
{

    /// <summary>
    /// Types of pages that can be created using the AddTemplateFile method
    /// </summary>
    public enum TemplateFileType
    {
        /// <summary>
        /// Classic ASPX page
        /// </summary>
        StandardPage = 0,

        /// <summary>
        /// Classic wiki page
        /// </summary>
        WikiPage = 1,

        /// <summary>
        /// Classic form page
        /// </summary>
        FormPage = 2,

        /// <summary>
        /// Modern page
        /// </summary>
        ClientSidePage = 3
    }
}
