namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the column by which to order a Recycle Bin query.
    /// </summary>
    public enum RecycleBinOrderBy: int
    {
        /// <summary>
        /// Name column.
        /// </summary>
        Title = 0,

        /// <summary>
        /// Original Location column.
        /// </summary>
        DirName = 1,

        /// <summary>
        /// Created By column.
        /// </summary>
        Author = 2,

        /// <summary>
        /// Deleted column.
        /// </summary>
        DeletedDate = 3,

        /// <summary>
        /// Size column.
        /// </summary>
        Size = 4,

        /// <summary>
        /// Deleted By column.
        /// </summary>
        DeletedBy = 5,

        /// <summary>
        /// No ordering is enforced. Can be used only for the first page.
        /// </summary>
        None = 6,

    }
}
