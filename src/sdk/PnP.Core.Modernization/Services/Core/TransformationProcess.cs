using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Services.Core
{
    /// <summary>
    /// Represents a Transformation Process
    /// </summary>
    public class TransformationProcess
    {
        /// <summary>
        /// The unique ID of a Transformation Process
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Starts the Transformation Process
        /// </summary>
        public virtual Task StartProcessAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stops the Transformation Process
        /// </summary>
        public virtual Task StopProcessAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Allows monitoring the progress of the Transformation Process
        /// </summary>
        public Func<TransformationExecutionStatus, Task> Progress { get; set; }

        /// <summary>
        /// Allows to retrieve the status of the Transformation Process
        /// </summary>
        /// <returns>The status of the Transformation Process</returns>
        public Task<TransformationExecutionStatus> GetStatusAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Raises the progress if property is set
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        protected virtual Task RaiseProgressAsync(TransformationExecutionStatus status)
        {
            if (Progress != null)
            {
                return Progress(status);
            }
            return Task.CompletedTask;
        }
    }
}
