using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Model.Me
{
    /// <summary>
    /// Contains information about the current user
    /// </summary>
    [ConcreteType(typeof(Me))]
    public interface IMe : IDataModel<IMe>, IDataModelGet<IMe>, IDataModelLoad<IMe>, IDataModelUpdate
    {
        /// <summary>
        /// The Unique ID of the User/Group
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Business phone for the current user
        /// </summary>
        public List<string> BusinessPhones { get; }

        /// <summary>
        /// Display name of the current user
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Given name of the current user
        /// </summary>
        public string GivenName { get; }

        /// <summary>
        /// Job title of the current user
        /// </summary>
        public string JobTitle { get; }

        /// <summary>
        /// Email address of the current user
        /// </summary>
        public string Mail { get; }

        /// <summary>
        /// Mobile phone number of the current user
        /// </summary>
        public string MobilePhone { get; }

        /// <summary>
        /// Office location of the current user
        /// </summary>
        public string OfficeLocation { get; }

        /// <summary>
        /// Preferred language used by the current user
        /// </summary>
        public string PreferredLanguage { get; }

        /// <summary>
        /// Surname of the current user
        /// </summary>
        public string SurName { get; }

        /// <summary>
        /// UPN of the current user
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
