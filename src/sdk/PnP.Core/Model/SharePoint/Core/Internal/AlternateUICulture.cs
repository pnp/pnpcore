namespace PnP.Core.Model.SharePoint
{
    internal class AlternateUICulture : BaseDataModel<IAlternateUICulture>, IAlternateUICulture
    {
        public int LCID { get => GetValue<int>(); set => SetValue(value); }
    }
}
