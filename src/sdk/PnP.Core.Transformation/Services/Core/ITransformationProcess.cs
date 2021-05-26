using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Represents a Transformation Process
    /// </summary>
    public interface ITransformationProcess
    {
        /// <summary>
        /// The unique ID of a Transformation Process
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Starts the Transformation Process
        /// </summary>
        Task StartProcessAsync(CancellationToken token = default);

        /// <summary>
        /// Stops the Transformation Process
        /// </summary>
        Task StopProcessAsync(CancellationToken token = default);

        /// <summary>
        /// Allows monitoring the progress of the Transformation Process
        /// </summary>
        Func<TransformationProcessStatus, Task> Progress { get; set; }

        /// <summary>
        /// Allows to retrieve the status of the Transformation Process
        /// </summary>
        /// <returns>The status of the Transformation Process</returns>
        Task<TransformationProcessStatus> GetStatusAsync(CancellationToken token = default);


    }
}
