namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Contains the values of the 3 allowed states for image tagging
    /// </summary>
    public enum ImageTaggingChoice
    {
        /// <summary>
        /// The image tagging option for the tenant is disabled
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// The image tagging option for the tenant is basic
        /// </summary>
        Basic,

        /// <summary>
        /// The image tagging option for the tenant is enhanced
        /// </summary>
        Enhanced
    }
}
