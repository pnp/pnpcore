using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// App Admin features
    /// </summary>
    public interface IAppManager<T> : IAppOperations where T : IApp
    {
        /// <summary>
        /// Gets available app by it's unique id.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns>An instance of the app.</returns>
        T GetAvailable(Guid id);

        /// <summary>
        /// Gets available app by it's unique id.
        /// </summary>
        /// <param name="id">The unique id of the app. Notice that this is not the product id as listed in the app catalog.</param>
        /// <returns>An instance of the app.</returns>
        Task<T> GetAvailableAsync(Guid id);

        /// <summary>
        /// Gets available app by it's title.
        /// </summary>
        /// <param name="title">An app's title.</param>
        /// <returns>An instance of the app.</returns>
        T GetAvailable(string title);

        /// <summary>
        /// Gets available app by it's title.
        /// </summary>
        /// <param name="title">An app's title.</param>
        /// <returns>An instance of the app.</returns>
        Task<T> GetAvailableAsync(string title);

        /// <summary>
        /// Gets all available apps from the app catalog.
        /// </summary>
        /// <returns>A collection of apps.</returns>
        IList<T> GetAvailable();

        /// <summary>
        /// Gets all available apps from the app catalog.
        /// </summary>
        /// <returns>A collection of apps.</returns>
        Task<IList<T>> GetAvailableAsync();

        /// <summary>
        /// Uploads a file to the app catalog.
        /// </summary>
        /// <param name="file">A byte array containing the file.</param>
        /// <param name="fileName">The filename (e.g. myapp.sppkg) of the file to upload.</param>
        /// <param name="overwrite">If true will overwrite an existing entry.</param>
        /// <returns>An instance of the app.</returns>
        T Add(byte[] file, string fileName, bool overwrite = false);

        /// <summary>
        /// Uploads a file to the app catalog.
        /// </summary>
        /// <param name="file">A byte array containing the file.</param>
        /// <param name="fileName">The filename (e.g. myapp.sppkg) of the file to upload.</param>
        /// <param name="overwrite">If true will overwrite an existing entry.</param>
        /// <returns>An instance of the app.</returns>
        Task<T> AddAsync(byte[] file, string fileName, bool overwrite = false);

        /// <summary>
        /// Uploads an app file to the app catalog.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="overwrite">If true will overwrite an existing entry.</param>
        /// <returns>An instance of the app.</returns>
        T Add(string path, bool overwrite = false);

        /// <summary>
        /// Uploads an app file to the app catalog.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="overwrite">If true will overwrite an existing entry.</param>
        /// <returns>An instance of the app.</returns>
        Task<T> AddAsync(string path, bool overwrite = false);
        
        /// <summary>
        /// Get the SharePoint Apps Service Principal enabling you to approve/reject permissions requests
        /// </summary>
        IServicePrincipal ServicePrincipal { get; }
    }
}
