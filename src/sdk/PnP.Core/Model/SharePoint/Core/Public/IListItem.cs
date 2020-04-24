using PnP.Core.Model.Base;

namespace PnP.Core.Model.SharePoint
{
    [ConcreteType(typeof(ListItem))]
    public interface IListItem : IDataModel<IListItem>, IDataModelUpdate, IDataModelDelete, IExpandoDataModel
    {
        public int Id { get; set; }

        public bool CommentsDisabled { get; set; }

        public string Title { get; set; }
    }
}
