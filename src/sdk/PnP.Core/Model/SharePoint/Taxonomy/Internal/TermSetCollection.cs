using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class TermSetCollection : QueryableDataModelCollection<ITermSet>, ITermSetCollection
    {
        public TermSetCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }

        #region Add methods

        public async Task<ITermSet> AddAsync(string name, string description = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            TermSet newSet = await PrepNewTermSet(name, description).ConfigureAwait(false);

            return await newSet.AddAsync().ConfigureAwait(false) as TermSet;
        }

        private async Task<TermSet> PrepNewTermSet(string name, string description)
        {
            // Ensure the default termstore language is loaded
            await PnPContext.TermStore.EnsurePropertiesAsync(p => p.DefaultLanguage).ConfigureAwait(false);

            var newSet = CreateNewAndAdd() as TermSet;

            // Assign field values
            (newSet.LocalizedNames as TermSetLocalizedNameCollection).Add(new TermSetLocalizedName() { Name = name, LanguageTag = PnPContext.TermStore.DefaultLanguage });

            if (description != null)
            {
                newSet.Description = description;
            }

            return newSet;
        }

        public ITermSet Add(string name, string description = null)
        {
            return AddAsync(name, description).GetAwaiter().GetResult();
        }

        public async Task<ITermSet> AddBatchAsync(Batch batch, string name, string description = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            TermSet newSet = await PrepNewTermSet(name, description).ConfigureAwait(false);

            return await newSet.AddBatchAsync(batch).ConfigureAwait(false) as TermSet;
        }

        public ITermSet AddBatch(Batch batch, string name, string description = null)
        {
            return AddBatchAsync(batch, name, description).GetAwaiter().GetResult();
        }

        public async Task<ITermSet> AddBatchAsync(string name, string description = null)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, name, description).ConfigureAwait(false);
        }

        public ITermSet AddBatch(string name, string description = null)
        {
            return AddBatchAsync(name, description).GetAwaiter().GetResult();
        }

        #endregion

        #region GetById methods

        public ITermSet GetById(string id, params Expression<Func<ITermSet, object>>[] selectors)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return BaseDataModelExtensions.BaseLinqGet(this, c => c.Id == id, selectors);
        }

        public async Task<ITermSet> GetByIdAsync(string id, params Expression<Func<ITermSet, object>>[] selectors)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await BaseDataModelExtensions.BaseLinqGetAsync(this, l => l.Id == id, selectors).ConfigureAwait(false);
        }
        #endregion

    }
}
