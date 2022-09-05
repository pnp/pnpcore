using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines if and who liked a list item.
    /// </summary>
    [ConcreteType(typeof(LikedByInformation))]
    public interface ILikedByInformation : IDataModel<ILikedByInformation>, IDataModelGet<ILikedByInformation>, IDataModelLoad<ILikedByInformation>
    {
        /// <summary>
        /// Is this list item liked?
        /// </summary>
        public bool IsLikedByUser { get; }

        /// <summary>
        /// Number of likes this list item got.
        /// </summary>
        public string LikeCount { get; }

        /// <summary>
        /// The people that liked this list item.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public ICommentLikeUserEntityCollection LikedBy { get; }
    }
}
