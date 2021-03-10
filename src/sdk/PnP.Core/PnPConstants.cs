using System;

namespace PnP.Core
{
    /// <summary>
    /// Support class to provide all the constants for the engine
    /// </summary>
    internal static class PnPConstants
    {
        /// <summary>
        /// The string base URL of the Microsoft Graph
        /// </summary>
        internal const string MicrosoftGraphBaseUrl = "https://graph.microsoft.com/";

        /// <summary>
        /// The base URI of the Microsoft Graph
        /// </summary>
        internal static readonly Uri MicrosoftGraphBaseUri = new Uri(MicrosoftGraphBaseUrl);

        /// <summary>
        /// Graph v1 endpoint
        /// </summary>
        internal const string GraphV1Endpoint = "v1.0";

        /// <summary>
        /// Graph beta endpoint 
        /// </summary>
        internal const string GraphBetaEndpoint = "beta";

        /// <summary>
        /// The name of the URI metadata attribute
        /// </summary>
        internal const string MetaDataUri = "uri";

        /// <summary>
        /// The name of the ID metadata attribute
        /// </summary>
        internal const string MetaDataId = "id";

        /// <summary>
        /// The name of the Data Type metadata attribute
        /// </summary>
        internal const string MetaDataType = "type";

        /// <summary>
        /// The name of the Graph ODATA metadata attribute
        /// </summary>
        internal const string MetaDataGraphType = "@odata.type";

        /// <summary>
        /// The name of the ETag metadata attribute
        /// </summary>
        internal const string MetaDataEtag = "etag";

        /// <summary>
        /// The name of the Microsoft Graph ID metadata attribute
        /// </summary>
        internal const string MetaDataGraphId = "graphId";

        /// <summary>
        /// The name of the REST Id metadata attribute
        /// </summary>
        internal const string MetaDataRestId = "restId";

        /// <summary>
        /// The name of the Entity Type Name metadata attribute
        /// </summary>
        internal const string MetaDataRestEntityTypeName = "EntityTypeName";

        /// <summary>
        /// Next link in Microsoft Graph
        /// </summary>
        internal const string GraphNextLink = "@odata.nextLink";

        /// <summary>
        /// Next link in SharePoint REST
        /// </summary>
        internal const string SharePointRestListItemNextLink = "__next";

        /// <summary>
        /// Metadata property in SharePoint REST
        /// </summary>
        internal const string SharePointRestMetadata = "__metadata";
    }
}
