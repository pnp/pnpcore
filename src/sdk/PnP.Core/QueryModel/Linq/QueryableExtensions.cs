using PnP.Core.Model;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.QueryModel
{

    /// <summary>
    ///     Useful extension methods for use with Entity Framework LINQ queries.
    /// </summary>
    public static class QueryableExtensions
    {

        #region AsBatch

        /// <summary>
        /// Adds the query to the current batch
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Task<IEnumerableBatchResult<TSource>> AsBatchAsync<TSource>(
            this IQueryable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Provider is IAsyncQueryProvider provider)
            {
                return provider.AddToCurrentBatchAsync<TSource>(source.Expression);
            }

            throw new InvalidOperationException(PnPCoreResources.Exception_InvalidOperation_NotAsyncQueryableSource);
        }

        /// <summary>
        /// Adds the query to the current batch
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerableBatchResult<TSource> AsBatch<TSource>(
            this IQueryable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Provider is IAsyncQueryProvider provider)
            {
                return provider.AddToCurrentBatchAsync<TSource>(source.Expression).GetAwaiter().GetResult();
            }

            throw new InvalidOperationException(PnPCoreResources.Exception_InvalidOperation_NotAsyncQueryableSource);
        }

        /// <summary>
        /// Adds the query to the specified batch
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="batch"></param>
        /// <returns></returns>
        public static Task<IEnumerableBatchResult<TSource>> AsBatchAsync<TSource>(
            this IQueryable<TSource> source,
            Batch batch)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Provider is IAsyncQueryProvider provider)
            {
                return provider.AddToBatchAsync<TSource>(source.Expression, batch);
            }

            throw new InvalidOperationException(PnPCoreResources.Exception_InvalidOperation_NotAsyncQueryableSource);
        }

        /// <summary>
        /// Adds the query to the specified batch
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="batch"></param>
        /// <returns></returns>
        public static IEnumerableBatchResult<TSource> AsBatch<TSource>(
            this IQueryable<TSource> source,
            Batch batch)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Provider is IAsyncQueryProvider provider)
            {
                return provider.AddToBatchAsync<TSource>(source.Expression, batch).GetAwaiter().GetResult();
            }

            throw new InvalidOperationException(PnPCoreResources.Exception_InvalidOperation_NotAsyncQueryableSource);
        }

        #endregion

        #region QueryProperties

        /// <summary>
        /// Extension method to declare a field/metadata property to load while executing the REST query
        /// </summary>
        /// <typeparam name="TResult">The type of the target entity</typeparam>
        /// <param name="source">The collection of items to load the field/metadata from</param>
        /// <param name="selectors">A selector for a field/metadata</param>
        /// <returns>The resulting collection</returns>
        public static ISupportQuery<TResult> QueryProperties<TResult>(
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA1801 // Review unused parameters
            this ISupportQuery<TResult> source, params Expression<Func<TResult, object>>[] selectors)
