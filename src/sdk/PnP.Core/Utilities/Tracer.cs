using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PnP.Core
{
    internal class Tracer : IDisposable
    {
        private readonly ILogger logger;
        internal string MethodName { get; set; }
        internal string FilePath { get; set; }
        internal int LineNumber { get; set; }
        internal Stopwatch Stopwatch { get; set; }
        internal DateTime StopwatchStart { get; set; }
        internal DateTime StopwatchStop { get; set; }

        private Tracer(ILogger loggerInstance, string methodName, string filePath, int lineNumber)
        {
            logger = loggerInstance;
            MethodName = methodName;
            FilePath = filePath;
            LineNumber = lineNumber;
            StopwatchStart = DateTime.UtcNow;
            Stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            this.Stopwatch.Stop();
            this.StopwatchStop = DateTime.UtcNow;
            LogMetrics();
        }

        private void LogMetrics()
        {
            logger.LogDebug("Method '{0}' | File '{1}' | Line Number: {2}  | Started: {3} | Ended {4} | Elapsed {5} ms",
                MethodName,
                FilePath,
                LineNumber,
                StopwatchStart.ToString("MM/dd/yyyy h:mm:ss.fff tt"),
                StopwatchStop.ToString("MM/dd/yyyy h:mm:ss.fff tt"),
                Stopwatch.ElapsedMilliseconds);
        }

        internal static Tracer Track(ILogger logger, [CallerMemberName] string callingMethodName = "", [CallerFilePath] string callingFilePath = "", [CallerLineNumber] int callingLineNumber = 0)
        {
            return new Tracer(logger, callingMethodName, callingFilePath, callingLineNumber);
        }
    }
}
