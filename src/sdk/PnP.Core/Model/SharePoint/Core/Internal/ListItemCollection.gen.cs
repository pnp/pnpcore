using PnP.Core.QueryModel.Model;

namespace PnP.Core.Model.SharePoint
{
    internal partial class ListItemCollection : QueryableDataModelCollection<IListItem>, IListItemCollection
    {
        public override IListItem CreateNew()
        {
            return NewListItem();
        }

        private ListItem AddNewListItem()
        {
            var newListItem = NewListItem();
            this.items.Add(newListItem);
            return newListItem;
        }

        private ListItem NewListItem()
        {
            var newListItem = new ListItem
            {
                PnPContext = this.PnPContext,
                Parent = this,
            };
            return newListItem;
        }
    }
}
