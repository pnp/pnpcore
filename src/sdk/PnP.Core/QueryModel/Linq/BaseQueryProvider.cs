using PnP.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace PnP.Core.QueryModel.Linq
{
    /// <summary>
    /// Base abstract class to implement the basic logic of an IQueryProvider
    /// </summary>
    public abstract class BaseQueryProvider : IQueryProvider
    {
        #region IQueryProvider implementation

        public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            // We get the left part of the method call (if any) as the already
            // requested collection of items, and the MethodInfo of the method call
            // to build the new expression
            (var alreadyRequestedQueryable, var newExpression) = GetExpressionForAlreadyRequestedQueryable(expression);

            if (alreadyRequestedQueryable != null && newExpression != null)
            {
                // We execute the new expression on the requested queryable collection
                return alreadyRequestedQueryable.Provider.CreateQuery<TResult>(newExpression);
            }

            // Support queries for the current type only, no projection
            if (!typeof(IQueryable<TResult>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentException("Argument expression is not valid");
            }

            return (IQueryable<TResult>)this.CreateQuery(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            // If the result of the Execute is of type IQueryable<T> or if it
            // is a single data type (like TModel, int, bool, etc.) we need 
            // to see if the resultset has already been requested from the 
            // back-end service
            if (typeof(IQueryable<TResult>).IsAssignableFrom(expression.Type) ||
                typeof(TResult).IsAssignableFrom(expression.Type))
            {
                // We get the left part of the method call (if any) as the already
                // requested collection of items, and the new expression to evaluate
                // using the new IQueryable<T> not related to our query provider
                (var alreadyRequestedQueryable, var newExpression) = GetExpressionForAlreadyRequestedQueryable(expression);

                if (alreadyRequestedQueryable != null && newExpression != null)
                {
                    // We execute the new expression on the requested queryable collection
                    return alreadyRequestedQueryable.Provider.Execute<TResult>(newExpression);
                }
            }
            else
            { 
                throw new ArgumentException("Argument expression is not valid");
            }

            // If the query has not been already requested
            // just execute it using our query service
            return (TResult)this.Execute(expression);
        }

        public abstract IQueryable CreateQuery(Expression expression);

        public abstract object Execute(Expression expression);

        #endregion

        private (IQueryable, Expression) GetExpressionForAlreadyRequestedQueryable(Expression expression)
        {
            // If the target of the query is a method call expression
            var methodCall = expression as MethodCallExpression;
            if (methodCall != null)
            {
                // If there is at least one argument
                if (methodCall.Arguments.Count > 0)
                {
                    try
                    {
                        // We get the first argument as a constant
                        var constant = methodCall.Arguments[0].GetConstantValue();

                        // We see if it is a IRequestableCollection
                        var requestableCollection = constant as IRequestableCollection;
                        if (requestableCollection != null &&
                            (requestableCollection.Requested ||
                            requestableCollection.Length > 0))
                        {
                            // If the collection has been already requested we return an 
                            // AsQueryable of the target expression to avoid any
                            // further IQueryable query via any other query engine
                            var requestedQueryableSource = requestableCollection.RequestedItems.AsQueryable();

                            // We define the input arguments for the new method call using
                            // the already requested collection as the first argument and
                            // all the already defined arguments of the previously received
                            // method call
                            var arguments = (new Expression[] { Expression.Constant(requestedQueryableSource) }).Concat(methodCall.Arguments.Skip(1)).ToArray();

                            // We create the new method call expression
                            var newExpression = Expression.Call(null, methodCall.Method, arguments);

                            return (requestedQueryableSource, newExpression);
                        }
                    }
                    catch (NotSupportedException ns)
                    {
                        // In this scenario we skip the NotSupportedException
                        // and we simply return the default (null, null),
                        // which will be handled by this method caller
                    }
                }
            }

            return (null, null);
        }
    }
}
