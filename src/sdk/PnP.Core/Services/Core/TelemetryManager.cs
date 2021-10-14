using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Reflection;
#if NET5_0
using System.Runtime.InteropServices;
#endif

namespace PnP.Core.Services
{
    internal class TelemetryManager
    {
        internal static string InstrumentationKey = "ffe6116a-bda0-4f0a-b0cf-d26f1b0d84eb";

        internal TelemetryManager(PnPGlobalSettingsOptions globalOptions)
        {
            // Ensure there's only one telemetry client and configuration created in the current process
            // Otherwise we're running into possible memory leaks: https://briancaos.wordpress.com/2020/05/07/c-azure-telemetryclient-will-leak-memory-if-not-implemented-as-a-singleton/
            (TelemetryConfiguration, TelemetryClient) = TelemetryClientFactory.GetTelemetryClientAndConfiguration(InstrumentationKey);

            GlobalOptions = globalOptions;

            Assembly coreAssembly = Assembly.GetExecutingAssembly();
            Version = ((AssemblyFileVersionAttribute)coreAssembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version;
        }

        /// <summary>
        /// Telemetry configuration
        /// </summary>
        internal TelemetryConfiguration TelemetryConfiguration { get; private set; }

        /// <summary>
        /// Azure AppInsights Telemetry client
        /// </summary>
        internal TelemetryClient TelemetryClient { get; private set; }

        /// <summary>
        /// Settings client
        /// </summary>
        internal PnPGlobalSettingsOptions GlobalOptions { get; private set; }

        /// <summary>
        /// File version of the PnP Core SDK
        /// </summary>
        internal string Version { get; private set; }

        internal virtual void LogServiceRequest(BatchRequest request, PnPContext context)
        {
            TelemetryClient.TrackEvent("PnPCoreRequest", PopulateRequestProperties(request, context));
        }

        internal virtual void LogInitRequest()
        {
            TelemetryClient.TrackEvent("PnPCoreInit", PopulateInitProperties());
        }

        internal Dictionary<string, string> PopulateInitProperties()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>(10)
            {
                { "PnPCoreSDKVersion", Version },
                { "AADTenantId", GlobalOptions.AADTenantId.ToString() },
                { "OS", GetOSVersion() },

            };

            return properties;
        }

        internal Dictionary<string, string> PopulateRequestProperties(BatchRequest request, PnPContext context)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>(10)
            {
                { "PnPCoreSDKVersion", Version },
                { "AADTenantId", GlobalOptions.AADTenantId.ToString() },
                { "Model", request.Model.GetType().FullName },
                { "ApiType", request.ApiCall.Type.ToString() },
                { "ApiMethod", request.Method.ToString() },
                { "GraphFirst", context.GraphFirst.ToString() },
                { "GraphCanUseBeta", context.GraphCanUseBeta.ToString() },
                { "GraphAlwaysUseBeta", context.GraphAlwaysUseBeta.ToString() },
                { "OS", GetOSVersion() },
                { "Operation", request.OperationName },
            };

            return properties;
        }

        private static string GetOSVersion()
        {

#if NET5_0
            if (RuntimeInformation.RuntimeIdentifier == "browser-wasm")
            {
                return "WASM";
            }
#endif

            if (OperatingSystem.IsWindows())
            {
                return "Windows";
            }
            else if (OperatingSystem.IsLinux())
            {
                return "Linux";
            }
            else if (OperatingSystem.IsMacOS())
            {
                return "MacOS";
            }

            return Environment.OSVersion.Platform.ToString();
        }
    }
}
