using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Defines the status of a transformation execution process
    /// </summary>
    public class TransformationExecutionStatus
    {
        /// <summary>
        /// Creates an instance for the process
        /// </summary>
        /// <param name="processId"></param>
        public TransformationExecutionStatus(Guid processId)
        {
            ProcessId = processId;
        }

        /// <summary>
        /// The ID of the process
        /// </summary>
        public Guid ProcessId { get;  }
    }
}
