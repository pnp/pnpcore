namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// The File Level for a File element
    /// </summary>
    public enum FileLevel
    {
        /// <summary>
        /// The file will be stored as a draft
        /// </summary>
        Draft,
        /// <summary>
        /// The file will be stored as a checked out item
        /// </summary>
        Checkout,
        /// <summary>
        /// The file will be stored as a published item
        /// </summary>
        Published,
    }
}
