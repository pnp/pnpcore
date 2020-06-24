using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldCalculated object
    /// </summary>
    [ConcreteType(typeof(FieldCalculated))]
    public interface IFieldCalculated : IDataModel<IFieldCalculated>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int CurrencyLocaleId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int DateFormat { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int DisplayFormat { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Formula { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int OutputType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShowAsPercentage { get; set; }

        #endregion

    }
}
