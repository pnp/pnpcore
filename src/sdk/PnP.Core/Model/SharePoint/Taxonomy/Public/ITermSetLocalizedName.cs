namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents the localized name used in the term store, which identifies the name in the localized language.
    /// </summary>
    public interface ITermSetLocalizedName : IComplexType
    {
        /// <summary>
        /// The language for the label.
        /// </summary>
        public string LanguageTag { get; set; }

        /// <summary>
        /// The name in the localized language.
        /// </summary>
        public string Name { get; set; }

    }
}
