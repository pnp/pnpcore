using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{    
    internal partial class PageCollection: QueryableDataModelCollection<IPage>, IPageCollection
    {
        public PageCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region GetByTitle methods

        public IPage GetByTitle(string title)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            return BaseDataModelExtensions.BaseLinqGet(this, l => l.Title == title);
        }

        public async Task<IPage> GetByTitleAsync(string title)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            return await BaseDataModelExtensions.BaseLinqGetAsync(this, l => l.Title == title).ConfigureAwait(false);
        }

        #endregion

    }
}
