using PnP.Core.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace PnP.Core.QueryModel
{
    /// <summary>
    /// Base abstract type for any LINQ IQueryable collection of the Domain Model
    /// In the real model, could inherit from BaseDataModelCollection&lt;TModel&gt;
    /// and implement IQueryable&lt;TModel&gt;
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
        public IQueryProvider Provider => this.provider;

        #endregion

        #region IEnumerable<T> implementation

        // Provides the IEnumerable<T> implementation
        public override IEnumerator<TModel> GetEnumerator()
        {
            if (this.provider != null && this.Expression != null && !this.Requested)
            {
                return ((IEnumerable<TModel>)this.provider.Execute(this.Expression)).GetEnumerator();
            }
            else
            {
                return this.items.GetEnumerator();
            }
        }

        #endregion

        #region IEnumerable implementation

        // Provides the IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this.provider != null & this.Expression != null && !this.Requested)
            {
                return ((IEnumerable)this.provider.Execute(this.Expression)).GetEnumerator();
            }
            else
            {
                return ((IEnumerable)this.items).GetEnumerator();
            }
        }

        #endregion

        #region IAsyncEnumerable<T> implementation

        public IAsyncEnumerator<TModel> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (this.provider != null & this.Expression != null && !this.Requested)
            {
                return this.provider.ExecuteAsync<IAsyncEnumerable<TModel>>(this.Expression, cancellationToken).GetAsyncEnumerator(cancellationToken);
            }
            else
            {
                return GetAsyncEnumerator();
            }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            async IAsyncEnumerator<TModel> GetAsyncEnumerator()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                foreach (TModel model in this.items)
                {
                    yield return model;
                }
            }
        }

        #endregion
    }
}
