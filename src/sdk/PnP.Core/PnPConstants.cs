using System;

namespace PnP.Core
{
    /// <summary>
    /// Support class to provide all the constants for the engine
    /// </summary>
    public static class PnPConstants
    {
        /// <summary>
        /// The string base URL of the Microsoft Graph
        /// </summary>
        public const string MicrosoftGraphBaseUrl = "https://graph.microsoft.com/";

        /// <summary>
        /// The base URI of the Microsoft Graph
        /// </summary>
        public static readonly Uri MicrosoftGraphBaseUri = new Uri(MicrosoftGraphBaseUrl);

        /// <summary>
        /// Graph v1 endpoint
        /// </summary>
        public static readonly string GraphV1Endpoint = "v1.0";

        /// <summary>
        /// Graph beta endpoint 
        /// </summary>
        public static readonly string GraphBetaEndpoint = "beta";

        /// <summary>
        /// The name of the URI metadata attribute
        /// </summary>
        public const string MetaDataUri = "uri";

        /// <summary>
        /// The name of the ID metadata attribute
        /// </summary>
        public const string MetaDataId = "id";

        /// <summary>
        /// The name of the Data Type metadata attribute
        /// </summary>
        public const string MetaDataType = "type";

        /// <summary>
        /// The name of the Graph ODATA metadata attribute
        /// </summary>
        public const string MetaDataGraphType = "@odata.type";

        /// <summary>
        /// The name of the ETag metadata attribute
        /// </summary>
        public const string MetaDataEtag = "etag";

        /// <summary>
        /// The name of the Microsoft Graph ID metadata attribute
        /// </summary>
        public const string MetaDataGraphId = "graphId";

        /// <summary>
        /// The name of the REST Id metadata attribute
        /// </summary>
        public const string MetaDataRestId = "restId";

        /// <summary>
        /// The name of the Entity Type Name metadata attribute
        /// </summary>
        public const string MetaDataRestEntityTypeName = "EntityTypeName";
    }
}
