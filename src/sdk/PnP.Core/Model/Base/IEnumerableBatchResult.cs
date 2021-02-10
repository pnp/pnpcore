using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model
{
    /// <summary>
    /// Provides the result of a batch when is executed
    /// </summary>
    public interface IBatchResult
    {
        /// <summary>
        /// Gets if the result is available
        /// </summary>
        bool IsAvailable { get; }
    }

    public interface IBatchSingleResult : IBatchResult
    {
        /// <summary>
        /// Gets the result when the batch is executed
        /// </summary>
        object ObjectResult { get; }
    }

    /// <summary>
    /// Provides the result of a batch when is executed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBatchSingleResult<out T> : IBatchSingleResult
    {
        /// <summary>
        /// Gets the result when the batch is executed
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
    /// <typeparam name="T"></typeparam>
    public interface IEnumerableBatchResult<out T> : IEnumerableBatchResult, IEnumerable<T>
    {
    }
}
