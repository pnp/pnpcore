using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Microsoft 365 Term store
    /// </summary>
    [ConcreteType(typeof(TermStore))]
    public interface ITermStore : IDataModel<ITermStore>, IDataModelGet<ITermStore>, IDataModelLoad<ITermStore>, IDataModelUpdate
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
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public ITermGroupCollection Groups { get; }
    }
}