using PnP.Core.Services;
using System;
using System.Collections.Generic;

namespace PnP.Core.Test.Common.Services
{
    internal class TestTelemetryManager : TelemetryManager
    {
        public Action<Dictionary<string, string>, string> TelemetryEvent { get; set; }

        internal TestTelemetryManager(PnPGlobalSettingsOptions globalOptions) : base(globalOptions)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            TelemetryConfiguration.InstrumentationKey = "6073339d-9e70-4004-9ff7-1345316ade97";
#pragma warning restore CS0618 // Type or member is obsolete
        }

        internal override void LogInitRequest()
        {
            var properties = PopulateInitProperties();
            if (TelemetryEvent != null)
            {
                TelemetryEvent.Invoke(properties, "Init");
            }
        }

        internal override void LogServiceRequest(BatchRequest request, PnPContext context)
        {
            var properties = PopulateRequestProperties(request, context);
            if (TelemetryEvent != null)
            {
                TelemetryEvent.Invoke(properties, "Request");
            }
        }

    }
}
