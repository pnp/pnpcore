using PnP.Core.Model.SharePoint;

namespace PnP.Core.Model
{
    /// <summary>
    /// A follow person request metadata object
    /// </summary>
    public class FollowPersonData : FollowData
    {
        /// <summary>
        /// The account name in a form of "i:0#.f|membership|admin@m365x790252.onmicrosoft.com".
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Represents an actor type. Can be either a user, site, document or tag
        /// </summary>
        public override SocialActorType ActorType => SocialActorType.User;
    }
}
