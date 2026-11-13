namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a SharePoint resource path in the Brand Center configuration.
    /// </summary>
    public interface IBrandcenterSPResourcePath
    {
        /// <summary>
        /// Gets the decoded URL of the SharePoint resource.
        /// </summary>
        string DecodedUrl { get; }
    }
}
