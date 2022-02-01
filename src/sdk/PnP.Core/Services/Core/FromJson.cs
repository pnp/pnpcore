using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace PnP.Core.Services
{
    /// <summary>
    /// Class that represents the JSON context that's being used during call outs when parsing JSON responses
    /// </summary>
    public class FromJson
    {
        internal FromJson(string fieldName, JsonElement jsonElement, Type targetType, ILogger logger, ApiResponse apiResponse)
        {
            FieldName = fieldName;
            JsonElement = jsonElement;
            TargetType = targetType;
            Log = logger;
            ApiResponse = apiResponse;
        }

        internal string FieldName { get; private set; }

        internal JsonElement JsonElement { get; private set; }

        internal Type TargetType { get; private set; }

        internal ILogger Log { get; private set; }

        internal ApiResponse ApiResponse { get; private set; }
    }
}
