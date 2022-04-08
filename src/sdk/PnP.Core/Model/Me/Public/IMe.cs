using System;
using System.Collections.Generic;
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
        /// 
        /// </summary>
        public List<string> BusinessPhones { get; }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 
        /// </summary>
        public string GivenName { get; }

        /// <summary>
        /// 
        /// </summary>
        public string JobTitle { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Mail { get; }

        /// <summary>
        /// 
        /// </summary>
        public string MobilePhone { get; }

        /// <summary>
        /// 
        /// </summary>
        public string OfficeLocation { get; }

        /// <summary>
        /// 
        /// </summary>
        public string PreferredLanguage { get; }

        /// <summary>
        /// 
        /// </summary>
        public string SurName { get; }

        /// <summary>
        /// 
        /// </summary>
        public string UserPrincipalName { get; }

        /// <summary>
        /// Collection of Chats of the user
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IChatCollection Chats { get; }

        
    }
}
