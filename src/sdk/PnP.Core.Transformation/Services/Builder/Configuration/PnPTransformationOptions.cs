using System;
using System.Collections.Generic;

namespace PnP.Core.Transformation.Services.Builder.Configuration
{
    /// <summary>
    /// Options for configuring PnP Core SDK
    /// </summary>
    public class PnPTransformationOptions
    {
        /// <summary>
        /// Turns on/off telemetry, can be customized via configuration. Defaults to false.
        /// </summary>
        public bool DisableTelemetry { get; set; }

        /// <summary>
        /// Defines the connection string for the configured persistence provider
        /// </summary>
        public string PersistenceProviderConnectionString { get; set; }

        /// <summary>
        /// Defines the path where to store logs, if any
        /// </summary>
        public string LogStoragePath { get; set; }

        // TODO: Here we can configure all the "generic" configuration settings for the engine
    }
}
