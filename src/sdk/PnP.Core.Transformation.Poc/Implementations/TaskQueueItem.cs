using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Poc.Implementations
{
    public class TaskQueueItem
    {
        public Uri SourcePageUri { get; set; }

        public Guid TaskId { get; set; }

        public Guid ProcessId { get; set; }
    }
}
