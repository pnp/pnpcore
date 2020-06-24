using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Audit object
    /// </summary>
    [ConcreteType(typeof(Audit))]
    public interface IAudit : IDataModel<IAudit>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int AuditFlags { get; set; }

        #endregion

    }
}
