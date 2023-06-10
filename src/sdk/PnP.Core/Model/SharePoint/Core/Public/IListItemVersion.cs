using PnP.Core.Model.Security;
using System;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ListItemVersion object
    /// </summary>
    [ConcreteType(typeof(ListItemVersion))]
    public interface IListItemVersion : IDataModel<IListItemVersion>, IDataModelGet<IListItemVersion>, IDataModelLoad<IListItemVersion>, IDataModelDelete, IExpandoDataModel, IQueryableDataModel
    {
        /// <summary>
        /// Gets a value that specifies the creation date and time for the list item version.
        /// </summary>
        public DateTime Created { get; }

        /// <summary>
        /// Gets the internal identifier for the list item version.
        /// SP REST property name: VersionId.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets a value that specifies whether the list item version is the current version.
        /// </summary>
        public bool IsCurrentVersion { get; }

        /// <summary>
        /// Gets a value that specifies the implementation specific identifier of the file.
        /// </summary>
        public string VersionLabel { get; }

        /// <summary>
        /// The <see cref="ISharePointUser"/> that created this version.
        /// </summary>
        public ISharePointUser CreatedBy { get; }

        /// <summary>
        /// Fields of this list item version.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IFieldCollection Fields { get; }

        /// <summary>
        /// <see cref="IFileVersion"/> of this list item version
        /// </summary>
        public IFileVersion FileVersion { get; }

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }

        // TODO:
        //#region DeleteObject
        //Task DeleteObjectAsync();

        //void DeleteObject(); 
        //#endregion
    }
}
