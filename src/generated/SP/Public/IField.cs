using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Field object
    /// </summary>
    [ConcreteType(typeof(Field))]
    public interface IField : IDataModel<IField>, IDataModelGet<IField>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool AutoIndexed { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanBeDeleted { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ClientSideComponentId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ClientSideComponentProperties { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ClientValidationFormula { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ClientValidationMessage { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string CustomFormatter { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DefaultFormula { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Direction { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnforceUniqueValues { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string EntityPropertyName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Filterable { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool FromBaseType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Group { get; set; }

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
        public bool Indexed { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int IndexStatus { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string InternalName { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string JSLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool PinnedToFiltersPane { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ReadOnlyField { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SchemaXml { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Sealed { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ShowInFiltersPane { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Sortable { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string StaticName { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int FieldTypeKind { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string TypeAsString { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string TypeDisplayName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string TypeShortDescription { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ValidationFormula { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ValidationMessage { get; set; }

        #endregion

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsModern { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool NoCrawl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SchemaXmlWithResourceTokens { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUserResource DescriptionResource { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUserResource TitleResource { get; }

        #endregion

    }
}
