using System;
using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class CopyMigrationInfo : ICopyMigrationInfo
    {
        public byte[] EncryptionKey { get; set; }

        public Guid JobId { get; set; }

        public Uri JobQueueUri { get; set; }

        public IList<string> SourceListItemUniqueIds { get; set; }
    }
}
