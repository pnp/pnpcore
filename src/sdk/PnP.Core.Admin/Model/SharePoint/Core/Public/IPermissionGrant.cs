namespace PnP.Core.Admin.Model.SharePoint
{
    public interface IPermissionGrant
    {
        string ClientId { get; set; }
        string ConsentType { get; set; }
        bool  IsDomainIsolated { get; set; }
        string ObjectId { get; set; }
        string PackageName { get; set; }
        string Resource { get; set; }
        string ResourceId { get; set; }
        string Scope { get; set; }
    }
}
