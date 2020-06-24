using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FileVersion object
    /// </summary>
    [ConcreteType(typeof(FileVersion))]
    public interface IFileVersion : IDataModel<IFileVersion>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string CheckInComment { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsCurrentVersion { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string VersionLabel { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser CreatedBy { get; }

        #endregion

    }
}
