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
        private static TelemetryConfiguration telemetryConfiguration;
        private static TelemetryClient telemetryClient;

        internal static Tuple<TelemetryConfiguration, TelemetryClient> GetTelemetryClientAndConfiguration(string instrumentationKey)
        {
            if (telemetryConfiguration == null)
            {
                telemetryConfiguration = TelemetryConfiguration.CreateDefault();
#pragma warning disable CS0618 // Type or member is obsolete
                telemetryConfiguration.InstrumentationKey = instrumentationKey;
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (telemetryClient == null)
            {
                telemetryClient = new TelemetryClient(telemetryConfiguration);
            }

            return new Tuple<TelemetryConfiguration, TelemetryClient>(telemetryConfiguration, telemetryClient);
        }
    }
}
