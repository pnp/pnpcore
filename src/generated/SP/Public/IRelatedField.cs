using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a RelatedField object
    /// </summary>
    [ConcreteType(typeof(RelatedField))]
    public interface IRelatedField : IDataModel<IRelatedField>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public Guid FieldId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ListId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int RelationshipDeleteBehavior { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid WebId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IList LookupList { get; }

        #endregion

    }
}
