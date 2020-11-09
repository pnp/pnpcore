using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Translation status of a page
    /// </summary>
    internal class TranslationStatusCollection
    {
        //[JsonProperty]
        public List<string> UntranslatedLanguages { get; set; }

        //[JsonProperty]
        public List<TranslationStatus> Items { get; set; }
    }
}
