namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Class holding a collection of client side webparts (retrieved via the _api/web/GetClientSideWebParts REST call)
    /// </summary>
    internal class AvailableClientSideComponents
    {
        internal ClientSideComponent[] value { get; set; }
    }
}
