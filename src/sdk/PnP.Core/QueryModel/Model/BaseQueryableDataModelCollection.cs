using PnP.Core.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.QueryModel
{
    /// <summary>
    /// Base abstract type for any LINQ IQueryable collection of the Domain Model
    /// In the real model, could inherit from <see cref="BaseDataModelCollection{TModel}"/>
    /// and implement <see cref="IQueryable{TModel}"/>
    /// </summary>
    /// <typeparam name="TModel">The Type of the collection</typeparam>
    internal abstract class BaseQueryableDataModelCollection<TModel> : BaseDataModelCollection<TModel>, IOrderedQueryable<TModel>, IAsyncEnumerable<TModel>
    {
        #region Members

        // Represents the LINQ Query Provider object
        protected DataModelQueryProvider<TModel> provider;

        #endregion

        #region IQueryable<T> implementation

        // The type of IQueryable is the TModel of the collection
        public Type ElementType => typeof(TModel);

        // This is the LINQ Expression of the query
        public Expression Expression { get; protected set; }

        // This is the LINQ Query Provider object instance
        public IQueryProvider Provider => provider;

        #endregion

        #region IEnumerable<T> implementation

        // Provides the IEnumerable<T> implementation
        public override IEnumerator<TModel> GetEnumerator()
        {
            if (provider != null && Expression != null)
            {
                var asyncEnumerable = ((IAsyncEnumerable<TModel>)provider.Execute(Expression));

                // Transform IAsyncEnumerable to IEnumerable
                // We need to prepare the list with all the items
                return ToList().GetAwaiter().GetResult().GetEnumerator();

                async Task<IEnumerable<TModel>> ToList()
                {
                    var result = new List<TModel>();
                    await foreach (var item in asyncEnumerable.ConfigureAwait(false))
                    {
                        result.Add(item);
                    }

                    return result;
                }
            }
            else
            {
                return items.GetEnumerator();
            }
        }

        #endregion

        #region IEnumerable implementation

        // Provides the IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IAsyncEnumerable<T> implementation

        public IAsyncEnumerator<TModel> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (provider != null && Expression != null)
            {
                return provider.ExecuteAsync<IAsyncEnumerable<TModel>>(Expression, cancellationToken).GetAsyncEnumerator(cancellationToken);
            }
            else
            {
                return GetAsyncEnumerator();
            }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            async IAsyncEnumerator<TModel> GetAsyncEnumerator()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                foreach (TModel model in items)
                {
                    yield return model;
                }
            }
        }

        #endregion

    }
}
