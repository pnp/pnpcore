namespace PnP.Core.Model.SharePoint
{
    internal partial class ContentType : BaseDataModel<IContentType>, IContentType
    {
        public string StringId { get => GetValue<string>(); set => SetValue(value); }

        public IContentTypeId Id { get => GetValue<IContentTypeId>(); set => SetValue(value); }

        public string ClientFormCustomFormatter { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayFormTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string DocumentTemplate { get => GetValue<string>(); set => SetValue(value); }

        public string DocumentTemplateUrl { get => GetValue<string>(); set => SetValue(value); }

        public string EditFormTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public string EditFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Group { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public string JSLink { get => GetValue<string>(); set => SetValue(value); }

        public string MobileDisplayFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string MobileEditFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string MobileNewFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string NewFormTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public string NewFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ReadOnly { get => GetValue<bool>(); set => SetValue(value); }

        public string SchemaXml { get => GetValue<string>(); set => SetValue(value); }

        public string SchemaXmlWithResourceTokens { get => GetValue<string>(); set => SetValue(value); }

        public string Scope { get => GetValue<string>(); set => SetValue(value); }

        public bool Sealed { get => GetValue<bool>(); set => SetValue(value); }

        [KeyProperty("StringId")]
        public override object Key { get => this.StringId; set => this.StringId = value.ToString(); }
    }
}
