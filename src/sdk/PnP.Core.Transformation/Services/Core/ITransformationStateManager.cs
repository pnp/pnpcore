using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Abstract interface to handle the state of a transformation process managed by an implementation of ITransformationExecutor
    /// </summary>
    public interface ITransformationStateManager
    {
        /// <summary>
        /// Allows to write a state variable
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="key">The key of the state variable</param>
        /// <param name="state">The value of the state variable</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        Task WriteStateAsync<T>(string key, T state, CancellationToken token = default);

        /// <summary>
        /// Returns the list of items with a prefix
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefix"></param>
        /// <param name="token"></param>
        /// <returns>List of items with a prefix</returns>
        IAsyncEnumerable<KeyValuePair<string, T>> ListStateAsync<T>(string prefix, CancellationToken token = default);

        /// <summary>
        /// Allows to read a state variable
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="key">The key of the state variable</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The value of the state variable</returns>
        Task<T> ReadStateAsync<T>(string key, CancellationToken token = default);

        /// <summary>
        /// Allows to remove a variable
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="key">The key of the state variable</param>
        /// <param name="token">The cancellation token</param>
        Task<bool> RemoveStateAsync<T>(string key, CancellationToken token = default);

        /// <summary>
        /// Allows to remove a variable by a prefix
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="prefix">The prefix of the state variable</param>
        /// <param name="token">The cancellation token</param>
        Task<bool> RemoveListStateAsync<T>(string prefix, CancellationToken token = default);
    }
}
