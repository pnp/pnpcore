using PnP.Core.Model.SharePoint;

namespace PnP.Core.Model
{
    /// <summary>
    /// A follow document request metadata object
    /// </summary>
    public class FollowDocumentData : FollowData
    {
        /// <summary>
        /// The unique url for the site.
        /// </summary>
        public string ContentUri { get; set; }

        /// <summary>
        /// Represents an actor type. Can be either a user, site, document or tag
        /// </summary>
        public override SocialActorType ActorType => SocialActorType.Document;
    }
}
