using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class ViewCollection
    {
        public ViewCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }

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
        
        public async Task<IView> AddBatchAsync(Batch batch, ViewOptions viewOptions)
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

        public async Task RemoveAsync(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!this.items.Any(o => o.Id == id))
            {
                throw new ArgumentOutOfRangeException(nameof(id),
                    PnPCoreResources.Exception_View_ViewNotFoundInCollection);
            }

            var view = this.items.FirstOrDefault(o => o.Id == id);

            if (view != default)
            {
                var vw = view as View;
                await vw.DeleteAsync().ConfigureAwait(false);
                this.items.Remove(vw);
            }
        }
    }
}
