namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines a theme that can be applied to a site
    /// </summary>
    public interface ITheme
    {
        /// <summary>
        /// Name of the theme
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Is this a custom theme added to the tenant?
        /// </summary>
        public bool IsCustomTheme { get; }

        /// <summary>
        /// The theme's JSON definition
        /// </summary>
        public string ThemeJson { get; }
    }
}
