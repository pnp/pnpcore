using PnP.Core.Model.SharePoint;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
        /// Timestamp at which the channel was created
        /// </summary>
        public DateTime CreatedDateTime { get; }

        /// <summary>
        /// Defines the Membership type for the Team Channel
        /// </summary>
        public TeamChannelMembershipType MembershipType { get; set; }

        /// <summary>
        /// The Web URL of the Team Channel
        /// </summary>
        public Uri WebUrl { get; }

        /// <summary>
        /// The fully qualified url for the SharePoint folder hosting this channel's files (uses Graph Beta)
        /// </summary>
        public Uri FilesFolderWebUrl { get; }

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

        /// <summary>
        /// Gets the <see cref="IFolder"/> hosting the files of this channel
        /// </summary>
        /// <param name="expressions">Properties of the folder to load</param>
        /// <returns>Folder hosting the files of this channel</returns>
        public Task<IFolder> GetFilesFolderAsync(params Expression<Func<IFolder, object>>[] expressions);

        /// <summary>
        /// Gets the <see cref="IFolder"/> hosting the files of this channel
        /// </summary>
        /// <param name="expressions">Properties of the folder to load</param>
        /// <returns>Folder hosting the files of this channel</returns>
        public IFolder GetFilesFolder(params Expression<Func<IFolder, object>>[] expressions);
    }
}
