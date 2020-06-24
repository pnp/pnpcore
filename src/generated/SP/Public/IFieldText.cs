using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldText object
    /// </summary>
    [ConcreteType(typeof(FieldText))]
    public interface IFieldText : IDataModel<IFieldText>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int MaxLength { get; set; }

        #endregion

    }
}
