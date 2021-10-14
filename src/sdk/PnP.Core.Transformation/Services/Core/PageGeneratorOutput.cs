using System;
using System.Collections.Generic;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Defines the output of a page generation
    /// </summary>
    public class PageGeneratorOutput
    {
        /// <summary>
        /// The URL of the generated page
        /// </summary>
        public Uri GeneratedPageUrl { get; set; }

        /// <summary>
        /// Defines a dictionary of custom properties retrieved during page generation and useful for telemetry
        /// </summary>
        public Dictionary<string, string> TelemetryProperties { get; set; } = new Dictionary<string, string>();
    }
}
