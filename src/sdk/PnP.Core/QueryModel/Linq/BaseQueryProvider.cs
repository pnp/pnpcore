using PnP.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.QueryModel
{
    /// <summary>
    /// Base abstract class to implement the basic logic of an IQueryProvider
    /// </summary>
    public abstract class BaseQueryProvider : IAsyncQueryProvider
    {
        private static readonly MethodInfo TryGetFromAlreadyRequestedQueryableMethod =
            typeof(BaseQueryProvider).GetRuntimeMethods().FirstOrDefault(n => n.Name == nameof(TryGetFromAlreadyRequestedQueryable));
        private static readonly MethodInfo GetAsyncEnumerableMethod =
            typeof(BaseQueryProvider).GetRuntimeMethods().FirstOrDefault(n => n.Name == nameof(GetAsyncEnumerable));
        private static readonly MethodInfo CastTaskMethod =
            typeof(BaseQueryProvider).GetRuntimeMethods().FirstOrDefault(n => n.Name == nameof(CastTask));

        #region IQueryProvider implementation

        /// <summary>
        /// Creates a query for the provided expression
        /// </summary>
        /// <typeparam name="TResult">Result type of the query</typeparam>
        /// <param name="expression">Expression that will be translated into a query</param>
        /// <returns>Created query</returns>
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
                throw new ArgumentException(PnPCoreResources.Exception_ArgumentExpressionNotValid);
            }

            return (IQueryable<TResult>)CreateQuery(expression);
        }

        /// <summary>
        /// Executes the provided expression
        /// </summary>
        /// <typeparam name="TResult">Resulting type of the linq expression execution</typeparam>
        /// <param name="expression">Expression to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Loaded model instace of type <typeparamref name="TResult"/></returns>
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            Type resultType = typeof(TResult);
            Type innerResultType = null;
            bool isEnumerable = false;
            // Since we expect types IAsyncEnumerable<> or Task<>
            // we get the inner generic type (real type)
            if (resultType.IsGenericType)
            {
                Type genericType = resultType.GetGenericTypeDefinition();
                if (genericType == typeof(IAsyncEnumerable<>))
                {
                    isEnumerable = true;
                    innerResultType = resultType.GetGenericArguments()[0];
                }
                else if (genericType == typeof(Task<>))
                {
                    isEnumerable = false;
                    innerResultType = resultType.GetGenericArguments()[0];
                }
            }
            // Other TResult types are not supported
            if (innerResultType == null)
            {
                throw new ArgumentException(
                    string.Format(PnPCoreResources.Exception_InvalidTResultType, typeof(IAsyncEnumerable<>), typeof(Task<>)));
            }

            if (!isEnumerable)
            {
                // Normal execution which prepares the result asynchronously
                Task<object> task = ExecuteObjectAsync(expression);
                cancellationToken.ThrowIfCancellationRequested();

                // Cast Task<object> to Task<TResult>
                return (TResult)CastTaskMethod.MakeGenericMethod(innerResultType).Invoke(this, new object[] { task, cancellationToken });
            }

            // Check if the resultset has already been requested from the back-end service
            // Invoke the function using the generic type
            object[] parameters = { expression, null };
            bool found = (bool)TryGetFromAlreadyRequestedQueryableMethod
                .MakeGenericMethod(innerResultType)
                .Invoke(this, parameters);
            if (found)
            {
                //// TODO: what to do in this case?
                //// parameters[1] contains the result
                //async IAsyncEnumerator<TResult> GetAsyncEnumerator()
                //{
                //    foreach (TResult model in (IAsyncEnumerable)parameters[1])
                //    {
                //        yield return model;
                //    }
                //}

            }

            // If the query has not been already requested
            // just execute it using our query service and wrapping it with a IAsyncEnumerable implementation
            return (TResult)GetAsyncEnumerableMethod.MakeGenericMethod(innerResultType).Invoke(this, new object[] { expression, cancellationToken });
        }

        /// <summary>
        /// Executes the provided expression
        /// </summary>
        /// <typeparam name="TResult">Resulting type of the linq expression execution</typeparam>
        /// <param name="expression">Expression to execute</param>
        /// <returns>Loaded model instace of type <typeparamref name="TResult"/></returns>
        public TResult Execute<TResult>(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            // Check if the resultset has already been requested from the back-end service
            if (TryGetFromAlreadyRequestedQueryable<TResult>(expression, out object result))
            {
                return (TResult)result;
            }

            // If the query has not been already requested
            // just execute it using our query service
            return (TResult)ExecuteObjectAsync(expression).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes the provided expression
        /// </summary>
        /// <param name="expression">Expression to execute</param>
        public object Execute(Expression expression)
        {
            return ExecuteObjectAsync(expression).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a query for the provided expression
        /// </summary>
        /// <param name="expression">Expression to create a query for</param>        
        public abstract IQueryable CreateQuery(Expression expression);

        /// <summary>
        /// Executes the provided expression
        /// </summary>
        /// <param name="expression">Expression to execute</param>
        public abstract Task<object> ExecuteObjectAsync(Expression expression);

        #endregion

        private static async Task<TResult> CastTask<TResult>(Task<object> task, CancellationToken token)
        {
            object result = await task.ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            return (TResult)result;
        }

        private async IAsyncEnumerable<TResult> GetAsyncEnumerable<TResult>(Expression expression, [EnumeratorCancellation] CancellationToken token)
        {
            IEnumerable<TResult> results = (IEnumerable<TResult>)await ExecuteObjectAsync(expression).ConfigureAwait(false);
            foreach (TResult result in results)
            {
                token.ThrowIfCancellationRequested();
                yield return result;
            }
        }

        private static bool TryGetFromAlreadyRequestedQueryable<TResult>(Expression expression, out object result)
        {
            result = default;

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
                    result = alreadyRequestedQueryable.Provider.Execute<TResult>(newExpression);
                    return true;
                }
            }
            else
            {
                throw new ArgumentException(PnPCoreResources.Exception_ArgumentExpressionNotValid);
            }

            return false;
        }

        private static (IQueryable, Expression) GetExpressionForAlreadyRequestedQueryable(Expression expression)
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
                    catch (NotSupportedException)
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
