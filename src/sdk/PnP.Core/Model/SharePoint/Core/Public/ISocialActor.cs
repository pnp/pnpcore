using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// An entity, which represents a following object. The type of the object can be identified based on the <i>ActorType</i> property
    /// </summary>
    [ConcreteType(typeof(SocialActor))]
    public interface ISocialActor
    {
        /// <summary>
        /// Gets the actor's account name. Only valid when ActorType is User
        /// </summary>
        string AccountName { get; set; }

        /// <summary>
        /// Gets the actor type.
        /// </summary>
        SocialActorType ActorType { get; set; }

        /// <summary>
        /// Returns true if the Actor can potentially be followed, false otherwise.
        /// </summary>
        bool CanFollow { get; set; }

        /// <summary>
        /// Gets the actor's content URI. Only valid when ActorType is Document, or Site
        /// </summary>
        string ContentUri { get; set; }

        /// <summary>
        /// Gets the actor's email address. Only valid when ActorType is User
        /// </summary>
        string EmailAddress { get; set; }

        /// <summary>
        /// Gets the URI of the actor's followed content folder. Only valid when this represents the current user
        /// </summary>
        string FollowedContentUri { get; set; }

        /// <summary>
        /// Group id, if the followed site is a group
        /// </summary>
        Guid GroupId { get; set; }

        /// <summary>
        /// Gets the actor's canonical URI.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets the actor's image URI. Only valid when ActorType is User, Document, or Site
        /// </summary>
        string ImageUri { get; set; }

        /// <summary>
        /// Returns true if the current user is following the actor, false otherwise.
        /// </summary>
        bool IsFollowed { get; set; }

        /// <summary>
        /// Gets the actor's library URI. Only valid when ActorType is Document
        /// </summary>
        string LibraryUri { get; set; }

        /// <summary>
        /// Gets the actor's display name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets the URI of the actor's personal site. Only valid when ActorType is User
        /// </summary>
        string PersonalSiteUri { get; set; }

        /// <summary>
        /// Gets a code that indicates recoverable errors that occurred during actor retrieval
        /// </summary>
        SocialStatusCode Status { get; set; }

        /// <summary>
        /// Gets the text of the actor's most recent post. Only valid when ActorType is User
        /// </summary>
        string StatusText { get; set; }

        /// <summary>
        /// Gets the actor's tag GUID. Only valid when ActorType is Tag
        /// </summary>
        Guid TagGuid { get; set; }

        /// <summary>
        /// Gets the actor's title. Only valid when ActorType is User
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Unique item uri
        /// </summary>
        string Uri { get; set; }
    }
}
