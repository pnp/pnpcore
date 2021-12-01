using PnP.Core.Services.Core.CSOM.Requests.Web;

namespace PnP.Core.Services.Core.CSOM.Requests.ListItems
{
    internal sealed class FolderPropertyBagUpdateRequest : UpdatePropertyBagRequest
    {
        internal FolderPropertyBagUpdateRequest(string parentId)
        {
            PropertyName = "Properties";
            ObjectId = $":folder:{parentId}";
        }
    }
}
