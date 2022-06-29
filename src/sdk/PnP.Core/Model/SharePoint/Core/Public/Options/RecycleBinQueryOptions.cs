namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines the recycle bin query criteria
    /// </summary>
    public class RecycleBinQueryOptions
    {

        /// <summary>
        /// Gets or sets a string used to get the next set of rows in the page.
        /// </summary>
        public string PagingInfo { get; set; }

        /// <summary>
        /// Gets or sets a limit for the number of items returned in the query per page. Defaults to 50.
        /// </summary>
        public int RowLimit { get; set; } = 50;

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether to sort in ascending order. Defaults to true.
        /// </summary>
        public bool IsAscending { get; set; } = true;

        /// <summary>
        /// Gets or sets the column by which to order the Recycle Bin query. Defaults to <see cref="RecycleBinOrderBy.Title"/>
        /// </summary>
        public RecycleBinOrderBy OrderBy { get; set; } = RecycleBinOrderBy.Title;

        /// <summary>
        /// Gets or sets the Recycle Bin stage of items to return in the query. Defaults to <see cref="RecycleBinItemState.FirstStageRecycleBin"/>.
        /// </summary>
        public RecycleBinItemState ItemState { get; set; } = RecycleBinItemState.FirstStageRecycleBin;

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether to get items deleted by other users. Defaults to false.
        /// </summary>
        public bool ShowOnlyMyItems { get; set; } = false;

    }
}
