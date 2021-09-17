using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Defines the basic interface for an asset persistence provider
    /// </summary>
    public interface IAssetPersistenceProvider
    {
        /// <summary>
        /// Writes the binary blob of an asset onto the persistence storage
        /// </summary>
        /// <param name="content">The blob content of the file</param>
        /// <param name="fileName">The name of the file to persist</param>
        /// <returns>The path of the persisted file</returns>
        Task<string> WriteAssetAsync(Stream content, string fileName);

        /// <summary>
        /// Reads the binary blob of an asset from the persistence storage
        /// </summary>
        /// <param name="fileName">The name of the persisted file</param>
        /// <returns>The blob content of the file</returns>
        Task<Stream> ReadAssetAsync(string fileName);
    }
}
