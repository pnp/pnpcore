using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PnP.Core
{
    /// <summary>
    /// Error class for authentication errors
    /// </summary>
    public class AuthenticationError : BaseError
    {
        /// <summary>
        /// Constructs an <see cref="AuthenticationError"/> instance
        /// </summary>
        /// <param name="type">Type of the error</param>
        /// <param name="error">Error content as <see cref="JsonElement"/></param>
        public AuthenticationError(ErrorType type, JsonElement error) : base(type)
        {
            ParseError(error);
        }

        /// <summary>
        /// Constructs an <see cref="AuthenticationError"/> instance
        /// </summary>
        /// <param name="type">Type of the error</param>
        /// <param name="error">Error content as <see cref="string"/></param>
        public AuthenticationError(ErrorType type, string error) : base(type)
        {
            if (!string.IsNullOrEmpty(error))
            {
                var body = JsonSerializer.Deserialize<JsonElement>(error.Replace("\r\n", " ").Trim());
                ParseError(body);
            }
        }

        /// <summary>
        /// Error code
        /// </summary>
        public string Code { get; internal set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// List of returned error codes
        /// </summary>
        public List<long> ErrorCodes { get; internal set; } = new List<long>();

        /// <summary>
        /// Time the error happened
        /// </summary>
        public string TimeStamp { get; internal set; }

        /// <summary>
        /// Trace id
        /// </summary>
        public Guid TraceId { get; internal set; }

        /// <summary>
        /// Correlation id
        /// </summary>
        public Guid CorrelationId { get; internal set; }

        /// <summary>
        /// Outputs a <see cref="AuthenticationError"/> to a string representation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var errorString = new StringBuilder();

            errorString.AppendLine($"Error: {Code}");
            errorString.AppendLine($"Message: {Message}");
            errorString.AppendLine($"TimeStamp: {TimeStamp}");
            errorString.AppendLine($"TraceId: {TraceId}");
            errorString.AppendLine($"CorrelationId: {CorrelationId}");

            foreach (var property in AdditionalData)
            {
                errorString.AppendLine($"{property.Key}: {property.Value}");
            }

            return errorString.ToString();
        }

        private void ParseError(JsonElement error)
        {
            // enumerate the properties in the error 
            foreach (var errorField in error.EnumerateObject())
            {
                if (errorField.Name == "error")
                {
                    Code = errorField.Value.GetString();
                }
                else if (errorField.Name == "error_description")
                {
                    Message = errorField.Value.GetString();
                }
                else if (errorField.Name == "timestamp")
                {
                    TimeStamp = errorField.Value.GetString();
                }
                else if (errorField.Name == "trace_id")
                {
                    if (errorField.Value.TryGetGuid(out Guid traceId))
                    {
                        TraceId = traceId;
                    }
                }
                else if (errorField.Name == "correlation_id")
                {
                    if (errorField.Value.TryGetGuid(out Guid correlationId))
                    {
                        CorrelationId = correlationId;
                    }
                }
                else if (errorField.Name == "error_codes")
                {
                    foreach (var errorCode in errorField.Value.EnumerateArray())
                    {
                        if (errorCode.TryGetInt64(out long errorCodeValue))
                        {
                            ErrorCodes.Add(errorCodeValue);
                        }
                    }
                }
                else
                {
                    AddAdditionalData(errorField.Name, errorField.Value);
                }
            }
        }

    }
}
