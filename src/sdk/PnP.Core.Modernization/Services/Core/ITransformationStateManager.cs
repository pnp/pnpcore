using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Services.Core
{
    /// <summary>
    /// Abstract interface to handle the state of a transformation process managed by an implementation of ITransformationExecutor
    /// </summary>
    public interface ITransformationStateManager
    {
        /// <summary>
        /// Allows to write a state variable for a specific Transformation Task
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="taskId">The ID of the Transformation Task</param>
        /// <param name="name">The name of the state variable</param>
        /// <param name="state">The value of the state variable</param>
        /// <returns></returns>
        Task WriteState<T>(Guid taskId, string name, T state);

        /// <summary>
        /// Allows to read a state variable for a specific Transformation Task
        /// </summary>
        /// <typeparam name="T">The Type of the state variable</typeparam>
        /// <param name="taskId">The ID of the Transformation Task</param>
        /// <param name="name">The name of the state variable</param>
        /// <returns>The value of the state variable</returns>
        Task<T> ReadState<T>(Guid taskId, string name);
    }
}
