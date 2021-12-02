namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents metadata information about your following data
    /// </summary>
    [ConcreteType(typeof(FollowingInfo))]
    public interface IFollowingInfo
    {
        /// <summary>
        /// The Uri to see all your followed documents
        /// </summary>
        string MyFollowedDocumentsUri { get; set; }

        /// <summary>
        /// The Uri to see all your followed sites
        /// </summary>
        string MyFollowedSitesUri { get; set; }

        /// <summary>
        /// A metadata for a following entity
        /// </summary>
        ISocialActor SocialActor { get; set; }
    }
}
