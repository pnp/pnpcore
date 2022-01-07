using PnP.Core.Services;
using PnP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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
                Dictionary<string, string> headers;
                if (MimeTypeMap.TryGetMimeType(fileName, out string mimeType))
                {
                    headers = new Dictionary<string, string>
                    {
                        { "Content-Type", mimeType }
                    };
                }
                else
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_NotAnImageMimeType);
                }

                // We're setting the group logo as that serves as the site logo thumbnail
                var api = new ApiCall("_api/GroupService/SetGroupImage", ApiType.SPORest)
                {
                    Interactive = true,
                    BinaryBody = ToByteArray(content),
                    Headers = headers
                };

                // Set the uploaded file as site logo
                await (PnPContext.Web as Web).RawRequestAsync(api, HttpMethod.Post, "SetGroupImage").ConfigureAwait(false);
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
