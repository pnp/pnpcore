namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options for move and copy operations
    /// </summary>
    public class MoveCopyOptions
    {
        /// <summary>
        /// Indicates whether both resources should be kept if a resource already exists at the specified destination.
        /// </summary>
        public bool KeepBoth { get; set; }

        /// <summary>
        /// Indicates whether to reset author and creation datetime on the copied resource.
        /// </summary>
        public bool ResetAuthorAndCreatedOnCopy { get; set; }

        /// <summary>
        /// Indicates whether the shared locks on the source resource should be by passed.
        /// </summary>
        public bool ShouldBypassSharedLocks { get; set; }
    }
}
