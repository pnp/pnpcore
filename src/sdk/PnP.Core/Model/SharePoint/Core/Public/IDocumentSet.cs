using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Document Set.
    /// </summary>
    [ConcreteType(typeof(DocumentSet))]
    public interface IDocumentSet : IDataModel<IDocumentSet>
    {
        #region Properties

        /// <summary>
        /// Content Type Id
        /// </summary>
        public string ContentTypeId { get; set; }

        /// <summary>
        /// Content types allowed in document set.
        /// </summary>
        public IList<IContentTypeInfo> AllowedContentTypes { get; set; }

        /// <summary>
        /// Default contents of document set.
        /// </summary>
        public IList<IDocumentSetContent> DefaultContents { get; set; }

        /// <summary>
        /// Indicates whether to add the name of the document set to each file name.
        /// </summary>
        public bool ShouldPrefixNameToFile { get; set; }

        /// <summary>
        /// Welcome page absolute URL.
        /// </summary>
        public string WelcomePageUrl { get; set; }

        /// <summary>
        /// Columns edited on the document set that synchronize to all documents in the set. 
        /// These are read-only on the documents themselves.
        /// </summary>
        public IList<IField> SharedColumns { get; set; }

        /// <summary>
        /// Specifies columns to show on the welcome page for the document set.
        /// </summary>
        public IList<IField> WelcomePageColumns { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the document set
        /// </summary>
        /// <returns></returns>
        Task<IDocumentSet> UpdateAsync(DocumentSetOptions options);

        /// <summary>
        /// Updates the document set
        /// </summary>
        IDocumentSet Update(DocumentSetOptions options);

        #endregion
    }
}
