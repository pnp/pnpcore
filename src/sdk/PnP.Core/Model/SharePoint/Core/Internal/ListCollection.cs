using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class ListCollection : QueryableDataModelCollection<IList>, IListCollection
    {
        public ListCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Add methods

        public async Task<IList> AddBatchAsync(string title, ListTemplateType templateType)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, title, templateType).ConfigureAwait(false);
        }

        public IList AddBatch(string title, ListTemplateType templateType)
        {
            return AddBatchAsync(title, templateType).GetAwaiter().GetResult();
        }

        public async Task<IList> AddBatchAsync(Batch batch, string title, ListTemplateType templateType)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (templateType == 0)
            {
                throw new ArgumentException(string.Format(PnPCoreResources.Exception_CannotBeZero, nameof(templateType)));
            }

            var newList = CreateNewAndAdd() as List;

            newList.Title = title;
            newList.TemplateType = templateType;

            return await newList.AddBatchAsync(batch).ConfigureAwait(false) as List;
        }

        public IList AddBatch(Batch batch, string title, ListTemplateType templateType)
        {
            return AddBatchAsync(batch, title, templateType).GetAwaiter().GetResult();
        }

        public async Task<IList> AddAsync(string title, ListTemplateType templateType)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (templateType == 0)
            {
                throw new ArgumentException(string.Format(PnPCoreResources.Exception_CannotBeZero, nameof(templateType)));
            }

            var newList = CreateNewAndAdd() as List;

            newList.Title = title;
            newList.TemplateType = templateType;

            return await newList.AddAsync().ConfigureAwait(false) as List;
        }

        public IList Add(string title, ListTemplateType templateType)
        {
            return AddAsync(title, templateType).GetAwaiter().GetResult();
        }

        #endregion

        #region GetByTitle methods

        public IList GetByTitle(string title, params Expression<Func<IList, object>>[] selectors)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            return GetByTitleAsync(title, selectors).GetAwaiter().GetResult();
        }

        public async Task<IList> GetByTitleAsync(string title, params Expression<Func<IList, object>>[] selectors)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            return await this.QueryProperties(selectors).FirstOrDefaultAsync(l => l.Title == title).ConfigureAwait(false);
        }

        #endregion

        #region GetById methods

        public IList GetById(Guid id, params Expression<Func<IList, object>>[] selectors)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetByIdAsync(id, selectors).GetAwaiter().GetResult();
        }

        public async Task<IList> GetByIdAsync(Guid id, params Expression<Func<IList, object>>[] selectors)
        {

            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await this.QueryProperties(selectors).FirstOrDefaultAsync(l => l.Id == id).ConfigureAwait(false);
        }

        #endregion

        #region GetByServerRelativeUrl methods

        public async Task<IList> GetByServerRelativeUrlAsync(string serverRelativeUrl, params Expression<Func<IList, object>>[] selectors)
        {
            if (serverRelativeUrl == null)
            {
                throw new ArgumentNullException(nameof(serverRelativeUrl));
            }

            if (string.IsNullOrEmpty(serverRelativeUrl))
            {
                throw new ArgumentException(PnPCoreResources.Exception_GetListByServerRelativeUrl_ServerRelativeUrl);
            }

            return await BaseDataModelExtensions.BaseGetAsync(this, new ApiCall($"_api/web/getlist('{serverRelativeUrl}')", ApiType.SPORest), selectors).ConfigureAwait(false);
        }

        public IList GetByServerRelativeUrl(string serverRelativeUrl, params Expression<Func<IList, object>>[] selectors)
        {
            if (serverRelativeUrl == null)
            {
                throw new ArgumentNullException(nameof(serverRelativeUrl));
            }

            return GetByServerRelativeUrlAsync(serverRelativeUrl, selectors).GetAwaiter().GetResult();
        }

        #endregion
    }
}
