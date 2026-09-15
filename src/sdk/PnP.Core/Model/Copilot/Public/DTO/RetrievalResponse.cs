using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.Copilot.Public.DTO
{
    /// <summary>
    /// Represents results from a retrieval query.
    /// </summary>
    public class RetrievalResponse
    {
        /// <summary>
        /// A collection of the retrieval results. If empty, no relevant results were found.
        /// </summary>
        [JsonPropertyName("retrievalHits")]
        public List<RetrievalHit> RetrievalHits { get; set; } = new List<RetrievalHit>();
    }
    /// <summary>
    /// Represents a single result within the list of retrieval results.
    /// </summary>
    public class RetrievalHit
    {
        /// <summary>
        /// The resource type of the item. Possible values: site, list, listItem, externalItem, drive, driveItem ,unknownFutureValue
        /// </summary>
        [JsonPropertyName("resourceType")]
        public string ResourceType { get; set; } = string.Empty;
        /// <summary>
        /// The URL of the item in which the extract was retrieved.
        /// </summary>
        [JsonPropertyName("webUrl")]
        public string WebUrl { get; set; } = string.Empty;
        /// <summary>
        /// An array of text extracts extracted from the document for Retrieval-Augmented Generation. Currently, only one text snippet is extracted.
        /// </summary>
        [JsonPropertyName("extracts")]
        public List<RetrievalExtract> Extracts { get; set; } = new List<RetrievalExtract>();
        /// <summary>
        /// A JSON object with information about the document's sensitivity label.
        /// </summary>
        [JsonPropertyName("sensitivityLabelInfo")]
        public SensitivityLabelInfo? SensitivityLabelInfo { get; set; }
        /// <summary>
        /// The requested SharePoint and Microsoft 365 Copilot connectors metadata from the request payload (empty if not applicable).
        /// </summary>
        [JsonPropertyName("resourceMetadata")]
        public Dictionary<string, object>? ResourceMetadata { get; set; }
    }
    /// <summary>
    /// Represents a single extract within the list of retrieval extracts.
    /// </summary>
    public class RetrievalExtract
    {
        /// <summary>
        /// The text extract received.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
        /// <summary>
        /// The cosine similarity between the text extract and the queryString, normalized to the 0-1 range. It's possible for a retrievalExtract to be returned without a relevance score.
        /// </summary>
        [JsonPropertyName("relevanceScore")]
        public float RelevanceScore { get; set; }
    }
    /// <summary>
    /// Describes the information protection label that details how to properly apply a sensitivity label to information.
    /// </summary>
    public class SensitivityLabelInfo
    {
        /// <summary>
        /// The ID of the sensitivity label.
        /// </summary>
        [JsonPropertyName("sensitivityLabelId")]
        public string SensitivityLabelId { get; set; } = string.Empty;
        /// <summary>
        /// The display name for the sensitivity label
        /// </summary>
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;
        /// <summary>
        /// The color that the UI should display for the label, if configured.
        /// </summary>
        [JsonPropertyName("color")]
        public string Color { get; set; } = string.Empty;
        /// <summary>
        /// The priority in which the sensitivity label is applied.
        /// </summary>
        [JsonPropertyName("priority")]
        public int Priority { get; set; }
        /// <summary>
        /// The tooltip that should be displayed for the label in a UI.
        /// </summary>
        [JsonPropertyName("tooltip")]
        public string Tooltip { get; set; } = string.Empty;
    }
}
