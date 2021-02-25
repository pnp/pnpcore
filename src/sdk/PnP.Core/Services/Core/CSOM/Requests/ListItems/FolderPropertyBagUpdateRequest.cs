using PnP.Core.Services.Core.CSOM.Requests.Web;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.Requests.ListItems
{
    class FolderPropertyBagUpdateRequest : UpdatePropertyBagRequest
    {
        internal FolderPropertyBagUpdateRequest(string parentId)
        {
            PropertyName = "Properties";
            ObjectId = $":folder:{parentId}";
        }
    }
}
