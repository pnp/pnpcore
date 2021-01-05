using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    internal partial class SharePointGroupCollection : QueryableDataModelCollection<ISharePointGroup>, ISharePointGroupCollection
    {
        public SharePointGroupCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        public ISharePointGroup Add(string name)
        {
            return AddAsync(name).GetAwaiter().GetResult();
        }

        public ISharePointGroup AddBatch(string name)
        {
            return AddBatchAsync(PnPContext.CurrentBatch, name).GetAwaiter().GetResult();
        }

        public ISharePointGroup AddBatch(Batch batch, string name)
        {
            return AddBatchAsync(batch, name).GetAwaiter().GetResult();
        }

        public async Task<ISharePointGroup> AddAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newGroup = CreateNewAndAdd() as SharePointGroup;
            newGroup.Title = name;
         
            return await newGroup.AddAsync().ConfigureAwait(false) as SharePointGroup;
        }

        public async Task<ISharePointGroup> AddBatchAsync(string name)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, name).ConfigureAwait(false);
        }

        public async Task<ISharePointGroup> AddBatchAsync(Batch batch, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newGroup = CreateNewAndAdd() as SharePointGroup;
            newGroup.Title = name;

            return await newGroup.AddBatchAsync(batch).ConfigureAwait(false) as SharePointGroup;
        }
    }
}