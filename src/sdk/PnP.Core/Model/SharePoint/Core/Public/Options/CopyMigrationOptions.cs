namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Optional options that can be passed to the migration job
    /// </summary>
    public class CopyMigrationOptions
    {
        /// <summary>
        /// This allows the item to move even if the target has a mismatched schema definition from the source list.
        /// </summary>
        public bool AllowSchemaMismatch { get; set; }
        
        /// <summary>
        /// This allows the move to take place if the target file has older version. By default it’s disallowed to prevent data loss.
        /// </summary>
        public bool AllowSmallerVersionLimitOnDestination { get; set; }
        
        /// <summary>
        /// If not specified, the version history will be ignored and not moved to the destination.
        /// </summary>
        public bool IgnoreVersionHistory { get; set; }
        
        /// <summary>
        /// By default, this is set to copy. For a move operation, set this parameter to true.
        /// </summary>
        public bool IsMoveMode { get; set; }
        
        /// <summary>
        /// If a name conflict occurs at the target site, the default reports a failure.
        /// </summary>
        public SPMigrationNameConflictBehavior NameConflictBehavior { get; set; }
        
        /// <summary>
        /// This indicates whether a file with a share lock can still be moved in a move job. If you want to move a file that is locked, you need to set this.
        /// </summary>
        public bool BypassSharedLock { get; set; }
        
        /// <summary>
        /// If set, and the source eTag doesn’t match the eTag specified, the copy and move won’t take place. If left NULL, no check will take place.
        /// </summary>
        public string[] ClientEtags { get; set; }
        
        /// <summary>
        /// Once set, this move operation is similar to copy. The file will move to destination, but the source content will not be deleted. If set, this will make a copy with the version history and preserve the original metadata. No source item deletions occurs at the end.
        /// This is not like the normal copy, which only copies the most recent major version and doesn't maintain all the metadata.
        /// </summary>
        public bool MoveButKeepSource { get; set; }
        
        /// <summary>
        /// For this operation, only the root level folder of the URL is copied. The sub-folders or files within the folder will not be moved or copied.
        /// </summary>
        public bool ExcludeChildren { get; set; }
        
    }
}
