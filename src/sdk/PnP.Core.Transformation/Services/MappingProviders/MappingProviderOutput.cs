using PnP.Core.Transformation.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the output for a mapping provider
    /// </summary>
    public class MappingProviderOutput: BaseMappingProviderOutput
    {
        /// <summary>
        /// The modern page to create as the result of the transformation
        /// </summary>
        public Page TargetPage { get; } = new Page();

        /// <summary>
        /// Defines the metadata fields of the target page
        /// </summary>
        public Dictionary<string, FieldData> Metadata { get; } = new Dictionary<string, FieldData>();

        /// <summary>
        /// Defines the list item permissions for the target page
        /// </summary>
        public ListItemPermission Permissions { get; set; }

        /// <summary>
        /// Defines a dictionary of custom properties retrieved during source content processing and useful for telemetry
        /// </summary>
        public Dictionary<string, string> TelemetryProperties { get; set; } = new Dictionary<string, string>();
    }
}
