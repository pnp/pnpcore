using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Field class, write your custom code here
    /// </summary>
    [SharePointType("SP.Field", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class Field : BaseDataModel<IField>, IField
    {
        #region Construction
        public Field()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public bool AutoIndexed { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanBeDeleted { get => GetValue<bool>(); set => SetValue(value); }

        public Guid ClientSideComponentId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ClientSideComponentProperties { get => GetValue<string>(); set => SetValue(value); }

        public string ClientValidationFormula { get => GetValue<string>(); set => SetValue(value); }

        public string ClientValidationMessage { get => GetValue<string>(); set => SetValue(value); }

        public string CustomFormatter { get => GetValue<string>(); set => SetValue(value); }

        public string DefaultFormula { get => GetValue<string>(); set => SetValue(value); }

        public string DefaultValue { get => GetValue<string>(); set => SetValue(value); }

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

        public int FieldTypeKind { get => GetValue<int>(); set => SetValue(value); }

        public string TypeAsString { get => GetValue<string>(); set => SetValue(value); }

        public string TypeDisplayName { get => GetValue<string>(); set => SetValue(value); }

        public string TypeShortDescription { get => GetValue<string>(); set => SetValue(value); }

        public string ValidationFormula { get => GetValue<string>(); set => SetValue(value); }

        public string ValidationMessage { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #region New properties

        public bool NoCrawl { get => GetValue<bool>(); set => SetValue(value); }

        public string SchemaXmlWithResourceTokens { get => GetValue<string>(); set => SetValue(value); }

        public IUserResource DescriptionResource { get => GetModelValue<IUserResource>(); }


        public IUserResource TitleResource { get => GetModelValue<IUserResource>(); }


        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }


        #endregion

        #region Extension methods
        #endregion
    }
}
