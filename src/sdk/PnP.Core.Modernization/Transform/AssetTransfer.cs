using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;

using PnP.Core.Modernization.Telemetry;
using PnP.Core.Provisioning.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PnP.Core.Modernization.Transform
{
    /// <summary>
    /// Class for operations for transferring the assets over to the target site collection
    /// </summary>
    public class AssetTransfer : BaseTransform
    {
        private PnPContext _sourceClientContext;
        private PnPContext _targetClientContext;
        private bool inSameSite;
        private SPVersion _sourceContextSPVersion;

        /// <summary>
        /// Constructor for the asset transfer class
        /// </summary>
        /// <param name="source">Source connection to SharePoint</param>
        /// <param name="target">Target connection to SharePoint</param>
        /// <param name="logObservers"></param>
        public AssetTransfer(PnPContext source, PnPContext target, IList<ILogObserver> logObservers = null)
        {
            if (logObservers != null)
            {
                foreach (var observer in logObservers)
                {
                    base.RegisterObserver(observer);
                }
            }

            _sourceClientContext = source;
            _targetClientContext = target;

            // Check if we're calling asset transfer when source and target are the same
            var sourceUrl = _sourceClientContext?.Web.GetUrl();
            var targetUrl = _targetClientContext?.Web.GetUrl();
            inSameSite = sourceUrl.Equals(targetUrl, StringComparison.InvariantCultureIgnoreCase);
            _sourceContextSPVersion = GetVersion(_sourceClientContext);

            Validate(); // Perform validation
        }

        /// <summary>
        /// Perform validation
        /// </summary>
        public void Validate()
        {
            if (_sourceClientContext == null || _targetClientContext == null)
            {
                LogError(LogStrings.Error_AssetTransferClientContextNull, LogStrings.Heading_AssetTransfer);
                throw new ArgumentNullException(LogStrings.Error_AssetTransferClientContextNull);
            }
        }

        /// <summary>
        /// Main entry point to perform the series of operations to transfer related assets
        /// </summary>
        public string TransferAsset(string sourceAssetRelativeUrl, string pageFileName)
        {
            // No point in looking any further as we're not going cross site
            if (inSameSite || string.IsNullOrEmpty(sourceAssetRelativeUrl))
            {
                return sourceAssetRelativeUrl;
            }

            // Deep validation of urls
            var isValid = ValidateAssetInSupportedLocation(sourceAssetRelativeUrl) && !string.IsNullOrEmpty(pageFileName);

            // Check the string is not null
            if (!string.IsNullOrEmpty(sourceAssetRelativeUrl) && isValid)
            {

                // Are we dealing with an _layouts image?
                if (sourceAssetRelativeUrl.ContainsIgnoringCasing("_layouts/", StringComparison.InvariantCultureIgnoreCase))
                {
                    // _layouts based images automatically exist on the target site, so let's build a server relative url for the _layout image on the target site
                    var targetRelativeSiteUrl = this._targetClientContext.Web.EnsureProperty(p => p.ServerRelativeUrl);

                    return $"{targetRelativeSiteUrl}/{sourceAssetRelativeUrl.Substring(sourceAssetRelativeUrl.IndexOf("_layouts/", StringComparison.InvariantCultureIgnoreCase))}";
                }

                // Check the target library exists
                string targetFolderServerRelativeUrl = EnsureDestination(pageFileName);
                // Read in a preferred location

                // Check that the operation to transfer an asset hasnt already been performed for the file on different web parts.
                var assetDetails = GetAssetTransferredIfExists(
                    new AssetTransferredEntity() { SourceAssetUrl = sourceAssetRelativeUrl, TargetAssetFolderUrl = targetFolderServerRelativeUrl });

                if (string.IsNullOrEmpty(assetDetails.TargetAssetTransferredUrl))
                {
                    // Copy the asset file
                    string newLocationUrl = CopyAssetToTargetLocation(sourceAssetRelativeUrl, targetFolderServerRelativeUrl);
                    assetDetails.TargetAssetTransferredUrl = newLocationUrl;

                    // Store a reference in the cache manager - ensure a test exists with multiple identical web parts
                    StoreAssetTransferred(assetDetails);

                }

                var finalPath = assetDetails.TargetAssetTransferredUrl;
                LogInfo($"{finalPath}", LogStrings.Heading_Summary, LogEntrySignificance.AssetTransferred);
                return finalPath;

            }

            // Fall back to send back the same link
            LogWarning($"{LogStrings.AssetTransferFailedFallback} {sourceAssetRelativeUrl}", LogStrings.Heading_AssetTransfer);
            return sourceAssetRelativeUrl;
        }


        /// <summary>
        /// Checks if the URL is located in a supported location
        /// </summary>
        public bool ValidateAssetInSupportedLocation(string sourceUrl)
        {
            //  Referenced assets should only be files e.g. 
            //      not aspx pages 
            //      located in the pages, site pages libraries

            var fileExtension = Path.GetExtension(sourceUrl).ToLower();

            // Check block list
            var containsBlockedExtension = Constants.BlockedAssetFileExtensions.Any(o => o == fileExtension.Replace(".", ""));
            if (containsBlockedExtension)
            {
                return false;
            }

            // Check allow list
            var containsAllowedExtension = Constants.AllowedAssetFileExtensions.Any(o => o == fileExtension.Replace(".", ""));
            if (!containsAllowedExtension)
            {
                return false;
            }

            // Additional check to see if image is outside SharePoint for OnPrem to Online scenario for root site and subsites in root site collection
            if (sourceUrl.ContainsIgnoringCasing("https://") || sourceUrl.ContainsIgnoringCasing("http://"))
            {
                var sourceBaseUrl = sourceUrl.GetBaseUrl();
                var sourceCCBaseUrl = _sourceClientContext.Uri.AbsoluteUri.GetBaseUrl();

                if (!sourceBaseUrl.Equals(sourceCCBaseUrl, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }

            //  Ensure the referenced assets exist within the source site collection
            var sourceSiteContextUrl = _sourceClientContext.Site.EnsureProperty(w => w.ServerRelativeUrl);

            // SourceUrl does contain encoded characters (e.g. %20 for space), while sourceSiteContextUrl does not
            if (!sourceUrl.ContainsIgnoringCasing(sourceSiteContextUrl.Replace(" ", "%20")))
            {
                return false;
            }

            //  Ensure the contexts are not e.g. cross-site the same site collection/web according to the level of transformation
            var targetSiteContextUrl = _targetClientContext.Site.EnsureProperty(w => w.ServerRelativeUrl);
            if (sourceSiteContextUrl == targetSiteContextUrl)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensure the site assets and page sub-folder exists in the target location
        /// </summary>
        public string EnsureDestination(string pageFileName)
        {
            // In this method we need to calculate the target location from the following factors
            //  Target Site Context + Site Assets Library + Folder (if located in or calculate based on SP method)
            //  Check the libary and folder exists in the target site collection
            //  Currently this method ignores anything from the source, will probabily need an override or params for target location

            // Ensure the Site Assets library exists
            IList siteAssetsLibrary = this.EnsureSiteAssetsLibrary();
            IFolder sitePagesFolder = siteAssetsLibrary.RootFolder.EnsureFolder("SitePages");

            var friendlyFolder = ConvertFileToFolderFriendlyName(pageFileName);
            friendlyFolder = friendlyFolder.StripInvalidUrlChars();
            IFolder pageFolder = sitePagesFolder.EnsureFolder(friendlyFolder);

            return pageFolder.EnsureProperty(o => o.ServerRelativeUrl);
        }

        /// <summary>
        /// Create a site assets library
        /// </summary>
        public IList EnsureSiteAssetsLibrary()
        {
            // Use a PnP Provisioning template to create a site assets library
            // We cannot assume the SiteAssets library exists, in the case of vanilla communication sites - provision a new library if none exists
            // If a site assets library exist, add a folder, into the library using the same format as SharePoint uses for creating sub folders for pages

            //Ensure that the Site Assets library is created using the out of the box creation mechanism
            //Site Assets that are created using the EnsureSiteAssetsLibrary method slightly differ from
            //default Document Libraries. See issue 512 (https://github.com/SharePoint/PnP-Sites-Core/issues/512)
            //for details about the issue fixed by this approach.
            IList createdList = this._targetClientContext.Web.Lists.EnsureSiteAssetsLibrary();
            //Check that Title and Description have the correct values
            createdList.LoadBatch(l => l.Title,l => l.RootFolder);
            this._targetClientContext.Web.PnPContext.Execute();

            return createdList;
        }

        /// <summary>
        /// Copy the file from the source to the target location
        /// </summary>
        /// <param name="sourceFileUrl"></param>
        /// <param name="targetLocationUrl"></param>
        /// <param name="fileChunkSizeInMB">Size of chunks in MB in which the file will be split to be copied</param>
        /// <remarks>
        ///     Based on the documentation: https://docs.microsoft.com/en-us/sharepoint/dev/solution-guidance/upload-large-files-sample-app-for-sharepoint
        /// </remarks>
        public string CopyAssetToTargetLocation(string sourceFileUrl, string targetLocationUrl, int fileChunkSizeInMB = 3)
        {
            if (fileChunkSizeInMB <= 0) throw new ArgumentOutOfRangeException(nameof(fileChunkSizeInMB));
            IFile sourceAssetFile;
            try
            {
                EnsureAssetContextIfRequired(sourceFileUrl);
                sourceAssetFile = _sourceClientContext.Web.GetFileByServerRelativeUrl(sourceFileUrl);
            }
            catch (ServiceException ex) when (ex.IsNotFound())
            {
                LogWarning("Asset was not transferred as it was not found in the source web. Asset: " + sourceFileUrl, LogStrings.Heading_AssetTransfer);
                return null;
            }

            var fileName = sourceAssetFile.EnsureProperty(p => p.Name);
            LogInfo(string.Format(LogStrings.AssetTransferUploading, fileName), LogStrings.Heading_AssetTransfer);
            var targetFolder = _targetClientContext.Web.GetFolderByServerRelativeUrl(targetLocationUrl);
            using (var stream = sourceAssetFile.GetContent())
            {
                if (stream.Length != sourceAssetFile.EnsureProperty(file => file.Length))
                {
                    throw new InvalidOperationException($"Downloaded asset length does not match the source file: {sourceFileUrl}.");
                }
                // Core automatically uses chunked uploads for large streams, with its configured retry policy.
                var uploaded = targetFolder.Files.Add(fileName, stream, overwrite: true);
                LogInfo(string.Format(LogStrings.AssetTransferUploadComplete, fileName), LogStrings.Heading_AssetTransfer);
                return uploaded.EnsureProperty(f => f.ServerRelativeUrl);
            }
        }

        /// <summary>
        /// Stores an asset transfer reference
        /// </summary>
        /// <param name="assetTransferredEntity"></param>
        public void StoreAssetTransferred(AssetTransferredEntity assetTransferredEntity)
        {
            // Using the Cache Manager store the asset transfer references
            // If update - treat the source URL as unique, if multiple web parts reference to this, then it will still refer to the single resource
            var cache = CacheManager.Instance;
                    
            if (!cache.GetAssetsTransferred().Any(asset =>
                 string.Equals(asset.TargetAssetTransferredUrl, assetTransferredEntity.TargetAssetFolderUrl, StringComparison.InvariantCultureIgnoreCase)))
            {
                cache.AddAssetTransferredEntity(assetTransferredEntity);
            }

        }

        /// <summary>
        /// Get asset transfer details if they already exist
        /// </summary>
        public AssetTransferredEntity GetAssetTransferredIfExists(AssetTransferredEntity assetTransferredEntity)
        {
            try
            {
                // Using the Cache Manager retrieve asset transfer references (all)
                var cache = CacheManager.Instance;

                var result = cache.GetAssetsTransferred().SingleOrDefault(
                    asset => string.Equals(asset.TargetAssetFolderUrl, assetTransferredEntity.TargetAssetFolderUrl, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(asset.SourceAssetUrl, assetTransferredEntity.SourceAssetUrl, StringComparison.InvariantCultureIgnoreCase));

                // Return the cached details if found, if not return original search 
                return result != default(AssetTransferredEntity) ? result : assetTransferredEntity;
            }
            catch (Exception ex)
            {
                LogError(LogStrings.Error_AssetTransferCheckingIfAssetExists, LogStrings.Heading_AssetTransfer, ex);
            }

            // Fallback in case of error - this will trigger a transfer of the asset
            return assetTransferredEntity;

        }

        /// <summary>
        /// Converts the file name into a friendly format
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string ConvertFileToFolderFriendlyName(string fileName)
        {
            // replace invalid path chars with an _
            fileName = ReplaceInvalidPathChars(fileName);

            var justFileName = Path.GetFileNameWithoutExtension(fileName);
            var friendlyName = justFileName.Replace(" ", "-");
            return friendlyName;
        }

        internal static string ReplaceInvalidPathChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidPathChars()));
        }

        /// <summary>
        /// Ensures that we have context of the source site collection
        /// </summary>
        internal void EnsureAssetContextIfRequired(string sourceUrl)
        {
            EnsureAssetContextIfRequired(_sourceClientContext, sourceUrl);
        }


        /// <summary>
        /// Ensures that we have context of the source site collection
        /// </summary>
        /// <param name="context">Source site context</param>
        /// <param name="sourceUrl"></param>
        internal void EnsureAssetContextIfRequired(PnPContext context, string sourceUrl)
        {
            var fileUrl = new Uri(new Uri(context.Uri.AbsoluteUri.TrimEnd('/') + "/"), sourceUrl);
            // File metadata can be read across webs, but Core's binary download must use the owning web.
            var webUrl = SharePointRest.GetWebUrlFromPageUrl(context, fileUrl);
            if (string.IsNullOrWhiteSpace(webUrl))
            {
                throw new InvalidOperationException($"The SharePoint web for asset '{sourceUrl}' could not be resolved.");
            }
            if (!webUrl.TrimEnd('/').Equals(context.Uri.AbsoluteUri.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
            {
                _sourceClientContext = context.Clone(new Uri(webUrl));
                LogDebug("Source Context Switched", "EnsureAssetContextIfRequired");
            }
        }

        #region Helper methods
        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }
        #endregion
    }
}
