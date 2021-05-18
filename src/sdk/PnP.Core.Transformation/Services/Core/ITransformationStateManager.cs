using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Abstract interface to handle the state of a transformation process managed by an implementation of ITransformationExecutor
    /// </summary>
    public interface ITransformationStateManager
    {
        /// <summary>
        /// Allows to write a state variable for a specific Transformation process
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="key">The key of the state variable</param>
        /// <param name="state">The value of the state variable</param>
        /// <returns></returns>
        Task WriteStateAsync<T>(object key, T state);

        /// <summary>
        /// Allows to read a state variable for a specific Transformation process
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="key">The key of the state variable</param>
        /// <returns>The value of the state variable</returns>
        Task<T> ReadStateAsync<T>(object key);
    }
}
