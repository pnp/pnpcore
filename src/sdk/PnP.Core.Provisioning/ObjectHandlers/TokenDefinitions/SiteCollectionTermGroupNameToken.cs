using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{sitecollectiontermgroupname}",
      Description = "Returns the name of the term group associated with the current site collection",
      Example = "{sitecollectiontermgroupname}",
      Returns = "Site Collection - mytenant.sharepoint.com-sites-mysite")]
    internal class SiteCollectionTermGroupNameToken : VolatileTokenDefinition
    {
        public SiteCollectionTermGroupNameToken(PnPContext context)
            : base(context, "{sitecollectiontermgroupname}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                CacheValue = (await SiteCollectionTermGroupResolver
                    .GetAsync(Context).ConfigureAwait(false))?.Name ?? string.Empty;
            }
            return CacheValue;
        }
    }
}