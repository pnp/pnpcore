namespace PnP.Core.Model.SharePoint
{
    internal sealed class Theme : ITheme
    {
        public string Name {get; set;}

        public bool IsCustomTheme { get; set; }

        public string ThemeJson { get; set; }
    }
}
