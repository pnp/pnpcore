using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the result of a batch when it is executed
    /// </summary>
    public interface IBatchResult
    {
        /// <summary>
        /// Gets whether the result is available or not
        /// </summary>
        bool IsAvailable { get; }
    }

    /// <summary>
    /// Defines a single result of a batch request
    /// </summary>
    public interface IBatchSingleResult : IBatchResult
    {
        /// <summary>
        /// Gets the result, once the batch is executed
        /// </summary>
        object ObjectResult { get; }
    }

    /// <summary>
    /// Defines the result of a batch when it is executed
    /// </summary>
    /// <typeparam name="T">The type of the result</typeparam>
    public interface IBatchSingleResult<out T> : IBatchSingleResult
    {
        /// <summary>
        /// Gets the result, once the batch is executed
        /// </summary>
        T Result { get; }
    }

    /// <summary>
    /// Provides the result of a batch when is executed
    /// </summary>
    public interface IEnumerableBatchResult : IEnumerable, IBatchResult
    {
    }

    /// <summary>
    /// Provides the result of a batch when is executed
    /// </summary>
    /// <typeparam name="T">The type of the result</typeparam>
    public interface IEnumerableBatchResult<out T> : IEnumerableBatchResult, IEnumerable<T>
    {
    }
}
