namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Publis level of this file
    /// </summary>
    public enum FileLevel
    {
        /// <summary>
        /// File is published
        /// </summary>
        Published = 1,

        /// <summary>
        /// File is in draft status
        /// </summary>
        Draft = 2,

        /// <summary>
        /// File was checked out
        /// </summary>
        Checkout = 255
    }
}
