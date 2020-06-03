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
        internal FromJson(string fieldName, JsonElement jsonElement, Type targetType, ILogger logger)
        {
            FieldName = fieldName;
            JsonElement = jsonElement;
            TargetType = targetType;
            Log = logger;
        }

        internal string FieldName { get; private set; }

        internal JsonElement JsonElement { get; private set; }

        internal Type TargetType { get; private set; }

        internal ILogger Log { get; private set; }
    }
}
