using System.Text;
using System.Text.Json;

namespace PnP.Core
{
    /// <summary>
    /// Microsoft Graph service error
    /// </summary>
    public class MicrosoftGraphError : ServiceError
    {
        /// <summary>
        /// Default constructor for the <see cref="MicrosoftGraphError"/> error
        /// </summary>
        /// <param name="type"><see cref="ErrorType"/> type of the error</param>
        /// <param name="httpResponseCode">Http response code of the service request that failed</param>
        /// <param name="error"><see cref="JsonElement"/> holding the json data of the service error</param>
        public MicrosoftGraphError(ErrorType type, int httpResponseCode, JsonElement error) : base(type, httpResponseCode)
        {
            ParseError(error);
        }

        /// <summary>
        /// Default constructor for the <see cref="MicrosoftGraphError"/> error
        /// </summary>
        /// <param name="type"><see cref="ErrorType"/> type of the error</param>
        /// <param name="httpResponseCode">Http response code of the service request that failed</param>
        /// <param name="response">String holding the information about the failed request</param>
        public MicrosoftGraphError(ErrorType type, int httpResponseCode, string response) : base(type, httpResponseCode)
        {
            if (!string.IsNullOrEmpty(response))
            {
                var body = JsonSerializer.Deserialize<JsonElement>(response);
                ParseError(body);
            }
        }

        /// <summary>
        /// Outputs a <see cref="MicrosoftGraphError"/> to a string representation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var errorString = new StringBuilder();

            errorString.AppendLine($"HttpResponseCode: {HttpResponseCode}");
            errorString.AppendLine($"Code: {Code}");
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
            var errorData = error.GetProperty("error");

            // enumerate the properties in the error 
            foreach (var errorField in errorData.EnumerateObject())
            {
                if (errorField.Name == "code")
                {
                    Code = errorField.Value.GetString();
                }
                else if (errorField.Name == "message")
                {
                    Message = errorField.Value.GetString();
                }
                else if (errorField.Name == "innerError" && errorField.Value.ValueKind == JsonValueKind.Object)
                {
                    foreach (var innerErrorField in errorField.Value.EnumerateObject())
                    {
                        if (innerErrorField.Name == "request-id")
                        {
                            ClientRequestId = innerErrorField.Value.GetString();
                        }
                        else
                        {
                            AddAdditionalData(innerErrorField.Name, innerErrorField.Value.ToString());
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
