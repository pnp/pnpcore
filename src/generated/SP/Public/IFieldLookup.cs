using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldLookup object
    /// </summary>
    [ConcreteType(typeof(FieldLookup))]
    public interface IFieldLookup : IDataModel<IFieldLookup>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowMultipleValues { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsDependentLookup { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsRelationship { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LookupField { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LookupList { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid LookupWebId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PrimaryFieldId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int RelationshipDeleteBehavior { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool UnlimitedLengthInDocumentLibrary { get; set; }

        #endregion

    }
}
