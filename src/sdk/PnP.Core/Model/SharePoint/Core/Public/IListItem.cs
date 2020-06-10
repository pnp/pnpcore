namespace PnP.Core.Model.SharePoint
{
    [ConcreteType(typeof(ListItem))]
    public interface IListItem : IDataModel<IListItem>, IDataModelUpdate, IDataModelDelete, IExpandoDataModel
    {
        public int Id { get; }

        public bool CommentsDisabled { get; set; }

        public string Title { get; set; }
    }
}
