using PnP.Core.Services;
using PnP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class HeaderOptions : IHeaderOptions
    {
        internal PnPContext PnPContext { get; set; }

        internal HeaderOptions(PnPContext pnpContext)
        {
            PnPContext = pnpContext;
        }

        public HeaderLayoutType Layout { get; set; }

        public VariantThemeType Emphasis { get; set; }

        public bool HideTitle { get; set; }

        public LogoAlignment LogoAlignment { get; set; }

        public async Task SetSiteLogoAsync(string fileName, Stream content, bool overwrite = false)
        {
            if (PnPContext.Site.GroupId == Guid.Empty)
            {
                // Upload the image
                IFile siteLogo = await UploadImageToSiteAssetsAsync(fileName, content, overwrite).ConfigureAwait(false);
                // Set the uploaded file as site logo
                await (PnPContext.Web as Web).RawRequestAsync(BuildSetSiteLogoApiCall(siteLogo.ServerRelativeUrl, SiteLogoType.WebLogo, SiteLogoAspect.Rectangular), HttpMethod.Post, "SetSiteLogo").ConfigureAwait(false);
            }
            else
            {
                await SetSiteLogoThumbnailAsync(fileName, content, overwrite).ConfigureAwait(false);
            }
        }

        public void SetSiteLogo(string fileName, Stream content, bool overwrite = false)
        {
            SetSiteLogoAsync(fileName, content, overwrite).GetAwaiter().GetResult();
        }

        public async Task ResetSiteLogoAsync()
        {
            if (PnPContext.Site.GroupId == Guid.Empty)
            {
                await (PnPContext.Web as Web).RawRequestAsync(BuildSetSiteLogoApiCall("", SiteLogoType.WebLogo, SiteLogoAspect.Rectangular), HttpMethod.Post, "SetSiteLogo").ConfigureAwait(false);
            }
            else
            {
                await ResetSiteLogoThumbnailAsync().ConfigureAwait(false);
            }
        }

        public void ResetSiteLogo()
        {
            ResetSiteLogoAsync().GetAwaiter().GetResult();
        }

        public async Task SetSiteLogoThumbnailAsync(string fileName, Stream content, bool overwrite = false)
        {
            if (PnPContext.Site.GroupId == Guid.Empty)
            {
                // Upload the image
                IFile siteLogo = await UploadImageToSiteAssetsAsync(fileName, content, overwrite).ConfigureAwait(false);
                // Set the uploaded file as site logo
                await (PnPContext.Web as Web).RawRequestAsync(BuildSetSiteLogoApiCall(siteLogo.ServerRelativeUrl, SiteLogoType.WebLogo, SiteLogoAspect.Square), HttpMethod.Post, "SetSiteLogo").ConfigureAwait(false);
            }
            else
            {
                var byteContent = new ByteArrayContent(ToByteArray(content));

                if (MimeTypeMap.TryGetMimeType(fileName, out string mimeType))
                {
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                }
                else
                {
                    throw new ClientException(ErrorType.Unsupported,
                        PnPCoreResources.Exception_Unsupported_NotAnImageMimeType);
                }

                var apiCall = new ApiCall($"groups/{PnPContext.Site.GroupId}/photo/$value", ApiType.Graph)
                {
                    Interactive = true, Content = byteContent
                };

                // Upload the image and set it as group logo
                await (PnPContext.Web as Web).RawRequestAsync(apiCall, HttpMethod.Put).ConfigureAwait(false);

                // get the site url and current used site logo url to check if it is set correctly
                await PnPContext.Web.EnsurePropertiesAsync(x => x.SiteLogoUrl, x => x.Url).ConfigureAwait(false);

                var correctSiteLogoUrl =
                    $"{PnPContext.Web.Url.PathAndQuery}/_api/GroupService/GetGroupImage?id='{PnPContext.Site.GroupId}'";

                // Use StartSWith to avoid issues with the query string that can contains &hash=xxxx
                if (string.IsNullOrEmpty(PnPContext.Web.SiteLogoUrl) ||
                    !PnPContext.Web.SiteLogoUrl.StartsWith(correctSiteLogoUrl))
                {
                    PnPContext.Web.SiteLogoUrl = correctSiteLogoUrl;
                    await PnPContext.Web.UpdateAsync().ConfigureAwait(false);

                    const string cachedSiteIcon = "__siteIcon__.png";

                    try
                    {
                        // check if we have a file named "__siteIcon__.jpg" in the SiteAssets folder
                        var file = await PnPContext.Web
                            .GetFileByServerRelativeUrlAsync(
                                $"{PnPContext.Uri.AbsolutePath}/siteassets/{cachedSiteIcon}")
                            .ConfigureAwait(false);

                        // delete the cached file to ensure the new logo is used https://learn.microsoft.com/en-us/sharepoint/troubleshoot/sites/error-when-changing-o365-site-logo
                        await file.DeleteAsync().ConfigureAwait(false);
                    }
                    catch (SharePointRestServiceException ex)
                    {
                        var error = ex.Error as SharePointRestError;

                        // If the exception indicated a non existing file then ignore, else throw
                        if (!File.ErrorIndicatesFileDoesNotExists(error))
                        {
                            throw;
                        }
                    }
                }
            }
        }

        public void SetSiteLogoThumbnail(string fileName, Stream content, bool overwrite = false)
        {
            SetSiteLogoThumbnailAsync(fileName, content, overwrite).GetAwaiter().GetResult();
        }

        public async Task ResetSiteLogoThumbnailAsync()
        {
            if (PnPContext.Site.GroupId == Guid.Empty)
            {
                await (PnPContext.Web as Web).RawRequestAsync(BuildSetSiteLogoApiCall("", SiteLogoType.WebLogo, SiteLogoAspect.Square), HttpMethod.Post, "SetSiteLogo").ConfigureAwait(false);
            }
            else
            {
                try
                {
                    string fileName = "__siteIcon__.jpg";
                    // check if we have a file named "__siteIcon__.jpg" in the SiteAssets folder
                    var file = await PnPContext.Web.GetFileByServerRelativeUrlAsync($"{PnPContext.Uri.AbsolutePath}/siteassets/{fileName}").ConfigureAwait(false);
                    // get the file bytes
                    using (var content = await file.GetContentAsync().ConfigureAwait(false))
                    {
                        // Set the original logo back
                        await SetSiteLogoThumbnailAsync(fileName, content, false).ConfigureAwait(false);
                    }
                }
                catch (SharePointRestServiceException ex)
                {
                    var error = ex.Error as SharePointRestError;

                    // If the exception indicated a non existing file then ignore, else throw
                    if (!File.ErrorIndicatesFileDoesNotExists(error))
                    {
                        throw;
                    }
                }
            }
        }

        public void ResetSiteLogoThumbnail()
        {
            ResetSiteLogoThumbnailAsync().GetAwaiter().GetResult();
        }

        public async Task SetHeaderBackgroundImageAsync(string fileName, Stream content, double focalX = 0, double focalY = 0, bool overwrite = false)
        {
            if (Layout != HeaderLayoutType.Extended)
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_BackgroundImageHeaderIsNotOfTypeExtended);
            }

            // Upload the image
            IFile siteLogo = await UploadImageToSiteAssetsAsync(fileName, content, overwrite).ConfigureAwait(false);
            // Set the uploaded file as header background
            await (PnPContext.Web as Web).RawRequestAsync(BuildSetSiteLogoApiCall(siteLogo.ServerRelativeUrl, SiteLogoType.HeaderBackground, SiteLogoAspect.Square, focalX, focalY), HttpMethod.Post, "SetSiteLogo").ConfigureAwait(false);
        }

        public void SetHeaderBackgroundImage(string fileName, Stream content, double focalX = 0, double focalY = 0, bool overwrite = false)
        {
            SetHeaderBackgroundImageAsync(fileName, content, focalX, focalY, overwrite).GetAwaiter().GetResult();
        }

        public async Task ClearHeaderBackgroundImageAsync()
        {
            await (PnPContext.Web as Web).RawRequestAsync(BuildSetSiteLogoApiCall("", SiteLogoType.HeaderBackground, SiteLogoAspect.Square, 0, 0), HttpMethod.Post, "SetSiteLogo").ConfigureAwait(false);
        }

        public void ClearHeaderBackgroundImage()
        {
            ClearHeaderBackgroundImageAsync().GetAwaiter().GetResult();
        }

        private async Task<IFile> UploadImageToSiteAssetsAsync(string fileName, Stream content, bool overwrite)
        {
            // Ensure there's a site assets library
            var siteAssetsLibrary = await PnPContext.Web.Lists.EnsureSiteAssetsLibraryAsync(p => p.RootFolder).ConfigureAwait(false);
            // Upload the image file
            var siteLogo = await siteAssetsLibrary.RootFolder.Files.AddAsync(fileName, content, overwrite).ConfigureAwait(false);
            return siteLogo;
        }

        private static ApiCall BuildSetSiteLogoApiCall(string serverRelativeUrl, SiteLogoType type, SiteLogoAspect aspect, double focalX = 0, double focalY = 0)
        {
            string jsonBody;
            if (type == SiteLogoType.HeaderBackground)
            {
                jsonBody = JsonSerializer.Serialize(new
                {
                    relativeLogoUrl = serverRelativeUrl,
                    type = (int)type,
                    aspect = (int)aspect,
                    focalx = focalX,
                    focaly = focalY
                });
            }
            else
            {
                jsonBody = JsonSerializer.Serialize(new
                {
                    relativeLogoUrl = serverRelativeUrl,
                    type = (int)type,
                    aspect = (int)aspect
                });
            }
            return new ApiCall("_api/siteiconmanager/setsitelogo", ApiType.SPORest, jsonBody);
        }

        private static byte[] ToByteArray(Stream source)
        {
            using (var memoryStream = new MemoryStream())
            {
                source.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}