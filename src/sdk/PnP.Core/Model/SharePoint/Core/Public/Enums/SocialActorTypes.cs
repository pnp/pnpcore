using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents an actor type, which defines types of following content to return
    /// </summary>
    [Flags]
    public enum SocialActorTypes
    {
        /// <summary>
        /// Defines not actor types
        /// </summary>
        None = 0,

        /// <summary>
        /// Users actor types
        /// </summary>
        Users = 1 << SocialActorType.User,

        /// <summary>
        /// Documents actor types
        /// </summary>
        Documents = 1 << SocialActorType.Document,

        /// <summary>
        /// Sites actor types
        /// </summary>
        Sites = 1 << SocialActorType.Site,

        /// <summary>
        /// Tags actor types
        /// </summary>
        Tags = 1 << SocialActorType.Tag,

        /// <summary>
        /// The set excludes documents and sites that do not have feeds.
        /// </summary>
        ExcludeContentWithoutFeeds = 268435456,

        /// <summary>
        /// The set includes group sites
        /// </summary>
        IncludeGroupsSites = 536870912,

        /// <summary>
        /// The set includes only items created within the last 24 hours
        /// </summary>
        WithinLast24Hours = 1073741824,

        /// <summary>
        /// Includes all actor types
        /// </summary>
        All = 15
    }
}
