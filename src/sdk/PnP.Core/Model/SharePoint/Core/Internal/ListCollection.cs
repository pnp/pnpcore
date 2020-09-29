using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class ListCollection
    {

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

#if DEBUG
        #region Only used for test purposes, hence marked as internal
        internal async Task<IList> BatchGetByTitleAsync(Batch batch, string title, params Expression<Func<IList, object>>[] expressions)
        {
            // Was this list previously loaded?
            if (!(items.FirstOrDefault(p => p.IsPropertyAvailable(p => p.Title) && p.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase)) is List listToLoad))
            {
                // List was not loaded before, so add it the current set of loaded lists
                listToLoad = CreateNewAndAdd() as List;
            }

            return await listToLoad.BatchGetByTitleAsync(batch, title, expressions).ConfigureAwait(false);
        }

        internal async Task<IList> BatchGetByTitleAsync(string title, params Expression<Func<IList, object>>[] expressions)
        {
            return await BatchGetByTitleAsync(PnPContext.CurrentBatch, title, expressions).ConfigureAwait(false);
        }
        #endregion
#endif

    }
}
