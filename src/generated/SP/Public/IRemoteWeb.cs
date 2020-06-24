using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a RemoteWeb object
    /// </summary>
    [ConcreteType(typeof(RemoteWeb))]
    public interface IRemoteWeb : IDataModel<IRemoteWeb>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanSendEmail { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShareByEmailEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShareByLinkEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IWeb Web { get; }

        #endregion

    }
}
