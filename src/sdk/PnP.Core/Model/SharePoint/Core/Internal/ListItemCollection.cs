using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class ListItemCollection
    {
        public ListItemCollection(PnPContext context, IDataModelParent parent)
            : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }

        public IListItem Add(Dictionary<string, object> values)
        {
            return Add(PnPContext.CurrentBatch, values);
        }

        public IListItem Add(Batch batch, Dictionary<string, object> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var newListItem = AddNewListItem();

            // Assign field values
            newListItem.Values.SystemAddRange(values);

            return newListItem.Add(batch) as ListItem;
        }

        public async Task<IListItem> AddAsync(Dictionary<string, object> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var newListItem = AddNewListItem();

            // Assign field values
            newListItem.Values.SystemAddRange(values);

            return await newListItem.AddAsync().ConfigureAwait(false) as ListItem;
        }

        public bool Contains(int id)
        {
            if (this.items.FirstOrDefault(p => p.Id == id) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Replace(int itemIndex, IListItem newItem)
        {
            (items[itemIndex] as TransientObject).Merge(newItem as TransientObject);
        }
    }
}
