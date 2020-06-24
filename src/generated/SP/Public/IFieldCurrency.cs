using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldCurrency object
    /// </summary>
    [ConcreteType(typeof(FieldCurrency))]
    public interface IFieldCurrency : IDataModel<IFieldCurrency>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int CurrencyLocaleId { get; set; }

        #endregion

    }
}
