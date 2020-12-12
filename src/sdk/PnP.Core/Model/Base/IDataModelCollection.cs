using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the very basic interface for every collection of Domain Model objects
    /// </summary>
    /// <typeparam name="TModel">The actual type of the Domain Model objects</typeparam>

    public interface IDataModelCollection<TModel> : IEnumerable<TModel>, IDataModelParent, IDataModelWithContext, IRequestableCollection
    {
        /// <summary>
        /// Enables using the .LoadProperties lambda expression syntax on a collection
        /// </summary>
        /// <param name="expressions">Expression</param>
        /// <returns>Null...return value is not needed</returns>
        public IQueryable<TModel> LoadProperties(params Expression<Func<TModel, object>>[] expressions);
    }
}
