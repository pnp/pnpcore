using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ChangeField object
    /// </summary>
    [ConcreteType(typeof(ChangeField))]
    public interface IChangeField : IDataModel<IChangeField>, IDataModelGet<IChangeField>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid FieldId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid WebId { get; set; }

        #endregion

    }
}
