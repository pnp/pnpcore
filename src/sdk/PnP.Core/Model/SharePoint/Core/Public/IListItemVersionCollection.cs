using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of file versions
    /// </summary>
    [ConcreteType(typeof(ListItemVersionCollection))]
    public interface IListItemVersionCollection : IQueryable<IListItemVersion>, IAsyncEnumerable<IListItemVersion>, IDataModelCollection<IListItemVersion>, IDataModelCollectionLoad<IListItemVersion>
    {
        #region GetById methods

        /// <summary>
        /// Method to select a list item version by Id
        /// </summary>
        /// <param name="id">The Id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list item version instance, if any</returns>
        public IListItemVersion GetById(int id, params Expression<Func<IListItemVersion, object>>[] selectors);

        /// <summary>
        /// Method to select a list item version by Id asynchronously
        /// </summary>
        /// <param name="id">The Id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list item version instance, if any</returns>
        public Task<IListItemVersion> GetByIdAsync(int id, params Expression<Func<IListItemVersion, object>>[] selectors);

        #endregion
    }
}
