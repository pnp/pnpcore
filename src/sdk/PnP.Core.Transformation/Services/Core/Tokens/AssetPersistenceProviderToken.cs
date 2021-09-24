using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

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

            var stream = this.assetPersistenceProvider.ReadAssetAsync(persistedFilePath);

            // TODO: Handle SPO upload

            return null;
        }
    }
}
