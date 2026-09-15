using PnP.Core.Model.Copilot.Public;
using PnP.Core.Model.Copilot.Public.DTO;
using PnP.Core.Model.Me;
using PnP.Core.Model.Security;
using PnP.Core.Services;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.Copilot.Internal
{
    /// <summary>
    /// Provides methods for interacting with Microsoft Graph services to perform retrieval operations using a specified
    /// Microsoft Graph client.
    /// </summary>
    internal sealed class Copilot : BaseDataModel<ICopilot>, ICopilot
    {
        /// <summary>
        /// Sends a retrieval request to MS Graph API and returns the deserialized response.
        /// </summary>
        /// <param name="request">The retrieval request containing the parameters to be sent to the service. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized retrieval
        /// response from the MS Graph API.</returns>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails or the response indicates an unsuccessful status code.</exception>
        public async Task<RetrievalResponse> RetrieveAsync(RetrievalRequest request)
        {
            var apiCall = new ApiCall($"copilot/retrieval", ApiType.Graph, JsonSerializer.Serialize(request, PnPConstants.JsonSerializer_IgnoreNullValues));
            apiCall.ExecuteRequestApiCall = true;

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false); 

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<RetrievalResponse>(response.Json);
            }
            else
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }
        }
    }
}
