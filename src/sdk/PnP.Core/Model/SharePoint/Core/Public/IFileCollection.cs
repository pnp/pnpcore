using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of File objects
    /// </summary>
    [ConcreteType(typeof(FileCollection))]
    public interface IFileCollection : IQueryable<IFile>, IDataModelCollection<IFile>, IDataModelCollectionDeleteByGuidId
    {
        #region Add
        /// <summary>
        /// Add a file to the file collection using batching (async)
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="content">The content of the file.</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <returns>The added file object.</returns>
        Task<IFile> AddAsync(string name, Stream content, bool overwrite = false);

        /// <summary>
        /// Add a file to the file collection using batching (sync)
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="content">The content of the file.</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <returns>The added file object.</returns>
        IFile Add(string name, Stream content, bool overwrite = false);

        /// <summary>
        /// Adds a file based upon an out of the box template
        /// </summary>
        /// <param name="serverRelativePageName">Server relative url of the page to add</param>
        /// <param name="templateFileType">Type of template file to add</param>
        /// <returns>The added <see cref="IFile"/></returns>
        Task<IFile> AddTemplateFileAsync(string serverRelativePageName, TemplateFileType templateFileType);

        /// <summary>
        /// Adds a file based upon an out of the box template
        /// </summary>
        /// <param name="serverRelativePageName">Server relative url of the page to add</param>
        /// <param name="templateFileType">Type of template file to add</param>
        /// <returns>The added <see cref="IFile"/></returns>
        IFile AddTemplateFile(string serverRelativePageName, TemplateFileType templateFileType);
        #endregion
    }
}