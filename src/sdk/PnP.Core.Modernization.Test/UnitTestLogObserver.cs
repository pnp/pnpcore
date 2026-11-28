using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Modernization.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Tests
{
    public class UnitTestLogObserver : ILogObserver
    {
        public bool FailOnNonCritical { get; private set; }

        public UnitTestLogObserver(bool failOnNonCritical = false)
        {
            FailOnNonCritical = failOnNonCritical;
        }

        public void Debug(LogEntry entry)
        {
            Console.WriteLine($"DEBUG: {entry.Heading}  - Message: {entry.Message}");
        }

        public void Error(LogEntry entry)
        {
            var error = entry.Exception != null ? entry.Exception.Message : "No error logged";
            Console.WriteLine($"ERROR: {entry.Heading} Message: {entry.Message} \n\t Source: {entry.Source}, Error: { error }");
            Console.WriteLine($"ERROR: Stack Trace: {entry.Exception.StackTrace}");

            if (entry.IsCriticalException || FailOnNonCritical)
            {
                Assert.Fail(entry.Message);
            }


        }

        public void Flush()
        {
            //Do nothing
        }

        public void Info(LogEntry entry)
        {
            //Console.WriteLine($"INFO: {entry.Heading} Message: {entry.Message} \n\t Source: {entry.Source}");
        }

        public void Warning(LogEntry entry)
        {
            Console.WriteLine($"WARNING: {entry.Heading} Message: {entry.Message} \n\t Source: {entry.Source}");
        }

        public void SetPageId(string pageId)
        {
            //throw new NotImplementedException();
        }

        public void Flush(bool clearLogData)
        {
            //Do nothing
        }
    }
}
