using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.Requests.ListItems
{
    class SystemUpdateListItemRequest : UpdateListItemRequest
    {
        internal SystemUpdateListItemRequest(string siteId, string webId, string listId, int itemId) : base(siteId, webId, listId, itemId)
        {
            UpdateMethodName = "SystemUpdate";
        }
    }
}
