using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class ViewCollection : QueryableDataModelCollection<IView>, IViewCollection
    {

        public ViewCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Add

        public async Task<IView> AddAsync(ViewOptions viewOptions)
        {
            if (viewOptions == null)
            {
                throw new ArgumentNullException(nameof(viewOptions));
            }

            var view = CreateNewAndAdd() as View;

            // Add the field options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { View.ViewOptionsAdditionalInformationKey, viewOptions}
            };


            return await view.AddAsync(additionalInfo).ConfigureAwait(false) as View;
        }

        public IView Add(ViewOptions viewOptions)
        {
            return AddAsync(viewOptions).GetAwaiter().GetResult();
        }

        #endregion

        #region Add Batch

        public async Task<IView> AddBatchAsync(Batch batch, ViewOptions viewOptions)
        {
            if (viewOptions == null)
            {
                throw new ArgumentNullException(nameof(viewOptions));
            }

            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }

            var view = CreateNewAndAdd() as View;

            // Add the field options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { View.ViewOptionsAdditionalInformationKey, viewOptions}
            };


            return await view.AddBatchAsync(batch, additionalInfo).ConfigureAwait(false) as View;
        }

        public async Task<IView> AddBatchAsync(ViewOptions viewOptions)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, viewOptions).ConfigureAwait(false);
        }

        public IView AddBatch(ViewOptions viewOptions)
        {
            return AddBatchAsync(PnPContext.CurrentBatch, viewOptions).GetAwaiter().GetResult();
        }

        public IView AddBatch(Batch batch, ViewOptions viewOptions)
        {
            return AddBatchAsync(batch, viewOptions).GetAwaiter().GetResult();
        }

        #endregion

    }
}
