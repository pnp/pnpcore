namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal partial class TermSetLocalizedName : BaseDataModel<ITermSetLocalizedName>, ITermSetLocalizedName
    {
        #region Properties
        public string LanguageTag { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(LanguageTag))]
        public override object Key { get => LanguageTag; set => LanguageTag = value.ToString(); }
        #endregion
    }
}