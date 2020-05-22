using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class ListCollection
    {
        public IList GetByTitleLegacy(Batch batch, string title, params Expression<Func<IList, object>>[] expressions)
        {
            // Was this list previously loaded?
            if (!(items.FirstOrDefault(p => p.IsPropertyAvailable(p => p.Title) && p.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase)) is List listToLoad))
            {
                // List was not loaded before, so add it the current set of loaded lists
                listToLoad = AddNewList();
            }

            return listToLoad.GetByTitle(batch, title, expressions);
        }

        public IList GetByTitleLegacy(string title, params Expression<Func<IList, object>>[] expressions)
        {
            return GetByTitleLegacy(PnPContext.CurrentBatch, title, expressions);
        }

        public async Task<IList> GetByTitleAsync(string title, params Expression<Func<IList, object>>[] expressions)
        {
            // Was this list previously loaded?
            if (!(items.FirstOrDefault(p => p.Title.Equals(title, StringComparison.InvariantCultureIgnoreCase)) is List listToLoad))
            {
                // List was not loaded before, so add it the current set of loaded lists
                listToLoad = AddNewList();
            }

            return await listToLoad.GetByTitleAsync(title, expressions).ConfigureAwait(false);
        }

        public IList Add(string title, ListTemplateType templateType)
        {
            return Add(PnPContext.CurrentBatch, title, templateType);
        }

        public IList Add(Batch batch, string title, ListTemplateType templateType)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (templateType == 0)
            {
                throw new ArgumentException($"{nameof(templateType)} cannot be 0");
            }

            var newList = AddNewList();

            newList.Title = title;
            newList.TemplateType = templateType;

            return newList.Add(batch) as List;
        }

        public async Task<IList> AddAsync(string title, ListTemplateType templateType)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (templateType == 0)
            {
                throw new ArgumentException($"{nameof(templateType)} cannot be 0");
            }

            var newList = AddNewList();

            newList.Title = title;
            newList.TemplateType = templateType;

            return await newList.AddAsync().ConfigureAwait(false) as List;
        }
    }
}
