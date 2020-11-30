using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Field object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(Field))]
    public interface IField : IDataModel<IField>, IDataModelGet<IField>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
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
        public DateTimeFieldFormatType DateFormat { get; set; }

        /// <summary>
        /// Gets or Sets the display format of the field
        /// CAUTION: Is an integer value since according to field type, the display format enum may change
        /// </summary>
        public int DisplayFormat { get; set; }

        /// <summary>
        /// Gets or Sets the edit format of the field
        /// </summary>
        public int EditFormat { get; set; }

        /// <summary>
        /// Gets or Sets whether the field should be shown as percentage
        /// Valid for Number field, Calculated field
        /// </summary>
        public bool ShowAsPercentage { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether a hyperlink is allowed as a value of the field.
        /// </summary>
        public bool AllowHyperlink { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether all changes to the value of the field are displayed in list forms.
        /// </summary>
        public bool AppendOnly { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the number of lines of text to display for the field.
        /// </summary>
        public int NumberOfLines { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the field supports a subset of rich formatting.
        /// </summary>
        public bool RestrictedMode { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the field supports rich formatting.
        /// </summary>
        public bool RichText { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the field supports unlimited length in document libraries.
        /// </summary>
        public bool UnlimitedLengthInDocumentLibrary { get; set; }

        #region Number Field
        /// <summary>
        /// Gets or sets the maximum value of a number field
        /// </summary>
        public double MaximumValue { get; set; }

        /// <summary>
        /// Gets or sets the minimum value of a number field
        /// </summary>
        public double MinimumValue { get; set; }
        #endregion

        #region Calculated Field

        /// <summary>
        /// Gets or sets the formula of a calculated field
        /// </summary>
        public string Formula { get; set; }

        /// <summary>
        /// Gets or sets the type of a calculated field output
        /// </summary>
        public FieldType OutputType { get; set; }
        #endregion

        #region Computed Field

        /// <summary>
        /// Gets or sets whether the lookup should be enabled for computed field
        /// </summary>
        public bool EnableLookup { get; set; }
        #endregion

        #region Choice Field

        /// <summary>
        /// Gets or sets whether choice field can be filled in by user
        /// </summary>
        public bool FillInChoice { get; set; }

        /// <summary>
        /// Gets the mappings of a choice field
        /// </summary>
        public string Mappings { get; }

        /// <summary>
        /// Gets or sets the choices of choice field
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public string[] Choices { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
        #endregion

        #region DateTime Field

        /// <summary>
        /// Gets or sets the type of calendar to use of a DateTime field
        /// </summary>
        public CalendarType DateTimeCalendarType { get; set; }

        /// <summary>
        /// Gets or sets the friendly format type of a DateTime field
        /// </summary>
        public DateTimeFieldFriendlyFormatType FriendlyDisplayFormat { get; set; }
        #endregion

        #region Lookup Field

        /// <summary>
        /// Gets or sets whether the lookup fields allows multiple values
        /// </summary>
        public bool AllowMultipleValues { get; set; }

        /// <summary>
        /// Gets or sets the dependent lookup internal names of a lookup field
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public string[] DependentLookupInternalNames { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Gets or sets whether a lookup field is dependent lookup
        /// </summary>
        public bool IsDependentLookup { get; set; }

        /// <summary>
        /// Gets or sets whether a lookup field is a relationship
        /// </summary>
        public bool IsRelationship { get; set; }

        /// <summary>
        /// Gets or sets the internal name of the related field
        /// </summary>
        public string LookupField { get; set; }

        /// <summary>
        /// Gets or sets the Id of the related list
        /// </summary>
        public string LookupList { get; set; }

        /// <summary>
        /// Gets or sets the lookup web Id
        /// </summary>
        public Guid LookupWebId { get; set; }

        /// <summary>
        /// Gets or sets the primary field Id of the lookup field
        /// </summary>
        public string PrimaryFieldId { get; set; }

        /// <summary>
        /// Gets or sets the deletion behavior with the relationship of the lookup field
        /// </summary>
        public RelationshipDeleteBehaviorType RelationshipDeleteBehavior { get; set; }
        #endregion

        #region User Field

        /// <summary>
        /// Gets or sets whether to allow display if the user name
        /// </summary>
        public bool AllowDisplay { get; set; }

        /// <summary>
        /// Gets or sets whether to display the presence indicator of the user
        /// </summary>
        public bool Presence { get; set; }

        /// <summary>
        /// Gets or sets the Id of the group to which the users to select belong to
        /// </summary>
        public int SelectionGroup { get; set; }

        /// <summary>
        /// Gets or sets the selection mode of the user field
        /// </summary>
        public FieldUserSelectionMode SelectionMode { get; set; }
        #endregion

        #region FieldValue object creation
        /// <summary>
        /// Creates a new <see cref="IFieldUrlValue"/> object
        /// </summary>
        /// <param name="url">Url value</param>
        /// <param name="description">Optional description value</param>
        /// <returns>Configured <see cref="IFieldUrlValue"/> object</returns>
        public IFieldUrlValue NewFieldUrlValue(string url, string description = null);

        /// <summary>
        /// Creates a new <see cref="IFieldLookupValue"/> object
        /// </summary>
        /// <param name="lookupId">Id of the lookup value</param>
        /// <returns>Configured <see cref="IFieldLookupValue"/> object</returns>
        public IFieldLookupValue NewFieldLookupValue(int lookupId);

        /// <summary>
        /// Creates a new <see cref="IFieldUserValue"/> object
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns>Configured <see cref="IFieldUserValue"/> object</returns>
        public IFieldUserValue NewFieldUserValue(int userId);

        /// <summary>
        /// Creates a new <see cref="IFieldTaxonomyValue"/> object
        /// </summary>
        /// <param name="termId">Name of the term to set</param>
        /// <param name="label">Label of the term to set</param>
        /// <param name="wssId">Optionally provide the wssId value</param>
        /// <returns>Configured <see cref="IFieldTaxonomyValue"/> object</returns>
        public IFieldTaxonomyValue NewFieldTaxonomyValue(Guid termId, string label, int wssId = -1);

        /// <summary>
        /// Creates a new collection to hold <see cref="IFieldValue"/> objects
        /// </summary>
        /// <param name="parent">List item values collection that's being updated by this collection</param>
        /// <returns></returns>
        public IFieldValueCollection NewFieldValueCollection(TransientDictionary parent);

        #endregion


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

        // ======= SP.Taxonomy.TaxonomyField ==========
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
