namespace PnP.Core.Model.SharePoint
{
    internal sealed class FontPackage : IFontPackage
    {
        public string ID { get; set; }
        public bool IsHidden { get; set; }
        public bool IsValid { get; set; }
        public string PackageJson { get; set; }
        public int Store { get; set; }
        public string Title { get; set; }
    }
}
