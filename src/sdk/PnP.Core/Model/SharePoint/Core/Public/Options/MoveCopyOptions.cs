namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options for move and copy operations
    /// </summary>
    public class MoveCopyOptions
    {
        /// <summary>
        /// Indicates whether both resources should be kept if a resource already exists at the specified destination. If
        /// set to true then when the target file exists then a new file with a number suffix will added.
        /// </summary>
        public bool KeepBoth { get; set; }

        /// <summary>
        /// Indicates whether to reset author and creation datetime on the copied resource.
        /// </summary>
        public bool ResetAuthorAndCreatedOnCopy { get; set; }

        /// <summary>
        /// Indicates whether to retain the editor and last modified datatime on the moved resource.
        /// </summary>
        public bool RetainEditorAndModifiedOnMove { get; set; }

        /// <summary>
        /// Indicates whether the shared locks on the source resource should be by passed.
        /// </summary>
        public bool ShouldBypassSharedLocks { get; set; }
    }
}
