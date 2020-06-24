using System;

namespace PnP.Core.Model.SharePoint
{
    internal partial class Field : BaseDataModel<IField>, IField
    {
        public bool AutoIndexed { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanBeDeleted { get => GetValue<bool>(); set => SetValue(value); }

        public Guid ClientSideComponentId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ClientSideComponentProperties { get => GetValue<string>(); set => SetValue(value); }

        public string ClientValidationFormula { get => GetValue<string>(); set => SetValue(value); }

        public string ClientValidationMessage { get => GetValue<string>(); set => SetValue(value); }

        public string CustomFormatter { get => GetValue<string>(); set => SetValue(value); }

        public string DefaultFormula { get => GetValue<string>(); set => SetValue(value); }

        public object DefaultValue { get => GetValue<object>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public string Direction { get => GetValue<string>(); set => SetValue(value); }

        public bool EnforceUniqueValues { get => GetValue<bool>(); set => SetValue(value); }

        public string EntityPropertyName { get => GetValue<string>(); set => SetValue(value); }

        public bool Filterable { get => GetValue<bool>(); set => SetValue(value); }

        public bool FromBaseType { get => GetValue<bool>(); set => SetValue(value); }

        public string Group { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public bool Indexed { get => GetValue<bool>(); set => SetValue(value); }

        public int IndexStatus { get => GetValue<int>(); set => SetValue(value); }

        public string InternalName { get => GetValue<string>(); set => SetValue(value); }

        public string JSLink { get => GetValue<string>(); set => SetValue(value); }

        public bool PinnedToFiltersPane { get => GetValue<bool>(); set => SetValue(value); }

        public bool ReadOnlyField { get => GetValue<bool>(); set => SetValue(value); }

        public bool Required { get => GetValue<bool>(); set => SetValue(value); }

        public string SchemaXml { get => GetValue<string>(); set => SetValue(value); }

        public string Scope { get => GetValue<string>(); set => SetValue(value); }

        public bool Sealed { get => GetValue<bool>(); set => SetValue(value); }

        public int ShowInFiltersPane { get => GetValue<int>(); set => SetValue(value); }

        public bool Sortable { get => GetValue<bool>(); set => SetValue(value); }

        public string StaticName { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public FieldType FieldTypeKind { get => GetValue<FieldType>(); set => SetValue(value); }

        public string TypeAsString { get => GetValue<string>(); set => SetValue(value); }

        public string TypeDisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string TypeShortDescription { get => GetValue<string>(); set => SetValue(value); }

        public string ValidationFormula { get => GetValue<string>(); set => SetValue(value); }

        public string ValidationMessage { get => GetValue<string>(); set => SetValue(value); }

        public int MaxLength { get => GetValue<int>(); set => SetValue(value); }

        public int CurrencyLocaleId { get => GetValue<int>(); set => SetValue(value); }

        public DateTimeFieldFormatType DateFormat { get => GetValue<DateTimeFieldFormatType>(); set => SetValue(value); }

        public int DisplayFormat { get => GetValue<int>(); set => SetValue(value); }

        public int EditFormat { get => GetValue<int>(); set => SetValue(value); }

        public bool ShowAsPercentage { get => GetValue<bool>(); set => SetValue(value); }

        public double MaximumValue { get => GetValue<double>(); set => SetValue(value); }

        public double MinimumValue { get => GetValue<double>(); set => SetValue(value); }

        public string Formula { get => GetValue<string>(); set => SetValue(value); }

        public FieldType OutputType { get => GetValue<FieldType>(); set => SetValue(value); }

        public bool FillInChoice { get => GetValue<bool>(); set => SetValue(value); }

        public string Mappings { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Choices", JsonPath = "results")]
        public string[] Choices { get => GetValue<string[]>(); set => SetValue(value); }

        public CalendarType DateTimeCalendarType { get => GetValue<CalendarType>(); set => SetValue(value); }

        public DateTimeFieldFriendlyFormatType FriendlyDisplayFormat { get => GetValue<DateTimeFieldFriendlyFormatType>(); set => SetValue(value); }

        public bool AllowDisplay { get => GetValue<bool>(); set => SetValue(value); }

        public bool Presence { get => GetValue<bool>(); set => SetValue(value); }

        public int SelectionGroup { get => GetValue<int>(); set => SetValue(value); }

        public FieldUserSelectionMode SelectionMode { get => GetValue<FieldUserSelectionMode>(); set => SetValue(value); }

        public bool AllowHyperlink { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool AppendOnly { get => GetValue<bool>(); set => SetValue(value); }
        
        public int NumberOfLines { get => GetValue<int>(); set => SetValue(value); }
        
        public bool RestrictedMode { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool RichText { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool UnlimitedLengthInDocumentLibrary { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableLookup { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowMultipleValues { get => GetValue<bool>(); set => SetValue(value); }
        
        public string[] DependentLookupInternalNames { get => GetValue<string[]>(); set => SetValue(value); }
        
        public bool IsDependentLookup { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool IsRelationship { get => GetValue<bool>(); set => SetValue(value); }
        
        public string LookupField { get => GetValue<string>(); set => SetValue(value); }
        
        public string LookupList { get => GetValue<string>(); set => SetValue(value); }
        
        public Guid LookupWebId { get => GetValue<Guid>(); set => SetValue(value); }
        
        public string PrimaryFieldId { get => GetValue<string>(); set => SetValue(value); }

        public RelationshipDeleteBehaviorType RelationshipDeleteBehavior { get => GetValue<RelationshipDeleteBehaviorType>(); set => SetValue(value); }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
        
    }
}
