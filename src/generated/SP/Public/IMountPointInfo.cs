using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a MountPointInfo object
    /// </summary>
    [ConcreteType(typeof(MountPointInfo))]
    public interface IMountPointInfo : IDataModel<IMountPointInfo>, IDataModelGet<IMountPointInfo>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string RedirectUrl { get; set; }

        #endregion

    }
}
