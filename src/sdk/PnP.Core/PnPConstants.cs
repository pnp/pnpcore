using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Utils.CustomConverters;
using PnP.Core.Test.Services.Core.CSOM.Utils.CustomConverters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        /// The id of the list holding an item
        /// </summary>
        internal const string MetaDataListId = "listId";

        /// <summary>
        /// The id of the TermSet holding a term
        /// </summary>
        internal const string MetaDataTermSetId = "termSetId";

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
        internal const string SharePointRestListItemNextLink = "odata.nextLink";

        /// <summary>
        /// Metadata property in SharePoint REST
        /// </summary>
        internal const string SharePointRestMetadata = "__metadata";

        /// <summary>
        /// Id of the generic request module
        /// </summary>
        internal static Guid GenericRequestModuleId = Guid.Parse("{711CA09B-92E7-4A45-BB4C-E6422632E7F0}");

        /// <summary>
        /// Id of the custom header request module
        /// </summary>
        internal static Guid CustomHeadersModuleId = Guid.Parse("{46307280-190E-4365-8AA1-085C451E7799}");

        /// <summary>
        /// SPRequestGuid SharePoint REST response header
        /// </summary>
        internal const string SPRequestGuidHeader = "SPRequestGuid";

        /// <summary>
        /// SPClientServiceRequestDuration SharePoint REST response header
        /// </summary>
        internal const string SPClientServiceRequestDurationHeader = "SPClientServiceRequestDuration";

        /// <summary>
        /// X-SharePointHealthScore SharePoint REST response header
        /// </summary>
        internal const string XSharePointHealthScoreHeader = "X-SharePointHealthScore";

        /// <summary>
        /// X-SP-SERVERSTATE SharePoint REST response header
        /// </summary>
        internal const string XSPServerStateHeader = "X-SP-SERVERSTATE";

        /// <summary>
        /// The string absolute path of the Content Type Hub
        /// </summary>
        internal const string ContentTypeHubUrl = "/sites/contenttypehub";
        
        #region Json serializers options

        /// <summary>
        /// Cached JsonSerializerOptions for performance using IgnoreNullValues = true
        /// See https://github.com/dotnet/runtime/issues/38982 and https://www.meziantou.net/avoid-performance-issue-with-jsonserializer-by-reusing-the-same-instance-of-json.htm
        /// </summary>
        internal static JsonSerializerOptions JsonSerializer_IgnoreNullValues = new JsonSerializerOptions
        {
#if NET5_0_OR_GREATER
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
#else
            IgnoreNullValues = true
#endif
        };

        /// <summary>
        /// Cached JsonSerializerOptions for performance using IgnoreNullValues = true and PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        /// </summary>
        internal static JsonSerializerOptions JsonSerializer_IgnoreNullValues_CamelCase = new JsonSerializerOptions()
        {
#if NET5_0_OR_GREATER
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#else
            IgnoreNullValues = true,
#endif
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Cached JsonSerializerOptions for performance using WriteIndented = false
        /// </summary>
        internal static JsonSerializerOptions JsonSerializer_WriteIndentedFalse = new JsonSerializerOptions() { WriteIndented = false };

        /// <summary>
        /// Cached JsonSerializerOptions for performance using WriteIndented = true
        /// </summary>
        internal static JsonSerializerOptions JsonSerializer_WriteIndentedTrue = new JsonSerializerOptions() { WriteIndented = true };

        /// <summary>
        /// Cached JsonSerializerOptions for performance using IgnoreNullValues = true with JsonStringEnumConverter
        /// </summary>
        internal static JsonSerializerOptions JsonSerializer_IgnoreNullValues_StringEnumConvertor = new JsonSerializerOptions()
        {
#if NET5_0_OR_GREATER
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#else
            IgnoreNullValues = true,
#endif
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        /// <summary>
        /// Cached JsonSerializerOptions for performance using WriteIndented = false, CamelCase with JsonStringEnumConverter
        /// </summary>
        internal static JsonSerializerOptions JsonSerializer_WriteIndentedFalse_CamelCase_JsonStringEnumConverter = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = false,
            // For some reason the naming policy is not applied on ExpandoObjects
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Cached JsonSerializerOptions for performance using PropertyNameCaseInsensitive = true
        /// </summary>
        internal static JsonSerializerOptions JsonSerializer_PropertyNameCaseInsensitiveTrue = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Cached JsonSerializerOptions for performance using AllowTrailingCommas = true
        /// </summary>
        internal static JsonSerializerOptions JsonSerializer_AllowTrailingCommasTrue = new JsonSerializerOptions { AllowTrailingCommas = true };

        /// <summary>
        /// Cached JsonSerializerOptions for performance with SPGuidConverter and DateTimeConverter
        /// </summary>
        internal static JsonSerializerOptions JsonSerializer_SPGuidConverter_DateTimeConverter = new JsonSerializerOptions()
        {
            Converters =
            {
                new SPGuidConverter(),
                new DateTimeConverter()
            }
        };

        /// <summary>
        /// Cached JsonSerializerOptions for performance using IgnoreNullValues = true with SharePointRestCollectionJsonConverter and JsonStringEnumConverter
        /// </summary>
        internal static JsonSerializerOptions JsonSerializer_IgnoreNullValues_SharePointRestCollectionJsonConverter_JsonStringEnumConverter = new JsonSerializerOptions()
        {
#if NET5_0_OR_GREATER
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#else
            IgnoreNullValues = true,
#endif
            Converters =
            {
                new SharePointRestCollectionJsonConverter<string>(),
                new JsonStringEnumConverter(allowIntegerValues:false)
            }
        };

#endregion
    }
}
