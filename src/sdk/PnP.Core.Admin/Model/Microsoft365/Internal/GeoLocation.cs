namespace PnP.Core.Admin.Model.Microsoft365
{
    internal class GeoLocation : IGeoLocation
    {
        public string DataLocationCode { get; set; }

        public string SharePointPortalUrl { get; set; }
        
        public string SharePointAdminUrl { get; set; }
        
        public string SharePointMySiteUrl { get; set; }
    }
}
