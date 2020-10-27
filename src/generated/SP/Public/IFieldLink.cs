using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a FieldLink object
    /// </summary>
    [ConcreteType(typeof(FieldLink))]
    public interface IFieldLink : IDataModel<IFieldLink>, IDataModelGet<IFieldLink>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string FieldInternalName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShowInDisplayForm { get; set; }

        #endregion

        #region New properties

        #endregion

    }
}
