using PnP.Core.Services;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class FooterOptions : IFooterOptions
    {
        internal PnPContext PnPContext { get; set; }

        internal MenuState MenuState { get; set; }

        internal FooterOptions(PnPContext pnpContext)
        {
            PnPContext = pnpContext;
        }

        public bool Enabled { get; set; }
        
        public FooterLayoutType Layout { get; set; }

        public FooterVariantThemeType Emphasis { get; set; }

        public string DisplayName { get; set; }

        public FooterLinkAlignment LinkAlignment { get; set; }
        public OverlayColorType OverlayColor { get; set; } = OverlayColorType.None;
        public int OverlayOpacity { get; set; } = 0;
        public OverlayGradientDirectionType OverlayGradientDirection { get; set; } = OverlayGradientDirectionType.TopToBottom;
        public int ColorIndexInLightMode { get; set; } = -1;
        public int ColorIndexInDarkMode { get; set; } = -1;

        public async Task SetLogoAsync(string fileName, Stream content, bool overwrite = false)
        {
            // Upload the image
            IFile siteLogo = await BrandingManager.UploadImageToSiteAssetsAsync(PnPContext, fileName, content, overwrite).ConfigureAwait(false);
            // Set the uploaded file as header background
            await (PnPContext.Web as Web).RawRequestAsync(BrandingManager.BuildSaveMenuStateApiCall(this, siteLogo.ServerRelativeUrl), HttpMethod.Post, "SetSiteLogo").ConfigureAwait(false);
        }

        public void SetLogo(string fileName, Stream content, bool overwrite = false)
        {
            SetLogoAsync(fileName, content, overwrite).GetAwaiter().GetResult();
        }

        public async Task ClearLogoAsync()
        {
            await (PnPContext.Web as Web).RawRequestAsync(BrandingManager.BuildSaveMenuStateApiCall(this, ""), HttpMethod.Post, "SetSiteLogo").ConfigureAwait(false);
        }

        public void ClearLogo()
        {
            ClearLogoAsync().GetAwaiter().GetResult();
        }

        internal MenuStateWrapper GetMenuStateToPersist(string serverRelativeUrl)
        {
            MenuStateWrapper menuStateWrapper = new MenuStateWrapper
            {
                MenuState = MenuState
            };

            if (menuStateWrapper.MenuState != null)
            {
                var titleNode = menuStateWrapper.MenuState.Nodes.FirstOrDefault(p => p.Title == "7376cd83-67ac-4753-b156-6a7b3fa0fc1f");
                if (titleNode != null)
                {
                    var displayNameNode = titleNode.Nodes.FirstOrDefault(p => p.NodeType == 0);
                    if (displayNameNode != null)
                    {
                        displayNameNode.Title = DisplayName;
                    }
                }

                if (serverRelativeUrl != null)
                {
                    var imageNode = menuStateWrapper.MenuState.Nodes.FirstOrDefault(p => p.Title == "2e456c2e-3ded-4a6c-a9ea-f7ac4c1b5100");
                    if (imageNode == null)
                    {
                        menuStateWrapper.MenuState.Nodes.Add(new MenuNode()
                        {
                            FriendlyUrlSegment = "",
                            NodeType = 0,
                            SimpleUrl = serverRelativeUrl,
                            Title = "2e456c2e-3ded-4a6c-a9ea-f7ac4c1b5100"
                        });
                    }
                    else
                    {
                        imageNode.SimpleUrl = serverRelativeUrl;
                    }
                }
            }

            return menuStateWrapper;
        }

        public async Task SetFooterBackgroundImageAsync(string fileName, Stream content, double focalX = 0, double focalY = 0, bool overwrite = false)
        {
            // Upload the image
            IFile siteLogo = await BrandingManager.UploadImageToSiteAssetsAsync(PnPContext, fileName, content, overwrite).ConfigureAwait(false);
            // Set the uploaded file as footer background
            await (PnPContext.Web as Web).RawRequestAsync(BrandingManager.BuildSetSiteLogoApiCall(siteLogo.ServerRelativeUrl, SiteLogoType.FooterBackground, SiteLogoAspect.Square, focalX, focalY), HttpMethod.Post, "SetSiteLogo").ConfigureAwait(false);
        }

        public void SetFooterBackgroundImage(string fileName, Stream content, double focalX = 0, double focalY = 0, bool overwrite = false)
        {
            SetFooterBackgroundImageAsync(fileName, content, focalX, focalY, overwrite).GetAwaiter().GetResult();
        }

        public async Task ClearFooterBackgroundImageAsync()
        {
            await (PnPContext.Web as Web).RawRequestAsync(BrandingManager.BuildSetSiteLogoApiCall("", SiteLogoType.FooterBackground, SiteLogoAspect.Square, 0, 0), HttpMethod.Post, "SetSiteLogo").ConfigureAwait(false);
        }

        public void ClearFooterBackgroundImage()
        {
            ClearFooterBackgroundImageAsync().GetAwaiter().GetResult();
        }
    }
}
