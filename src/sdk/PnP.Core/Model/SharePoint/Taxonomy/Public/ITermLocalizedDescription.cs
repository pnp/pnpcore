namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents the localized name used in the term store, which identifies the name in the localized language.
    /// </summary>
    [ConcreteType(typeof(TermLocalizedDescription))]
    public interface ITermLocalizedDescription : IDataModel<ITermLocalizedDescription>
    {
        /// <summary>
        /// The language for the label.
        /// </summary>
        public string LanguageTag { get; set; }

        /// <summary>
        /// The description in the localized language.
        /// </summary>
        public string Description { get; set; }

    }
}