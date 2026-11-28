using PnP.Core.Modernization.Telemetry;
using System;
using System.Collections.Generic;

namespace PnP.Core.Modernization.Entities
{
    /// <summary>
    /// Transformation analysis report
    /// </summary>
    public class TransformationLogAnalysis
    {
        /// <summary>
        /// Constructor for transformation report
        /// </summary>
        public TransformationLogAnalysis()
        {
            Warnings = new List<Tuple<LogLevel, LogEntry>>();
            Errors = new List<Tuple<LogLevel, LogEntry>>();
            SourcePage = string.Empty;
            TargetPage = string.Empty;
            SourceSite = string.Empty;
            TargetSite = string.Empty;
            BaseSourceUrl = string.Empty;
            BaseTargetUrl = string.Empty;
            AssetsTransferred = new List<Tuple<LogLevel, LogEntry>>();
            PageLogsOrdered = new List<Tuple<LogLevel, LogEntry>>();
            TransformationVerboseSummary = new List<Tuple<LogLevel, LogEntry>>();
            TransformationVerboseDetails = new List<Tuple<LogLevel, LogEntry>>();
            TransformationSettings = new List<Tuple<string, string>>();
        }

        /// <summary>
        /// Source page name
        /// </summary>
        public string SourcePage { get; set; }

        /// <summary>
        /// Source site
        /// </summary>
        public string SourceSite { get; set; }

        /// <summary>
        /// Target page name
        /// </summary>
        public string TargetPage { get; set; }

        /// <summary>
        /// Target site
        /// </summary>
        public string TargetSite { get; set; }

        /// <summary>
        /// Date report generated
        /// </summary>
        public DateTime ReportDate { get; set; }

        /// <summary>
        /// Base source url
        /// </summary>
        public string BaseSourceUrl { get; set; }

        /// <summary>
        /// Base target url
        /// </summary>
        public string BaseTargetUrl { get; set; }

        /// <summary>
        /// Duration of the page tranformation
        /// </summary>
        public TimeSpan TransformationDuration { get; set; }

        /// <summary>
        /// Indication if this was the first transformation report added
        /// </summary>
        public bool IsFirstAnalysis { get; set; }

        /// <summary>
        /// ID used to group entries by transformed page
        /// </summary>
        public string PageId { get; set; }

        /// <summary>
        /// Log entries for the transferred assets
        /// </summary>
        public List<Tuple<LogLevel, LogEntry>> AssetsTransferred { get; set; }

        /// <summary>
        /// List of warnings raised
        /// </summary>
        public List<Tuple<LogLevel, LogEntry>> Warnings { get; set; }

        /// <summary>
        /// List of errors raised
        /// </summary>
        public List<Tuple<LogLevel, LogEntry>> Errors { get; set; }

        /// <summary>
        /// List of critical application error
        /// </summary>
        public List<Tuple<LogLevel, LogEntry>> CriticalErrors { get; set; }

        /// <summary>
        /// Page Logs ordered
        /// </summary>
        public List<Tuple<LogLevel, LogEntry>> PageLogsOrdered { get; set; }

        /// <summary>
        /// Logs that contain summary data for verbose logging
        /// </summary>
        public List<Tuple<LogLevel, LogEntry>> TransformationVerboseSummary { get; set; }

        /// <summary>
        /// Logs that contain verbose details of this transformation
        /// </summary>
        public List<Tuple<LogLevel, LogEntry>> TransformationVerboseDetails { get; set; }

        /// <summary>
        /// List containting the applied transformation settings
        /// </summary>
        public List<Tuple<string, string>> TransformationSettings { get; set; }
    }
}