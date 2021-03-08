namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Information about the library to unpublish a Syntex model from
    /// </summary>
    public class SyntexModelUnPublishOptions
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SyntexModelUnPublishOptions()
        {
        }

        /// <summary>
        /// Server relative url of the library registered with the model
        /// </summary>
        public string TargetLibraryServerRelativeUrl { get; set; }

        /// <summary>
        /// Fully qualified URL of the site collection holding the library registered with the model
        /// </summary>
        public string TargetSiteUrl { get; set; }

        /// <summary>
        /// Server relative url of the web holding the library registered with the model
        /// </summary>
        public string TargetWebServerRelativeUrl { get; set; }
    }
}
