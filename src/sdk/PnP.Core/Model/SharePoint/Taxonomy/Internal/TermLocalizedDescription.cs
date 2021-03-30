namespace PnP.Core.Model.SharePoint
{
    [GraphType]
    internal partial class TermLocalizedDescription : BaseDataModel<ITermLocalizedDescription>, ITermLocalizedDescription
    {
        #region Properties
        public string LanguageTag { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(LanguageTag))]
        public override object Key { get => LanguageTag; set => LanguageTag = value.ToString(); }
        #endregion
    }
}