using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldComputed object
    /// </summary>
    [ConcreteType(typeof(FieldComputed))]
    public interface IFieldComputed : IDataModel<IFieldComputed>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableLookup { get; set; }

        #endregion

    }
}
