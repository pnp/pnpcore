using PnP.Core.Model;

namespace PnP.Core.Services
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
            XmlBody = null;
            ReceivingProperty = receivingProperty;
            RawRequest = false;
            Commit = false;
            Interactive = false;
            BinaryBody = null;
        }

        internal ApiCall(string xmlBody, string receivingProperty = null)
        {
            Request = null;
            Type = ApiType.CSOM;
            JsonBody = null;
            XmlBody = xmlBody;
            ReceivingProperty = receivingProperty;
            RawRequest = false;
            Commit = false;
            Interactive = false;
            BinaryBody = null;
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
        /// Defines the XML body of the request, if any
        /// </summary>
        internal string XmlBody { get; set; }

        /// <summary>
        /// Typically the JSON response will be mapped to the current model object, but sometimes a call 
        /// will need to be mapped to a specific property in the current model. This for example is 
        /// done when a property requires it's own Graph Get request.
        /// 
        /// Current this property is set automatically, no need to manually fiddle with it in API call overrides!
        /// </summary>
        internal string ReceivingProperty { get; set; }

        /// <summary>
        /// Is this a raw request that does not require automatic parsing of the returned json?
        /// </summary>
        internal bool RawRequest { get; set; }

        /// <summary>
        /// When set to true the current model item will be committed as changes are synced with the server
        /// </summary>
        internal bool Commit { get; set; }

        /// <summary>
        /// When set to true this request will be executed interactively, so without being wrapped in a batch request
        /// </summary>
        internal bool Interactive { get; set; }

        /// <summary>
        /// Binary content for this API call
        /// </summary>
        internal byte[] BinaryBody { get; set; }
    }
}
