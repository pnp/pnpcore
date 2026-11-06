using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// Class that instantiates a telemetry client and configuration once per process
    /// </summary>
    internal static class TelemetryClientFactory
    {
        internal static TelemetryConfiguration telemetryConfiguration;
        internal static TelemetryClient telemetryClient;

        internal static Tuple<TelemetryConfiguration, TelemetryClient> GetTelemetryClientAndConfiguration(string instrumentationKey)
        {
            if (instrumentationKey == null)
            {
                throw new ArgumentNullException(nameof(instrumentationKey));
            }

            if (telemetryConfiguration == null)
            {
                // Deliberately not TelemetryConfiguration.CreateDefault(): on Application Insights v3 that
                // returns a process wide shared instance, so setting the connection string on it would point
                // the hosting application's telemetry at the PnP instrumentation key, and would throw once
                // anything else in the process has built a TelemetryClient from it. A library needs its own
                // isolated configuration.
                telemetryConfiguration = new TelemetryConfiguration();
                telemetryConfiguration.ConnectionString = $"InstrumentationKey={instrumentationKey}";
            }

            if (telemetryClient == null)
            {
                telemetryClient = new TelemetryClient(telemetryConfiguration);
            }

            return new Tuple<TelemetryConfiguration, TelemetryClient>(telemetryConfiguration, telemetryClient);
        }
    }
}
