using PnP.Core.QueryModel.Model;
using System.ComponentModel;

namespace PnP.Core.Model.SharePoint
{
    internal partial class ListItemCollection : QueryableDataModelCollection<IListItem>, IListItemCollection
    {
        // PAOLO: It looks like this method is not used
        //public override IListItem Add()
        //{
        //    return AddNewListItem();
        //}

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
