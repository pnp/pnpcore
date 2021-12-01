using System;
using System.Linq;

namespace PnP.Core.Model.Teams
{
    /// <summary>
    /// Defines a Channel of Microsoft Teams
    /// </summary>
    [ConcreteType(typeof(TeamChannel))]
    public interface ITeamChannel : IDataModel<ITeamChannel>, IDataModelGet<ITeamChannel>, IDataModelLoad<ITeamChannel>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
        /// <summary>
        /// The Unique ID of the Team Channel
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The display name of the Team Channel
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The description of the Team Channel
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Defines whether the Team Channel is favorite by default in the Team
        /// </summary>
        public bool IsFavoriteByDefault { get; set; }

        /// <summary>
        /// The email address of the Team Channel
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Defines the Membership type for the Team Channel
        /// </summary>
        public TeamChannelMembershipType MembershipType { get; set; }

        /// <summary>
        /// The Web URL of the Team Channel
        /// </summary>
        public Uri WebUrl { get; }

        /// <summary>
        /// Tabs in this Channel
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public ITeamChannelTabCollection Tabs { get; }

        /// <summary>
        /// Messages in this Team Channel
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public ITeamChatMessageCollection Messages { get; }
    }
}
