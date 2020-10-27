using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ChangeView object
    /// </summary>
    [ConcreteType(typeof(ChangeView))]
    public interface IChangeView : IDataModel<IChangeView>, IDataModelGet<IChangeView>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ViewId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ListId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid WebId { get; set; }

        #endregion

    }
}
