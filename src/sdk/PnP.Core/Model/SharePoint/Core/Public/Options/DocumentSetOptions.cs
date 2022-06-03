using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available options for a Document Set
    /// </summary>
    public class DocumentSetOptions
    {
        /// <summary>
        /// Indicates whether to add the name of the document set to each file name.
        /// </summary>
        public bool? ShouldPrefixNameToFile { get; set; }

        /// <summary>
        /// Welcome page absolute URL.
        /// </summary>
        public string WelcomePageUrl { get; set; }

        /// <summary>
        /// Specifies whether to push welcome page changes to inherited content types.
        /// </summary>
        public bool? PropagateWelcomePageChanges { get; set; }

        /// <summary>
        /// Defines if we keep the existing content types that are already allowed in the document set
        /// </summary>
        public bool KeepExistingContentTypes { get; set; } = true;

        /// <summary>
        /// List of the allowed content types in the document set
        /// </summary>
        public List<IContentType> AllowedContentTypes { get; set; }

        /// <summary>
        /// Defines if we keep the existing shared columns or delete those (by not adding them to our body)
        /// </summary>
        public bool KeepExistingSharedColumns { get; set; } = true;

        /// <summary>
        /// Columns edited on the document set that synchronize to all documents in the set. 
        /// These are read-only on the documents themselves.
        /// </summary>
        public List<IField> SharedColumns { get; set; }

        /// <summary>
        /// Defines if we keep the existing welcome page columns or delete those (by not adding them to our body)
        /// </summary>
        public bool KeepExistingWelcomePageColumns { get; set; } = true;

        /// <summary>
        /// Specifies columns to show on the welcome page for the document set.
        /// </summary>
        public List<IField> WelcomePageColumns { get; set; }

        /// <summary>
        /// Defines if we keep the existing default contents or delete those (by not adding them to our body)
        /// </summary>
        public bool KeepExistingDefaultContent { get; set; } = true;

        /// <summary>
        /// Default contents of document set.
        /// </summary>
        public List<DocumentSetContentOptions> DefaultContents { get; set; }
    }
}
