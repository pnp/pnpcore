using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldNumber object
    /// </summary>
    [ConcreteType(typeof(FieldNumber))]
    public interface IFieldNumber : IDataModel<IFieldNumber>, IDataModelGet<IFieldNumber>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool CommaSeparator { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string CustomUnitName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CustomUnitOnRight { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int DisplayFormat { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShowAsPercentage { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Unit { get; set; }

        #endregion

    }
}
