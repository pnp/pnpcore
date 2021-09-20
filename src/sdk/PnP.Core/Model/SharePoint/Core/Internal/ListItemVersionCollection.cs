using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class ListItemVersionCollection : QueryableDataModelCollection<IListItemVersion>, IListItemVersionCollection
    {
        public ListItemVersionCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region GetById methods

        public IListItemVersion GetById(int id, params Expression<Func<IListItemVersion, object>>[] selectors)
        {
            return GetByIdAsync(id, selectors).GetAwaiter().GetResult();
        }

        public async Task<IListItemVersion> GetByIdAsync(int id, params Expression<Func<IListItemVersion, object>>[] selectors)
        {
            return await this.QueryProperties(selectors).FirstOrDefaultAsync(l => l.Id == id).ConfigureAwait(false);
        }

        #endregion
    }
}
