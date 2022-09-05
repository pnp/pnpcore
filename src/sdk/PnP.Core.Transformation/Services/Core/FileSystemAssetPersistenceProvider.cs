using Microsoft.Extensions.Options;
using PnP.Core.Transformation.Services.Builder.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Implements the basic functionality of an asset persistence provider using the local file system
    /// </summary>
    public class FileSystemAssetPersistenceProvider : IAssetPersistenceProvider
    {
        private readonly IOptions<PnPTransformationOptions> pnpTransformationOptions;

        /// <summary>
        /// Public constructor for dependency injection
        /// </summary>
        /// <param name="pnpTransformationOptions">PnP Transformation options to customize the behavior of the persistence provider</param>
        public FileSystemAssetPersistenceProvider(IOptions<PnPTransformationOptions> pnpTransformationOptions)
        {
            this.pnpTransformationOptions = pnpTransformationOptions ?? throw new ArgumentNullException(nameof(pnpTransformationOptions));
        }

        /// <summary>
        /// Writes the binary blob of an asset onto the file system
        /// </summary>
        /// <param name="content">The blob content of the file</param>
        /// <param name="fileName">The name of the file to persist</param>
        /// <returns>The path of the persisted file</returns>
        public async Task<string> WriteAssetAsync(Stream content, string fileName)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (!content.CanSeek)
            {
                throw new Exception(TransformationResources.Error_InvalidAssetStreamToPersist);
            }

            // Determine the destination path for the file
            var path = Path.Combine(this.pnpTransformationOptions.Value.PersistenceProviderConnectionString, fileName);

            // Simply copy the blob stream into a file on the file system
            using (var fs = File.Create(path))
            {
                content.Seek(0, SeekOrigin.Begin);
                await content.CopyToAsync(fs).ConfigureAwait(false);
            }

            // Return the actual file name
            return path;
        }

        /// <summary>
        /// Reads the binary blob of an asset from the file system
        /// </summary>
        /// <param name="fileName">The name of the persisted file</param>
        /// <returns>The blob content of the file</returns>
        public async Task<Stream> ReadAssetAsync(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            // Prepare an empty result
            var result = new MemoryStream();

            // Determine the source path for the file
            var path = Path.Combine(this.pnpTransformationOptions.Value.PersistenceProviderConnectionString, fileName);

            // Simply read the file from the file system
            using (var fs = File.OpenRead(path))
            {
                // And copy its binary content into an in-memory stream
                await fs.CopyToAsync(result).ConfigureAwait(false);
                result.Position = 0;
            }

            return result;
        }
    }
}
