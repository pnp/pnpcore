namespace PnP.Core.Model.SharePoint
{
    internal sealed class FooterOptions : IFooterOptions
    {
        public bool Enabled { get; set; }
        
        public FooterLayoutType Layout { get; set; }

        public FooterVariantThemeType Emphasis { get; set; }
    }
}
