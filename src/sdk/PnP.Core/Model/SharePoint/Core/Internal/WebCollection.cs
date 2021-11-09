using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class WebCollection : QueryableDataModelCollection<IWeb>, IWebCollection
    {
        public WebCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        public IWeb Add(WebOptions webOptions)
        {
            return AddAsync(webOptions).GetAwaiter().GetResult();
        }

        public async Task<IWeb> AddAsync(WebOptions webOptions)
        {
            if (webOptions == null)
            {
                throw new ArgumentNullException(nameof(webOptions));
            }

            var newWeb = CreateNewAndAdd() as Web;

            // Add the field options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { Web.WebOptionsAdditionalInformationKey, webOptions}
            };

            return await newWeb.AddAsync(additionalInfo).ConfigureAwait(false) as Web;
        }

        public IWeb AddBatch(WebOptions webOptions)
        {
            return AddBatchAsync(webOptions).GetAwaiter().GetResult();
        }

        public async Task<IWeb> AddBatchAsync(WebOptions webOptions)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, webOptions).ConfigureAwait(false);
        }

        public IWeb AddBatch(Batch batch, WebOptions webOptions)
        {
            return AddBatchAsync(batch, webOptions).GetAwaiter().GetResult();
        }

        public async Task<IWeb> AddBatchAsync(Batch batch, WebOptions webOptions)
        {
            var newWeb = CreateNewAndAdd() as Web;

            // Add the field options as arguments for the add method
            var additionalInfo = new Dictionary<string, object>()
            {
                { Web.WebOptionsAdditionalInformationKey, webOptions}
            };

            return await newWeb.AddBatchAsync(batch, additionalInfo).ConfigureAwait(false) as Web;
        }
    }
}
