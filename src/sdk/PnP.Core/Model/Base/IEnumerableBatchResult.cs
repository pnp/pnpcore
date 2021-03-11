using System.Collections;
using System.Collections.Generic;

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
#pragma warning disable CA1010 // Generic interface should also be implemented
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public interface IEnumerableBatchResult : IEnumerable, IBatchResult
#pragma warning restore CA1710 // Identifiers should have correct suffix
#pragma warning restore CA1010 // Generic interface should also be implemented
    {
    }

    /// <summary>
    /// Provides the result of a batch when is executed
    /// </summary>
    /// <typeparam name="T">The type of the result</typeparam>
    public interface IEnumerableBatchResult<out T> : IEnumerableBatchResult, IReadOnlyList<T>
    {
    }
}
