namespace PnP.Core.Modernization.Telemetry
{
    /// <summary>
    /// Interface that needs to be implemented by any logger
    /// </summary>
    public interface ILogObserver
    {
        /// <summary>
        /// Log Information
        /// </summary>
        /// <param name="entry">LogEntry object</param>
        void Info(LogEntry entry);
        /// <summary>
        /// Warning Log
        /// </summary>
        /// <param name="entry">LogEntry object</param>
        void Warning(LogEntry entry);
        /// <summary>
        /// Error Log
        /// </summary>
        /// <param name="entry">LogEntry object</param>
#pragma warning disable CA1716
        void Error(LogEntry entry);
#pragma warning restore CA1716
        /// <summary>
        /// Debug Log
        /// </summary>
        /// <param name="entry">LogEntry object</param>
        void Debug(LogEntry entry);

        /// <summary>
        /// Pushes all output to destination and clears the cached log information
        /// </summary>
        void Flush();

        /// <summary>
        /// Pushes all output to destination and clears the cached log information if needed
        /// </summary>
        void Flush(bool clearLogData);

        /// <summary>
        /// Sets the id of the page that's being transformed
        /// </summary>
        /// <param name="pageId">id of the page</param>
        void SetPageId(string pageId);
    }
}
