namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SharePoint Online list item
    /// </summary>
    [ConcreteType(typeof(ListItem))]
    public interface IListItem : IDataModel<IListItem>, IDataModelGet<IListItem>, IListItemBase, IDataModelUpdate, IDataModelDelete, IExpandoDataModel, IQueryableDataModel
    {
    }
}
