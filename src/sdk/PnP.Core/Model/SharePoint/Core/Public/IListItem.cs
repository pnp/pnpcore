namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SharePoint Online list item
    /// </summary>
    [ConcreteType(typeof(ListItem))]
    public interface IListItem : IDataModel<IListItem>, IDataModelUpdate, IDataModelDelete, IExpandoDataModel
    {
        /// <summary>
        /// Id of the list item
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Can comments be added to this list item
        /// </summary>
        public bool CommentsDisabled { get; set; }

        /// <summary>
        /// Title value of the list item
        /// </summary>
        public string Title { get; set; }
    }
}
