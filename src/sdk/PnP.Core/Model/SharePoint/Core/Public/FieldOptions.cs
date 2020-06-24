using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available options for all SharePoint fields
    /// </summary>
    public abstract class FieldOptions
    {
        /// <summary>
        /// Sets whether the field is required
        /// </summary>
        public bool? Required { get; set; }
    }

    /// <summary>
    /// Available options for adding most of SharePoint fields
    /// (Excepted Lookup fields)
    /// </summary>
    public abstract class CommonFieldOptions : FieldOptions
    {
        /// <summary>
        /// Sets the description of the field
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Sets the default formula of the field
        /// </summary>
        public string DefaultFormula { get; set; }

        /// <summary>
        /// Sets whether the field enforces unique values
        /// </summary>
        public bool? EnforceUniqueValues { get; set; }

        /// <summary>
        /// Sets the group of the field
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Sets whether the field is hidden
        /// </summary>
        public bool? Hidden { get; set; }

        /// <summary>
        /// Sets whether the field is indexed
        /// </summary>
        public bool? Indexed { get; set; }

        /// <summary>
        /// Sets whether the field is required
        /// </summary>
        public bool? Required { get; set; }

        /// <summary>
        /// Sets the validation formula of the field
        /// </summary>
        public string ValidationFormula { get; set; }

        /// <summary>
        /// Sets the validation message of the field
        /// </summary>
        public string ValidationMessage { get; set; }


    }


    /// <summary>
    /// Available options for SharePoint Text fields
    /// </summary>
    public class FieldTextOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets the maximum length of the text field.
        /// </summary>
        public int? MaxLength { get; set; }
    }

    /// <summary>
    /// Available options for SharePoint Multiline Text fields
    /// </summary>
    public class FieldMultilineTextOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies whether a hyperlink is allowed as a value of the field.
        /// </summary>
        public bool? AllowHyperlink { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether all changes to the value of the field are displayed in list forms.
        /// </summary>
        public bool? AppendOnly { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the number of lines of text to display for the field.
        /// </summary>
        public int? NumberOfLines { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the field supports a subset of rich formatting.
        /// </summary>
        public bool? RestrictedMode { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the field supports rich formatting.
        /// </summary>
        public bool? RichText { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the field supports unlimited length in document libraries.
        /// </summary>
        public bool? UnlimitedLengthInDocumentLibrary { get; set; }

        // READONLY - TODO Remove from options => Double check it is present and readonly in model entity
        //public bool WikiLinking { get; }
    }

    /// <summary>
    /// Available options for SharePoint URL fields
    /// </summary>
    public class FieldUrlOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies the display format for the value in the field.
        /// </summary>
        public UrlFieldFormatType DisplayFormat { get; set; }
    }

    /// <summary>
    /// Available options for SharePoint Number fields
    /// </summary>
    public class FieldNumberOptions : CommonFieldOptions
    {
        // (Not an enum in CSOM FieldNumber class : https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-csom/mt827700%28v%3doffice.15%29 )
        /// <summary>
        /// Gets or sets the display format of the field.
        /// Althought in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
        /// </summary>
        //public int DisplayFormat { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the maximum allowed value for the field.
        /// </summary>
        public double? MaximumValue { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the minimum allowed value for the field.
        /// </summary>
        public double? MinimumValue { get; set; }

        /// <summary>
        /// Gets or sets whether the field must be shown as percentage.
        /// </summary>
        public bool? ShowAsPercentage { get; set; }
    }

    /// <summary>
    /// Available options for SharePoint DateTime fields
    /// </summary>
    public class FieldDateTimeOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies the calendar type of the field.
        /// </summary>
        public CalendarType DateTimeCalendarType { get; set; }

        /// <summary>
        /// Gets or sets the type of date and time format that is used in the field.
        /// </summary>
        public DateTimeFieldFormatType DisplayFormat { get; set; }

        /// <summary>
        /// Gets or sets the type of friendly display format that is used in the field.
        /// </summary>
        public DateTimeFieldFriendlyFormatType FriendlyDisplayFormat { get; set; }
    }

    /// <summary>
    /// Available options for SharePoint Calculated fields
    /// </summary>
    public class FieldCalculatedOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies the language code identifier (LCID) used to format the value of the field.
        /// </summary>
        public int? CurrencyLocaleId { get; set; }

        /// <summary>
        /// Gets or sets the type of date and time format that is used in the field.
        /// </summary>
        public DateTimeFieldFormatType? DateFormat { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the formula for the field.
        /// </summary>
        public string Formula { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the output format for the field.
        /// </summary>
        public FieldType OutputType { get; set; }

        /// <summary>
        /// Gets or sets whether the field must be shown as percentage.
        /// </summary>
        public bool? ShowAsPercentage { get; set; }

        ///// <summary>
        ///// Gets or sets the display format of the field.
        ///// Althought in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
        ///// </summary>
        //public int? DisplayFormat { get; set; }
    }

    /// <summary>
    /// Available options for SharePoint Currency fields
    /// </summary>
    public class FieldCurrencyOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies the language code identifier (LCID) used to format the value of the field.
        /// </summary>
        public int? CurrencyLocaleId { get; set; }
    }

    /// <summary>
    /// Available options for SharePoint Multi Choice fields
    /// </summary>
    public class FieldMultiChoiceOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies whether the field can accept values other than those specified in Choices.
        /// </summary>
        public bool? FillInChoice { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies values that are available for selection in the field.
        /// </summary>
        public string[] Choices { get; set; }
    }

    /// <summary>
    /// Available options for SharePoint Choice fields
    /// </summary>
    public class FieldChoiceOptions : FieldMultiChoiceOptions
    {
        /// <summary>
        /// Determines whether to display the choice field as option buttons (also known as “radio buttons”) or as a drop-down list.
        /// </summary>
        public ChoiceFormatType EditFormat { get; set; }
    }

    /// <summary>
    /// Available options for SharePoint lookup fields
    /// </summary>
    public class FieldLookupOptions : FieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies the internal field name of the field used as the lookup values.
        /// </summary>
        public string LookupFieldName { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the list identifier of the list that contains the field to use as the lookup values.
        /// </summary>
        public Guid LookupListId { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the GUID that identifies the site containing the list which contains the field used as the lookup values.
        /// </summary>
        public Guid? LookupWebId { get; set; }
    }

    /// <summary>
    /// Available options for SharePoint user fields
    /// </summary>
    public class FieldUserOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies whether to allow multiple values.
        /// </summary>
        public bool? AllowMultipleValues  { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether to display the name of the user in a survey list.
        /// </summary>
        public bool? AllowDisplay { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether presence is enabled on the field.
        /// </summary>
        public bool? Presence { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the identifier of the SharePoint group whose members can be selected as values of the field.
        /// </summary>
        public int? SelectionGroup { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether users and groups or only users can be selected.
        /// </summary>
        public FieldUserSelectionMode SelectionMode { get; set; }
    }

    #region NOT (YET) SUPPORTED
    // NOTE Is it relevant to implement ?
    // Looks like REST API doesn't work to add those type of fields
    /// <summary>
    /// Available options for SharePoint computed fields
    /// </summary>
    //public class FieldComputedOptions : FieldOptions
    //{
    //    /// <summary>
    //    /// Gets or sets a value that specifies whether a lookup field can reference the field.
    //    /// </summary>
    //    public bool EnableLookup { get; set; }
    //}

    ///// <summary>
    ///// Available options for SharePoint Rating Scale fields
    ///// </summary>
    //public class FieldRatingScale : FieldMultiChoiceOptions
    //{
    //    /// <summary>
    //    /// Gets or sets a value that specifies the end number for the rating scale.
    //    /// </summary>
    //    public int GridEndNumber { get; set; }

    //    /// <summary>
    //    /// Gets or sets a value that specifies the display text corresponding to the choice in the rating scale that indicates the non-applicable option.
    //    /// </summary>
    //    public string GridNAOptionText { get; set; }

    //    /// <summary>
    //    /// Gets or sets a value that specifies the start number for the rating scale.
    //    /// </summary>
    //    public int GridStartNumber { get; set; }

    //    /// <summary>
    //    /// Gets or sets a value that specifies the display text corresponding to the average of the rating scale.
    //    /// </summary>
    //    public string GridTextRangeAverage { get; set; }

    //    /// <summary>
    //    /// Gets or sets a value that specifies the display text corresponding to the maximum of the rating scale.
    //    /// </summary>
    //    public string GridTextRangeHigh { get; set; }

    //    /// <summary>
    //    /// Gets or sets a value that specifies the display text corresponding to the minimum of the rating scale.
    //    /// </summary>
    //    public string GridTextRangeLow { get; set; }

    //    /// <summary>
    //    /// Gets pr sets the range count of the field.
    //    /// </summary>
    //    public int RangeCount { get; set; }
    //}
    #endregion
}
