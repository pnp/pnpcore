using PnP.Core.Provisioning.Utilities;
using System;
using System.Linq;

namespace PnP.Core.Provisioning.Model.Teams
{
    /// <summary>
    /// Defines a Channel for a Team
    /// </summary>
    public partial class TeamChannel : BaseModel, IEquatable<TeamChannel>
    {
        #region Public Members

        /// <summary>
        /// Defines a collection of Tabs for a Channel in a Team
        /// </summary>
        public TeamTabCollection Tabs { get; private set; }

        /// <summary>
        /// Defines a collection of Resources for Tabs in a Team Channel
        /// </summary>
        public TeamTabResourceCollection TabResources { get; private set; }

        /// <summary>
        /// Defines a collection of Messages for a Team Channe
        /// </summary>
        public TeamChannelMessageCollection Messages { get; private set; }

        /// <summary>
        /// Defines the Display Name of the Channel
        /// </summary>
        public String DisplayName { get; set; }

        /// <summary>
        /// Defines the Description of the Channel
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// Defines whether the Channel is Favorite by default for all members of the Team
        /// </summary>
        public Boolean? IsFavoriteByDefault { get; set; }

        /// <summary>
        /// Declares the ID for the Channel
        /// </summary>
        public String ID { get; set; }

        /// <summary>
        /// Declares whether the Channel is private or not
        /// </summary>
        public bool Private {
            get { return this.MembershipType == MembershipType.Private; }
            set { this.MembershipType = value ? MembershipType.Private : MembershipType.Standard; } 
        }

        /// <summary>
        /// Declares whether the Channel is Public, Private, or Shared, optional attribute (default public).
        /// </summary>
        public MembershipType MembershipType { get; set; }

        /// <summary>
        /// Declares whether the Channel allows messages from BOTs or not, optional attribute (default false).
        /// </summary>
        public bool AllowNewMessageFromBots { get; set; }

        /// <summary>
        /// Declares whether the Channel allows messages from Connectors or not, optional attribute (default false).
        /// </summary>
        public bool AllowNewMessageFromConnectors { get; set; }

        /// <summary>
        /// Declares the Channel reply restrictions, optional attribute (default everyone).
        /// </summary>
        public ReplyRestriction ReplyRestriction { get; set; }

        /// <summary>
        /// Declares the Channel reply restrictions, optional attribute (default everyone).
        /// </summary>
        public UserNewMessageRestriction UserNewMessageRestriction { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for TeamChannel
        /// </summary>
        public TeamChannel()
        {
            this.Tabs = new TeamTabCollection(this.ParentTemplate);
            this.TabResources = new TeamTabResourceCollection(this.ParentTemplate);
            this.Messages = new TeamChannelMessageCollection(this.ParentTemplate);
        }

        #endregion

        #region Comparison code

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Returns HashCode</returns>
        public override int GetHashCode()
        {
            return (String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|",
                Tabs.Aggregate(0, (acc, next) => acc += (next != null ? next.GetHashCode() : 0)),
                TabResources.Aggregate(0, (acc, next) => acc += (next != null ? next.GetHashCode() : 0)),
                Messages.Aggregate(0, (acc, next) => acc += (next != null ? next.GetHashCode() : 0)),
                DisplayName?.GetHashCode() ?? 0,
                Description?.GetHashCode() ?? 0,
                IsFavoriteByDefault.GetHashCode(),
                ID?.GetHashCode() ?? 0,
                MembershipType.GetHashCode(),
                AllowNewMessageFromBots.GetHashCode(),
                AllowNewMessageFromConnectors.GetHashCode(),
                ReplyRestriction.GetHashCode(),
                UserNewMessageRestriction.GetHashCode()
            ).GetHashCode());
        }

        /// <summary>
        /// Compares object with TeamChannel class
        /// </summary>
        /// <param name="obj">Object that represents TeamChannel</param>
        /// <returns>Checks whether object is TeamChannel class</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is TeamChannel))
            {
                return (false);
            }
            return (Equals((TeamChannel)obj));
        }

        /// <summary>
        /// Compares TeamChannel object based on Tabs, TabResources, Messages, DisplayName, Description, and IsFavoriteByDefault
        /// </summary>
        /// <param name="other">TeamChannel Class object</param>
        /// <returns>true if the TeamChannel object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TeamChannel other)
        {
            if (other == null)
            {
                return (false);
            }

            return (this.Tabs.DeepEquals(other.Tabs) &&
                this.TabResources.DeepEquals(other.TabResources) &&
                this.Messages.DeepEquals(other.Messages) &&
                this.DisplayName == other.DisplayName &&
                this.Description == other.Description &&
                this.IsFavoriteByDefault == other.IsFavoriteByDefault &&
                this.ID == other.ID &&
                this.MembershipType == other.MembershipType &&
                this.AllowNewMessageFromBots == other.AllowNewMessageFromBots &&
                this.AllowNewMessageFromConnectors == other.AllowNewMessageFromConnectors &&
                this.ReplyRestriction == other.ReplyRestriction &&
                this.UserNewMessageRestriction == other.UserNewMessageRestriction
                );
        }

        #endregion
    }

    /// <summary>
    /// Declares whether the Channel is Public, Private, or Shared, optional attribute (default public).
    /// </summary>
    public enum MembershipType
    {
        /// <summary>
        /// The channel is Standard (i.e. Public)
        /// </summary>
        Standard,
        /// <summary>
        /// The channel is Private
        /// </summary>
        Private,
        /// <summary>
        /// The channel is Shared
        /// </summary>
        Shared,
    }

    /// <summary>
    /// Declares the Channel reply restrictions, optional attribute (default everyone).
    /// </summary>
    public enum ReplyRestriction
    {
        /// <summary>
        /// Everyone is allowed to reply to the teams channel.
        /// </summary>
        Everyone,
        /// <summary>
        /// Authors and Moderators are allowed to reply to the teams channel.
        /// </summary>
        AuthorAndModerators
    }

    /// <summary>
    /// Declares the Channel reply restrictions, optional attribute (default everyone).
    /// </summary>
    public enum UserNewMessageRestriction
    {
        /// <summary>
        /// Everyone is allowed to post messages to teams channel.
        /// </summary>
        Everyone,
        /// <summary>
        /// Everyone except Guests is allowed to post messages to teams channel.
        /// </summary>
        EveryoneExceptGuests,
        /// <summary>
        /// Moderators are allowed to post messages to teams channel.
        /// </summary>
        Moderators
    }
}
