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
        /// Creates a new instance based on a uri
        /// </summary>
        /// <param name="uri">The URI of the item</param>
        public SharePointSourceItem(Uri uri) : this(new SharePointSourceItemId(uri))
        {            
        }

        /// <summary>
        /// Creates a new instance based on an id
        /// </summary>
        /// <param name="sourceItemId">The ID of the item</param>
        public SharePointSourceItem(SharePointSourceItemId sourceItemId)
        {
            this.sourceItemId = sourceItemId ?? throw new ArgumentNullException(nameof(sourceItemId));
        }

        /// <summary>
        /// Gets the id of the source item
        /// </summary>
        ISourceItemId ISourceItem.Id => sourceItemId;

        /// <summary>
        /// Gets the id of the source item
        /// </summary>
        public SharePointSourceItemId Id => sourceItemId;
    }
}
