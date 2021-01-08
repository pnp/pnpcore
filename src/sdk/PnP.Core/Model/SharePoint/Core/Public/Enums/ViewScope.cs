namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the recursive scope of a view for a document library.
    /// </summary>
    public enum ViewScope : int
    {
        /// <summary>
        /// Show only the files and subfolders of a specific folder.
        /// </summary>
        Default,

        /// <summary>
        /// Show all files of all folders.
        /// </summary>
        Recursive,

        /// <summary>
        /// Show all files and all subfolders of all folders.
        /// </summary>
        RecursiveAll,

        /// <summary>
        /// Show only the files of a specific folder.
        /// </summary>
        FilesOnly,
    }
}
