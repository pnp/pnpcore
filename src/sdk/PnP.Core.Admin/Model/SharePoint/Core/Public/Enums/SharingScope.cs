namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// The intended audience width of a sharing link
    /// </summary>
    public enum SharingScope
    {
        /// <summary>
        /// Not initialized
        /// </summary>
        Uninitialized = -1,

        /// <summary>
        /// Anyone could have permission
        /// </summary>
        Anyone = 0,

        /// <summary>
        /// Only people within the organization could have permission
        /// </summary>
        Organization = 1,

        /// <summary>
        /// Only people specified in the permission could have permission
        /// </summary>
        SpecificPeople = 2,
    }
}
