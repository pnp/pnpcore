using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ProvisioningUrlUtility = PnP.Core.Provisioning.Utilities.UrlUtility;

namespace PnP.Core.Provisioning.Connectors
{
    /// <summary>
    /// Connector for files stored in SharePoint.
    /// </summary>
    public class SharePointConnector : FileConnectorBase
    {
        #region public variables

        /// <summary>
        /// Key under which the <see cref="PnPContext"/> is held in <see cref="FileConnectorBase.Parameters"/>.
        /// </summary>
        public const string CLIENTCONTEXT = "ClientContext";

        #endregion

        #region Constructors

        /// <summary>
        /// Base constructor
        /// </summary>
        public SharePointConnector()
            : base()
        {
        }

        /// <summary>
        /// SharePointConnector constructor. Allows to directly set root folder and sub folder
        /// </summary>
        /// <param name="context">PnP Core context for the SharePoint connection</param>
        /// <param name="connectionString">Site collection URL (e.g. https://yourtenant.sharepoint.com/sites/dev)</param>
        /// <param name="container">Library + folder that holds the files (mydocs/myfolder)</param>
        public SharePointConnector(PnPContext context, string connectionString, string container)
            : base()
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(nameof(connectionString));
            }

            if (string.IsNullOrEmpty(container))
            {
                throw new ArgumentException(nameof(container));
            }

            container = container.Replace('\\', '/');

