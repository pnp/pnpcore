namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal partial class TermLocalizedDescription : BaseComplexType<ITermLocalizedDescription>, ITermLocalizedDescription
    {
        public string LanguageTag { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }
    }
}
