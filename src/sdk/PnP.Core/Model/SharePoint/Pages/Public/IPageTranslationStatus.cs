using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Translation status for a page
    /// </summary>
    public interface IPageTranslationStatus
    {
        /// <summary>
        /// The culture of this translation
        /// </summary>
        public string Culture { get; }

        /// <summary>
        /// The web-relative path to this page translation
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Last modified date of this translation
        /// </summary>
        public DateTime LastModified { get; }

        /// <summary>
        /// The file status (checked out, draft, published) of this translation
        /// </summary>
        public FileLevel FileStatus { get; }

        /// <summary>
        /// The file status (checked out, draft, published) of this translation
        /// </summary>
        public bool HasPublishedVersion { get; }

        /// <summary>
        /// The page title of this translation
        /// </summary>
        public string Title { get; }

    }
}
