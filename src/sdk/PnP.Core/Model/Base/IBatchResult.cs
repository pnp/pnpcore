using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model
{
    /// <summary>
    /// Provides the result of a batch when is executed
    /// </summary>
    public interface IBatchResult : IEnumerable
    {
        /// <summary>
        /// Gets if the result is available
        /// </summary>
        bool IsAvailable { get; }
    }

    /// <summary>
    /// Provides the result of a batch when is executed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBatchResult<out T> : IEnumerable<T>
    {
    }
}
