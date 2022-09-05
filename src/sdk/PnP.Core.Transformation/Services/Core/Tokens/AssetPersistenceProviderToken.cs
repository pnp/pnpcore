using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core.Tokens
{
    /// <summary>
    /// Resolves an asset URL and eventually uploads the asset from the persistence provider to the target site
    /// </summary>
    internal class AssetPersistenceProviderToken : ITokenDefinition
    {
        private readonly IAssetPersistenceProvider assetPersistenceProvider;

        public AssetPersistenceProviderToken(IAssetPersistenceProvider assetPersistenceProvider)
        {
            // Retrieve the currently configured instance of the assets persistence provider
            this.assetPersistenceProvider = assetPersistenceProvider ?? throw new ArgumentNullException(nameof(assetPersistenceProvider));
        }

        public string Name { get => "AssetPersistenceProvider"; }

        public string GetValue(PnPContext context, string argument)
        {
            var arguments = argument.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            if (arguments.Length != 2)
            {
                throw new ArgumentException(TransformationResources.Error_InvalidArgumentForAssetPersistenceProviderToken);
            }

            var persistedFilePath = arguments[0];
            var assetSiteRelativeUrl = arguments[1];
            var targetFilePath = assetSiteRelativeUrl.Substring(0, assetSiteRelativeUrl.LastIndexOf('/'));

            // In case the file comes from a publishing portal
            // we need to replace PublishingImages with SiteAssets
            if (targetFilePath.Equals("/PublishingImages", StringComparison.InvariantCultureIgnoreCase))
            {
                targetFilePath = "/SiteAssets";
            }
            var targetFileName = assetSiteRelativeUrl.Substring(assetSiteRelativeUrl.LastIndexOf('/') + 1);

            Task.Run(async () => {

                // Get the file stream from the persistence provider
                var stream = await this.assetPersistenceProvider.ReadAssetAsync(persistedFilePath).ConfigureAwait(false);

                // Handle SPO upload
                IFolder targetFolder = await context.Web.GetFolderByServerRelativeUrlAsync($"{context.Web.ServerRelativeUrl}{targetFilePath}").ConfigureAwait(false);
                IFile addedFile = await targetFolder.Files.AddAsync(targetFileName, stream, true).ConfigureAwait(false);

            }).GetAwaiter().GetResult();

            return $"{context.Web.ServerRelativeUrl}{targetFilePath}/{targetFileName}";
        }
    }
}
