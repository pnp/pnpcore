namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options to set when creating a new web
    /// </summary>
    public class WebOptions
    {
        /// <summary>
        /// Title of the new web (e.g. My Sub Web)
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Relative url of the new web (e.g. mysubweb)
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Description to set on the new web
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The site template to use for the new web (e.g. STS#3)
        /// </summary>
        public string Template { get; set; } = "STS#3";

        /// <summary>
        /// Language to set for the new web, defaults to 1033
        /// </summary>
        public int Language { get; set; } = 1033;

        /// <summary>
        /// Inherit permissions from the current web, defaults to true.
        /// </summary>
        public bool InheritPermissions { get; set; } = true;
    }
}
