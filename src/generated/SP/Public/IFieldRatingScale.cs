using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldRatingScale object
    /// </summary>
    [ConcreteType(typeof(FieldRatingScale))]
    public interface IFieldRatingScale : IDataModel<IFieldRatingScale>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int GridEndNumber { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string GridNAOptionText { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int GridStartNumber { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string GridTextRangeAverage { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string GridTextRangeHigh { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string GridTextRangeLow { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int RangeCount { get; set; }

        #endregion

    }
}
