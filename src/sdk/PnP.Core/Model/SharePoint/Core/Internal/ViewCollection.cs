using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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

        #region Remove

        public async Task RemoveAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!items.Any(o => o.Id == id))
            {
                throw new ArgumentOutOfRangeException(nameof(id),
                    PnPCoreResources.Exception_View_ViewNotFoundInCollection);
            }

            var view = items.FirstOrDefault(o => o.Id == id);

            if (view != default)
            {
                var vw = view as View;
                await vw.DeleteAsync().ConfigureAwait(false);
                items.Remove(vw);
            }
        }

        public void Remove(Guid id)
        {
            RemoveAsync(id).GetAwaiter().GetResult();
        }

        #endregion

        #region Remove Batch

        public async Task RemoveBatchAsync(Batch batch, Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }

            if (!items.Any(o => o.Id == id))
            {
                throw new ArgumentOutOfRangeException(nameof(id),
                    PnPCoreResources.Exception_View_ViewNotFoundInCollection);
            }

            var view = items.FirstOrDefault(o => o.Id == id);

            if (view != default)
            {
                var vw = view as View;
                await vw.DeleteBatchAsync(batch).ConfigureAwait(false);
                items.Remove(vw);
            }
        }

        public async Task RemoveBatchAsync(Guid id)
        {
            await RemoveBatchAsync(PnPContext.CurrentBatch, id).ConfigureAwait(false);
        }

        public void RemoveBatch(Guid id)
        {
            RemoveBatchAsync(PnPContext.CurrentBatch, id).GetAwaiter().GetResult();
        }

        public void RemoveBatch(Batch batch, Guid id)
        {
            RemoveBatchAsync(batch, id).GetAwaiter().GetResult();
        }

        #endregion
    }
}
