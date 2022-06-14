using PnP.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define the Content Type Hub object of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(ContentTypeHub))]
    public interface IContentTypeHub: IDataModel<IContentTypeHub>, IDataModelGet<IContentTypeHub>, IDataModelLoad<IContentTypeHub>
    {
        #region Properties

        /// <summary>
        /// Collection of content types in the current Web object
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IFieldCollection Fields { get; }

        /// <summary>
        /// Collection of content types in the current Web object
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IContentTypeCollection ContentTypes { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the site id of the content type hub
        /// </summary>
        /// <returns>Site id of the ct hub</returns>
        public Task<string> GetSiteIdAsync();


        /// <summary>
        /// Gets the site id of the content type hub
        /// </summary>
        /// <returns>Site id of the ct hub</returns>
        public string GetSiteId();

        #endregion
    }
}
