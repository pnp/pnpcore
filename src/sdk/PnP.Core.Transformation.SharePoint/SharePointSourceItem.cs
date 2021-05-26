using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.SharePoint
{
    /// <summary>
    /// SharePoint implementation of <see cref="ISourceItem"/>
    /// </summary>
    public class SharePointSourceItem : ISourceItem
    {
        private readonly SharePointSourceItemId sourceItemId;

        /// <summary>
        /// Creates a new instance based on uri
        /// </summary>
        /// <param name="uri"></param>
        public SharePointSourceItem(Uri uri) : this(new SharePointSourceItemId(uri))
        {
            
        }

        /// <summary>
        /// Creates a new instance based on id
        /// </summary>
        /// <param name="sourceItemId"></param>
        public SharePointSourceItem(SharePointSourceItemId sourceItemId)
        {
            this.sourceItemId = sourceItemId ?? throw new ArgumentNullException(nameof(sourceItemId));
        }

        /// <summary>
        /// Gets the id of the source item
        /// </summary>
        public ISourceItemId Id => sourceItemId;
    }
}
