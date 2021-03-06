﻿using PnP.Core.Model;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace PnP.Core.QueryModel
{
    /// <summary>
    /// Base type for any LINQ IQueryable collection of the Domain Model
    /// In the real model, could inherit from <see cref="BaseDataModelCollection{TModel}"/>
    /// and implement <see cref="IQueryable{TModel}"/>
    /// </summary>
    /// <typeparam name="TModel">The Type of the collection</typeparam>
    internal class QueryableDataModelCollection<TModel> : BaseQueryableDataModelCollection<TModel>
    {
        #region Constructors

        // Keep this constructor protected to avoid creation from outside
        protected QueryableDataModelCollection(PnPContext context, IDataModelParent parent, string memberName = null)
        {
            var queryService = new DataModelQueryService<TModel>(context, parent, memberName);
            provider = new DataModelQueryProvider<TModel>(queryService);
            Expression = Expression.Constant(this);
        }

        // Public constructor that accepts a LINQ Query Provider object instance
        // it uses the current object as a constant LINQ Expression
        public QueryableDataModelCollection(DataModelQueryProvider<TModel> provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = Expression.Constant(this);
        }

        // Public constructor that accepts a LINQ Query Provider object instance
        // it uses the provided expression as the LINQ Expression
        public QueryableDataModelCollection(DataModelQueryProvider<TModel> provider, Expression expression)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Expression = expression;
        }

        #endregion

        /// <summary>
        /// Returns OData query string for the current expression
        /// </summary>
        public override string ToString()
        {
            return DataModelQueryProvider<TModel>.Translate(Expression).ToString();
        }
    }
}
