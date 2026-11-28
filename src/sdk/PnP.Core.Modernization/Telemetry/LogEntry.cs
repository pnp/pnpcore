using System;

namespace PnP.Core.Modernization.Telemetry
{
    /// <summary>
    /// Defines an entry to log
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Create a new Log Entry
        /// </summary>
        public LogEntry()
        {
            EntryTime = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets Log message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets CorrelationId of type Guid
        /// </summary>
        public Guid CorrelationId { get; set; }
        
        /// <summary>
        /// Gets or sets Log source
        /// </summary>
        public string Source { get; set; }
        
        /// <summary>
        /// Gets or sets Log Exception
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Specified the logical grouping for the messages based on the stage of transformation
        /// </summary>
        public string Heading { get; set; }

        /// <summary>
        /// For those areas where we swallow errors or they are non-critical to report
        /// </summary>
        public bool IgnoreException { get; set; }

        /// <summary>
        /// Time in which the log entry was made
        /// </summary>
        public DateTime EntryTime { get; private set; }

        /// <summary>
        /// Page that's being transformed
        /// </summary>
        public string PageId { get; set; }

        /// <summary>
        /// Extra significance of the entry for the logs
        /// </summary>
        public LogEntrySignificance Significance { get; set; }

        /// <summary>
        /// Marks this error as a critical exception that prevents transformation
        /// </summary>
        public bool IsCriticalException { get; set; }
    }

    /// <summary>
    /// Specfies to the loggers that a specific entry has significance.
    /// </summary>
    public enum LogEntrySignificance
    {
        None,
        SourcePage,
        TargetPage,
        AssetTransferred,
        SourceSiteUrl,
        TargetSiteUrl,
        SharePointVersion,
        TransformMode,
        WebServiceFallback
    }
}
