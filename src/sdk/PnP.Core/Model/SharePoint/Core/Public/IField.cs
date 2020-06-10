using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Field object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(Field))]
    public interface IField : IDataModel<IField>, IDataModelUpdate, IDataModelDelete
    {
        /// <summary>
        /// Gets or Sets whether the field is auto indexed
        /// </summary>
        public bool AutoIndexed { get; set; }

        /// <summary>
        /// Gets or Sets whether the field can be deleted
        /// </summary>
        public bool CanBeDeleted { get; set; }

        /// <summary>
        /// Gets or Sets the client side component Id associated with the field
        /// </summary>
        public Guid ClientSideComponentId { get; set; }

        /// <summary>
        /// Gets or Sets the properties of the client side component associated with the field
        /// </summary>
        public string ClientSideComponentProperties { get; set; }

        /// <summary>
        /// Gets or Sets the client validation formula of the field
        /// </summary>
        public string ClientValidationFormula { get; set; }

        /// <summary>
        /// Gets or Sets the client validation message of the field
        /// </summary>
        public string ClientValidationMessage { get; set; }

        /// <summary>
        /// Gets or Sets custom formatter of the field
        /// </summary>
        public string CustomFormatter { get; set; }

        /// <summary>
        /// Gets or Sets the default formula of the field
        /// </summary>
        public string DefaultFormula { get; set; }

        /// <summary>
        /// Gets or Sets the default value of the field
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Gets or Sets the description of the field
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets the direction of the field
        /// TODO use enum for this field
        /// </summary>
        public string Direction { get; set; }

        /// <summary>
        /// Gets or Sets whether the field enforces unique values
        /// </summary>
        public bool EnforceUniqueValues { get; set; }

        /// <summary>
        /// Gets or Sets the entity property name of the field
        /// </summary>
        public string EntityPropertyName { get; set; }

        /// <summary>
        /// Gets or Sets whether the field can be used as filter
        /// </summary>
        public bool Filterable { get; set; }

        /// <summary>
        /// Gets or Sets whether the field is from base type
        /// TODO : Review comment
        /// </summary>
        public bool FromBaseType { get; set; }

        /// <summary>
        /// Gets or Sets the group of the field
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Gets or Sets whether the field is hidden
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or Sets the id of the field
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets or Sets whether the field is indexed
        /// </summary>
        public bool Indexed { get; set; }

        /// <summary>
        /// Gets or Sets the index status of the field
        /// TODO: What should be done for read-only fields ?
        /// </summary>
        public int IndexStatus { get; set; }

        /// <summary>
        /// Gets or Sets the internal name of the field
        /// </summary>
        public string InternalName { get; }

        /// <summary>
        /// Gets or Sets the JS link of the field
        /// </summary>
        public string JSLink { get; set; }

        /// <summary>
        /// Gets or Sets whether the field is pinned to filters pane
        /// </summary>
        public bool PinnedToFiltersPane { get; set; }

        /// <summary>
        /// Gets or Sets whether the field is read only
        /// </summary>
        public bool ReadOnlyField { get; set; }

        /// <summary>
        /// Gets or Sets whether the field is required
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or Sets the schema XML of the field
        /// </summary>
        public string SchemaXml { get; set; }

        /// <summary>
        /// Gets or Sets the scope of the field
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Gets or Sets whether the field is sealed
        /// </summary>
        public bool Sealed { get; set; }

        /// <summary>
        /// Gets or Sets whether the field is shown in filters pane
        /// </summary>
        public int ShowInFiltersPane { get; set; }

        /// <summary>
        /// Gets or Sets whether the field can be sorted
        /// </summary>
        public bool Sortable { get; set; }

        /// <summary>
        /// Gets or Sets the static name of the field
        /// </summary>
        public string StaticName { get; }

        /// <summary>
        /// Gets or Sets the title of the field
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or Sets the field type kind of the field
        /// https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-server/ee540543%28v%3doffice.15%29
        /// </summary>
        public FieldType FieldTypeKind { get; }

        /// <summary>
        /// Gets or Sets the type of the field as string value
        /// </summary>
        public string TypeAsString { get; set; }

        /// <summary>
        /// Gets or Sets the display name of the type of the field
        /// </summary>
        public string TypeDisplayName { get; set; }

        /// <summary>
        /// Gets or Sets a short description of the type of the field
        /// </summary>
        public string TypeShortDescription { get; set; }

        /// <summary>
        /// Gets or Sets the validation formula of the field
        /// </summary>
        public string ValidationFormula { get; set; }

        /// <summary>
        /// Gets or Sets the validation message of the field
        /// </summary>
        public string ValidationMessage { get; set; }

        /// <summary>
        /// Gets or Sets the maximum length of the field
        /// </summary>
        public int MaxLength { get; set; }

        // Complex Type ( to implement )
        //public object DescriptionResource { get; set; }

        // Complex Type ( to implement )
        //public object TitleResource { get; set; }

        /// <summary>
        /// Gets or Sets the Locale Id of the currency
        /// </summary>
        public int CurrencyLocaleId { get; set; }

        /// <summary>
        /// Gets or Sets the Date Format 
        /// Valid for DateTime field, Calculated field
        /// </summary>
        public int DateFormat { get; set; }

        public int DisplayFormat { get; set; }
        public int EditFormat { get; set; }

        /// <summary>
        /// Gets or Sets whether the field should be shown as percentage
        /// Valid for Number field, Calculated field
        /// </summary>
        public bool ShowAsPercentage { get; set; }

        #region Number Field
        public double MaximumValue { get; set; }
        public double MinimumValue { get; set; }
        #endregion

        #region Calculated Field

        public string Formula { get; set; }
        public int OutputType { get; set; }
        #endregion

        #region Choice Field
        public bool FillInChoice { get; set; }
        public string Mappings { get; set; }
        public string[] Choices { get; set; }
        #endregion

        #region DateTime Fields
        public int DateTimeCalendarType { get; set; }
        public int FriendlyDisplayFormat { get; set; }
        #endregion

        public bool AllowDisplay { get; set; }
        public bool Presence { get; set; }
        public int SelectionGroup { get; set; }
        public int SelectionMode { get; set; }

        // TODO Add the following properties
        // ======= SP.FieldRatingScale ==========
        // https://s-kainet.github.io/sp-rest-explorer/#/entity/SP.FieldRatingScale
        //GridEndNumber Int32
        //GridNAOptionText String
        //GridStartNumber Int32
        //GridTextRangeAverage String
        //GridTextRangeHigh String
        //GridTextRangeLow String
        //RangeCount Int32
        // ======= SP.FieldMultiLineText ==========
        // https://s-kainet.github.io/sp-rest-explorer/#/entity/SP.FieldMultiLineText
        //AllowHyperlink Boolean
        //AppendOnly Boolean
        //NumberOfLines Int32
        //RestrictedMode Boolean
        //RichText Boolean
        //UnlimitedLengthInDocumentLibrary Boolean
        //WikiLinking Boolean
        // ======= SP.FieldLink ==========
        // https://s-kainet.github.io/sp-rest-explorer/#/entity/SP.FieldLink
        //DisplayName String
        //FieldInternalName String
        //Hidden Boolean
        //Id Guid
        //Name String
        //ReadOnly Boolean
        //Required Boolean
        //ShowInDisplayForm Boolean
        // ======= SP.FieldLookup ==========
        // https://s-kainet.github.io/sp-rest-explorer/#/entity/SP.FieldLookup
        //AllowMultipleValues Boolean
        //DependentLookupInternalNames Collection(String)
        //IsDependentLookup Boolean
        //IsRelationship Boolean
        //LookupField String
        //LookupList String
        //LookupWebId Guid
        //PrimaryFieldId String
        //RelationshipDeleteBehavior Int32
        //UnlimitedLengthInDocumentLibrary Boolean
        // ======= SP.FieldLookup ==========
        // https://s-kainet.github.io/sp-rest-explorer/#/entity/SP.Taxonomy.TaxonomyField
        //AnchorId Guid
        //CreateValuesInEditForm Boolean
        //IsAnchorValid Boolean
        //IsKeyword Boolean
        //IsPathRendered Boolean
        //IsTermSetValid Boolean
        //Open Boolean
        //SspId Guid
        //TargetTemplate String
        //TermSetId Guid
        //TextField Guid
        //UserCreated Boolean
        // TODO Check if nothing is missing
    }
}
