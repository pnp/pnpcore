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

        public async Task SetLogoAsync(string fileName, Stream content, bool overwrite = false)
        {
            // Upload the image
            IFile siteLogo = await UploadImageToSiteAssetsAsync(fileName, content, overwrite).ConfigureAwait(false);
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

        private async Task<IFile> UploadImageToSiteAssetsAsync(string fileName, Stream content, bool overwrite)
        {
            // Ensure there's a site assets library
            var siteAssetsLibrary = await PnPContext.Web.Lists.EnsureSiteAssetsLibraryAsync(p => p.RootFolder).ConfigureAwait(false);
            // Upload the image file
            var siteLogo = await siteAssetsLibrary.RootFolder.Files.AddAsync(fileName, content, overwrite).ConfigureAwait(false);
            return siteLogo;
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

    }
}
