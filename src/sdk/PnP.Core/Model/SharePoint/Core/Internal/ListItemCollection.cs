using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class ListItemCollection : QueryableDataModelCollection<IListItem>, IListItemCollection
    {
        public ListItemCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Add methods

        public async Task<IListItem> AddBatchAsync(Dictionary<string, object> values)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, values).ConfigureAwait(false);
        }

        public IListItem AddBatch(Dictionary<string, object> values)
        {
            return AddBatchAsync(values).GetAwaiter().GetResult();
        }

        public async Task<IListItem> AddBatchAsync(Batch batch, Dictionary<string, object> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var newListItem = CreateNewAndAdd() as ListItem;

            // Assign field values
            newListItem.Values.SystemAddRange(values);

            return await newListItem.AddBatchAsync(batch).ConfigureAwait(false) as ListItem;
        }

        public IListItem AddBatch(Batch batch, Dictionary<string, object> values)
        {
            return AddBatchAsync(batch, values).GetAwaiter().GetResult();
        }

        public async Task<IListItem> AddAsync(Dictionary<string, object> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var newListItem = CreateNewAndAdd() as ListItem;

            // Assign field values
            newListItem.Values.SystemAddRange(values);

            return await newListItem.AddAsync().ConfigureAwait(false) as ListItem;
        }

        public IListItem Add(Dictionary<string, object> values)
        {
            return AddAsync(values).GetAwaiter().GetResult();
        }

        #endregion

        #region Contains method

        public bool Contains(int id)
        {
            if (items.FirstOrDefault(p => p.Id == id) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region GetById methods

        public IListItem GetById(int id)
        {
            return GetByIdAsync(id).GetAwaiter().GetResult();
        }

        public async Task<IListItem> GetByIdAsync(int id)
        {
            return await GetFirstOrDefaultAsync(l => l.Id == id).ConfigureAwait(false);
        }

        #endregion        

        public override void Replace(int itemIndex, IListItem newItem)
        {
            (items[itemIndex] as TransientObject).Merge(newItem as TransientObject);
        }
    }
}
