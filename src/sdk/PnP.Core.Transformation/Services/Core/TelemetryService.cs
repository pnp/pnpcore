using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Service that handles telemetry for pages transformation and/or errors
    /// </summary>
    public class TelemetryService
    {
        // Intentionally hard-coded
        private const string TELEMETRY_INSTRUMENTATION_KEY = "373400f5-a9cc-48f3-8298-3fd7f4c063d6";
        private const string TELEMETRY_ROLE_INSTANCE = "SharePointPnPPageTransformation";

        private readonly TelemetryClient telemetryClient;
        private readonly TelemetryConfiguration telemetryConfiguration = TelemetryConfiguration.CreateDefault();

        private string version;

        internal const string CorrelationId = "PnPCorrelationId";
        internal const string AADTenantId = "AADTenantId";
        private const string PageTransformed = "PageTransformed";
        private const string EngineVersion = "Version";
        private const string Duration = "Duration";

        #region Construction
        /// <summary>
        /// Instantiates the telemetry client
        /// </summary>
        public TelemetryService()
        {
            try
            {
                this.version = this.GetType().Assembly.FullName;

#pragma warning disable CS0618 // Type or member is obsolete
                this.telemetryConfiguration.InstrumentationKey = TELEMETRY_INSTRUMENTATION_KEY;
#pragma warning restore CS0618 // Type or member is obsolete

                this.telemetryClient = new TelemetryClient(this.telemetryConfiguration);

                this.telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
                this.telemetryClient.Context.Cloud.RoleInstance = TELEMETRY_ROLE_INSTANCE;
                this.telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            }
            catch
            {
                this.telemetryClient = null;
            }
        }
        #endregion

        /// <summary>
        /// Sends a transformation completed event
        /// </summary>
        /// <param name="duration">Duration of the page transformation</param>
        /// <param name="telemetryProperties">Additional properties to store with telemetry</param>
        public void LogTransformationCompleted(TimeSpan duration, Dictionary<string, string> telemetryProperties)
        {
            if (this.telemetryClient == null)
            {
                return;
            }

            try
            {
                try
                {
                    // Prepare event data
                    Dictionary<string, string> properties = new Dictionary<string, string>(telemetryProperties);
                    Dictionary<string, double> metrics = new Dictionary<string, double>(5);

                    // Page transformation engine version
                    properties.Add(EngineVersion, TelemetryService.GetVersion());

                    // Populate metrics
                    if (duration != TimeSpan.Zero)
                    {
                        // How long did it take to transform this page
                        metrics.Add(Duration, duration.TotalSeconds);
                    }

                    // Send the event
                    this.telemetryClient.TrackEvent(PageTransformed, properties, metrics);
                }
                finally
                {
                    // before exit, flush the remaining data
                    this.telemetryClient.Flush();
                }
            }
            catch
            {
                // Eat all exceptions 
            }
        }

        /// <summary>
        /// Logs a page transformation error
        /// </summary>
        /// <param name="ex">Exception object</param>
        /// <param name="telemetryProperties">Additional properties to store with telemetry</param>
        /// <param name="location">Location that generated this error</param>
        public void LogError(Exception ex, Dictionary<string, string> telemetryProperties, string location = null)
        {
            if (this.telemetryClient == null || ex == null)
            {
                return;
            }

            try
            {
                try
                {
                    // Prepare event data
                    Dictionary<string, string> properties = new Dictionary<string, string>(telemetryProperties);
                    Dictionary<string, double> metrics = new Dictionary<string, double>();

                    // Page transformation engine version
                    properties.Add(EngineVersion, TelemetryService.GetVersion());

                    if (!string.IsNullOrEmpty(location))
                    {
                        properties.Add("Location", location);
                    }

                    this.telemetryClient.TrackException(ex, properties, metrics);
                }
                finally
                {
                    // before exit, flush the remaining data
                    this.telemetryClient.Flush();
                }
            }
            catch
            {
                // Eat all exceptions 
            }
        }

        /// <summary>
        /// Gets the version of the assembly
        /// </summary>
        /// <returns></returns>
        internal static string GetVersion()
        {
            try
            {
                var coreAssembly = Assembly.GetExecutingAssembly();
                return ((AssemblyFileVersionAttribute)coreAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version.ToString();
            }
            catch
            {
                // Catch all and skip
            }

            return "undefined";
        }
    }
}
