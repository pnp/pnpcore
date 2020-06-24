using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ChangeUser object
    /// </summary>
    [ConcreteType(typeof(ChangeUser))]
    public interface IChangeUser : IDataModel<IChangeUser>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool Activate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int UserId { get; set; }

        #endregion

    }
}