#pragma warning restore CA1801 // Review unused parameters
#pragma warning restore IDE0060 // Remove unused parameter
        {
            throw new InvalidOperationException(PnPCoreResources.Exception_Unsupported_QueryPropertiesUse);
        }

        /// <summary>
        /// Extension method to declare the fields/metadata properties to load while executing the REST query
        /// </summary>
        /// <typeparam name="TResult">The type of the target entity</typeparam>
        /// <param name="source">The collection of items to load fields/metadata from</param>
        /// <param name="selectors">An array of selectors for the fields/metadata</param>
        /// <returns>The resulting collection</returns>
        public static IQueryable<TResult> QueryProperties<TResult>(
            this IQueryable<TResult> source, params Expression<Func<TResult, object>>[] selectors)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selectors is null)
            {
                throw new ArgumentNullException(nameof(selectors));
            }

            return source.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    QueryableMethods.QueryProperties.MakeGenericMethod(typeof(TResult)),
                    new Expression[] { source.Expression, Expression.NewArrayInit(typeof(Expression<Func<TResult, object>>), selectors) }
                ));
        }

        #endregion

        #region First/FirstOrDefault

        /// <summary>
        /// Asynchronously returns the first element of a sequence.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <param name="source"> An <see cref="IQueryable{T}" /> to return the first element of. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the first element in <paramref name="source" />.
        /// </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="source" /> is <see langword="null" />. </exception>
        /// <exception cref="InvalidOperationException"> <paramref name="source" /> contains no elements. </exception>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public static Task<TSource> FirstAsync<TSource>(
            this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.FirstWithoutPredicate, source, cancellationToken);
        }

        /// <summary>
        /// Asynchronously returns the first element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <param name="source"> An <see cref="IQueryable{T}" /> to return the first element of. </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the first element in <paramref name="source" /> that passes the test in
        /// <paramref name="predicate" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     <para>
        ///         No element satisfies the condition in <paramref name="predicate" />
        ///     </para>
        ///     <para>
        ///         -or -
        ///     </para>
        ///     <para>
        ///         <paramref name="source" /> contains no elements.
        ///     </para>
        /// </exception>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public static Task<TSource> FirstAsync<TSource>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.FirstWithPredicate, source, predicate, cancellationToken);
        }

        /// <summary>
        /// Asynchronously returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <param name="source"> An <see cref="IQueryable{T}" /> to return the first element of. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <see langword="default" /> ( <typeparamref name="TSource" /> ) if
        /// <paramref name="source" /> is empty; otherwise, the first element in <paramref name="source" />.
        /// </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="source" /> is <see langword="null" />. </exception>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public static Task<TSource> FirstOrDefaultAsync<TSource>(
            this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            return ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.FirstOrDefaultWithoutPredicate, source, cancellationToken);
        }

        /// <summary>
        /// Asynchronously returns the first element of a sequence that satisfies a specified condition
        /// or a default value if no such element is found.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <param name="source"> An <see cref="IQueryable{T}" /> to return the first element of. </param>
        /// <param name="predicate"> A function to test each element for a condition. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <see langword="default" /> ( <typeparamref name="TSource" /> ) if <paramref name="source" />
        /// is empty or if no element passes the test specified by <paramref name="predicate" /> ; otherwise, the first
        /// element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public static Task<TSource> FirstOrDefaultAsync<TSource>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.FirstOrDefaultWithPredicate, source, predicate, cancellationToken);
        }

        #endregion

        #region ToDictionary

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it
        /// asynchronously
        /// according to a specified key selector function.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" />. </typeparam>
        /// <param name="source"> An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from. </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            CancellationToken cancellationToken = default)
            where TKey : notnull
            => ToDictionaryAsync(source, keySelector, e => e, comparer: null, cancellationToken);

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it
        /// asynchronously
        /// according to a specified key selector function and a comparer.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" />. </typeparam>
        /// <param name="source"> An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from. </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="comparer"> An <see cref="IEqualityComparer{TKey}" /> to compare keys. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
            where TKey : notnull
            => ToDictionaryAsync(source, keySelector, e => e, comparer, cancellationToken);

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it
        /// asynchronously
        /// according to a specified key selector and an element selector function.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" />. </typeparam>
        /// <typeparam name="TElement"> The type of the value returned by <paramref name="elementSelector" />. </typeparam>
        /// <param name="source"> An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from. </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
        /// <typeparamref name="TElement" /> selected from the input sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken cancellationToken = default)
            where TKey : notnull
            => ToDictionaryAsync(source, keySelector, elementSelector, comparer: null, cancellationToken);

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it
        /// asynchronously
        /// according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" />. </typeparam>
        /// <typeparam name="TElement"> The type of the value returned by <paramref name="elementSelector" />. </typeparam>
        /// <param name="source"> An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from. </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="comparer"> An <see cref="IEqualityComparer{TKey}" /> to compare keys. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
        /// <typeparamref name="TElement" /> selected from the input sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default)
            where TKey : notnull
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }
            if (elementSelector == null)
            {
                throw new ArgumentNullException(nameof(elementSelector));
            }

            var d = new Dictionary<TKey, TElement>(comparer);
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                d.Add(keySelector(element), elementSelector(element));
            }

            return d;
        }

        #endregion

        #region ForEach

        /// <summary>
        /// Asynchronously enumerates the query results and performs the specified action on each element.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="T"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <param name="source"> An <see cref="IQueryable{T}" /> to enumerate. </param>
        /// <param name="action"> The action to perform on each element. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" /> or <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public static async Task ForEachAsync<T>(
            this IQueryable<T> source,
            Action<T> action,
            CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                action(element);
            }
        }

        #endregion

        #region ToList/Array

        /// <summary>
        /// Asynchronously creates a <see cref="List{T}" /> from an <see cref="IQueryable{T}" /> by enumerating it
        /// asynchronously.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <param name="source"> An <see cref="IQueryable{T}" /> to create a list from. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="List{T}" /> that contains elements from the input sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="source" /> is <see langword="null" />. </exception>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public static async Task<List<TSource>> ToListAsync<TSource>(
            this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            var list = new List<TSource>();
            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                list.Add(element);
            }

            return list;
        }

        /// <summary>
        /// Asynchronously creates an array from an <see cref="IQueryable{T}" /> by enumerating it asynchronously.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <param name="source"> An <see cref="IQueryable{T}" /> to create an array from. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an array that contains elements from the input sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="source" /> is <see langword="null" />. </exception>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public static async Task<TSource[]> ToArrayAsync<TSource>(
            this IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
            => (await source.ToListAsync(cancellationToken).ConfigureAwait(false)).ToArray();

        #endregion

        #region AsAsyncEnumerable

        /// <summary>
        /// Returns an <see cref="IAsyncEnumerable{T}" /> which can be enumerated asynchronously.
        /// </summary>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use <see langword="await" /> to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <param name="source"> An <see cref="IQueryable{T}" /> to enumerate. </param>
        /// <returns> The query results. </returns>
        /// <exception cref="InvalidOperationException"> <paramref name="source" /> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="source" /> is not a <see cref="IAsyncEnumerable{T}" />. </exception>
        public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(
            this IQueryable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source is IAsyncEnumerable<TSource> asyncEnumerable)
            {
                return asyncEnumerable;
            }

            throw new InvalidOperationException(PnPCoreResources.Exception_InvalidOperation_NotAsyncQueryableSource);
        }

        #endregion

        #region AsEnumerable

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}" /> which can be enumerated without executing an actual LINQ query on the target data provider.
        /// </summary>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <param name="source"> An <see cref="IDataModelCollection{T}" /> to enumerate. </param>
        /// <returns> The query results. </returns>
        /// <exception cref="InvalidOperationException"> <paramref name="source" /> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="source" /> is not a <see cref="IAsyncEnumerable{T}" />. </exception>
        public static IEnumerable<TSource> AsRequested<TSource>(
            this IDataModelCollection<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return (IEnumerable<TSource>)source.RequestedItems;

            // Below check impacts testability via mocking, so we removed it
            //if (source is BaseQueryableDataModelCollection<TSource> enumerable)
            //{
            //    return (IEnumerable<TSource>)enumerable.RequestedItems;
            //}

            //throw new InvalidOperationException(PnPCoreResources.Exception_InvalidOperation_NotAsyncQueryableSource);
        }

        #endregion

        #region Impl.

        private static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            Expression expression,
            CancellationToken cancellationToken = default)
        {
            // Create a typed version of the method
            if (operatorMethodInfo.IsGenericMethod)
            {
                operatorMethodInfo
                    = operatorMethodInfo.GetGenericArguments().Length == 2
                        ? operatorMethodInfo.MakeGenericMethod(typeof(TSource), typeof(TResult).GetGenericArguments().Single())
                        : operatorMethodInfo.MakeGenericMethod(typeof(TSource));
            }

            if (source.Provider is IAsyncQueryProvider provider)
            {
                return provider.ExecuteAsync<TResult>(
                    Expression.Call(
                        instance: null,
                        method: operatorMethodInfo,
                        arguments: expression == null
                            ? new[] { source.Expression }
                            : new[] { source.Expression, expression }),
                    cancellationToken);
            }

            var r = source.ToArray();

            // This case occurs when original source has been already loaded
            var v = Expression.Call(
                instance: null,
                method: operatorMethodInfo,
                arguments: expression == null
                    ? new[] { source.Expression }
                    : new[] { source.Expression, expression });
            return source.Provider.Execute<TResult>(
                v);
        }

        private static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            LambdaExpression expression,
            CancellationToken cancellationToken = default)
            => ExecuteAsync<TSource, TResult>(
                operatorMethodInfo, source, Expression.Quote(expression), cancellationToken);

        private static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            CancellationToken cancellationToken = default)
            => ExecuteAsync<TSource, TResult>(
#nullable enable
                operatorMethodInfo, source, (Expression?)null, cancellationToken);
#nullable disable

        #endregion
    }
}