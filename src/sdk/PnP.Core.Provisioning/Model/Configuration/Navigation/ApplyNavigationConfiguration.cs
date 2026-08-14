using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Navigation
{
    public class ApplyNavigationConfiguration
    {
        [JsonPropertyName("clearNavigation")]
        public bool ClearNavigation { get; set; }
    }
}
