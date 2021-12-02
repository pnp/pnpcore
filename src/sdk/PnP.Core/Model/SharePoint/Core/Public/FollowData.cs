using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// A metadata object following requests
    /// </summary>
    public abstract class FollowData
    {
        /// <summary>
        /// Represents an actor type. Can be either a user, site, document or tag
        /// </summary>
        public abstract SocialActorType ActorType { get; }
    }
}
