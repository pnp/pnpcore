using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class UserCustomActionCollection : QueryableDataModelCollection<IUserCustomAction>, IUserCustomActionCollection
    {
        public UserCustomActionCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }

        #region Add
        public IUserCustomAction Add(AddUserCustomActionOptions options)
        {
            return AddAsync(options).GetAwaiter().GetResult();
        }

        public async Task<IUserCustomAction> AddAsync(AddUserCustomActionOptions options)
        {
            if (null == options)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var newUserCustomAction = CreateNewAndAdd() as UserCustomAction;

            // Add the field options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { UserCustomAction.AddUserCustomActionOptionsAdditionalInformationKey, options }
            };

            return await newUserCustomAction.AddAsync(additionalInfo).ConfigureAwait(false) as UserCustomAction;
        }

        public async Task<IUserCustomAction> AddBatchAsync(Batch batch, AddUserCustomActionOptions options)
        {
            if (null == options)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var newUserCustomAction = CreateNewAndAdd() as UserCustomAction;

            // Add the field options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { UserCustomAction.AddUserCustomActionOptionsAdditionalInformationKey, options }
            };

            return await newUserCustomAction.AddBatchAsync(batch, additionalInfo).ConfigureAwait(false) as UserCustomAction;
        }

        public IUserCustomAction AddBatch(Batch batch, AddUserCustomActionOptions options)
        {
            return AddBatchAsync(batch, options).GetAwaiter().GetResult();
        }

        public async Task<IUserCustomAction> AddBatchAsync(AddUserCustomActionOptions options)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, options).ConfigureAwait(false);
        }

        public IUserCustomAction AddBatch(AddUserCustomActionOptions options)
        {
            return AddBatchAsync(options).GetAwaiter().GetResult();
        }
        #endregion
    }
}
