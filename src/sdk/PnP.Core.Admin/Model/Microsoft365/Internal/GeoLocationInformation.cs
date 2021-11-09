namespace PnP.Core.Admin.Model.Microsoft365
{
    internal sealed class GeoLocationInformation : IGeoLocationInformation
    {
        public GeoLocation DataLocationCode { get; set; }

        public string SharePointPortalUrl { get; set; }

        public string SharePointAdminUrl { get; set; }

        public string SharePointMySiteUrl { get; set; }
    }
}
