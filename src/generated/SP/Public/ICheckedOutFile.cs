using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a CheckedOutFile object
    /// </summary>
    [ConcreteType(typeof(CheckedOutFile))]
    public interface ICheckedOutFile : IDataModel<ICheckedOutFile>, IDataModelGet<ICheckedOutFile>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int CheckedOutById { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser CheckedOutBy { get; }

        #endregion

    }
}
