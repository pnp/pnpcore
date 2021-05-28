using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// In memory implementation of <see cref="ITransformationStateManager"/>
    /// <remarks>Class is thread safety</remarks>
    /// </summary>
    public class InMemoryTransformationStateManager : ITransformationStateManager
    {
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> states =
            new ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>();

        /// <summary>
        /// Allows to write a state variable for a specific Transformation process
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="key">The name of the state variable</param>
        /// <param name="state">The value of the state variable</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        public Task WriteStateAsync<T>(string key, T state, CancellationToken token = default)
        {
            // T is the main key for the first level
            states.AddOrUpdate(typeof(T),
                k => new ConcurrentDictionary<string, object>(),
                (k, o) =>
                {
                    o.AddOrUpdate(key, state, (k, v) => state);
                    return o;
                });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Allows to read a state variable for a specific Transformation process
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="key">The name of the state variable</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The value of the state variable</returns>
        public Task<T> ReadStateAsync<T>(string key, CancellationToken token = default)
        {
            if (states.TryGetValue(typeof(T), out var dictionary) && dictionary.TryGetValue(key, out var value) && value is T v)
            {
                return Task.FromResult(v);
            }

            return Task.FromResult(default(T));
        }

        /// <summary>
        /// Allows to remove a variable
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="key">The key of the state variable</param>
        /// <param name="token">The cancellation token</param>
        public Task<bool> RemoveStateAsync<T>(string key, CancellationToken token = default)
        {
            if (states.TryGetValue(typeof(T), out var dictionary))
            {
                dictionary.TryRemove(key, out _);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// Allows to remove a variable by a prefix
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="prefix">The prefix of the state variable</param>
        /// <param name="token">The cancellation token</param>
        public Task<bool> RemoveListStateAsync<T>(string prefix, CancellationToken token = default)
        {
            bool r = false;
            if (states.TryGetValue(typeof(T), out var dictionary))
            {
                foreach (var key in dictionary.Keys)
                {
                    if (key.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        dictionary.TryRemove(key, out _);
                        r = true;
                    }
                }
            }
            return Task.FromResult(r);
        }

        /// <summary>
        /// Returns the list of items with a prefix
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefix"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<KeyValuePair<string, T>> ListStateAsync<T>(string prefix, [EnumeratorCancellation] CancellationToken token = default)
        {
            if (states.TryGetValue(typeof(T), out var dictionary))
            {
                foreach (var pair in dictionary)
                {
                    if (pair.Key.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        yield return new KeyValuePair<string, T>(pair.Key, (T)pair.Value);
                    }
                }
            }
        }

    }
}
