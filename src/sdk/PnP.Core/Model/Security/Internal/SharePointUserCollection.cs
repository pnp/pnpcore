using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    internal sealed class SharePointUserCollection : QueryableDataModelCollection<ISharePointUser>, ISharePointUserCollection
    {
        public SharePointUserCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        public async Task<ISharePointUser> AddAsync(string loginName)
        {
            if (string.IsNullOrEmpty(loginName))
            {
                throw new ArgumentNullException(nameof(loginName));
            }

            var newUser = CreateNewAndAdd() as SharePointUser;
            newUser.LoginName = loginName;

            return await newUser.AddAsync().ConfigureAwait(false) as SharePointUser;
        }

        public ISharePointUser Add(string loginName)
        {
            return AddAsync(loginName).GetAwaiter().GetResult();
        }

        public async Task<ISharePointUser> AddBatchAsync(Batch batch, string loginName)
        {
            if (string.IsNullOrEmpty(loginName))
            {
                throw new ArgumentNullException(nameof(loginName));
            }

            var newUser = CreateNewAndAdd() as SharePointUser;
            newUser.LoginName = loginName;

            return await newUser.AddBatchAsync(batch).ConfigureAwait(false) as SharePointUser;
        }

        public ISharePointUser AddBatch(Batch batch, string loginName)
        {
            return AddBatchAsync(batch, loginName).GetAwaiter().GetResult();
        }

        public async Task<ISharePointUser> AddBatchAsync(string loginName)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, loginName).ConfigureAwait(false);
        }

        public ISharePointUser AddBatch(string loginName)
        {
            return AddBatchAsync(loginName).GetAwaiter().GetResult();
        }
    }
}
