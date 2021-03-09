namespace PnP.Core.Services.Core.CSOM.Requests.ListItems
{
    class UpdateOverwriteVersionRequest : UpdateListItemRequest
    {
        internal UpdateOverwriteVersionRequest(string siteId, string webId, string listId, int itemId) : base(siteId, webId, listId, itemId)
        {
            UpdateMethodName = "UpdateOverwriteVersion";
        }
    }
}
