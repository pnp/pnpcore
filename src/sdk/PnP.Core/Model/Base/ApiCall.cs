namespace PnP.Core.Model
{
    /// <summary>
    /// Struct that defines the API call to make
    /// </summary>
    internal struct ApiCall
    {
        internal ApiCall(string request, ApiType apiType, string jsonBody = null, string receivingProperty = null)
        {
            Type = apiType;
            Request = request;
            JsonBody = jsonBody;
            ReceivingProperty = receivingProperty;
        }

        /// <summary>
        /// Defines the type of API to call: SPO Rest or Microsoft Graph
        /// </summary>
        internal ApiType Type { get; set; }

        /// <summary>
        /// Defines the URL of the request
        /// </summary>
        internal string Request { get; set; }

        /// <summary>
        /// Defines the JSON body of the request, if any
        /// </summary>
        internal string JsonBody { get; set; }

        /// <summary>
        /// Typically the JSON response will be mapped to the current model object, but sometimes a call 
        /// will need to be mapped to a specific property in the current model. This for example is 
        /// done when a property requires it's own Graph Get request.
        /// 
        /// Current this property is set automatically, no need to manually fiddle with it in API call overrides!
        /// </summary>
        internal string ReceivingProperty { get; set; }
    }
}
