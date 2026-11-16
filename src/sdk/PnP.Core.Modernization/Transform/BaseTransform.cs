using AngleSharp;
using Microsoft.SharePoint.Client;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

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
        public static string GetExactVersion(ClientRuntimeContext clientContext)
        {
            Uri urlUri = new Uri(clientContext.Url);

            if (!string.IsNullOrEmpty(CacheManager.Instance.GetExactSharePointVersion(urlUri)))
            {
                return CacheManager.Instance.GetExactSharePointVersion(urlUri);
            }
            else
            {
                GetVersion(clientContext);
                return CacheManager.Instance.GetExactSharePointVersion(urlUri);
            }
        }

        /// <summary>
        /// Gets the version of SharePoint
        /// </summary>
        /// <param name="clientContext"></param>
        /// <returns></returns>
        public static SPVersion GetVersion(ClientRuntimeContext clientContext)
        {
            Uri urlUri = new Uri(clientContext.Url);

            var spVersionFromCache = CacheManager.Instance.GetSharePointVersion(urlUri); 
            if (spVersionFromCache != SPVersion.Unknown)
            {
                return spVersionFromCache;
            }
            else
            {
                try
                {
                    
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{urlUri.Scheme}://{urlUri.DnsSafeHost}:{urlUri.Port}/_vti_pvt/service.cnf");
                    //request.Credentials = clientContext.Credentials;
                    request.AddAuthenticationData(clientContext as ClientContext);

                    var response = request.GetResponse();

                    using (var dataStream = response.GetResponseStream())
                    {
                        // Open the stream using a StreamReader for easy access.
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(dataStream))
                        {
                            // Read the content.Will be in this format
                            // SPO:
                            // vti_encoding: SR | utf8 - nl
                            // vti_extenderversion: SR | 16.0.0.8929
                            // SP2019:
                            // vti_encoding:SR|utf8-nl
                            // vti_extenderversion:SR|16.0.0.10340
                            // SP2016:
                            // vti_encoding: SR | utf8 - nl
                            // vti_extenderversion: SR | 16.0.0.4732
                            // SP2013:
                            // vti_encoding:SR|utf8-nl
                            // vti_extenderversion: SR | 15.0.0.4505
                            // Version numbers from https://buildnumbers.wordpress.com/sharepoint/

                            // Microsoft Developer Blog - 
                            //      https://developer.microsoft.com/en-us/sharepoint/blogs/updated-versions-of-the-sharepoint-on-premises-csom-nuget-packages/
                            // Todd Klindt's Blog - 
                            //      http://www.toddklindt.com/sp2010builds
                            //      http://www.toddklindt.com/sp2013builds
                            //      http://www.toddklindt.com/sp2016builds
                            //      http://www.toddklindt.com/sp2019builds


                            string version = reader.ReadToEnd().Split('|')[2].Trim();
                            CacheManager.Instance.SetExactSharePointVersion(urlUri, version);

                            if (Version.TryParse(version, out Version v))
                            {
                                if (v.Major == 14)
                                {
                                    CacheManager.Instance.SetSharePointVersion(urlUri, SPVersion.SP2010);
                                    return SPVersion.SP2010;
                                }
                                else if (v.Major == 15)
                                {
                                    // You can change the output to SP2013 to use standard CSOM calls.
                                    CacheManager.Instance.SetSharePointVersion(urlUri, SPVersion.SP2013Legacy);
                                    return SPVersion.SP2013Legacy;

                                }
                                else if (v.Major == 16)
                                {
                                    if (v.MinorRevision < 6000)
                                    {
                                        //if(v.MinorRevision < 4690)
                                        //{
                                        //    // Pre May 2018 CU
                                        //    CacheManager.Instance.SharepointVersions.TryAdd(urlUri, SPVersion.SP2016Legacy);
                                        //    return SPVersion.SP2016Legacy;
                                        //}
                                        
                                        CacheManager.Instance.SetSharePointVersion(urlUri, SPVersion.SP2016Legacy);
                                        return SPVersion.SP2016Legacy;
                                    }
                                    // Set to 12000 because some SPO reports as 12012 and SP2019 build numbers are increasing very slowly
                                    else if (v.MinorRevision > 10300 && v.MinorRevision < 12000)
                                    {
                                        CacheManager.Instance.SetSharePointVersion(urlUri, SPVersion.SP2019);
                                        return SPVersion.SP2019;
                                    }
                                    else
                                    {
                                        CacheManager.Instance.SetSharePointVersion(urlUri, SPVersion.SPO);
                                        return SPVersion.SPO;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (WebException)
                {
                    // todo
                }
            }

            CacheManager.Instance.SetSharePointVersion(urlUri, SPVersion.SPO);
            return SPVersion.SPO;
        }
        #endregion
    }
}
