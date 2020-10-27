using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ListItemVersion object
    /// </summary>
    [ConcreteType(typeof(ListItemVersion))]
    public interface IListItemVersion : IDataModel<IListItemVersion>, IDataModelGet<IListItemVersion>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsCurrentVersion { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int VersionId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string VersionLabel { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser CreatedBy { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFieldCollection Fields { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFileVersion FileVersion { get; }

        #endregion

    }
}
