using System.Collections.Generic;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options to define the file thumbnail retrieval
    /// </summary>
    public class ThumbnailOptions
    {
        /// <summary>
        /// Standard size(s) of the thumbnail to get
        /// </summary>
        public List<ThumbnailSize> StandardSizes { get; set; }

        /// <summary>
        /// Custom size(s) of the thumbnail to get
        /// </summary>
        public List<CustomThumbnailOptions> CustomSizes { get; set; }
    }
}
