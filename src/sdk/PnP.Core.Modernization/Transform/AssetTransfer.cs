using Microsoft.SharePoint.Client;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Extensions;
using PnP.Core.Modernization.Telemetry;
using PnP.Framework.Utilities;
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
        private ClientContext _sourceClientContext;
        private ClientContext _targetClientContext;
        private bool inSameSite;
        private SPVersion _sourceContextSPVersion;

        /// <summary>
        /// Constructor for the asset transfer class
        /// </summary>
        /// <param name="source">Source connection to SharePoint</param>
        /// <param name="target">Target connection to SharePoint</param>
        /// <param name="logObservers"></param>
        public AssetTransfer(ClientContext source, ClientContext target, IList<ILogObserver> logObservers = null)
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
                    // Ensures the source context is set to the location of the asset file
                    EnsureAssetContextIfRequired(sourceAssetRelativeUrl);

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
                var sourceCCBaseUrl = _sourceClientContext.Url.GetBaseUrl();

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
            var siteAssetsLibrary = this.EnsureSiteAssetsLibrary();
            var sitePagesFolder = siteAssetsLibrary.RootFolder.EnsureFolder("SitePages");

            var friendlyFolder = ConvertFileToFolderFriendlyName(pageFileName);
            friendlyFolder = friendlyFolder.StripInvalidUrlChars();
            var pageFolder = sitePagesFolder.EnsureFolder(friendlyFolder);

            return pageFolder.EnsureProperty(o => o.ServerRelativeUrl);
        }

        /// <summary>
        /// Create a site assets library
        /// </summary>
        public List EnsureSiteAssetsLibrary()
        {
            // Use a PnP Provisioning template to create a site assets library
            // We cannot assume the SiteAssets library exists, in the case of vanilla communication sites - provision a new library if none exists
            // If a site assets library exist, add a folder, into the library using the same format as SharePoint uses for creating sub folders for pages

            //Ensure that the Site Assets library is created using the out of the box creation mechanism
            //Site Assets that are created using the EnsureSiteAssetsLibrary method slightly differ from
            //default Document Libraries. See issue 512 (https://github.com/SharePoint/PnP-Sites-Core/issues/512)
            //for details about the issue fixed by this approach.
            var createdList = this._targetClientContext.Web.Lists.EnsureSiteAssetsLibrary();
            //Check that Title and Description have the correct values
            this._targetClientContext.Web.Context.Load(createdList, l => l.Title, l => l.RootFolder);
            this._targetClientContext.Web.Context.ExecuteQueryRetry();

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
            // This copies the latest version of the asset to the target site collection
            // Going to need to add a bunch of checks to ensure the target file exists

            // Each sliced upload requires a unique ID.
            Guid uploadId = Guid.NewGuid();
            // Calculate block size in bytes.
            int blockSize = fileChunkSizeInMB * 1024 * 1024;
            bool fileOverwrite = true;

            Stream sourceStream = null;
            var sourceAssetFile = _sourceClientContext.Web.GetFileByServerRelativeUrl(sourceFileUrl);
            _sourceClientContext.Load(sourceAssetFile, s => s.Exists);
            _sourceClientContext.ExecuteQueryRetry();

            if (sourceAssetFile.Exists)
            {
                // Test ByPass
                //if 2010 then

                if (_sourceContextSPVersion == SPVersion.SP2010)
                {
                    throw new Exception("SharePoint 2010 is not supported");
                    //sourceStream = new MemoryStream();

                    //if (_sourceClientContext.HasPendingRequest)
                    //{
                    //    _sourceClientContext.ExecuteQueryRetry();
                    //}
                    //var fileBinary = File.OpenBinaryDirect(_sourceClientContext, sourceFileUrl);
                    //_sourceClientContext.ExecuteQueryRetry();
                    //Stream tempSourceStream = fileBinary.Stream;

                    //CopyStream(tempSourceStream, sourceStream);

                    ////Fix: https://stackoverflow.com/questions/47510815/sharepoint-uploadfile-specified-argument-was-out-of-range-of-valid-values
                    //sourceStream.Seek(0, SeekOrigin.Begin);

                }
                else
                {
                    // Get the file from SharePoint

                    ClientResult<System.IO.Stream> sourceAssetFileData = sourceAssetFile.OpenBinaryStream();
                    _sourceClientContext.Load(sourceAssetFile);
                    _sourceClientContext.ExecuteQueryRetry();
                    sourceStream = sourceAssetFileData.Value;

                }

                using (Stream sourceFileStream = sourceStream)
                {

                    string fileName = sourceAssetFile.EnsureProperty(p => p.Name);

                    LogInfo(string.Format(LogStrings.AssetTransferUploading, fileName), LogStrings.Heading_AssetTransfer);

                    // New File object.
                    Microsoft.SharePoint.Client.File uploadFile;

                    // Get the information about the folder that will hold the file.
                    // Add the file to the target site
                    Folder targetFolder = _targetClientContext.Web.GetFolderByServerRelativeUrl(targetLocationUrl);
                    _targetClientContext.Load(targetFolder);
                    _targetClientContext.ExecuteQueryRetry();

                    // Get the file size
                    long fileSize = sourceFileStream.Length;

                    // Process with two approaches
                    if (fileSize <= blockSize)
                    {

                        // Use regular approach.

                        FileCreationInformation fileInfo = new FileCreationInformation();
                        fileInfo.ContentStream = sourceFileStream;
                        fileInfo.Url = fileName;
                        fileInfo.Overwrite = fileOverwrite;

                        uploadFile = targetFolder.Files.Add(fileInfo);
                        _targetClientContext.Load(uploadFile);
                        _targetClientContext.ExecuteQueryRetry();

                        LogInfo(string.Format(LogStrings.AssetTransferUploadComplete, fileName), LogStrings.Heading_AssetTransfer);
                        // Return the file object for the uploaded file.
                        return uploadFile.EnsureProperty(o => o.ServerRelativeUrl);

                    }
                    else
                    {
                        // Use large file upload approach.
                        ClientResult<long> bytesUploaded = null;

                        using (BinaryReader br = new BinaryReader(sourceFileStream))
                        {
                            byte[] buffer = new byte[blockSize];
                            Byte[] lastBuffer = null;
                            long fileoffset = 0;
                            long totalBytesRead = 0;
                            int bytesRead;
                            bool first = true;
                            bool last = false;

                            // Read data from file system in blocks. 
                            while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                totalBytesRead = totalBytesRead + bytesRead;

                                // You've reached the end of the file.
                                if (totalBytesRead == fileSize)
                                {
                                    last = true;
                                    // Copy to a new buffer that has the correct size.
                                    lastBuffer = new byte[bytesRead];
                                    Array.Copy(buffer, 0, lastBuffer, 0, bytesRead);
                                }

                                if (first)
                                {
                                    using (MemoryStream contentStream = new MemoryStream())
                                    {
                                        // Add an empty file.
                                        FileCreationInformation fileInfo = new FileCreationInformation();
                                        fileInfo.ContentStream = contentStream;
                                        fileInfo.Url = fileName;
                                        fileInfo.Overwrite = fileOverwrite;
                                        uploadFile = targetFolder.Files.Add(fileInfo);

                                        // Start upload by uploading the first slice. 
                                        using (MemoryStream s = new MemoryStream(buffer))
                                        {
                                            // Call the start upload method on the first slice.
                                            bytesUploaded = uploadFile.StartUpload(uploadId, s);
                                            _targetClientContext.ExecuteQueryRetry();
                                            // fileoffset is the pointer where the next slice will be added.
                                            fileoffset = bytesUploaded.Value;
                                        }

                                        // You can only start the upload once.
                                        first = false;
                                    }
                                }
                                else
                                {
                                    // Get a reference to your file.
                                    var fileUrl = targetFolder.ServerRelativeUrl + System.IO.Path.AltDirectorySeparatorChar + fileName;
                                    uploadFile = _targetClientContext.Web.GetFileByServerRelativeUrl(fileUrl);

                                    if (last)
                                    {
                                        // Is this the last slice of data?
                                        using (MemoryStream s = new MemoryStream(lastBuffer))
                                        {
                                            // End sliced upload by calling FinishUpload.
                                            uploadFile = uploadFile.FinishUpload(uploadId, fileoffset, s);
                                            _targetClientContext.ExecuteQueryRetry();

                                            LogInfo(string.Format(LogStrings.AssetTransferUploadComplete, fileName), LogStrings.Heading_AssetTransfer);
                                            // Return the file object for the uploaded file.
                                            return fileUrl;
                                        }
                                    }
                                    else
                                    {
                                        using (MemoryStream s = new MemoryStream(buffer))
                                        {
                                            // Continue sliced upload.
                                            bytesUploaded = uploadFile.ContinueUpload(uploadId, fileoffset, s);
                                            _targetClientContext.ExecuteQueryRetry();
                                            // Update fileoffset for the next slice.
                                            fileoffset = bytesUploaded.Value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            LogWarning("Asset was not transferred as it was not found in the source web. Asset: " + sourceFileUrl, LogStrings.Heading_AssetTransfer);
            return null;
        }

        /// <summary>
        /// Stores an asset transfer reference
        /// </summary>
        /// <param name="assetTransferredEntity"></param>
        public void StoreAssetTransferred(AssetTransferredEntity assetTransferredEntity)
        {
            // Using the Cache Manager store the asset transfer references
            // If update - treat the source URL as unique, if multiple web parts reference to this, then it will still refer to the single resource
            var cache = Cache.CacheManager.Instance;
                    
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
                var cache = Cache.CacheManager.Instance;

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
        internal void EnsureAssetContextIfRequired(ClientContext context, string sourceUrl)
        {
            // There is two scenarios to check
            //  - If the asset resides on the root site collection
            //  - If the asset resides on another subsite
            //  - If the asset resides on a subsite below this context

            try
            {
                context.Site.EnsureProperties(o => o.ServerRelativeUrl, o => o.Url, o => o.RootWeb.Id);
                context.Web.EnsureProperties(o => o.ServerRelativeUrl, o => o.Id);

                string match = string.Empty;

                // Break the URL into segments and deteremine which URL detects the file in the structure.
                // Use Web IDs to validate content isnt the same on the root

                var fullSiteCollectionUrl = context.Site.Url;
                var relativeSiteCollUrl = context.Site.ServerRelativeUrl;
                var sourceCtxUrl = context.Web.GetUrl();

                // Lets break into segments
                var fileName = Path.GetFileName(sourceUrl);

                // Could already be relative
                //var sourceUrlWithOutBaseAddr = sourceUrl.Replace(fullSiteCollectionUrl, "").Replace(relativeSiteCollUrl,"");
                var urlSegments = sourceUrl.Split('/');

                // Need null tests
                var filteredUrlSegments = urlSegments.Where(o => !string.IsNullOrEmpty(o) && o != fileName).Reverse();

                //Assume the last segment is the filename
                //Assume the segment before the last is either a folder or library

                //Url to strip back until detected as subweb
                var remainingUrl = sourceUrl.Replace(fileName, ""); //remove file name

                //Urls to try to determine web
                foreach (var segment in filteredUrlSegments) //Assume the segment before the last is either a folder or library
                {
                    try
                    {
                        var testUrl = UrlUtility.Combine(fullSiteCollectionUrl.ToLower(), remainingUrl.ToLower().Replace(relativeSiteCollUrl.ToLower(), ""));

                        //No need to recurse this
                        var exists = context.WebExistsFullUrl(testUrl);

                        if (exists)
                        {
                            //winner
                            match = testUrl;
                            break;
                        }
                        else
                        {
                            remainingUrl = remainingUrl.TrimEnd('/').TrimEnd($"{segment}".ToCharArray());
                        }
                    }
                    catch
                    {
                        // Nope not the right web - Swallow
                    }
                }

                // Check if the asset is on the root site collection
                if (string.IsNullOrEmpty(match))
                {
                    // Does it contain a relative reference
                    if (sourceUrl.StartsWith("/") && !sourceUrl.ContainsIgnoringCasing(context.Web.GetUrl()))
                    {
                        match = fullSiteCollectionUrl.ToLower();
                    }
                }

                if (!string.IsNullOrEmpty(match) && !match.Equals(context.Web.GetUrl(), StringComparison.InvariantCultureIgnoreCase))
                {

                    _sourceClientContext = context.Clone(match);
                    LogDebug("Source Context Switched", "EsureAssetContextIfRequired");
                }
            }
            catch (Exception ex)
            {
                LogError(LogStrings.Error_CannotGetSiteCollContext, LogStrings.Heading_AssetTransfer, ex);
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
