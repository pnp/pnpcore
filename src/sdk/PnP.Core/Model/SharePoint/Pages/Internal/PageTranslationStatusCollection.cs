using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Translation status of a page
    /// </summary>
    internal class PageTranslationStatusCollection : IPageTranslationStatusCollection
    {
        public List<string> UntranslatedLanguages { get; set; }

        public List<IPageTranslationStatus> TranslatedLanguages { get; set; }
    }
}
