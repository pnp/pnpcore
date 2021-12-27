namespace PnP.Core.Model.SharePoint
{
    internal sealed class HeaderOptions : IHeaderOptions
    {
        public HeaderLayoutType Layout { get; set; }

        public VariantThemeType Emphasis { get; set; }

        public bool HideTitle { get; set; }

        public LogoAlignment LogoAlignment { get; set; }
    }
}
