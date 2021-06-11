using PnP.Core.Model.Security;
using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ListItemVersion object
    /// </summary>
    [ConcreteType(typeof(ListItemVersion))]
    public interface IListItemVersion : IDataModel<IListItemVersion>, IDataModelGet<IListItemVersion>, IDataModelLoad<IListItemVersion>, IExpandoDataModel, IQueryableDataModel
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

        public IFieldCollection Fields { get; }

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
