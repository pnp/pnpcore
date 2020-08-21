namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal partial class TermLocalizedLabel : BaseComplexType<ITermLocalizedLabel>, ITermLocalizedLabel
    {
        public string LanguageTag { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public bool IsDefault { get => GetValue<bool>(); set => SetValue(value); }
    }
}
