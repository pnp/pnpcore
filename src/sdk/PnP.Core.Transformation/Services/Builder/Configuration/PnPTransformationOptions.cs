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

        // TODO: Define all the configuration settings
    }
}
