using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Translation status of a page
    /// </summary>
    public interface IPageTranslationStatusCollection
    {
        /// <summary>
        /// List of languages for which this page was not yet translated
        /// </summary>
        public List<string> UntranslatedLanguages { get; }

        /// <summary>
        /// List of <see cref="IPageTranslationStatus"/> objects for the translations of this page
        /// </summary>
        public List<IPageTranslationStatus> TranslatedLanguages { get; }
    }
}
