using System;

namespace PnP.Core.Model.Security
{

    /// <summary>
    /// Properties that can be set when creating a new Organizational Link
    /// </summary>
    [ConcreteType(typeof(OrganizationalLinkOptions))]
    public interface IOrganizationalLinkOptions
    {
        /// <summary>
        /// The type of sharing link to create.
        /// </summary>
        public ShareType Type { get; set; }
    }
}
