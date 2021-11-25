using System;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Microsoft 365 Term group
    /// </summary>
    [ConcreteType(typeof(TermGroup))]
    public interface ITermGroup : IDataModel<ITermGroup>, IDataModelGet<ITermGroup>, IDataModelLoad<ITermGroup>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
        /// <summary>
        /// The Unique ID of the Group.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Name of the group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description giving details on the group.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Date and time of group creation. Read-only.
        /// </summary>
        public DateTimeOffset CreatedDateTime { get; }

        /// <summary>
        /// Returns type of group. Possible values are 'global', 'system' and 'siteCollection'.
        /// </summary>
        public TermGroupScope Scope { get; set; }

        /// <summary>
        /// Collection of term sets in this term group
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public ITermSetCollection Sets { get; }
    }
}