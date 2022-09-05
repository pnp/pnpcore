using PnP.Core.Model.SharePoint;
using System;

namespace PnP.Core.Model
{
    /// <summary>
    /// A follow tag request metadata object
    /// </summary>
    public class FollowTagData : FollowData
    {
        /// <summary>
        /// The unique Id for the tag.
        /// </summary>
        public Guid TagGuid { get; set; }

        /// <summary>
        /// Represents an actor type. Can be either a user, site, document or tag
        /// </summary>
        public override SocialActorType ActorType => SocialActorType.Tag;
    }
}
