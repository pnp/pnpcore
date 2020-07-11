using System.Text;
using System.Text.Json;

namespace PnP.Core
{
    /// <summary>
    /// Csom service error
    /// </summary>
    public class CsomError : ServiceError
    {
        /// <summary>
        /// Default constructor for the <see cref="SharePointRestError"/> error
        /// </summary>
        /// <param name="type"><see cref="ErrorType"/> type of the error</param>
        /// <param name="httpResponseCode">Http response code of the service request that failed</param>
        /// <param name="errorJson">Json containing the error information</param>
        public CsomError(ErrorType type, int httpResponseCode, JsonElement errorJson) : base(type, httpResponseCode)
        {
            ParseError(errorJson);
        }

        /// <summary>
        /// SharePoint server error code
        /// </summary>
        public long ServerErrorCode { get; private set; }

        /// <summary>
        /// Outputs a <see cref="CsomError"/> to a string representation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var errorString = new StringBuilder();

            errorString.AppendLine($"HttpResponseCode: {HttpResponseCode}");
            errorString.AppendLine($"Message: {Message}");
            errorString.AppendLine($"ClientRequestId: {ClientRequestId}");

            foreach (var property in AdditionalData)
            {
                errorString.AppendLine($"{property.Key}: {property.Value}");
            }

            return errorString.ToString();
        }

        private void ParseError(JsonElement error)
        {
            ClientRequestId = error.GetProperty("TraceCorrelationId").GetGuid().ToString();
         
            var errorInfo = error.GetProperty("ErrorInfo");
            if (errorInfo.ValueKind != JsonValueKind.Null)
            {
                if (errorInfo.TryGetProperty("ErrorMessage", out JsonElement errorMessage))
                {
                    if (errorMessage.ValueKind == JsonValueKind.String)
                    {
                        Message = errorMessage.GetString();
                    }
                }
                if (errorInfo.TryGetProperty("ErrorCode", out JsonElement errorCode))
                {
                    if (errorCode.ValueKind == JsonValueKind.Number)
                    {
                        ServerErrorCode = errorCode.GetInt64();
                    }
                }
                if (errorInfo.TryGetProperty("ErrorValue", out JsonElement errorValue))
                {
                    if (errorValue.ValueKind != JsonValueKind.Null)
                    {
                        AddAdditionalData("ErrorValue", errorValue.GetRawText());
                    }
                }
                if (errorInfo.TryGetProperty("ErrorTypeName", out JsonElement errorTypeName))
                {
                    if (errorTypeName.ValueKind != JsonValueKind.Null)
                    {
                        AddAdditionalData("ErrorTypeName", errorTypeName.GetRawText());
                    }
                }
            }
        }
    }
}
