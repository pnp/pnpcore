using System;
using System.Collections.Generic;

namespace PnP.Core.Modernization.Services.Builder.Configuration
{
    /// <summary>
    /// Options for configuring PnP Core SDK
    /// </summary>
    public class PnPModernizationOptions
    {
        /// <summary>
        /// Turns on/off telemetry, can be customized via configuration. Defaults to false.
        /// </summary>
        public bool DisableTelemetry { get; set; }
    }
}
