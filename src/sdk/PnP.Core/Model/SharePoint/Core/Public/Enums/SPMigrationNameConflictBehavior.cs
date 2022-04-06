namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Enum for the behavior when a name conflict occurs
    /// </summary>
    public enum SPMigrationNameConflictBehavior
    {
        /// <summary>
        /// Returns an error
        /// </summary>
        Fail = 0,
        
        /// <summary>
        /// Replaces the file if conflict occurs
        /// </summary>
        Replace = 1,
        
        /// <summary>
        /// Keeps both the files (will append a character / number to the filename)
        /// </summary>
        KeepBoth = 2
    }
}
