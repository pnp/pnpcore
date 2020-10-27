using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Microsoft 365 Term store
    /// </summary>
    [ConcreteType(typeof(TermStore))]
    public interface ITermStore : IDataModel<ITermStore>, IDataModelGet<ITermStore>, IDataModelUpdate
    {
        /// <summary>
        /// The Unique ID of the Term Store
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Default language of the termstore.
        /// </summary>
        public string DefaultLanguage { get; set; }

        /// <summary>
        /// List of languages for the term store.
        /// </summary>
        public List<string> Languages { get; }

        /// <summary>
        /// Collection of term groups in this term store
        /// </summary>
        public ITermGroupCollection Groups { get; }
    }
}
