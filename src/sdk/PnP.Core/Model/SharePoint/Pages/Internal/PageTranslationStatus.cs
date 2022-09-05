using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Translation status for a page
    /// </summary>
    internal sealed class PageTranslationStatus : IPageTranslationStatus
    {
        /// <summary>
        /// The culture of this translation
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// The web-relative path to this translation
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Last modified date of this translation
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// The file status (checked out, draft, published) of this translation
        /// </summary>
        public FileLevel FileStatus { get; set; }

        /// <summary>
        /// The file status (checked out, draft, published) of this translation
        /// </summary>
        public bool HasPublishedVersion { get; set; }

        /// <summary>
        /// The page title of this translation
        /// </summary>
        public string Title { get; set; }
    }
}
