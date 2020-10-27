using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FileVersion object
    /// </summary>
    [ConcreteType(typeof(FileVersion))]
    public interface IFileVersion : IDataModel<IFileVersion>, IDataModelGet<IFileVersion>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public string CheckInComment { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime Created { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsCurrentVersion { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string VersionLabel { get; }

        #endregion

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser CreatedBy { get; }

        #endregion

    }
}
