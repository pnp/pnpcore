using System;
using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents the migration information after creating a migration job
    /// </summary>
    public interface ICopyMigrationInfo
    {
        /// <summary>
        /// AES256CBC encryption key used to decrypt messages from job/manifest queue
        /// </summary>
        public byte[] EncryptionKey { get; }
        
        /// <summary>
        /// Return a unique Job ID associated with this asynchronous read
        /// </summary>
        public Guid JobId { get; }
        
        /// <summary>
        /// URL for accessing Azure queue used for returning notification of copy and move process
        /// </summary>
        public Uri JobQueueUri { get; }
        
        /// <summary>
        /// Return the source
        /// </summary>
        public IList<string> SourceListItemUniqueIds { get; }
    }
}
