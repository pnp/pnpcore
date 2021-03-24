using PnP.Core.Services.Core.CSOM.Requests.Web;

namespace PnP.Core.Services.Core.CSOM.Requests.ListItems
{
    internal class FilePropertyBagUpdateRequest : UpdatePropertyBagRequest
    {
        internal FilePropertyBagUpdateRequest(string fileId)
        {
            PropertyName = "Properties";
            ObjectId = $":file:{fileId}";
        }
    }
}
