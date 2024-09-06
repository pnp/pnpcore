using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Field object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(Field))]
    public interface IField : IDataModel<IField>, IDataModelGet<IField>, IDataModelLoad<IField>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the field is auto-indexed
        /// </summary>
        public bool AutoIndexed { get; set; }

        /// <summary>
        /// Specifies whether or not the field can be deleted
        /// </summary>
        public bool CanBeDeleted { get; }

        /// <summary>
        /// Gets or Sets the client side component Id associated with the field
        /// </summary>
        public Guid ClientSideComponentId { get; set; }

        /// <summary>
        /// Gets or Sets the properties of the client side component associated with the field
        /// </summary>
        public string ClientSideComponentProperties { get; set; }

        /// <summary>
        /// Gets or Sets the validation formula
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
        /// Gets or sets the default formula for a calculated field
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
        /// Specifies the direction of the reading order for the field. A string that contains LTR if the reading order is left-to-right, 
        /// RTL if it is right-to-left or none
        /// </summary>
        public string Direction { get; set; }

        /// <summary>
        /// Gets or Sets whether the field enforces unique values
        /// </summary>
        public bool EnforceUniqueValues { get; set; }

        /// <summary>
        /// Gets the name of the entity property for the list item entity that uses this field
        /// </summary>
        public string EntityPropertyName { get; }

        /// <summary>
        /// Returns whether the field can be filtered
        /// </summary>
        public bool Filterable { get; }

        /// <summary>
        /// Gets a Boolean value that indicates whether the field derives from a base field type.      
        /// </summary>
        public bool FromBaseType { get; }

        /// <summary>
        /// Gets or Sets the group of the field
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Specifies whether the field is displayed in the list
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Provides the id of the field
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets or Sets whether the field is indexed
        /// </summary>
        public bool Indexed { get; set; }

        /// <summary>
        /// Describes whether a field is indexed, and whether the data
        /// in the index is complete
        /// </summary>
        public FieldIndexStatus IndexStatus { get; }

        /// <summary>
        /// Specifies the internal name of the field
        /// </summary>
        public string InternalName { get; }

        /// <summary>
        /// Gets or sets the name of an external JS file containing any client rendering logic for fields of this type
        /// </summary>
        public string JSLink { get; set; }

        /// <summary>
        /// Specifies whether values in the field can be modified
        /// </summary>
        public bool ReadOnlyField { get; set; }

        /// <summary>
        /// Gets or Sets whether the field is required
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Specifies the schema that defines the field
        /// </summary>
        public string SchemaXml { get; set; }

        /// <summary>
        /// Specifies the ServerRelativeUrl of the of the web site folder in which the field belongs to
        /// </summary>
        public string Scope { get; }

        /// <summary>
        /// Specifies whether the field can be changed or deleted
        /// </summary>
        public bool Sealed { get; set; }

        /// <summary>
        /// Represents status to determine whether filters pane will show the field
        /// </summary>
        public ShowInFiltersPaneStatus ShowInFiltersPane { get; set; }

        /// <summary>
        /// Returns whether the field can be sorted
        /// </summary>
        public bool Sortable { get; }

        /// <summary>
        /// Specifies the static name of the field
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
        /// Specifies the FieldTypeKind of the field as a string value
        /// </summary>
        public string TypeAsString { get; }

        /// <summary>
        /// Specifies the display name for FieldTypeKind of the field
        /// </summary>
        public string TypeDisplayName { get; }

        /// <summary>
        /// Specifies the description of the FieldTypeKind of the field
        /// </summary>
        public string TypeShortDescription { get; }

        /// <summary>
        /// Specifies the formula referred by the field and gets evaluated when a list item is added or updated in the list
        /// </summary>
        public string ValidationFormula { get; set; }

        /// <summary>
        /// Specifies the message to display if validation formula fails for the field when a list item is added or updated in the list
        /// </summary>
        public string ValidationMessage { get; set; }

        /// <summary>
        /// Specifies the maximum number of characters that can be typed in the field
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
        /// Determines whether to display the choice field as radio buttons or as a drop-down list
        /// </summary>
        public ChoiceFormatType EditFormat { get; set; }

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
        /// Specifies the formula that is used for calculation in the field
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
        /// Gets the dependent lookup internal names of a lookup field
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public string[] DependentLookupInternalNames { get; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Check whether a lookup field is a dependent lookup field
        /// </summary>
        public bool IsDependentLookup { get; }

        /// <summary>
        /// Specifies whether this Lookup field is discoverable from the List being looked up to
        /// </summary>
        public bool IsRelationship { get; set; }

        /// <summary>
        /// Specifies the name of the Field used as the lookup values
        /// </summary>
        public string LookupField { get; set; }

        /// <summary>
        /// Specifies the id (GUID) of the List that contains the Field to use as the lookup values
        /// </summary>
        public string LookupList { get; set; }

        /// <summary>
        /// Specifies the id of the Site that contains the List which contains the
        /// Field used as the lookup values
        /// </summary>
        public Guid LookupWebId { get; set; }

        /// <summary>
        /// Specifies the GUID of the primary lookup field if this is a dependent lookup field. Otherwise, it is empty string
        /// </summary>
        public string PrimaryFieldId { get; set; }

        /// <summary>
        /// Specifies the Delete Behavior of the Lookup Field
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

        #region Taxonomy Field

        /// <summary>
        /// Gets or sets the GUID of the anchor Term object for a TaxonomyField
        /// </summary>
        public Guid AnchorId { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the new Term objects can be added to the TermSet while typing in the TaxonomyField editor control.
        /// </summary>
        public bool CreateValuesInEditForm { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that indicates whether the TaxonomyField value points to the Enterprise Keywords TermSet object.
        /// </summary>
        public bool IsKeyword { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the default Label objects of all the parent Term objects of a Term in the TaxonomyField object 
        /// will be rendered in addition to the default label of that Term.
        /// </summary>
        public bool IsPathRendered { get; set; }

        /// <summary>
        /// Gets a Boolean value that specifies whether the Term object identified by the AnchorId property is valid.
        /// </summary>
        public bool IsAnchorValid { get; }

        /// <summary>
        /// Gets a Boolean value that specifies whether the TermSet object identified by the TermSetId property exists and is available for tagging.
        /// </summary>
        public bool IsTermSetValid { get; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the new Term objects can be added to the TermSet while typing in the TaxonomyField editor control.
        /// </summary>
        public bool Open { get; set; }

        /// <summary>
        /// Gets or sets the GUID that identifies the TermStore object, which contains the Enterprise Keywords for the site that the current TaxonomyField belongs to.
        /// </summary>
        public Guid SspId { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the TermSet object that contains the Term objects used by the current TaxonomyField object.
        /// </summary>
        public Guid TermSetId { get; set; }

        /// <summary>
        /// Gets or sets the GUID that identifies the hidden text field in an item.
        /// </summary>
        public Guid TextField { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the TaxonomyField object is linked to a customized TermSet object.
        /// </summary>
        public bool UserCreated { get; set; }

        /// <summary>
        /// Gets or sets the Web-relative URL of the target page that is used to construct the hyperlink on each Term object when the TaxonomyField object is rendered.
        /// </summary>
        public string TargetTemplate { get; set; }

        #endregion

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }

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
        /// Creates a new <see cref="IFieldUserValue"/> object
        /// </summary>
        /// <param name="principal"><see cref="ISharePointUser"/> or <see cref="ISharePointGroup"/></param>
        /// <returns>Configured <see cref="IFieldUserValue"/> object</returns>
        public IFieldUserValue NewFieldUserValue(ISharePointPrincipal principal);

        /// <summary>
        /// Creates a new <see cref="IFieldTaxonomyValue"/> object
        /// </summary>
        /// <param name="termId">Name of the term to set</param>
        /// <param name="label">Label of the term to set</param>
        /// <param name="wssId">Optionally provide the wssId value</param>
        /// <returns>Configured <see cref="IFieldTaxonomyValue"/> object</returns>
        public IFieldTaxonomyValue NewFieldTaxonomyValue(Guid termId, string label, int wssId = -1);

        /// <summary>
        /// Creates a new <see cref="IFieldLocationValue"/> object
        /// </summary>
        /// <param name="displayName">Name to display for this location</param>
        /// <param name="latitude">Latitude for the location</param>
        /// <param name="longitude">Longitude for the location</param>
        /// <returns>Configured <see cref="IFieldLocationValue"/> object</returns>
        public IFieldLocationValue NewFieldLocationValue(string displayName, double latitude, double longitude);

        /// <summary>
        /// Creates a new collection to hold <see cref="IFieldValue"/> objects
        /// </summary>
        /// <returns></returns>
        public IFieldValueCollection NewFieldValueCollection();

        /// <summary>
        /// Creates a new collection to hold <see cref="IFieldValue"/> objects
        /// </summary>
        /// <param name="fieldValues">Collection of field values to add</param>
        /// <returns></returns>
        public IFieldValueCollection NewFieldValueCollection(IEnumerable<IFieldValue> fieldValues);

        /// <summary>
        /// Creates a new collection to hold <see cref="IFieldTaxonomyValue"/> objects
        /// </summary>
        /// <param name="fieldValues">Collection of field values to add</param>
        /// <returns></returns>
        public IFieldValueCollection NewFieldValueCollection(IEnumerable<KeyValuePair<Guid, string>> fieldValues);

        #endregion

        /// <summary>
        /// Updates the field and pushes changes down to lists consuming this field
        /// </summary>
        /// <returns></returns>
        Task UpdateAndPushChangesAsync();

        /// <summary>
        /// Updates the field and pushes changes down to lists consuming this field
        /// </summary>
        /// <returns></returns>
        void UpdateAndPushChanges();

        /// <summary>
        /// Updates the field and pushes changes down to lists consuming this field
        /// </summary>
        /// <returns></returns>
        Task UpdateAndPushChangesBatchAsync();

        /// <summary>
        /// Updates the field and pushes changes down to lists consuming this field
        /// </summary>
        /// <returns></returns>
        void UpdateAndPushChangesBatch();

        /// <summary>
        /// Updates the field and pushes changes down to lists consuming this field
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns></returns>
        Task UpdateAndPushChangesBatchAsync(Batch batch);

        /// <summary>
        /// Updates the field and pushes changes down to lists consuming this field
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns></returns>
        void UpdateAndPushChangesBatch(Batch batch);

        /// <summary>
        /// Configure the visibility of the field in a Display form
        /// </summary>
        /// <param name="show">True when the field will be configured as visible (= default), false otherwise</param>
        /// <returns></returns>
        Task SetShowInDisplayFormAsync(bool show);

        /// <summary>
        /// Configure the visibility of the field in a Display form
        /// </summary>
        /// <param name="show">True when the field will be configured as visible (= default), false otherwise</param>
        /// <returns></returns>
        void SetShowInDisplayForm(bool show);

        /// <summary>
        /// Configure the visibility of the field in a Edit form
        /// </summary>
        /// <param name="show">True when the field will be configured as visible (= default), false otherwise</param>
        /// <returns></returns>
        Task SetShowInEditFormAsync(bool show);

        /// <summary>
        /// Configure the visibility of the field in a Edit form
        /// </summary>
        /// <param name="show">True when the field will be configured as visible (= default), false otherwise</param>
        /// <returns></returns>
        void SetShowInEditForm(bool show);

        /// <summary>
        /// Configure the visibility of the field in a New form
        /// </summary>
        /// <param name="show">True when the field will be configured as visible (= default), false otherwise</param>
        /// <returns></returns>
        Task SetShowInNewFormAsync(bool show);

        /// <summary>
        /// Configure the visibility of the field in a New form
        /// </summary>
        /// <param name="show">True when the field will be configured as visible (= default), false otherwise</param>
        /// <returns></returns>
        void SetShowInNewForm(bool show);
    }
}
