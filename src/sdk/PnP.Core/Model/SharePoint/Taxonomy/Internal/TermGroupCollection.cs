using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class TermGroupCollection : QueryableDataModelCollection<ITermGroup>, ITermGroupCollection
    {
        public TermGroupCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Add methods

        public async Task<ITermGroup> AddAsync(string name, string description = null, TermGroupScope scope = TermGroupScope.Global)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newGroup = CreateNewAndAdd() as TermGroup;

            // Assign field values
            newGroup.Name = name;

            if (scope != TermGroupScope.Global)
            {
                newGroup.Scope = scope;
            }

            if (description != null)
            {
                newGroup.Description = description;
            }

            return await newGroup.AddAsync().ConfigureAwait(false) as TermGroup;
        }

        public ITermGroup Add(string name, string description = null, TermGroupScope scope = TermGroupScope.Global)
        {
            return AddAsync(name, description).GetAwaiter().GetResult();
        }

        public async Task<ITermGroup> AddBatchAsync(Batch batch, string name, string description = null, TermGroupScope scope = TermGroupScope.Global)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newGroup = CreateNewAndAdd() as TermGroup;

            // Assign field values
            newGroup.Name = name;

            if (scope != TermGroupScope.Global)
            {
                newGroup.Scope = scope;
            }

            if (description != null)
            {
                newGroup.Description = description;
            }

            return await newGroup.AddBatchAsync(batch).ConfigureAwait(false) as TermGroup;
        }

        public ITermGroup AddBatch(Batch batch, string name, string description = null, TermGroupScope scope = TermGroupScope.Global)
        {
            return AddBatchAsync(batch, name, description).GetAwaiter().GetResult();
        }

        public async Task<ITermGroup> AddBatchAsync(string name, string description = null, TermGroupScope scope = TermGroupScope.Global)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, name, description).ConfigureAwait(false);
        }

        public ITermGroup AddBatch(string name, string description = null, TermGroupScope scope = TermGroupScope.Global)
        {
            return AddBatchAsync(name, description).GetAwaiter().GetResult();
        }

        #endregion

        #region GetById methods

        public ITermGroup GetById(string id, params Expression<Func<ITermGroup, object>>[] selectors)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return BaseDataModelExtensions.BaseLinqGet(this, c => c.Id == id, selectors);
        }

        public async Task<ITermGroup> GetByIdAsync(string id, params Expression<Func<ITermGroup, object>>[] selectors)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await BaseDataModelExtensions.BaseLinqGetAsync(this, l => l.Id == id, selectors).ConfigureAwait(false);
        }

        #endregion

        #region GetByName methods

        public ITermGroup GetByName(string name, params Expression<Func<ITermGroup, object>>[] selectors)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return BaseDataModelExtensions.BaseLinqGet(this, c => c.Name == name, selectors);
        }

        public async Task<ITermGroup> GetByNameAsync(string name, params Expression<Func<ITermGroup, object>>[] selectors)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return await BaseDataModelExtensions.BaseLinqGetAsync(this, l => l.Name == name, selectors).ConfigureAwait(false);
        }

        #endregion


    }
}
