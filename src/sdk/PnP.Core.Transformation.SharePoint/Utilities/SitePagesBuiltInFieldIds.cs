using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.SharePoint.Utilities
{
    /// <summary>
    /// Defines a list of IDs for built in fields of Site Pages library
    /// </summary>
    public class SitePagesBuiltInFieldIds
    {
        /// <summary>
        /// Wiki Content
        /// </summary>
        public static readonly Guid WikiFieldID = new Guid("c33527b4-d920-4587-b791-45024d00068a");

        /// <summary>
        /// Authoring Canvas Content
        /// </summary>
        public static readonly Guid CanvasContent1ID = new Guid("4966388e-6e12-4bc6-8990-5b5b66153eae");

        /// <summary>
        /// Banner Image URL
        /// </summary>
        public static readonly Guid BannerImageUrlID = new Guid("5baf6db5-9d25-4738-b15e-db5789298e82");

        /// <summary>
        /// Description
        /// </summary>
        public static readonly Guid DescriptionID = new Guid("3f155110-a6a2-4d70-926c-94648101f0e8");

        /// <summary>
        /// Promoted State
        /// </summary>
        public static readonly Guid PromotedStateID = new Guid("f5ad16a2-85be-46b2-b5f0-2bb8b4a5074a");

        /// <summary>
        /// First Published Date
        /// </summary>
        public static readonly Guid FirstPublishedDateID = new Guid("c84f8697-331e-457d-884a-c4fb8f30ea74");

        /// <summary>
        /// Page Layout Content
        /// </summary>
        public static readonly Guid LayoutWebpartsContentID = new Guid("261075db-0525-4fb8-a6ea-772014186599");

        /// <summary>
        /// Author Byline
        /// </summary>
        public static readonly Guid AuthorBylineID = new Guid("1a7348e7-1bb7-4a47-9790-088e7cb20b58");

        /// <summary>
        /// Topic header
        /// </summary>
        public static readonly Guid TopicHeaderID = new Guid("d60d65ff-ff42-4044-a684-ac3f7a5e598c");

        /// <summary>
        /// Site Page Flags
        /// </summary>
        public static readonly Guid SPSitePageFlagsID = new Guid("9de685c5-fdf5-4319-b987-3edf55efb36f");

        /// <summary>
        /// Original Source Url
        /// </summary>
        public static readonly Guid OriginalSourceUrlID = new Guid("0e7b982f-698a-4d0c-aacb-f16906f66d30");

        /// <summary>
        /// Original Source Site ID
        /// </summary>
        public static readonly Guid OriginalSourceSiteIdID = new Guid("36193413-dd5c-4096-8c1e-1b40098b9ba3");

        /// <summary>
        /// Original Source Web ID
        /// </summary>
        public static readonly Guid OriginalSourceWebIdID = new Guid("3477a5bc-c605-4b2e-a7c1-8db8f13c017e");

        /// <summary>
        /// Original Source List ID
        /// </summary>
        public static readonly Guid OriginalSourceListIdID = new Guid("139da674-dbf6-439f-98e0-4eb05fa9a669");

        /// <summary>
        /// Original Source Item ID
        /// </summary>
        public static readonly Guid OriginalSourceItemIdID = new Guid("91e86a43-75f2-426f-80da-35edfb47d55d");


        // TODO: Define the real list of built in fields for the Site Pages library

        // Hashset to contain the whole list of built in fields
        private static HashSet<Guid> builtInFieldsHashSet;
        private static object builtInFieldsHashSetSyncLock = new object();

        /// <summary>
        /// This method returns a Boolean value that specifies whether or not the current object matches the specified GUID. This value is used as a file identifier for an object that is associated with a Windows SharePoint Services Web site.
        /// </summary>
        /// 
        /// <returns>
        /// Returns a GUID.
        /// </returns>
        /// <param name="fid">File identifier.</param>
        public static bool Contains(Guid fid)
        {
            if (builtInFieldsHashSet == null)
            {
                lock (builtInFieldsHashSetSyncLock)
                {
                    if (builtInFieldsHashSet == null)
                    {
                        builtInFieldsHashSet = new HashSet<Guid>(
                            new Guid[] {
                                WikiFieldID,
                                CanvasContent1ID,
                                BannerImageUrlID,
                                DescriptionID,
                                PromotedStateID,
                                FirstPublishedDateID,
                                LayoutWebpartsContentID,
                                AuthorBylineID,
                                TopicHeaderID,
                                SPSitePageFlagsID,
                                OriginalSourceUrlID,
                                OriginalSourceSiteIdID,
                                OriginalSourceWebIdID,
                                OriginalSourceListIdID,
                                OriginalSourceItemIdID
                            });
                    }
                }
            }

            if (builtInFieldsHashSet != null)
            {
                return builtInFieldsHashSet.Contains(fid);
            }
            else
            {
                return false;
            }
        }
    }
}
