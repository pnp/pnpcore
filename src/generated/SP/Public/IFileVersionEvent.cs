using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FileVersionEvent object
    /// </summary>
    [ConcreteType(typeof(FileVersionEvent))]
    public interface IFileVersionEvent : IDataModel<IFileVersionEvent>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Editor { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string EditorEmail { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime Time { get; set; }

        #endregion

    }
}
