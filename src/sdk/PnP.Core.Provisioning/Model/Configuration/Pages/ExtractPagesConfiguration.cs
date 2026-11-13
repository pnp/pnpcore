using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Pages
{
    public class ExtractPagesConfiguration
    {
        [JsonPropertyName("excludeAuthorInformation")]
        public bool ExcludeAuthorInformation { get; set; }

        [JsonPropertyName("includeAllClientSidePages")]
        public bool IncludeAllClientSidePages { get; set; }

        [JsonPropertyName("extractPageAsPublished")]
        public bool ExtractPageAsPublished { get; set; } = true;

        /// <summary>
        /// Names of the pages to extract, relative to the pages library (e.g. <c>News.aspx</c>,
        /// <c>Folder/News.aspx</c> or <c>Templates/Page.aspx</c>). When set, only these pages (and their
        /// translations) are extracted and <see cref="IncludeAllClientSidePages"/> is ignored.
        /// </summary>
        [JsonPropertyName("pageNames")]
        public List<string> PageNames { get; set; } = new List<string>();
    }
}
