using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ChangeFile object
    /// </summary>
    [ConcreteType(typeof(ChangeFile))]
    public interface IChangeFile : IDataModel<IChangeFile>, IDataModelGet<IChangeFile>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid UniqueId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid WebId { get; set; }

        #endregion

    }
}
