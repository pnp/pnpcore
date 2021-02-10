using PnP.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.QueryModel
{
    /// <summary>
    /// Base abstract class to implement the basic logic of an IQueryProvider
    /// </summary>
    public abstract class BaseQueryProvider : IAsyncQueryProvider
    {
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

            // Support queries for the current type only, no projection
            if (!typeof(IQueryable<TResult>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentException(PnPCoreResources.Exception_ArgumentExpressionNotValid);
            }

            return (IQueryable<TResult>)CreateQuery(expression);
        }

        /// <summary>
        ///     Adds the expression to the current batch
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        public abstract Task<IBatchResult<TResult>> AddToCurrentBatchAsync<TResult>(Expression expression);

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
        
    }
}
