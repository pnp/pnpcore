using AngleSharp;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace PnP.Core.Modernization.Transform
{
    /// <summary>
    /// Base logging implementation
    /// </summary>
    public class BaseTransform
    {
        private IList<ILogObserver> _logObservers;
        private Guid _correlationId;

        /// <summary>
        /// List of registered log observers
        /// </summary>
        public IList<ILogObserver> RegisteredLogObservers {
            get{
                return _logObservers;
            }
        }

        /// <summary>
        /// Instantiation of base transform class
        /// </summary>
        public BaseTransform()
        {
            _logObservers = new List<ILogObserver>();
            _correlationId = Guid.NewGuid();
        }
       
        /// <summary>
        /// Registers the observer
        /// </summary>
        /// <param name="observer">The observer.</param>
        public void RegisterObserver(ILogObserver observer)
        {
            if (!_logObservers.Contains(observer))
            {
                _logObservers.Add(observer);
            }
        }

        /// <summary>
        /// Flush all log observers
        /// </summary>
        public void FlushObservers()
        {
            int i = 0;
            foreach (ILogObserver observer in _logObservers)
            {
                i++;

                if (i == _logObservers.Count)
                {
                    observer.Flush(true);
                }
                else
                {
                    observer.Flush(false);
                }
            }
        }

        /// <summary>
        /// Flush Specific Observer of a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void FlushSpecificObserver<T>()
        {
            var observerType = typeof(T);

            foreach (ILogObserver observer in _logObservers)
            {
                if (observer.GetType() == observerType)
                {
                    observer.Flush();
                }
            }
        }


        /// <summary>
        /// Notifies the observers of error messages
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="heading">The logical grouping for the messages based on the stage of transformation</param>
        /// <param name="exception">Exception object</param>
        /// <param name="ignoreException">For those areas where we swallow errors or they are non-critical to report</param>
        /// <param name="isCriticalException">Marks this error as a critical exception that prevents transformation</param>
        public void LogError(string message, string heading = "", Exception exception = null, bool ignoreException = false, bool isCriticalException = false)
        {
            StackTrace stackTrace = new StackTrace();
            var logEntry = new LogEntry() {
                Heading = heading,
                Message = message,
                CorrelationId = _correlationId,
                Source = stackTrace.GetFrame(1).GetMethod().ToString(),
                Exception = exception,
                IgnoreException = ignoreException,
                IsCriticalException = isCriticalException
            };

            Log(logEntry, LogLevel.Error);
        }

        /// <summary>
        /// Notifies the observers of info messages
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="heading">The logical grouping for the messages based on the stage of transformation</param>
        /// <param name="significance">Extra significance of the entry for the logs</param>
        public void LogInfo(string message, string heading = "", LogEntrySignificance significance = LogEntrySignificance.None)
        {
            StackTrace stackTrace = new StackTrace();
            var logEntry = new LogEntry() { Heading = heading, Message = message, CorrelationId = _correlationId, Source = stackTrace.GetFrame(1).GetMethod().ToString(), Significance = significance };

            Log(logEntry, LogLevel.Information);
        }

        /// <summary>
        /// Notifies the observers of warning messages
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="heading">The logical grouping for the messages based on the stage of transformation</param>
        public void LogWarning(string message, string heading = "")
        {
            StackTrace stackTrace = new StackTrace();
            var logEntry = new LogEntry() { Heading = heading, Message = message, CorrelationId = _correlationId, Source = stackTrace.GetFrame(1).GetMethod().ToString() };

            Log(logEntry, LogLevel.Warning);
        }

        /// <summary>
        /// Notifies the observers of debug messages
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="heading">The logical grouping for the messages based on the stage of transformation</param>
        public void LogDebug(string message, string heading = "")
        {
            StackTrace stackTrace = new StackTrace();
            var logEntry = new LogEntry() { Heading = heading, Message = message, CorrelationId = _correlationId, Source = stackTrace.GetFrame(1).GetMethod().ToString() };

            Log(logEntry, LogLevel.Debug);
        }

        /// <summary>
        /// Log entries into the observers
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="level"></param>
        public void Log(LogEntry entry, LogLevel level)
        {
            foreach (ILogObserver observer in _logObservers)
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        observer.Debug(entry);
                        break;
                    case LogLevel.Error:
                        observer.Error(entry);
                        break;
                    case LogLevel.Warning:
                        observer.Warning(entry);
                        break;
                    case LogLevel.Information:
                        observer.Info(entry);
                        break;
                    default:
                        observer.Info(entry);
                        break;
                }
                
            }
        }

        /// <summary>
        /// Sets the page name of the page being transformed
        /// </summary>
        /// <param name="pageId">Name of the page being transformed</param>
        public void SetPageId(string pageId)
        {
            foreach (ILogObserver observer in _logObservers)
            {
                observer.SetPageId(pageId);
            }
        }

        #region Helper methods

        /// <summary>
        /// Gets exact version of SharePoint
        /// </summary>
        /// <param name="clientContext"></param>
        /// <returns></returns>
        public static string GetExactVersion(PnPContext clientContext)
        {
            SharePointOnlineSupport.Validate(clientContext);
            var cached = CacheManager.Instance.GetExactSharePointVersion(clientContext.Uri);
            if (!string.IsNullOrEmpty(cached))
            {
                return cached;
            }
            var response = clientContext.Web.ExecuteRequest(new ApiRequest(
                HttpMethod.Post, ApiRequestType.SPORest,
                "_api/contextinfo", null, new Dictionary<string, string>
                {
                    ["Accept"] = "application/json;odata=nometadata"
                }));
            using (var json = JsonDocument.Parse(response.Response))
            {
                var value = json.RootElement.GetProperty("LibraryVersion").GetString();
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidDataException("SharePoint did not return its library version.");
                }
                CacheManager.Instance.SetExactSharePointVersion(clientContext.Uri, value);
                return value;
            }
        }

        /// <summary>
        /// Gets the version of SharePoint
        /// </summary>
        /// <param name="clientContext"></param>
        /// <returns></returns>
        public static SPVersion GetVersion(PnPContext clientContext)
        {
            SharePointOnlineSupport.Validate(clientContext);
            CacheManager.Instance.SetSharePointVersion(clientContext.Uri, SPVersion.SPO);
            return SPVersion.SPO;
        }
        #endregion
    }
}
