using System;
using System.Linq;

namespace PnP.Core.Model.Me
{
    /// <summary>
    /// 
    /// </summary>
    [ConcreteType(typeof(Me))]
    public interface IMe : IDataModel<IMe>, IDataModelGet<IMe>, IDataModelLoad<IMe>, IDataModelUpdate
    {
        /// <summary>
        /// The Unique ID of the User/Group
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Collection of Chats of the user
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IChatCollection Chats { get; }
    }
}
