using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ChangeContentType object
    /// </summary>
    [ConcreteType(typeof(ChangeContentType))]
    public interface IChangeContentType : IDataModel<IChangeContentType>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid WebId { get; set; }

        #endregion

    }
}
