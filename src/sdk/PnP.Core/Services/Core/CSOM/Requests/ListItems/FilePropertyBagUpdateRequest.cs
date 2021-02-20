using PnP.Core.Services.Core.CSOM.Requests.Web;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.Requests.ListItems
{
    class FilePropertyBagUpdateRequest : UpdatePropertyBagRequest
    {
        internal FilePropertyBagUpdateRequest(string fileId)
        {
            PropertyName = "Properties";
            ObjectId = $":file:{fileId}";
        }
    }
}
