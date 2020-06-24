using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldMultiLineText object
    /// </summary>
    [ConcreteType(typeof(FieldMultiLineText))]
    public interface IFieldMultiLineText : IDataModel<IFieldMultiLineText>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowHyperlink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AppendOnly { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int NumberOfLines { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool RestrictedMode { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool RichText { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool UnlimitedLengthInDocumentLibrary { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool WikiLinking { get; set; }

        #endregion

    }
}
