using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint.Pages.Public
{
    /// <summary>
    /// Model for PageConmponentManifest
    /// </summary>
    /// <typeparam name="T">Type of component properties</typeparam>
    public class PageComponentManifest<T>
    {
        [JsonPropertyName("alias")]
        public string Alias { get; set; }
        [JsonPropertyName("componentType")]
        public string ComponentType { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("proconfiguredEnties")]
        public List<PageComponentPreconfiguration<T>> PreconfiguredEnties { get; set; }
    }
}
