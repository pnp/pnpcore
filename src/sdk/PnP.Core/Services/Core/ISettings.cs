using System;

namespace PnP.Core.Services
{
    /// <summary>
    /// PnP Core SDK settings
    /// </summary>
    public interface ISettings
    {
        string VersionTag { get; }
        string UserAgent { get; }
        bool DisableTelemetry { get; }
        Guid AADTenantId { get; set; }
    }
}
