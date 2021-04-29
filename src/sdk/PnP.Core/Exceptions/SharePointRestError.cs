using System;
using System.Text;
using System.Text.Json;

namespace PnP.Core
{
    /// <summary>
    /// SharePoint Rest service error
    /// </summary>
    public class SharePointRestError : ServiceError
    {
        /// <summary>
        /// Default constructor for the <see cref="SharePointRestError"/> error
        /// </summary>
        /// <param name="type"><see cref="ErrorType"/> type of the error</param>
        /// <param name="httpResponseCode">Http response code of the service request that failed</param>
        /// <param name="response">Service request response content</param>
        public SharePointRestError(ErrorType type, int httpResponseCode, string response) : base(type, httpResponseCode)
        {
            if (!string.IsNullOrEmpty(response))
            {
                if (response.StartsWith("{"))
                {
                    var body = JsonSerializer.Deserialize<JsonElement>(response);
                    ParseError(body);
                }
                else
                {
                    Message = response;
                }
            }
        }

        /// <summary>
        /// SharePoint server error code
        /// </summary>
        public long ServerErrorCode { get; private set; }

        /// <summary>
        /// Outputs a <see cref="SharePointRestError"/> to a string representation
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var errorString = new StringBuilder();

            errorString.AppendLine($"HttpResponseCode: {HttpResponseCode}");
            errorString.AppendLine($"Code: {Code}");
            errorString.AppendLine($"Message: {Message}");
            errorString.AppendLine($"ClientRequestId: {ClientRequestId}");

            if (AdditionalData != null)
            {
                foreach (var property in AdditionalData)
                {
                    errorString.AppendLine($"{property.Key}: {property.Value}");
                }
            }

            return errorString.ToString();
        }

        private void ParseError(JsonElement error)
        {
            if (error.TryGetProperty("error", out JsonElement errorData))
            {
                // enumerate the properties in the error 
                foreach (var errorField in errorData.EnumerateObject())
                {
                    if (errorField.Name == "code")
                    {
                        var errorString = errorField.Value.GetString();
                        if (errorString.Contains(","))
                        {
                            var splitErrorMessage = errorString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            Code = splitErrorMessage[1].Trim();
                            if (long.TryParse(splitErrorMessage[0], out long errorCode))
                            {
                                ServerErrorCode = errorCode;
                            }
                        }
                        else
                        {
                            Code = errorString;
                        }
                    }
                    else if (errorField.Name == "message")
                    {
                        Message = errorField.Value.GetProperty("value").ToString();
                    }
                }
            }
            else
            {
                // If for some reason to received error payload is not what we expect just return the whole payload as error message
                Message = error.ToString();
            }
        }


    }
}
