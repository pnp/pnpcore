using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.Copilot.Public.DTO
{
    /// <summary>
    /// Represents a request to retrieve relevant text extracts from specified data sources using a natural language
    /// query and optional filtering or configuration parameters.
    /// </summary>
    /// <remarks>Use this class to specify the parameters for retrieving content from supported sources such
    /// as SharePoint, OneDrive for Business, or Copilot connectors. Configure the query, data source, and optional
    /// metadata or filtering to tailor the retrieval to your application's needs. Ensure that required properties are
    /// set before submitting the request. The class supports limiting the number of results and customizing the
    /// metadata returned for each item.</remarks>
    public class RetrievalRequest
    {
        /// <summary>
        /// Natural language query string used to retrieve relevant text extracts. This parameter has a limit of 1,500 characters. Your queryString should be a single sentence, and you should avoid spelling errors in context-rich keywords. Required
        /// </summary>
        [JsonPropertyName("queryString")]
        public string QueryString { get; set; } = "*";
        /// <summary>
        /// Indicates whether extracts should be retrieved from SharePoint, OneDrive, or Copilot connectors. Acceptable values are sharePoint, oneDriveBusiness, and externalItem. Required.
        /// </summary>
        [JsonPropertyName("dataSource")]
        public string DataSource { get; set; } = "sharepoint";
        /// <summary>
        /// Contains additional configuration information for applicable data sources. dataSourceConfiguration includes an object called externalItem, where you can configure Copilot connectors retrieval. Optional.
        /// </summary>
        [JsonPropertyName("dataSourceConfiguration")]
        public DataSourceConfiguration? DataSourceConfiguration { get; set; }
        /// <summary>
        /// A list of metadata fields to be returned for each item in the response. Only retrievable metadata properties can be included in this list. By default, no metadata is returned. Optional.
        /// </summary>
        [JsonPropertyName("resourceMetadata")]
        public List<string>? ResourceMetadata { get; set; }
        /// <summary>
        /// The number of results that are returned in the response. Must be between 1 and 25. By default, returns up to 25 results. Optional.
        /// </summary>
        [JsonPropertyName("maximumNumberOfResults")]
        public int MaximumNumberOfResults { get; set; } = 25;
        /// <summary>
        /// Keyword Query Language (KQL) expression with queryable SharePoint, OneDrive, or Copilot connectors properties and attributes to scope the retrieval before the query runs.
        /// </summary>
        [JsonPropertyName("filterExpression")]
        public string? FilterExpression { get; set; }
    }
    /// <summary>
    /// Represents the data source configuration
    /// </summary>
    public class DataSourceConfiguration
    {
        [JsonPropertyName("externalItem")]
        public ExternalItemConfiguration? ExternalItem { get; set; }
    }
    /// <summary>
    /// Represents configuration options for retrieving data from Copilot connectors
    /// </summary>
    public class ExternalItemConfiguration
    {
        /// <summary>
        /// An array of connection objects specifying the Copilot connector connection identifiers to include in retrieval. Required.
        /// </summary>
        [JsonPropertyName("connections")]
        public List<ExternalItemConnection> Connections { get; set; } = new List<ExternalItemConnection>();
    }
    /// <summary>
    /// Represents a Copilot connector to include in retrieval operations
    /// </summary>
    public class ExternalItemConnection
    {
        /// <summary>
        /// The ID of a Copilot connector connection to include. Required.
        /// </summary>
        [JsonPropertyName("connectionId")]
        public string ConnectionId { get; set; } = string.Empty;
    }

}
