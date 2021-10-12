using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Service that creates a unique Correlation ID for every page transformation request
    /// </summary>
    public class CorrelationService
    {
        /// <summary>
        /// Returns a string enriched with the current Task Correlation ID
        /// </summary>
        /// <param name="taskId">The ID to correlate the string to</param>
        /// <param name="input">The string to correlate</param>
        /// <returns></returns>
        public string CorrelateString(Guid taskId, string input)
        {
            return $"[{taskId}] {input}";
        }
    }
}

