using System;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Tenant
{
    public class ApplyTenantConfiguration
    {
        [JsonPropertyName("doNotWaitForSitesToBeFullyCreated")]
        public bool DoNotWaitForSitesToBeFullyCreated { get; set; }

        [JsonIgnore]
        [Obsolete("Use DoNotWaitForSitesToBeFullyCreated")]
        public int DelayAfterModernSiteCreation { get; set; }
    }
}
