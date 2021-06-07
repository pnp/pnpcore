using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Services;
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
        /// <param name="sourceContext">The source context</param>
        public SharePointSourceItem(Uri uri, PnPContext sourceContext) : this(new SharePointSourceItemId(uri), sourceContext)
        {            
        }

        /// <summary>
        /// Creates a new instance based on an id
        /// </summary>
        /// <param name="sourceItemId">The ID of the item</param>
        /// <param name="sourceContext">The source context</param>
        public SharePointSourceItem(SharePointSourceItemId sourceItemId, PnPContext sourceContext)
        {
            SourceContext = sourceContext;
            this.sourceItemId = sourceItemId ?? throw new ArgumentNullException(nameof(sourceItemId));
        }

        /// <summary>
        /// Gets the source context
        /// </summary>
        public PnPContext SourceContext { get; }

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