            this.AddParameter(CLIENTCONTEXT, context);
            this.AddParameterAsString(CONNECTIONSTRING, connectionString);
            this.AddParameterAsString(CONTAINER, container);
        }

        #endregion

        #region Base class overrides

        /// <summary>
        /// Get the files available in the default container
        /// </summary>
        /// <returns>List of files</returns>
        public override List<string> GetFiles()
        {
            return GetFiles(GetContainer());
        }

        /// <summary>
        /// Get the files available in the specified container
        /// </summary>
        /// <param name="container">Name of the container to get the files from</param>
        /// <returns>List of files</returns>
        public override List<string> GetFiles(string container)
        {
            if (string.IsNullOrEmpty(container))
            {
                throw new ArgumentException(nameof(container));
            }

            return GetFilesAsync(container.Replace('\\', '/')).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get the folders of the default container
        /// </summary>
        /// <returns>List of folders</returns>
        public override List<string> GetFolders()
        {
            return GetFolders(GetContainer());
        }

        /// <summary>
        /// Get the folders of a specified container
        /// </summary>
        /// <param name="container">Name of the container to get the folders from</param>
        /// <returns>List of folders</returns>
        public override List<string> GetFolders(string container)
        {
            if (string.IsNullOrEmpty(container))
            {
                throw new ArgumentException(nameof(container));
            }

            return GetFoldersAsync(container.Replace('\\', '/')).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a file as string from the default container
        /// </summary>
        /// <param name="fileName">Name of the file to get</param>
        /// <returns>String containing the file contents</returns>
        public override string GetFile(string fileName)
        {
            return GetFile(fileName, GetContainer());
        }

        /// <summary>
        /// Gets a file as string from the specified container
        /// </summary>
        /// <param name="fileName">Name of the file to get</param>
        /// <param name="container">Name of the container to get the file from</param>
        /// <returns>String containing the file contents</returns>
        public override string GetFile(string fileName, string container)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(nameof(fileName));
            }

            if (container != null)
            {
                container = container.Replace('\\', '/');
            }

            using (MemoryStream stream = GetFileFromStorageAsync(fileName, container).GetAwaiter().GetResult())
            {
                if (stream == null)
                {
                    return null;
                }

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// Gets a file as stream from the default container
        /// </summary>
        /// <param name="fileName">Name of the file to get</param>
        /// <returns>Stream containing the file contents</returns>
        public override Stream GetFileStream(string fileName)
        {
            return GetFileStream(fileName, GetContainer());
        }

        /// <summary>
        /// Gets a file as stream from the specified container
        /// </summary>
        /// <param name="fileName">Name of the file to get</param>
        /// <param name="container">Name of the container to get the file from</param>
        /// <returns>Stream containing the file contents</returns>
        public override Stream GetFileStream(string fileName, string container)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(nameof(fileName));
            }

            if (container != null)
            {
                container = container.Replace('\\', '/');
            }

            return GetFileFromStorageAsync(fileName, container).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Saves a stream to the default container with the given name. If the file exists it will be overwritten
        /// </summary>
        /// <param name="fileName">Name of the file to save</param>
        /// <param name="stream">Stream containing the file contents</param>
        public override void SaveFileStream(string fileName, Stream stream)
        {
            SaveFileStream(fileName, GetContainer(), stream);
        }

        /// <summary>
        /// Saves a stream to the specified container with the given name. If the file exists it will be overwritten
        /// </summary>
        /// <param name="fileName">Name of the file to save</param>
        /// <param name="container">Name of the container to save the file to</param>
        /// <param name="stream">Stream containing the file contents</param>
        public override void SaveFileStream(string fileName, string container, Stream stream)
        {
            if (container != null)
            {
                container = container.Replace('\\', '/');
            }

            SaveFileStreamAsync(fileName, container, stream).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Deletes a file from the default container
        /// </summary>
        /// <param name="fileName">Name of the file to delete</param>
        public override void DeleteFile(string fileName)
        {
            DeleteFile(fileName, GetContainer());
        }

        /// <summary>
        /// Deletes a file from the specified container
        /// </summary>
        /// <param name="fileName">Name of the file to delete</param>
        /// <param name="container">Name of the container to delete the file from</param>
        public override void DeleteFile(string fileName, string container)
        {
            if (container != null)
            {
                container = container.Replace('\\', '/');
            }

            DeleteFileAsync(fileName, container).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Returns a filename without a path
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <returns>Returns a filename without a path</returns>
        public override string GetFilenamePart(string fileName)
        {
            return Path.GetFileName(fileName);
        }

        #endregion

        #region Private methods

        private async Task<List<string>> GetFilesAsync(string container)
        {
            List<string> result = new List<string>();

            IFolder folder = await GetContainerFolderAsync(container, false).ConfigureAwait(false);
            if (folder == null)
            {
                return result;
            }

            await folder.LoadAsync(f => f.Files).ConfigureAwait(false);
            foreach (IFile file in folder.Files.AsRequested())
            {
                result.Add(file.Name);
            }

            return result;
        }

        private async Task<List<string>> GetFoldersAsync(string container)
        {
            List<string> result = new List<string>();

            IFolder folder = await GetContainerFolderAsync(container, false).ConfigureAwait(false);
            if (folder == null)
            {
                return result;
            }

            await folder.LoadAsync(f => f.Folders).ConfigureAwait(false);
            foreach (IFolder subFolder in folder.Folders.AsRequested())
            {
                result.Add(subFolder.Name);
            }

            return result;
        }

        private async Task<MemoryStream> GetFileFromStorageAsync(string fileName, string container)
        {
            try
            {
                PnPContext context = GetContext();
                string fileServerRelativeUrl = await GetFileServerRelativeUrlAsync(fileName, container).ConfigureAwait(false);

                IFile file = await context.Web.GetFileByServerRelativeUrlOrDefaultAsync(fileServerRelativeUrl).ConfigureAwait(false);
                if (file == null)
                {
                    return null;
                }

                MemoryStream stream = new MemoryStream();
                using (Stream content = await file.GetContentAsync().ConfigureAwait(false))
                {
                    await content.CopyToAsync(stream).ConfigureAwait(false);
                }

                stream.Position = 0;
                return stream;
            }
            catch
            {
                return null;
            }
        }

        private async Task SaveFileStreamAsync(string fileName, string container, Stream stream)
        {
            try
            {
                IFolder folder = await GetContainerFolderAsync(container, true).ConfigureAwait(false);

                await folder.Files.AddAsync(fileName, stream, true).ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
        }

        private async Task DeleteFileAsync(string fileName, string container)
        {
            try
            {
                PnPContext context = GetContext();
                string fileServerRelativeUrl = await GetFileServerRelativeUrlAsync(fileName, container).ConfigureAwait(false);

                IFile file = await context.Web.GetFileByServerRelativeUrlOrDefaultAsync(fileServerRelativeUrl).ConfigureAwait(false);
                if (file != null)
                {
                    await file.DeleteAsync().ConfigureAwait(false);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Resolves the folder a container points at.
        /// </summary>
        /// <param name="container">Container in "library/folder" form, or null/empty for the web root folder</param>
        /// <param name="ensure">When true any missing sub folders are created</param>
        private async Task<IFolder> GetContainerFolderAsync(string container, bool ensure)
        {
            PnPContext context = GetContext();

            if (string.IsNullOrEmpty(container))
            {
                return await context.Web.GetFolderByServerRelativeUrlAsync(await GetWebServerRelativeUrlAsync().ConfigureAwait(false)).ConfigureAwait(false);
            }

            string webUrl = await GetWebServerRelativeUrlAsync().ConfigureAwait(false);
            string folderUrl = ProvisioningUrlUtility.Combine(webUrl, container);

            if (!ensure)
            {
                return await context.Web.GetFolderByServerRelativeUrlAsync(folderUrl).ConfigureAwait(false);
            }

            string library = GetDocumentLibrary(container);
            string subFolders = GetUrlFolders(container).TrimStart('/');

            IFolder libraryFolder = await context.Web.GetFolderByServerRelativeUrlAsync(ProvisioningUrlUtility.Combine(webUrl, library)).ConfigureAwait(false);

            if (string.IsNullOrEmpty(subFolders))
            {
                return libraryFolder;
            }

            return await libraryFolder.EnsureFolderAsync(subFolders).ConfigureAwait(false);
        }

        private async Task<string> GetFileServerRelativeUrlAsync(string fileName, string container)
        {
            string webUrl = await GetWebServerRelativeUrlAsync().ConfigureAwait(false);

            if (string.IsNullOrEmpty(container))
            {
                return ProvisioningUrlUtility.Combine(webUrl, fileName);
            }

            return ProvisioningUrlUtility.Combine(ProvisioningUrlUtility.Combine(webUrl, container), fileName);
        }

        private async Task<string> GetWebServerRelativeUrlAsync()
        {
            PnPContext context = GetContext();
            await context.Web.EnsurePropertiesAsync(w => w.ServerRelativeUrl).ConfigureAwait(false);
            return context.Web.ServerRelativeUrl;
        }

        private static string GetDocumentLibrary(string container)
        {
            string[] parts = container.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 1)
            {
                if (parts[0].Equals("_catalogs", StringComparison.InvariantCultureIgnoreCase))
                {
                    return $"_catalogs/{parts[1]}";
                }
            }

            return parts[0];
        }

        private static string GetUrlFolders(string container)
        {
            string[] parts = container.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 1)
            {
                int startFrom = 1;
                if (parts[0].Equals("_catalogs", StringComparison.InvariantCultureIgnoreCase))
                {
                    startFrom = 2;
                }

                string folder = "";
                for (int i = startFrom; i < parts.Length; i++)
                {
                    folder = folder + "/" + parts[i];
                }

                return folder;
            }

            return "";
        }

        private PnPContext GetContext()
        {
            if (this.Parameters.ContainsKey(CLIENTCONTEXT))
            {
                return this.Parameters[CLIENTCONTEXT] as PnPContext;
            }

            throw new InvalidOperationException("No PnPContext specified");
        }

        #endregion

        internal override string GetContainer()
        {
            if (this.Parameters.ContainsKey(CONTAINER))
            {
                return this.Parameters[CONTAINER].ToString();
            }

            return string.Empty;
        }
    }
}
