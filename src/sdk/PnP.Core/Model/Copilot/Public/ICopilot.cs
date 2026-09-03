
using PnP.Core.Model.Copilot.Public.DTO;
using System.Threading.Tasks;

namespace PnP.Core.Model.Copilot.Public
{
    /// <summary>
    /// Represents MS Graph API endpoint under Copilot path.
    /// </summary>
    [ConcreteType(typeof(Internal.Copilot))]
    public interface ICopilot
    {
        /// <summary>
        /// The Microsoft 365 Copilot Retrieval API allows for the retrieval of relevant text extracts from SharePoint, OneDrive, and Copilot connectors content that the calling user has access to, while respecting the defined access controls within the tenant. Use the Retrieval API to ground your generative AI solutions with Microsoft 365 data while optimizing for context recall.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<RetrievalResponse> RetrieveAsync(RetrievalRequest request);
    }
}
