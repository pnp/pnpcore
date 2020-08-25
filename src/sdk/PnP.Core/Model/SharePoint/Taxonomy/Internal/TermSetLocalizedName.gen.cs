namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal partial class TermSetLocalizedName : BaseComplexType<ITermSetLocalizedName>, ITermSetLocalizedName
    {
        public string LanguageTag { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }
    }
}
